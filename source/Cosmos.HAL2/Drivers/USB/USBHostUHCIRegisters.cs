using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL;
using Cosmos;
using System.ComponentModel;
using Cosmos.Core;

namespace Cosmos.HAL.USB
{
    public class USBHostUHCIRegisters
    {
        /* Command Register */
        public const uint _USBCMD = 0;
        public const uint _USBCMD_RS = 0x01; /* Run/Stop */
        public const uint _USBCMD_HCRESET = 0x02; /* Host Reset */
        public const uint _USBCMD_GRESET = 0x04; /* Global Reset */
        public const uint _USBCMD_EGSM = 0x08; /* Global Suspend Mode */
        public const uint _USBCMD_FGR = 0x010; /* Force Global Resume */
        public const uint _USBCMD_SWDBG = 0x20; /* SW Debug Mode */
        public const uint _USBCMD_CF = 0x40; /* Config Flag (sw only) */
        public const uint _USBCMD_MAXP = 0x80; /* Max Packet (0 = 32 , 1 = 64) */
        private PCIBaseAddressBar regs;

        public USBHostUHCIRegisters(PCIBaseAddressBar reqs)
        {
            regs = reqs;

        }
        public ushort USBCMD { get { return regs.Read16(0x00); } set { regs.Write16(0x00, (ushort)value); } }
        public ushort USBSTS { get { return regs.Read16(0x02); } set { regs.Write16(0x02, (ushort)value); } }
        public ushort  USBINTR { get { return regs.Read16(0x04); } set { regs.Write16(0x04, (ushort)value); } }
        public ushort FRNUM { get { return regs.Read16(0x06); } set { regs.Write16(0x06, (ushort)value); } }
        public ushort FRBASEADD { get { return regs.Read16(0x08); } set { regs.Write16(0x08, (ushort)value); } }
        public ushort SOFMOD { get { return regs.Read16(0x0C); } set { regs.Write16(0x0C, (ushort)value); } }
        public ushort PORTSC1 { get { return regs.Read16(0x10); } set { regs.Write16(0x10, (ushort)value); } }
        public ushort PORTSC2 { get { return regs.Read16(0x12); } set { regs.Write16(0x12, (ushort)value); } }
    }





    public enum USBUHCIStates : byte
    {
        Reset = 0, Resume = 1, Operational = 2, Suspend = 3
    }
}
