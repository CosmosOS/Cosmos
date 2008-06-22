using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {

    public class PCIBus {
        //TODO: Change to Dictionary<UInt32, string> when the IL2CPU bug is fixed
        public class DeviceID {
            UInt32 key;

            public UInt32 Key {
                get { return key; }
                set { key = value; }
            }

            String value;

            public String Value {
                get { return value; }
                set { this.value = value; }
            }

            public DeviceID(UInt32 pkey, String pvalue) {
                key = pkey;
                value = pvalue;
            }
        }

        //Dont make static. We dont want all the strings loaded in RAM
        // all the time.
        public class DeviceIDs {
            protected TempDictionary<String> mVendors = new TempDictionary<String>();

            public DeviceIDs() {
                // Current QEMU hardware
                mVendors.Add(0x8086, "Intel");
                // 1237  440FX - 82441FX PMC [Natoma]
	            // 7000  82371SB PIIX3 ISA [Natoma/Triton II]
    	        // 7010  82371SB PIIX3 IDE [Natoma/Triton II]
	            // 7113  82371AB/EB/MB PIIX4 ACPI
		        //    Need to check sub attr - maybe we have this:
                //    15ad 1976  Virtual Machine Chipset
                mVendors.Add(0x1013, "Cirrus Logic");
	            // 00b8  GD 5446
                mVendors.Add(0x10EC, "Realtek Semiconductor Co., Ltd.");
                //8139  RTL-8029(AS)
                    //0357 000a  TTP-Monitoring Card V2.0
                    //1025 005a  TravelMate 290
                    //1025 8920  ALN-325
                    //1025 8921  ALN-325
                    //103c 006a  NX9500
                    //1043 1045  L8400B or L3C/S notebook
                    //1043 8109  P5P800-MX Mainboard
                    //1071 8160  MIM2000
                    //10bd 0320  EP-320X-R
                    //10ec 8139  RT8139
                    //10f7 8338  Panasonic CF-Y5 laptop
                    //1113 ec01  FNC-0107TX
                    //1186 1300  DFE-538TX
                    //1186 1320  SN5200
                    //1186 8139  DRN-32TX
                    //11f6 8139  FN22-3(A) LinxPRO Ethernet Adapter
                    //1259 2500  AT-2500TX
                    //1259 2503  AT-2500TX/ACPI
                    //1429 d010  ND010
                    //1432 9130  EN-9130TX
                    //1436 8139  RT8139
                    //144d c00c  P30/P35 notebook
                    //1458 e000  GA-7VM400M/7VT600 Motherboard
                    //1462 788c  865PE Neo2-V Mainboard
                    //146c 1439  FE-1439TX
                    //1489 6001  GF100TXRII
                    //1489 6002  GF100TXRA
                    //149c 139a  LFE-8139ATX
                    //149c 8139  LFE-8139TX
                    //14cb 0200  LNR-100 Family 10/100 Base-TX Ethernet
                    //1565 2300  P4TSV Onboard LAN (RTL8100B)
                    //1695 9001  Onboard RTL8101L 10/100 MBit
                    //1799 5000  F5D5000 PCI Card/Desktop Network PCI Card
                    //1904 8139  RTL8139D Fast Ethernet Adapter
                    //2646 0001  EtheRx
                    //8e2e 7000  KF-230TX
                    //8e2e 7100  KF-230TX/2
                    //a0a0 0007  ALN-325C
            }

            public string FindVendor(UInt32 aVendorID) {
                return mVendors[aVendorID];
            }

            /*protected List<DeviceID> mVendors = new List<DeviceID>();

            public DeviceIDs()
            {
                mVendors.Add(new DeviceID(0x8086, "Intel"));
            }

            public string FindVendor(UInt32 aVendorID)
            {
                for (int i = 0; i < mVendors.Count; i++)
                {

                    if (mVendors[i].Key == aVendorID)
                        return mVendors[i].Value;
                }
                return null;
            }*/
        }

        protected static PCIDevice[] mDevices;
        public static PCIDevice[] Devices {
            get { return mDevices; }
        }

        protected static bool mEnumerationCompleted = false;

        public static PCIDevice GetPCIDevice(byte bus, byte slot, byte function) {
            if (!mEnumerationCompleted) {
                Init();
            }

            foreach (PCIDevice dev in PCIBus.Devices) {
                if (dev.Bus == bus &&
                    dev.Slot == slot &&
                    dev.Function == function)
                    return dev;
            }

            return null;
        }

        public static void Init() {
            var xDevices = new List<PCIDevice>();
            EnumerateBus(0, ref xDevices);
            mDevices = xDevices.ToArray();
            mEnumerationCompleted = true;
        }

        /// <summary>
        /// Enumerates a given bus, adding devices to Devices
        /// </summary>
        /// <param name="Bus">The bus number to enumerate</param>
        /// <param name="Devices">The list of Devices</param>
        private static void EnumerateBus(byte aBus, ref List<PCIDevice> rDevices) {
            //Console.WriteLine("Enumerate " + Bus ); 
            
            for (byte xSlot = 0; xSlot < 32; xSlot++) {                
                byte xMaxFunctions = 1;
                for (byte xFunction = 0; xFunction < xMaxFunctions; xFunction++) {
                    PCIDevice xPCIDevice = new PCIDeviceNormal(aBus, xSlot, xFunction);

                    if (xPCIDevice.DeviceExists) {
                        //if (xPCIDevice.HeaderType == 0 /* PCIHeaderType.Normal */)
                        //  xPCIDevice = xPCIDevice;

                        if (xPCIDevice.HeaderType == 2) { /* PCIHeaderType.Cardbus */
                            xPCIDevice = new PCIDeviceCardBus(aBus, xSlot, xFunction);
                        }

                        if (xPCIDevice.HeaderType == 1) { /* PCIHeaderType.Bridge */
                            xPCIDevice = new PCIDeviceBridge(aBus, xSlot, xFunction);
                        }

                        rDevices.Add(xPCIDevice);

                        if (xPCIDevice is PCIDeviceBridge) {
                            EnumerateBus(((PCIDeviceBridge)xPCIDevice).SecondaryBus, ref rDevices);
                        }

                        if (xPCIDevice.IsMultiFunction) {
                            xMaxFunctions = 8;
                        }
                    }
                }
            }
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

            IOMaps = new Kernel.AddressSpace[NumberOfBaseAddresses()];

            for (byte i = 0; i < NumberOfBaseAddresses(); i++)
            {
                UInt32 address = GetBaseAddressInternal(i);
                SetBaseAddressInternal(i, 0xffffffff);
                UInt32 flags = GetBaseAddressInternal(i);
                SetBaseAddressInternal(i, address);

                if (address == 0)
                {
                    //Console.WriteLine("register " + i + " - none " + PCIBus.ToHex(flags,8));

                    IOMaps[i] = null;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_MEMORY)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_MEM_MASK & flags)+1;

                    IOMaps[i] = new Kernel.MemoryAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b mem");

                    NeedsIO = true;
                }
                else if ((address & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_IO)
                {
                    UInt32 size = ~(PCI_BASE_ADDRESS_IO_MASK & flags) +1;

                    IOMaps[i] = new Kernel.IOAddressSpace(address, size);
                    //Console.WriteLine("register " + i + " - " + size + "b io");

                    NeedsMemory = true;
                }
            }

            _NeedsLayingout = false;
        }

        private Kernel.AddressSpace[] IOMaps;

        /// <summary>
        /// Gets the AddressSpace object assosiated with the device
        /// </summary>
        /// <param name="index">Address Space to return</param>
        /// <returns></returns>
        public Kernel.AddressSpace GetAddressSpace(byte index)
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

        public UInt32 GetBaseAddressInternal(byte index)
        {
            return Read32((byte)(0x10 + index * 4));
        }
        
        private void SetBaseAddressInternal(byte index, UInt32 value)
        {
            Write32((byte)(0x10 + index * 4), value);
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
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read32(ConfigData);
        }

        protected UInt16 Read16(byte aRegister)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read16(ConfigData);
        }

        protected byte Read8(byte aRegister)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            return Kernel.CPUBus.Read8(ConfigData);
        }

        protected void Write32(byte aRegister, UInt32 value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write32(ConfigData, value);
        }

        protected void Write16(byte aRegister, UInt16 value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write16(ConfigData, value);
        }

        protected void Write8(byte aRegister, byte value)
        {
            Kernel.CPUBus.Write32(ConfigAddr, GetAddress(aRegister));
            Kernel.CPUBus.Write8(ConfigData, value);
        } 
    }
}