using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.IOGroup
{
    public class ATA : IOGroup
    {
        public readonly IOPortRead Error; // BAR0 + 1 - read only
        public readonly IOPortWrite Features; // BAR0 + 1 - write only
        public readonly IOPort Data; // BAR0
        public readonly IOPort SectorCount; // BAR0 + 2
        public readonly IOPort LBA0; // BAR0 + 3
        public readonly IOPort LBA1; // BAR0 + 4
        public readonly IOPort LBA2; // BAR0 + 5
        public readonly IOPort DeviceSelect; // BAR0 + 6
        public readonly IOPortWrite Command; // BAR0 + 7 - write only
        public readonly IOPortRead Status; // BAR0 + 7 - read only
        public readonly IOPort SectorCountLBA48; // BAR0 + 8
        public readonly IOPort LBA3; // BAR0 + 9
        public readonly IOPort LBA4; // BAR0 + 10
        public readonly IOPort LBA5; // BAR0 + 11
        public readonly IOPortRead AlternateStatus; // BAR1 + 2 - read only 
        public readonly IOPortWrite Control; // BAR1 + 2 - write only
        //* DEVADDRESS: BAR1 + 2; // I don't know what is the benefit from this register

        internal ATA(bool aSecondary)
        {
            if (aSecondary)
            {
                Global.mDebugger.Send("Creating Secondary ATA IOGroup");
            }
            else
            {
                Global.mDebugger.Send("Creating Primary ATA IOGroup");
            }

            var xBAR0 = GetBAR0(aSecondary);
            var xBAR1 = GetBAR1(aSecondary);
            Error = new IOPortRead(xBAR0, 1);
            Features = new IOPortWrite(xBAR0, 1);
            Data = new IOPort(xBAR0);
            SectorCount = new IOPort(xBAR0, 2);
            LBA0 = new IOPort(xBAR0, 3);
            LBA1 = new IOPort(xBAR0, 4);
            LBA2 = new IOPort(xBAR0, 5);
            DeviceSelect = new IOPort(xBAR0, 6);
            Status = new IOPortRead(xBAR0, 7);
            Command = new IOPortWrite(xBAR0, 7);
            SectorCountLBA48 = new IOPort(xBAR0, 8);
            LBA3 = new IOPort(xBAR0, 9);
            LBA4 = new IOPort(xBAR0, 10);
            LBA5 = new IOPort(xBAR0, 11);
            AlternateStatus = new IOPortRead(xBAR1, 2);
            Control = new IOPortWrite(xBAR1, 2);
        }

        public void Wait()
        {
            // Used for the PATA and IOPort latency
            // Widely accepted method is to read the status register 4 times - approx. 400ns delay.
            byte wait;
            wait = Status.Byte;
            wait = Status.Byte;
            wait = Status.Byte;
            wait = Status.Byte;
        }

        private static ushort GetBAR1(bool aSecondary)
        {
            UInt16 xBAR1 = (UInt16)(aSecondary ? 0x0374 : 0x03F4);
            return xBAR1;
        }

        private static ushort GetBAR0(bool aSecondary)
        {
            UInt16 xBAR0 = (UInt16)(aSecondary ? 0x0170 : 0x01F0);
            return xBAR0;
        }
    }
}
