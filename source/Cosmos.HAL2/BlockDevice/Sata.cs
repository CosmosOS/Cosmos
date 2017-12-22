using System;
using System.Collections.Generic;
using Cosmos.Core;
using Cosmos.Core.Memory.Old;
using Cosmos.HAL.BlockDevice.Registers;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice.Ports
{
    public class SATA : StoragePort
    {
        internal static Debugger mSATADebugger = new Debugger("HAL", "SATA");

        public override PortType mPortType => PortType.SATA;
        public override string mPortName => "SATA";
        public override uint mPortNumber => mPortReg.mPortNumber;

        public PortRegisters mPortReg;
        public Core.MemoryGroup.SATA Mem;

        // Constants
        public const ulong RegularSectorSize = 512UL;

        public SATA(PortRegisters aSATAPort)
        {
            // Check if it is really a SATA Port!
            if(aSATAPort.mPortType != PortType.SATA || (aSATAPort.CMD & (1U << 24)) != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\n[Error]");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" 0:{aSATAPort.mPortNumber} is not a SATA port!\n");
                return;
            }

            Mem = new Core.MemoryGroup.SATA();

            mPortReg = aSATAPort;

            // Setting Offset arg to Global offset
            mBlockSize = RegularSectorSize;
            mBlockCount = 256;

            SendSATACommand(ATACommands.Identify);
            UInt16[] xBuffer = new UInt16[256];
            System.Threading.Thread.Sleep(1000);
            Mem.DataBlock.Read16(xBuffer);
            
            var xBlockCount48 = (xBuffer[102] << 32 | (xBuffer[101] << 16 | xBuffer[100])) - 1;
            mBlockCount = (ulong)xBlockCount48;

        }

        public void SendSATACommand(ATACommands aCommand)
        {
            bool isIdentify = (aCommand == ATACommands.Identify);

            mPortReg.IS = unchecked((uint)-1);

            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;
            
            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = (ushort)((isIdentify) ? 1 : 0);
            xCMDHeader.Write = 0;
            
            xCMDHeader.CTBA = Heap.MemAlloc(256);

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, (uint)xSlot);
            
            if (isIdentify)
            {
                Console.WriteLine("Identify");
                xCMDTable.PRDTEntry = new HBAPRDTEntry[xCMDHeader.PRDTL];
                for (uint i = 0; i < xCMDTable.PRDTEntry.Length; i++)
                {
                    xCMDTable.PRDTEntry[i] = new HBAPRDTEntry(xCMDHeader.CTBA + 0x80, i);
                }

                var BaseAddress = Mem.DataBlock.Base;
                xCMDTable.PRDTEntry[xCMDTable.PRDTEntry.Length - 1].DBA = BaseAddress - 2;
                xCMDTable.PRDTEntry[xCMDTable.PRDTEntry.Length - 1].DBC = 511;
                xCMDTable.PRDTEntry[xCMDTable.PRDTEntry.Length - 1].InterruptOnCompletion = 0;
            }
            
            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)aCommand,
                Device = 0,
            };
            
            int xSpin = 0;
            do xSpin++; while ((mPortReg.TFD & 0x88) != 0 && xSlot < 1000000);
            
            mPortReg.CI = 1U << xSlot;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0)
                {
                    break;
                }
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("\n[Fatal]: ");
                    Console.Write("Fatal error occurred while sending command!\n");
                    Console.ResetColor();
                    return;
                }
            }

            if ((mPortReg.IS & (1 << 30)) != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\n[Fatal]: ");
                Console.Write("Fatal error occurred while sending command!\n");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n[Success]: ");
            Console.Write("Command has been sent successfully!\n");
            Console.ResetColor();

            while (mPortReg.CI != 0) ;

            return;
        }

        public void SendSATA24Command(ATACommands aCommand, uint aStart, uint aCount)
        {
            mPortReg.IS = unchecked((uint)-1);

            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;

            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);
            xCMDHeader.Write = 0;

            xCMDHeader.CTBA = Heap.MemAlloc(256);

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, (uint)xSlot);

            xCMDTable.PRDTEntry = new HBAPRDTEntry[xCMDHeader.PRDTL];
            for (uint i = 0; i < xCMDTable.PRDTEntry.Length; i++)
            {
                xCMDTable.PRDTEntry[i] = new HBAPRDTEntry(xCMDHeader.CTBA + 0x80, i);
            }

            uint BaseAddress = Mem.DataBlock.Base;
            for (uint i = 0; i < xCMDHeader.PRDTL - 1; i++)
            {
                xCMDTable.PRDTEntry[i].DBA = BaseAddress;
                xCMDTable.PRDTEntry[i].DBC = 8191;
                xCMDTable.PRDTEntry[i].InterruptOnCompletion = 0;
                BaseAddress += 8192;
                aCount -= 16;
            }

            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = BaseAddress - 2;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = 512 * aCount - 1;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 0;

            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)aCommand,

                LBA0 = (byte)(aStart),
                LBA1 = (byte)(aStart >> 8),
                LBA2 = (byte)(aStart >> 16),
                Device = (byte)(0x40 | ((aStart >> 24) & 0x0F)),

                CountL = (byte)(aCount & 0xFF)
            };

            int xSpin = 0;
            do xSpin++; while ((mPortReg.TFD & 0x88) != 0 && xSlot < 1000000);

            mPortReg.CI = 1U << xSlot;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0)
                {
                    break;
                }
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("\n[Fatal]: ");
                    Console.Write("Fatal error occurred while sending command!\n");
                    Console.ResetColor();
                    return;
                }
            }

            if ((mPortReg.IS & (1 << 30)) != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\n[Fatal]: ");
                Console.Write("Fatal error occurred while sending command!\n");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n[Success]: ");
            Console.Write("Command has been sent successfully!\n");
            Console.ResetColor();

            while (mPortReg.CI != 0) ;

            return;
        }

        public void SendSATA48Command(ATACommands aCommand, ulong aStart, uint aCount)
        {
            mPortReg.IS = unchecked((uint)-1);

            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;
            
            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);
            xCMDHeader.Write = 0;

            xCMDHeader.CTBA = Heap.MemAlloc(256);

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, (uint)xSlot);

            xCMDTable.PRDTEntry = new HBAPRDTEntry[xCMDHeader.PRDTL];
            for (uint i = 0; i < xCMDTable.PRDTEntry.Length; i++)
            {
                xCMDTable.PRDTEntry[i] = new HBAPRDTEntry(xCMDHeader.CTBA + 0x80, i);
            }

            uint BaseAddress = Mem.DataBlock.Base;
            for (uint i = 0; i < xCMDHeader.PRDTL - 1; i++)
            {
                xCMDTable.PRDTEntry[i].DBA = BaseAddress;
                xCMDTable.PRDTEntry[i].DBC = 8191;
                xCMDTable.PRDTEntry[i].InterruptOnCompletion = 0;
                BaseAddress += 8192;
                aCount -= 16;
            }

            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = BaseAddress - 2;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = 512 * aCount - 1;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 0;

            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)aCommand,

                LBA0 = (byte)(aStart),
                LBA1 = (byte)(aStart >> 8),
                LBA2 = (byte)(aStart >> 16),
                LBA3 = (byte)(aStart >> 24),
                LBA4 = (byte)(aStart >> 32),
                LBA5 = (byte)(aStart >> 40),
                Device = 0x40,

                CountL = (byte)(aCount & 0xFF),
                CountH = (byte)((aCount >> 8))
            };

            int xSpin = 0;
            do xSpin++; while ((mPortReg.TFD & 0x88) != 0 && xSlot < 1000000);

            mPortReg.CI = 1U << xSlot;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0)
                {
                    break;
                }
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("\n[Fatal]: ");
                    Console.Write("Fatal error occurred while sending command!\n");
                    Console.ResetColor();
                    return;
                }
            }

            if ((mPortReg.IS & (1 << 30)) != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("\n[Fatal]: ");
                Console.Write("Fatal error occurred while sending command!\n");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n[Success]: ");
            Console.Write("Command has been sent successfully!\n");
            Console.ResetColor();

            while (mPortReg.CI != 0) ;

            return;
        }

        public int FindCMDSlot()
        {
            // If not set in SACT and CI, the slot is free
            var xSlots = (mPortReg.SACT | mPortReg.CI);
        
            for (int i = 1; i < 32; i++)
            {
                if ((xSlots & 1) == 0)
                    return i;
                xSlots >>= 1;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n[Error]: ");
            Console.Write("Cannot find a free command slot!\n");
            Console.ResetColor();
            return -1;
        }

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            SendSATA48Command(ATACommands.ReadDmaExt, (uint)aBlockNo, (uint)aBlockCount);
            System.Threading.Thread.Sleep(500);
            Mem.DataBlock.Read8(aData);
            foreach (byte xData in aData)
                mSATADebugger.SendNumber(xData);
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            Mem.DataBlock.Write8(aData);
            SendSATA48Command(ATACommands.WriteDmaExt, (uint)(aBlockNo), (uint)aBlockCount);
            SendSATACommand(ATACommands.CacheFlush);
        }
    }
}
