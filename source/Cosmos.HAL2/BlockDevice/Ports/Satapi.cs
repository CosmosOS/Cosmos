using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core.Memory.Old;
using Cosmos.HAL.BlockDevice.Registers;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice.Ports
{
    public class SATAPI : StoragePort
    {
        internal static Debugger mSATAPIDebugger = new Debugger("HAL", "SATAPI");

        public PortRegisters mPortReg;

        public override PortType mPortType => PortType.SATAPI;
        public override string mPortName => "SATAPI";
        public override uint mPortNumber => mPortReg.mPortNumber;

        public SATAPI(PortRegisters aSATAPIPort)
        {

            // Check if it is really a SATAPI Port!
            if (aSATAPIPort.mPortType != PortType.SATAPI || (aSATAPIPort.CMD & (1U << 24)) == 0)
            {
                throw new Exception($"SATAPI Error: 0:{mPortNumber} is not SATAPI port!");
            }
            mSATAPIDebugger.Send("SATAPI Constructor");

            mPortReg = aSATAPIPort;

            mBlockSize = 2048;
        }

        public void SendSATAPICommand(ATACommands aCommand, uint aStart, uint aCount)
        {
            mPortReg.IS = unchecked((uint)-1);

            int xSlot = FindCMDSlot(mPortReg);
            if (xSlot == -1) return;

            HBACommandHeader xCMDHeader = new HBACommandHeader(mPortReg.CLB, (uint)xSlot);
            xCMDHeader.CFL = 5;
            xCMDHeader.ATAPI = 1;
            xCMDHeader.PRDTL = (ushort)(((aCount - 1) >> 4) + 1);
            xCMDHeader.Write = 0;

            xCMDHeader.CTBA = Heap.MemAlloc(128 + ((uint)xCMDHeader.PRDTL) * 16);

            HBACommandTable xCMDTable = new HBACommandTable(xCMDHeader.CTBA, xCMDHeader.PRDTL);

            uint DataBaseAddress = 0x0046C000;
            for (int i = 0; i < xCMDHeader.PRDTL - 1; i++)
            {
                xCMDTable.PRDTEntry[i].DBA = DataBaseAddress;
                xCMDTable.PRDTEntry[i].DBC = 8 * 1024 - 1;   // 8K bytes (this value should always be set to 1 less than the actual value)
                xCMDTable.PRDTEntry[i].InterruptOnCompletion = 1;
                DataBaseAddress += 8 * 1024;    // 4K words
                aCount -= 16;    // 16 sectors
            }

            // Last entry
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBA = DataBaseAddress;
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].DBC = aCount * 512 - 1;   // 8K bytes (this value should always be set to 1 less than the actual value)
            xCMDTable.PRDTEntry[xCMDHeader.PRDTL - 1].InterruptOnCompletion = 1;

            FISRegisterH2D xCMDFIS = new FISRegisterH2D(xCMDTable.CFIS)
            {
                FISType = (byte)FISType.FIS_Type_RegisterH2D,
                IsCommand = 1,
                Command = (byte)ATACommands.Packet,
                Device = 1 << 4
            };

            byte[] xATAPICMD = new byte[12];
            xATAPICMD[0] = (byte)aCommand;
            xATAPICMD[2] = (byte)((aStart >> 0x18) & 0xFF);
            xATAPICMD[3] = (byte)((aStart >> 0x10) & 0xFF);
            xATAPICMD[4] = (byte)((aStart >> 0x08) & 0xFF);
            xATAPICMD[5] = (byte)((aStart >> 0x00) & 0xFF);
            xATAPICMD[9] = (byte)(aCount);
            for (uint i = 0; i < xATAPICMD.Length; i++)
            new Core.MemoryBlock(xCMDTable.ACMD, 12).Bytes[i] = xATAPICMD[i];
            
            int xSpin = 0;
            do xSpin++; while ((mPortReg.TFD & 0x88) != 0 && xSpin < 1000000);

            if (xSpin == 1000000)
            {
                mSATAPIDebugger.Send($"Port {mPortNumber} timed out!");
                return;
            };

            mPortReg.CI = 1U;

            while(true)
            {
                if((mPortReg.CI & (1 << xSlot)) == 0) break;
                if ((mPortReg.IS & (1 << 30)) != 0)
                {
                    throw new Exception("SATA Fatal error: Command aborted");
                    //mSATADebugger.Send("[Fatal]: Fatal error occurred while sending command!");
                    //PortReset(mPortReg);
                    return;
                }
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("\n[Success]: ");
            //Console.Write("Command has been sent successfully!");
            //Console.ResetColor();

            return;
        }

        public int FindCMDSlot(PortRegisters aPort)
        {
            // If not set in SACT and CI, the slot is free
            var xSlots = (aPort.SACT | aPort.CI);

            for (int i = 1; i < 32; i++)
            {
                if ((xSlots & 1) == 0)
                    return i;
                xSlots >>= 1;
            }
            mSATAPIDebugger.Send("SATA Error: Cannot find a free command slot!");
            return -1;
        }

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            SendSATAPICommand(ATACommands.Read, (uint)aBlockNo, (uint)aBlockCount);
            byte[] xByte = new byte[512];
            new MemoryBlock(0x0046C000, 512).Read8(xByte);
            for (int i = 0; i < 512; i++)
            {
                Console.Write(xByte[i]);
            }
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            // To be implemented!
        }
    }
}
