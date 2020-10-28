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
        public UInt32 USBCMD { get { return regs.Read(0x00); } set { regs.Write(0x00, value); } }
        public UInt32 USBSTS { get { return regs.Read(0x02); } set { regs.Write(0x02, value); } }
        public UInt32  USBINTR { get { return regs.Read(0x04); } set { regs.Write(0x04, value); } }
        public UInt32 FRNUM { get { return regs.Read(0x06); } set { regs.Write(0x06, value); } }
        public UInt32 FRBASEADD { get { return regs.Read(0x08); } set { regs.Write(0x08, value); } }
        public UInt32 SOFMOD { get { return regs.Read(0x0C); } set { regs.Write(0x0C, value); } }
        public UInt32 PORTSC1 { get { return regs.Read(0x10); } set { regs.Write(0x10, value); } }
        public UInt32 PORTSC2 { get { return regs.Read(0x12); } set { regs.Write(0x12, value); } }
    }





    public enum USBUHCIStates : byte
    {
        Reset = 0, Resume = 1, Operational = 2, Suspend = 3
    }
}
