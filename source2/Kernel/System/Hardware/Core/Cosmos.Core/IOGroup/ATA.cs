using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core.IOGroup {
    public class ATA : IOGroup {
        public readonly IOPort Data;
        //* Error Register: BAR0 + 1; // Read Only
        //* Features Register: BAR0 + 1; // Write Only
        //* SECCOUNT0: BAR0 + 2; // Read-Write
        //* LBA0: BAR0 + 3; // Read-Write
        //* LBA1: BAR0 + 4; // Read-Write
        //* LBA2: BAR0 + 5; // Read-Write
        //* HDDEVSEL: BAR0 + 6; // Read-Write, used to select a drive in the channel.
        //* Command Register: BAR0 + 7; // Write Only.
        //* Status Register: BAR0 + 7; // Read Only.
        //* Alternate Status Register: BAR1 + 2; // Read Only.
        public readonly IOPortWrite Control;
        //* DEVADDRESS: BAR1 + 2; // I don't know what is the benefit from this register
        
        internal ATA(bool aSecondary) {
            UInt16 xBAR0 = (UInt16)(aSecondary ? 0x0170 : 0x01F0);
            UInt16 xBAR1 = (UInt16)(aSecondary ? 0x0374 : 0x03F4);
            Data = new IOPort(xBAR0);
            Control = new IOPortWrite(xBAR1, 2);
        }
    }
}
