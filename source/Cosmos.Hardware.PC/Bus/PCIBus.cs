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
                for (byte xSlot = 0; xSlot < 32; xSlot++)
                {
                    byte xMaxFunctions = 1;

                    for (byte xFunction = 0; xFunction < xMaxFunctions; xFunction++)
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
                            Console.Write(ToHex(xPCIDevice.DeviceID, 4));
                            Console.Write(" Type: ");
                            Console.Write(xPCIDevice.HeaderType.ToString());
                            Console.Write(" Class: ");
                            Console.Write(xPCIDevice.GetClassInfo());
                            Console.WriteLine();
                            Console.Write("Memory requirements: ");
                            xPCIDevice.BaseAddress0 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress0, 8));
                            Console.Write(",");
                            xPCIDevice.BaseAddress1 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress1, 8));
                            Console.Write(",");
                            xPCIDevice.BaseAddress2 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress2, 8));
                            Console.Write(",");
                            xPCIDevice.BaseAddress3 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress3, 8));
                            Console.Write(",");
                            xPCIDevice.BaseAddress4 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress4, 8));
                            Console.Write(",");
                            xPCIDevice.BaseAddress5 = 0xffffffff;
                            Console.Write(ToHex(xPCIDevice.BaseAddress5, 8));

                            Console.WriteLine();

                            if (xPCIDevice.IsMultiFunction)
                                xMaxFunctions = 8;

                        }
                    }
                }
            }
            Console.WriteLine("Done");
        }

        //private static char[] hex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'e', 'd', 'f' };

        private static string ToHex(UInt32 num, int length)
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
        Cardbus = 2,
        Multifunction = 0x10
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
            new string[] {},
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
            if (cc >= classtext.Length)            
                return "unknown class / subclass";
            
            int sc = SubClass;
            if (sc >= subclasstext[cc].Length)            
                return String.Concat(classtext[cc], " / unknown subclass");
            
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
        private const UInt32 PCI_BASE_ADDRESS_MEM_MASK = ~(UInt32)0x0f;
        private const UInt32 PCI_BASE_ADDRESS_IO_MASK = ~(UInt32)0x03;


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
        public bool IsMultiFunction { get { return (HeaderType & PCIHeaderType.Multifunction) != 0; } }

        public UInt32 VendorID { get { return Read16(0x0); } }
        public UInt16 DeviceID { get { return Read16(0x2); } }

        public PCICommand Command { get { return (PCICommand)Read16(0x4); } set { Write16(0x4, (ushort)value); } }
        public PCIStatus Status { get { return (PCIStatus)Read16(0x6); } set { Write16(0x6, (ushort)value); } }

        public byte RevisionID { get { return Read8(0x8); } }
        public byte ProgIF { get { return Read8(0x9); } }
        public byte SubClass { get { return Read8(0xa); } }
        public byte ClassCode { get { return Read8(0xb); } }

        public byte CacheLineSize { get { return Read8(0x0c); } set { Write8(0x0c, value); } }
        public byte LatencyTimer { get { return Read8(0x0d); } set { Write8(0x0d, value); } }
        public PCIHeaderType HeaderType { get { return (PCIHeaderType)Read8(0x0e); } set { Write8(0x0e, (byte)value); } }
        public PCIBist Bist { get { return (PCIBist)Read8(0x0f); } set { Write8(0x0f, (byte)value); } }

        public UInt32 GetBaseAddress(byte index)
        {
            return Read32((byte)(0x10b + index << 2));
        }
        public void SetBaseAddress(byte index, UInt32 value)
        {
            Write32((byte)(0x10b + index << 2), value);
        }

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

