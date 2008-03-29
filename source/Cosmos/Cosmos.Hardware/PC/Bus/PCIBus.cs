using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus
{

    public class PCIBus : Cosmos.Hardware.Bus.PCIBus
    {
        public static PCIDevice[] Devices = new PCIDevice[0];

        private static bool haveEnumerated = false;

        public static PCIDevice GetPCIDevice(byte bus, byte slot, byte function)
        {
            if (!haveEnumerated)
                Init();

            foreach (PCIDevice dev in PCIBus.Devices)
            {
                if (dev.Bus == bus &&
                    dev.Slot == slot &&
                    dev.Function == function)

                    return dev;
            }

            return null;
        }

        public static void Init()
        {
            //Console.WriteLine("Cosmos.Hardware.PC.Bus.Init()");

            List<PCIDevice> devices = new List<PCIDevice>();
            //Console.WriteLine("- created generic");

            EnumerateBus(0, ref devices);
            Devices = devices.ToArray();

            haveEnumerated = true;
        }

        /// <summary>
        /// Enumerates a given bus, adding devices to Devices
        /// </summary>
        /// <param name="Bus">The bus number to enumerate</param>
        /// <param name="Devices">The list of Devices</param>
        private static void EnumerateBus(byte Bus, ref List<PCIDevice> Devices)
        {
            //Console.WriteLine("Enumerate " + Bus ); 
            
            for (byte xSlot = 0; xSlot < 32; xSlot++)
            {                
                byte xMaxFunctions = 1;

                for (byte xFunction = 0; xFunction < xMaxFunctions; xFunction++)
                {
                    PCIDevice xPCIDevice = new PCIDeviceNormal(Bus, xSlot, xFunction);

                    if (xPCIDevice.DeviceExists)
                    {


                        //if (xPCIDevice.HeaderType == 0 /* PCIHeaderType.Normal */)
                        //  xPCIDevice = xPCIDevice;

                        if (xPCIDevice.HeaderType == 2 /* PCIHeaderType.Cardbus */)
                            xPCIDevice = new PCIDeviceCardBus(Bus, xSlot, xFunction);

                        if (xPCIDevice.HeaderType == 1 /* PCIHeaderType.Bridge */)
                            xPCIDevice = new PCIDeviceBridge(Bus, xSlot, xFunction);


                        Devices.Add(xPCIDevice);

                        if (xPCIDevice is PCIDeviceBridge)
                            EnumerateBus(((PCIDeviceBridge)xPCIDevice).SecondaryBus, ref Devices);

                        if (xPCIDevice.IsMultiFunction)
                            xMaxFunctions = 8;
                    }
                }
            }
        }

        public static void DebugLSPCI()
        {
            PCIBus.DeviceIDs xDeviceIDs = new PCIBus.DeviceIDs();

            for(int d =0;d<Cosmos.Hardware.PC.Bus.PCIBus.Devices.Length;d++){
				var xPCIDevice = Cosmos.Hardware.PC.Bus.PCIBus.Devices[d];
                string xVendor = xDeviceIDs.FindVendor(xPCIDevice.VendorID);

                if (xVendor == default(string))
                    xVendor = ToHex(xPCIDevice.VendorID, 4);

                System.Console.Write(xPCIDevice.Bus + "-" + xPCIDevice.Slot + "-" + xPCIDevice.Function);
                System.Console.Write(" " + xVendor + ":" + ToHex(xPCIDevice.DeviceID, 4));
                System.Console.WriteLine(" Type: " + xPCIDevice.HeaderType + " IRQ: " + xPCIDevice.InterruptLine);
                //                                /*Enum.GetName(typeof(PCIHeaderType), */ xPCIDevice.HeaderType/*) */);
                //                           Console.WriteLine(" Status: " + xPCIDevice.Status + " " +
                //                               /*Enum.GetName(typeof(PCIStatus),  */xPCIDevice.Status/*) */);
                //                            Console.WriteLine(" Command: " + xPCIDevice.Command + " " +
                //                                /*Enum.GetName(typeof(PCICommand), */xPCIDevice.Command /* ) */);
                System.Console.Write(" Class [" + ToHex((UInt32)((xPCIDevice.ClassCode << 8) | xPCIDevice.SubClass), 4) + "] " + xPCIDevice.GetClassInfo());
                System.Console.WriteLine();
                System.Console.Write(" Memory: ");

                for (byte i = 0; i < xPCIDevice.NumberOfBaseAddresses(); i++)
                {
                    AddressSpace a = xPCIDevice.GetAddressSpace(i);

                    if (a != null)
                    {
                        System.Console.Write("register " + i + " @ " + ToHex(a.Offset, 8) + " (" + a.Size + "b) ");
                        if (a is MemoryAddressSpace)
                            Console.WriteLine("mem");
                        else
                            Console.WriteLine("io");
                    }
                }

                System.Console.WriteLine();


                System.Console.WriteLine();


            }
        }

        public static string ToHex(UInt32 num, int length)
        {
            char[] ret = new char[length];
            UInt32 cpy = num;

            for (int index = length - 1; index >= 0; index--)
            {
                ret[index] = hex(cpy & 0xf);
                cpy = cpy / 16;
            }

            return "0x" + new string(ret);
        }

        private static char hex(uint p)
        {
            switch (p)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
            }
            return ' ';
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


    /// <summary>
    /// Class representing CardBus PCI Device device
    /// </summary>
    public class PCIDeviceCardBus : PCIDevice
    {
        public PCIDeviceCardBus(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
            //get
            //{
                return 6;
            //}
        }
    }

    /// <summary>
    /// Class representing a PCI to PCI Bridge PCI Device
    /// </summary>
    public class PCIDeviceBridge : PCIDevice
    {
        public PCIDeviceBridge(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
           //  get
           // {
                return 2;
            ////}
        }

        
        public byte PrimaryBus { get { return Read8(0x18); } set { Write8(0x18, value); } }
        public byte SecondaryBus { get { return Read8(0x19); } set { Write8(0x19, value); } }
        public byte SubordinateBus { get { return Read8(0x1a); } set { Write8(0x1a, value); } }
        public byte SecondaryLatencyTime { get { return Read8(0x1b); } set { Write8(0x1b, value); } }
    }


    /// <summary>
    /// Class representing a Normal PCI Device
    /// </summary>
    public class PCIDeviceNormal : PCIDevice
    {
        public PCIDeviceNormal(byte bus, byte slot, byte function)
            : base (bus,slot,function)
        {
        }

        public override int NumberOfBaseAddresses()
        {
            //get
            //{
                return 6;
            //}
        }

        [Obsolete("Use PciDevice.GetAddressSpace(2)")]
        public UInt32 BaseAddress2 { get { return Read32(0x18); } set { Write32(0x18, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(3)")]
        public UInt32 BaseAddress3 { get { return Read32(0x1a); } set { Write32(0x1a, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(4)")]
        public UInt32 BaseAddress4 { get { return Read32(0x20); } set { Write32(0x20, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(5)")]
        public UInt32 BaseAddress5 { get { return Read32(0x24); } set { Write32(0x24, value); } }
       
    }

    public abstract class PCIDevice
    {
        /// <summary>
        /// Gets the number of address spaces this device type has
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBaseAddresses();
       //{
        //   get;
       //}
           

       private static string[] classtext = new string[]          
       {
        "pre pci 2.0",		// 00
        "disk",		// 01
        "network",		// 02
        "display",		// 03
        "multimedia",	// 04
        "memory",		// 05
        "bridge",		// 06
        "communication",	// 07
        "system peripheral",// 08
        "input",		// 09
        "docking station",	// 0A
        "CPU",		// 0B
        "serial bus",	// 0C
       };

        private static string[][] subclasstext = new string[][]
        { 
            new string[] { "VGA Device", "non VGA device"},
            new string[] { "SCSI" ,"IDE" , "floppy","IPI","RAID", "other" },
            new string[] { "Ethernet", "TokenRing", "FDDI" , "ATM" , "other" },
            new string[] { "VGA", "SuperVGA","XGA", "other"},
            new string[] { "video" ,"audio", "other"},
            new string[] { "RAM", "Flash memory" , "other"},
            new string[] { "CPU/PCI" ,"PCI/ISA" , "PCI/EISA" , "PCI/MCA","PCI/PCI" , "PCI/PCMCIA", "PCI/NuBus", "PCI/CardBus", "other"},
            new string[] { "serial", "parallel", "other"},
            new string[] { "PIC", "DMAC" , "timer" ,"RTC", "other"},
            new string[] { "keyboard","digitizer","mouse", "other" },
            new string[] { "generic" , "other" },
            new string[] { "386", "486","Pentium" , "P6" ,"Alpha","coproc","other" },
            new string[] { "Firewire", "ACCESS.bus" , "SSA", "USB" ,"Fiber Channel" , "other"},
        };


        public string GetClassInfo()
        {
            int cc = ClassCode;
            int sc = SubClass; 
            
            if (cc >= classtext.Length)            
                return "unknown class (" + cc.ToString() + ") / subclass (" + sc.ToString() + ")";
            
            
            if (sc >= subclasstext[cc].Length)            
                return String.Concat(classtext[cc], " / unknown subclass (", sc.ToString(), ")");
            
            return String.Concat( classtext[cc] , " / " , subclasstext[cc][sc]);
        }

        private const UInt32 PCI_BASE_ADDRESS_SPACE = 0x01;    /* 0 = memory, 1 = I/O */
        private const UInt32 PCI_BASE_ADDRESS_SPACE_IO = 0x01;
        private const UInt32 PCI_BASE_ADDRESS_SPACE_MEMORY = 0x00;
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_MASK = 0x06;
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_32 = 0x00;   /* 32 bit address */
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_1M = 0x02;   /* Below 1M [obsolete] */
        private const UInt32 PCI_BASE_ADDRESS_MEM_TYPE_64 = 0x04;   /* 64 bit address */
        private const UInt32 PCI_BASE_ADDRESS_MEM_PREFETCH = 0x08;  /* prefetchable? */
        private const UInt32 PCI_BASE_ADDRESS_MEM_MASK = 0xfffffff0;
        private const UInt32 PCI_BASE_ADDRESS_IO_MASK = 0xfffffffc;


        protected PCIDevice(byte bus, byte slot, byte function)
        {
            this.Bus = bus;
            this.Slot = slot;
            this.Function = function;
        }

        private bool NeedsIO = false;
        private bool NeedsMemory = false;

        private bool _NeedsLayingout = true;

        /// <summary>
        /// Creates the AddressSpaces for this device.
        /// </summary>
        private void LayoutIO()
        {
            //Console.WriteLine("Checking AdressSpaces of PCI(" + Bus + ", " + Slot + ", " + Function + ")");

            IOMaps = new AddressSpace[NumberOfBaseAddresses()];

            for (byte i = 0; i < NumberOfBaseAddresses(); i++)
            {
                UInt32 address = GetBaseAddress(i);
                SetBaseAddress(i, 0xffffffff);
                UInt32 flags = GetBaseAddress(i);
                SetBaseAddress(i, address);

                if (address == 0)
                {
                    //Console.WriteLine("register " + i + " - none " + PCIBus.ToHex(flags,8));

                    IOMaps[i] = null;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_MEMORY)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_MEM_MASK & flags)+1;                   

                    IOMaps[i] = new MemoryAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b mem");

                    NeedsIO = true;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_IO)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_IO_MASK & flags) +1;                    

                    IOMaps[i] = new IOAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b io");

                    NeedsMemory = true;
                }
            }

            _NeedsLayingout = false;
        }

        private AddressSpace[] IOMaps;

        /// <summary>
        /// Gets the AddressSpace object assosiated with the device
        /// </summary>
        /// <param name="index">Address Space to return</param>
        /// <returns></returns>
        public AddressSpace GetAddressSpace(byte index)
        {
            if (index < 0 || index >= NumberOfBaseAddresses())
                throw new ArgumentOutOfRangeException("index");
            
            if (_NeedsLayingout)
                LayoutIO();

            return IOMaps[index];
        }

        /// <summary>
        /// The bus the device is on
        /// </summary>
        public byte Bus { get; private set; }
        /// <summary>
        /// The slot the device is using
        /// </summary>
        public byte Slot { get; private set; }
        /// <summary>
        /// The function number of the device
        /// </summary>
        public byte Function { get; private set; }

        /// <summary>
        /// Is this an actual device?
        /// </summary>
        public bool DeviceExists { get { return VendorID != 0xFFFF && VendorID != 0x0; } }
        /// <summary>
        /// Is this a multifunction devie?
        /// </summary>
        public bool IsMultiFunction { get { return (Read8(0x0e) & 0xf0) != 0; } }

        /// <summary>
        /// The Vendor ID
        /// </summary>
        public UInt32 VendorID { get { return Read16(0x0); } }
        /// <summary>
        /// The Device ID
        /// </summary>
        public UInt16 DeviceID { get { return Read16(0x2); } }

        /// <summary>
        /// the command register of this PCI Device
        /// </summary>
        public PCICommand Command { get { return (PCICommand)Read16(0x4); } set { Write16(0x4, (ushort)value); } }
        
        /// <summary>
        /// The Status Register of this PCI Device
        /// </summary>
        public PCIStatus Status { get { return (PCIStatus)Read16(0x6); } set { Write16(0x6, (ushort)value); } }

        /// <summary>
        /// The Revision ID of this PCI Device
        /// </summary>
        public byte RevisionID { get { return Read8(0x8); } }
        /// <summary>
        /// The Programming Interface Number of this PCI Device
        /// </summary>
        public byte ProgIF { get { return Read8(0x9); } }
        /// <summary>
        /// The Sub class of this PCI Device
        /// </summary>
        public byte SubClass { get { return Read8(0xa); } }
        /// <summary>
        /// The Class of this PCI Device
        /// </summary>
        public byte ClassCode { get { return Read8(0xb); } }

        /// <summary>
        /// The Cache Line Size of this PCI Device
        /// </summary>
        public byte CacheLineSize { get { return Read8(0x0c); } set { Write8(0x0c, value); } }
        /// <summary>
        /// The Latency Timer of this PCI Device
        /// </summary>
        public byte LatencyTimer { get { return Read8(0x0d); } set { Write8(0x0d, value); } }
        /// <summary>
        /// The header type of this PCI Device
        /// </summary>
        public byte HeaderType { get { return (byte)(Read8(0x0e) & 0xf); } }
        /// <summary>
        /// The BIST Register of this PCI Device
        /// </summary>
        public PCIBist Bist { get { return (PCIBist)Read8(0x0f); } set { Write8(0x0f, (byte)value); } }

        [Obsolete("This function should be private. Use PciDevice.GetAddressSpace(index)")]
        public UInt32 GetBaseAddress(byte index)
        {
            return Read32((byte)(0x10 + index * 4));
        }
        [Obsolete("This function should be private")]
        public void SetBaseAddress(byte index, UInt32 value)
        {
            Write32((byte)(0x10 + index *4), value);
        }

        [Obsolete("Use PciDevice.GetAddressSpace(0)")]
        public UInt32 BaseAddress0 { get { return Read32(0x10); } set { Write32(0x10, value); } }
        [Obsolete("Use PciDevice.GetAddressSpace(1)")]
        public UInt32 BaseAddress1 { get { return Read32(0x14); } set { Write32(0x14, value); } }
         
        public byte InterruptLine { get { return Read8(0x3c); } set { Write8(0x3c, value); } }
        public byte InterruptPin { get { return Read8(0x3d); } set { Write8(0x3d, value); } }
        public byte MinGNT { get { return Read8(0x3e); } set { Write8(0x3e, value); } }
        public byte MaxLAT { get { return Read8(0x3f); } set { Write8(0x3f, value); } }

        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        /// <summary>
        /// Disables the device
        /// </summary>
        public void DisableDevice()
        {
            Command = Command & (~PCICommand.IO & ~PCICommand.Master & PCICommand.Memort);
        }

        /// <summary>
        /// enables the device
        /// </summary>
        public void EnableDevice()
        {
            Command = Command & ((NeedsIO ? PCICommand.IO : 0) & PCICommand.Master & (NeedsMemory ? PCICommand.Memort : 0));
        }

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

        protected UInt32 Read32(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read32(ConfigData);
        }

        protected UInt16 Read16(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read16(ConfigData);
        }

        protected byte Read8(byte aRegister)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return CPUBus.Read8(ConfigData);
        }

        protected void Write32(byte aRegister, UInt32 value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write32(ConfigData, value);
        }

        protected void Write16(byte aRegister, UInt16 value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write16(ConfigData, value);
        }

        protected void Write8(byte aRegister, byte value)
        {
            CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            CPUBus.Write8(ConfigData, value);
        } 
    }
}