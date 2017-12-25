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
        public Core.MemoryGroup.AHCI Mem;

        // Constants
        public const ulong RegularSectorSize = 512UL;

        // Properties
        private string mSerialNo;
        private string mFirmwareRev;
        private string mModelNo;

        public string SerialNo { get => mSerialNo; }
        public string FirmwareRev { get => mFirmwareRev; }
        public string ModelNo { get => mModelNo; }

        public SATA(PortRegisters aSATAPort)
        {
            // Check if it is really a SATA Port!
            if (aSATAPort.mPortType != PortType.SATA || (aSATAPort.CMD & (1U << 24)) != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("[Error]");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" 0:{aSATAPort.mPortNumber} is not a SATA port!\n");
                return;
            }

            Mem = new Core.MemoryGroup.AHCI((uint)RegularSectorSize);

            mPortReg = aSATAPort;

            // Setting Offset arg to Global offset
            mBlockSize = RegularSectorSize;

            // TODO: Use SendSATACommand(ATACommands.Identify) and copy the useful isIdentify if's from SendSATA28Command
            //       But make sure that isIdentify returns the exact value (true if the command is identify
            //       or false if not identify).
            SendSATA28Command((ATACommands)0x00, 0, 0);
            UInt16[] xBuffer = new UInt16[256];
            Mem.DataBlock.Read16(xBuffer);
            
            mSerialNo = GetString(xBuffer, 10, 20);
            mFirmwareRev = GetString(xBuffer, 23, 8);
            mModelNo = GetString(xBuffer, 27, 40);

            mBlockCount = ((UInt32)xBuffer[61] << 16 | xBuffer[60]) - 1;

        }

        public void SendSATACommand(ATACommands aCommand)
        {
            mPortReg.IS = 0xFFFF;

            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;

            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = 1;
            xCMDHeader.Write = 0;

            xCMDHeader.CTBA = (uint)((uint)(Base.AHCI + 0xA000) + (0x2000 * mPortNumber) + (0x100 * xSlot));

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, xCMDHeader.PRDTL);
            
            uint DataBaseAddress = Mem.DataBlock.Base;
            xCMDTable.PRDTEntry[0].DBA = DataBaseAddress;
            xCMDTable.PRDTEntry[0].DBC = 511;
            xCMDTable.PRDTEntry[0].InterruptOnCompletion = 1;

            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)aCommand,
                Device = 0
            };
            
            while ((mPortReg.TFD & 0x88) != 0) ;
            
            mPortReg.CI = 1U;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0) break;
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    mSATADebugger.Send("[Fatal]: Fatal error occurred while sending command!");
                    PortReset(mPortReg);
                    return;
                }
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("[Success]: ");
            //Console.Write("Command has been sent successfully!\n");
            //Console.ResetColor();

            return;
        }

        public void SendSATA28Command(ATACommands aCommand, uint aStart, uint aCount)
        {
            bool isIdentify = false;
            if (aStart == 0 && aCount == 0) isIdentify = true;

            mPortReg.IS = 0xFFFF;
            
            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;
            
            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = 1;
            xCMDHeader.Write = 0;
            
            xCMDHeader.CTBA = (uint)((uint)(Base.AHCI + 0xA000) + (0x2000 * mPortNumber) + (0x100 * xSlot));

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, xCMDHeader.PRDTL);

            uint DataBaseAddress = Mem.DataBlock.Base;

            // Last entry
            if (isIdentify)
            {
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = DataBaseAddress;
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = 511;
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 1;
            }
            else
            {
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = DataBaseAddress;
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = aCount * 512 - 1;   // 8K bytes (this value should always be set to 1 less than the actual value)
                xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 1;
            }

            if (isIdentify)
            {
                FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
                {
                    FISType = (byte)FISType.FIS_Type_RegisterH2D,
                    IsCommand = 1,
                    Command = (byte)ATACommands.Identify,
                    Device = 0
                };
            }
            else
            {
                FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
                {
                    FISType = (byte)FISType.FIS_Type_RegisterH2D,
                    IsCommand = 1,
                    Command = (byte)aCommand,

                    LBA0 = (byte)((aStart) & 0xFF),
                    LBA1 = (byte)((aStart >> 8) & 0xFF),
                    LBA2 = (byte)((aStart >> 16) & 0xFF),
                    Device = (byte)(0x40 | ((aStart >> 24) & 0x0F)),

                    CountL = (byte)(aCount & 0xFF)
                };
            }
            
            while ((mPortReg.TFD & 0x88) != 0);
            
            mPortReg.CI = 1U;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0) break;
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    mSATADebugger.Send("[Fatal]: Fatal error occurred while sending command!");
                    PortReset(mPortReg);
                    return;
                }
            }
            
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("[Success]: ");
            //Console.Write("Command has been sent successfully!\n");
            //Console.ResetColor();

            return;
        }

        public void SendSATA48Command(ATACommands aCommand, ulong aStart, uint aCount)
        {
            mPortReg.IS = 0xFFFF;

            int xSlot = FindCMDSlot();
            if (xSlot == -1) return;
            
            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.PRDTL = 1;
            xCMDHeader.Write = 0;

            xCMDHeader.CTBA = (uint)((uint)(Base.AHCI + 0xA000) + (0x2000 * mPortNumber) + (0x100 * xSlot));

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, xCMDHeader.PRDTL);

            uint DataBaseAddress = Mem.DataBlock.Base;

            // Last entry
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = DataBaseAddress;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = (aCount * 512) - 1;   // 8K bytes (this value should always be set to 1 less than the actual value)
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 1;

            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)aCommand,

                LBA0 = (byte)((aStart >> 00) & 0xFF),
                LBA1 = (byte)((aStart >> 08) & 0xFF),
                LBA2 = (byte)((aStart >> 16) & 0xFF),
                LBA3 = (byte)((aStart >> 24) & 0xFF),
                LBA4 = (byte)((aStart >> 32) & 0xFF),
                LBA5 = (byte)((aStart >> 40) & 0xFF),

                Device = 1 << 6,

                CountL = (byte)(aCount & 0xFF),
                CountH = (byte)((aCount >> 8) & 0xFF)
            };
            
            while ((mPortReg.TFD & 0x88) != 0) ;

            mPortReg.CI = 1U;

            while (true)
            {
                if ((mPortReg.CI & (1 << xSlot)) == 0) break;
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    mSATADebugger.Send("[Fatal]: Fatal error occurred while sending command!");
                    PortReset(mPortReg);
                    return;
                }
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("[Success]: ");
            //Console.Write("Command has been sent successfully!\n");
            //Console.ResetColor();
            return;
        }

        public static void PortReset(PortRegisters aPort)
        {
            // TODO: Make a connection between AHCI Methods and SATA

            // Semi-StopCMD()
            aPort.CMD &= ~(1U << 0);
            int i;
            for(i = 0; i <= 50; i++)
            {
                if ((aPort.CMD & (1 << 0)) == 0) break;
                AHCI.Wait(10000);
            }
            if (i == 101) AHCI.HBAReset();

            aPort.SCTL = 1;
            AHCI.Wait(1000);
            aPort.SCTL &= ~(1U << 0);

            while ((aPort.SSTS & 0x0F) != 3) ;

            aPort.SERR = 1;

            while ((aPort.SCTL & 0x0F) != 0) ;
        }

        private void HBAReset() => AHCI.HBAReset();

        private int FindCMDSlot()
        {
            // If not set in SACT and CI, the slot is free
            var xSlots = (mPortReg.SACT | mPortReg.CI);
        
            for (int i = 0; i < 32; i++)
            {
                if ((xSlots & 1) == 0)
                    return i;
                xSlots >>= 1;
            }

            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.Write("[Error]: ");
            //Console.Write("Cannot find a free command slot!\n");
            //Console.ResetColor();
            return -1;
        }

        protected string GetString(UInt16[] aBuffer, int aIndexStart, int aStringLength)
        {
            // Would be nice to convert to byte[] and use
            // new string(ASCIIEncoding.ASCII.GetChars(xBytes));
            // But it requires some code Cosmos doesnt support yet
            var xChars = new char[aStringLength];
            for (int i = 0; i < aStringLength / 2; i++)
            {
                UInt16 xChar = aBuffer[aIndexStart + i];
                xChars[i * 2] = (char)(xChar >> 8);
                xChars[i * 2 + 1] = (char)xChar;
            }
            return new string(xChars);
        }

        // BlockDevice Methods
        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            //CheckDataSize(aData, aBlockCount);
            //CheckBlockNo(aBlockNo, aBlockCount);
            SendSATA48Command(ATACommands.ReadDmaExt, (uint)aBlockNo, (uint)aBlockCount);
            Mem.DataBlock.Read8(aData);
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            Mem.DataBlock.Write8(aData);
            SendSATA48Command(ATACommands.WriteDmaExt, (uint)(aBlockNo), (uint)aBlockCount);
            SendSATACommand(ATACommands.CacheFlush);
        }
    }
}
