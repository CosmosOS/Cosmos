using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware.Network;

namespace Cosmos.Hardware.Network.Devices.RTL8139.Register
{
    class MainRegister
    {
        private MemoryAddressSpace mem;
        public MainRegister(MemoryAddressSpace mem)
        {
            this.mem = mem;
        }

        public MACAddress Mac
        {
            get
            {
                byte[] b = new byte[6];

                for (byte i = 0; i < 6; i++)
                    b[i] = mem.Read8Unchecked((UInt32)Bit.MAC0 + i);
                
                return new MACAddress(b);
            }
            set
            {
                for (byte b=0; b<6; b++)
                    mem.Write8Unchecked((UInt32)Bit.MAC0 + b, value.bytes[b]);      
            }
        }


        public byte[] Mar
        {
            get
            {
                byte[] b = new byte[6];

                for (byte i = 0; i < 6; i++)
                    b[i] = mem.Read8Unchecked((UInt32)Bit.MAR0 + i);

                return b;
            }
            set
            {
                for (byte b = 0; b < 6; b++)
                    mem.Write8Unchecked((UInt32)Bit.MAR0 + b, value[b]);
            }
        }

        public byte Config0
        {
            get
            {
                return mem.Read8Unchecked((UInt32)Bit.Config0);
            }
            set
            {
                mem.Write8Unchecked((UInt32)Bit.Config0, value);
            }
        }
        public byte Config1
        {
            get
            {
                return mem.Read8Unchecked((UInt32)Bit.Config1);
            }
            set
            {
                mem.Write8Unchecked((UInt32)Bit.Config1, value);
            }
        }

        public UInt32 TxConfig
        {
            get
            {
                return mem.Read32((UInt32)Bit.TxConfig);
            }
            set
            {
                mem.Write32((UInt32)Bit.TxConfig, value);
            }
        }

        public ChipCommandFlags ChipCmd
        {
            get
            {
                return (ChipCommandFlags)mem.Read8Unchecked((UInt32)Bit.ChipCmd);
            }
            set
            {
                mem.Write8Unchecked((UInt32)Bit.ChipCmd, (byte)value);
            }
        }
        public bool ChipCmdTest(ChipCommandFlags mask)
        {
            return (ChipCmd & mask) == mask;
        }

        [Flags]
        public enum ChipCommandFlags : byte
        {
            BUFE = 1,
            TE = 4,
            RE = 8,
            RST = 16
        }

        /// <summary>
        /// The RTL8139 contains 64 x 16 bit EEPROM registers.
        /// </summary>
        public enum Bit : byte
        {
            MAC0 = 0x00,            // Ethernet hardware address
            MAR0 = 0x08,            // Multicast filter
            TSD0 = 0x10,       // Transmit status (Four 32bit registers)
            TSD1 = 0x14,
            TSD2 = 0x18,
            TSD3 = 0x1C,
            TSAD0 = 0x20,         // Tx descriptors (also four 32bit)
            TSAD1 = 0x24,
            TSAD2 = 0x28,
            TSAD3 = 0x2C,
            RxBuf = 0x30,
            RxEarlyCnt = 0x34,
            RxEarlyStatus = 0x36,
            ChipCmd = 0x37,
            RxBufPtr = 0x38,
            RxBufAddr = 0x3A,
            IntrMask = 0x3C,
            IntrStatus = 0x3E,
            TxConfig = 0x40,
            RxConfig = 0x44,
            Timer = 0x48,           // A general-purpose counter
            RxMissed = 0x4C,        // 24 bits valid, write clears
            Cfg9346 = 0x50,
            Config0 = 0x51,
            Config1 = 0x52,
            FlashReg = 0x54,
            GPPinData = 0x58,
            GPPinDir = 0x59,
            MII_SMI = 0x5A,
            HltClk = 0x5B,
            MultiIntr = 0x5C,
            TxSummary = 0x60,
            MII_BMCR = 0x62,
            MII_BMSR = 0x64,
            NWayAdvert = 0x66,
            NWayLPAR = 0x68,
            NWayExpansion = 0x6A
        }

    }
}
