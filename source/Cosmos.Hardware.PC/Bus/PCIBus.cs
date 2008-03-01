using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus
{

    public class PCIBus : Cosmos.Hardware.Bus.PCIBus
    {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init()
        {
            var xDeviceIDs = new DeviceIDs();
            Console.WriteLine("PCI Devices");
            Console.WriteLine();

            byte xMaxBus = GetMaxBus();
            for (byte xBus = 0; xBus <= xMaxBus; xBus++)
            {
                for (byte xSlot = 0; xSlot <= 31; xSlot++)
                {
                    for (byte xFunction = 0; xFunction <= 7; xFunction++)
                    {

                        PCIDevice xPCIDevice = new PCIDevice(xBus, xSlot, xFunction);

                        if (xPCIDevice.DeviceExists)
                        {
                            string xVendor = xDeviceIDs.FindVendor(xPCIDevice.VendorID);

                            Console.Write(xPCIDevice.Bus.ToString());
                            Console.Write("-");
                            Console.Write(xPCIDevice.Slot.ToString());
                            Console.Write("-");
                            Console.Write(xPCIDevice.Function.ToString());
                            Console.Write(" ");
                            Console.Write(xVendor);
                            Console.Write(": ");
                            Console.Write(xPCIDevice.DeviceID.ToString());

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        private static byte GetMaxBus()
        {
            /*
union REGS r;

r.h.ah = PCI_FUNCTION_ID; //0xb1

r.h.al = PCI_BIOS_PRESENT; //0x01

int86(0x1a, &r, &r);

GetMaxBus = r.w.cx;
             */

            return 255;
        }
    }

    [Flags]
    public enum PCICommand : short
    {
        IO = 0x1,     /* Enable response in I/O space */
        Memort = 0x2,     /* Enable response in Memory space */
        Master = 0x4,     /* Enable bus mastering */
        Special = 0x8,     /* Enable response to special cycles */
        Invalidate = 0x10,    /* Use memory write and invalidate */
        VGA_Pallete = 0x20,   /* Enable palette snooping */
        Parity = 0x40,    /* Enable parity checking */
        Wait = 0x80,    /* Enable address/data stepping */
        SERR = 0x100,   /* Enable SERR */
        Fast_Back = 0x200,   /* Enable back-to-back writes */
    }

    [Flags]
    public enum PCIStatus : int
    {
        CAP_LIST = 0x10,   /* Support Capability List */
        SUPPORT_66MHZ = 0x20,    /* Support 66 Mhz PCI 2.1 bus */
        UDF = 0x40,    /* Support User Definable Features [obsolete] */
        FAST_BACK = 0x80,    /* Accept fast-back to back */
        PARITY = 0x100,   /* Detected parity error */
        DEVSEL_MASK = 0x600,   /* DEVSEL timing */
        DEVSEL_FAST = 0x000,
        DEVSEL_MEDIUM = 0x200,
        DEVSEL_SLOW = 0x400,
        SIG_TARGET_ABORT = 0x800, /* Set on target abort */
        REC_TARGET_ABORT = 0x1000, /* Master ack of " */
        REC_MASTER_ABORT = 0x2000, /* Set on master abort */
        SIG_SYSTEM_ERROR = 0x4000, /* Set when we drive SERR */
        DETECTED_PARITY = 0x8000 /* Set on parity error */
    }


    [Flags]
    public enum PCIHeaderType : byte
    {
        Normal = 0,
        Bridge = 1,
        Cardbus = 2
    }
    [Flags]
    public enum PCIBist : byte
    {
        CocdMask = 0x0f,   /* Return result */
        Start = 0x40,   /* 1 to start BIST, 2 secs or less */
        Capable = 0x80    /* 1 if BIST capable */
    }

    public class PCIDevice
    {

        public PCIDevice(byte bus, byte slot, byte function)
        {
            this.Bus = bus;
            this.Slot = slot;
            this.Function = function;
        }

        public byte Bus { get; private set; }
        public byte Slot { get; private set; }
        public byte Function { get; private set; }

        public bool DeviceExists { get { return VendorID != 0xFFFF && VendorID != 0x0; } }

        public UInt32 VendorID { get { return Read16(0x0); } }
        public UInt16 DeviceID { get { return Read16(0x2); } }

        public PCICommand Command { get { return (PCICommand)Read16(0x4); } set { Write16(0x4, (ushort)value); } }
        public PCIStatus Status { get { return (PCIStatus)Read16(0x6); } set { Write16(0x6, (ushort)value); } }

        public UInt32 ClassCode { get { return Read32(0x8) >> 8; } }
        public byte RevisionID { get { return Read8(0x8); } }

        public byte CacheLineSize { get { return Read8(0x0c); } set { Write8(0x0c, value); } }
        public byte LatencyTimer { get { return Read8(0x0d); } set { Write8(0x0d, value); } }
        public PCIHeaderType HeaderType { get { return (PCIHeaderType)Read8(0x0e); } set { Write8(0x0e, (byte)value); } }
        public PCIBist Bist { get { return (PCIBist)Read8(0x0f); } set { Write8(0x0f, (byte)value); } }

        public UInt32 BaseAddress0 { get { return Read32(0x10); } set { Write32(0x10, value); } }
        public UInt32 BaseAddress1 { get { return Read32(0x14); } set { Write32(0x14, value); } }
        public UInt32 BaseAddress2 { get { return Read32(0x18); } set { Write32(0x18, value); } }
        public UInt32 BaseAddress3 { get { return Read32(0x1a); } set { Write32(0x1a, value); } }
        public UInt32 BaseAddress4 { get { return Read32(0x20); } set { Write32(0x20, value); } }
        public UInt32 BaseAddress5 { get { return Read32(0x24); } set { Write32(0x24, value); } }
        // more registers need to be filled in

        public byte InterruptLine { get { return Read8(0x3c); } set { Write8(0x3c, value); } }
        public byte InterruptPin { get { return Read8(0x3d); } set { Write8(0x3d, value); } }
        public byte MinGNT { get { return Read8(0x3e); } set { Write8(0x3e, value); } }
        public byte MaxLAT { get { return Read8(0x3f); } set { Write8(0x3f, value); } }

        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        private UInt32 GetAddress(byte aRegister)
        {
            return (UInt32)(
                // Bits 23-16
                ((UInt32)Bus << 16)
                // Bits 15-11
                | (((UInt32)Slot & 0x1F) << 11)
                // Bits 10-8
                | (((UInt32)Function & 0x07) << 8)
                // Bits 7-0
                | ((UInt32)aRegister & 0xFF)
                // Enable bit - must be set
                | 0x80000000);
        }

        public UInt32 Read32(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read32(ConfigData);
        }

        public UInt16 Read16(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read16(ConfigData);
        }

        public byte Read8(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read8(ConfigData);
        }

        public void Write32(byte aRegister, UInt32 value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write32(ConfigData, value);
        }

        public void Write16(byte aRegister, UInt16 value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write16(ConfigData, value);
        }

        public void Write8(byte aRegister, byte value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write8(ConfigData, value);
        }
    }
}