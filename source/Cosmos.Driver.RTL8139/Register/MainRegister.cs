using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Driver.RTL8139.Register
{
    class MainRegister
    {
        /// <summary>
        /// The RTL8139 contains 64 x 16 bit EEPROM registers.
        /// </summary>
        public enum Bit : byte
        {
            MAC0 = 0x00,            // Ethernet hardware address
            MAR0 = 0x08,            // Multicast filter
            TxStatus0 = 0x10,       // Transmit status (Four 32bit registers)
            TxAddr0 = 0x20,         // Tx descriptors (also four 32bit)
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