#if false
        
        public void DisableDevice()
        {
            Command = Command & (!PCICommand.IO & !PCICommand.Master & PCICommand.Memort)
        }
        public bool LayoutDevice()
        {
            
	
            PCICommand cmd = Command;
	

	for (int reg = 0; reg < 6; reg++)
	{
		/*
		 * Figure out how much space and of what type this
		 * device wants.
		 */

        SetBaseAddress(reg, 0xffffffff);

        UInt32 bas = GetBaseAddress(reg);

		
		if (bas == 0)
		{
			/* this base-address register is unused */
			continue;
		}

		/*
		 * We've read the base address register back after
		 * writing all ones and so now we must decode it.
		 */

		if ((base & PCI_BASE_ADDRESS_SPACE) == PCI_BASE_ADDRESS_SPACE_IO)
		{
			/*
			 * I/O space base address register.
			 */

			cmd |= PCI_COMMAND_IO;

			base &= PCI_BASE_ADDRESS_IO_MASK;
			mask = (~base << 1) | 0x1;
			size = (mask & base) & 0xffffffff;

			/*
			 * Align to multiple of size of minimum base.
			 */

			alignto = max_t(unsigned int, 0x040, size);
			base = ALIGN(io_base, alignto);
			io_base = base + size;
			pci_write_config_dword(dev, reg, base | PCI_BASE_ADDRESS_SPACE_IO);

			dev->resource[i].start = base;
			dev->resource[i].end = dev->resource[i].start + size - 1;
			dev->resource[i].flags = IORESOURCE_IO | PCI_BASE_ADDRESS_SPACE_IO;

			DBG_DEVS(("layout_dev: IO address: %lX\n", base));
		}
		else
		{
			unsigned int type;

			/*
			 * Memory space base address register.
			 */

			cmd |= PCI_COMMAND_MEMORY;
			type = base & PCI_BASE_ADDRESS_MEM_TYPE_MASK;
			base &= PCI_BASE_ADDRESS_MEM_MASK;
			mask = (~base << 1) | 0x1;
			size = (mask & base) & 0xffffffff;
			switch (type)
			{
			case PCI_BASE_ADDRESS_MEM_TYPE_32:
			case PCI_BASE_ADDRESS_MEM_TYPE_64:
				break;

			case PCI_BASE_ADDRESS_MEM_TYPE_1M:
				printk("bios32 WARNING: slot %d, function %d "
				       "requests memory below 1MB---don't "
				       "know how to do that.\n",
				       PCI_SLOT(dev->devfn),
				       PCI_FUNC(dev->devfn));
				continue;
			}

			/*
			 * Align to multiple of size of minimum base.
			 */

			alignto = max_t(unsigned int, 0x1000, size);
			base = ALIGN(mem_base, alignto);
			mem_base = base + size;
			pci_write_config_dword(dev, reg, base);

			dev->resource[i].start = base;
			dev->resource[i].end = dev->resource[i].start + size - 1;
			dev->resource[i].flags = IORESOURCE_MEM;

			if (type == PCI_BASE_ADDRESS_MEM_TYPE_64)
			{
				/*
				 * 64-bit address, set the highest 32 bits
				 * to zero.
				 */

				reg += 4;
				pci_write_config_dword(dev, reg, 0);

				i++;
				dev->resource[i].start = 0;
				dev->resource[i].end = 0;
				dev->resource[i].flags = 0;
			}
		}
	}

	/*
	 * Enable device:
	 */

	if (dev->class >> 8 == PCI_CLASS_NOT_DEFINED ||
	    dev->class >> 8 == PCI_CLASS_NOT_DEFINED_VGA ||
	    dev->class >> 8 == PCI_CLASS_DISPLAY_VGA ||
	    dev->class >> 8 == PCI_CLASS_DISPLAY_XGA)
	{
		/*
		 * All of these (may) have I/O scattered all around
		 * and may not use i/o-base address registers at all.
		 * So we just have to always enable I/O to these
		 * devices.
		 */
		cmd |= PCI_COMMAND_IO;
	}

	pci_write_config_word(dev, PCI_COMMAND, cmd | PCI_COMMAND_MASTER);

	pci_write_config_byte(dev, PCI_LATENCY_TIMER, (disable_pci_burst) ? 0 : 32);

	if (bus_info != NULL)
		bus_info->conf_device(dev);	/* Machine dependent configuration. */

	DBG_DEVS(("layout_dev: bus %d  slot 0x%x  VID 0x%x  DID 0x%x  class 0x%x\n",
		  dev->bus->number, PCI_SLOT(dev->devfn), dev->vendor, dev->device, dev->class));
}

        }
#endif
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