using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core.IOGroup;
using Cosmos.Common.Extensions;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public class PCIDevice
    {

        #region Enums
        public enum PCIHeaderType : byte
        {
            Normal = 0x00,
            Bridge = 0x01,
            Cardbus = 0x02
        };

        public enum PCIBist : byte
        {
            CocdMask = 0x0f,   /* Return result */
            Start = 0x40,   /* 1 to start BIST, 2 secs or less */
            Capable = 0x80    /* 1 if BIST capable */
        }

        public enum PCIInterruptPIN : byte
        {
            None = 0x00,
            INTA = 0x01,
            INTB = 0x02,
            INTC = 0x03,
            INTD = 0x04
        };

        public enum PCICommand : short
        {
            IO = 0x1,     /* Enable response in I/O space */
            Memory = 0x2,     /* Enable response in Memory space */
            Master = 0x4,     /* Enable bus mastering */
            Special = 0x8,     /* Enable response to special cycles */
            Invalidate = 0x10,    /* Use memory write and invalidate */
            VGA_Pallete = 0x20,   /* Enable palette snooping */
            Parity = 0x40,    /* Enable parity checking */
            Wait = 0x80,    /* Enable address/data stepping */
            SERR = 0x100,   /* Enable SERR */
            Fast_Back = 0x200,   /* Enable back-to-back writes */
        }

        public enum Config : byte
        {
            VendorID = 0, DeviceID = 2,
            Command = 4, Status = 6,
            RevisionID = 8, ProgIF = 9, SubClass = 10, Class = 11,
            CacheLineSize = 12, LatencyTimer = 13, HeaderType = 14, BIST = 15,
            BAR0 = 16,
            BAR1 = 20,
            PrimaryBusNo = 24, SecondaryBusNo = 25, SubBusNo = 26, SecondarLT = 27,
            IOBase = 28, IOLimit = 29, SecondaryStatus = 30,
            MemoryBase = 32, MemoryLimit = 34,
            PrefMemoryBase = 36, PrefMemoryLimit = 38,
            PrefBase32Upper = 40,
            PrefLimit32upper = 44,
            PrefBase16Upper = 48, PrefLimit16upper = 50,
            CapabilityPointer = 52, Reserved = 53,
            ExpROMBaseAddress = 56,
            InterruptLine = 60, InterruptPIN = 61, BridgeControl = 62
        };
        #endregion

        public readonly uint bus;
        public readonly uint slot;
        public readonly uint function;

        public readonly uint BAR0;

        public readonly ushort VendorID;
        public readonly ushort DeviceID;

        public readonly ushort Status;

        public readonly byte RevisionID;
        public readonly byte ProgIF;
        public readonly byte Subclass;
        public readonly byte ClassCode;
        public readonly byte SecondaryBusNumber;

        public readonly bool DeviceExists;

        public readonly PCIHeaderType HeaderType;
        public readonly PCIBist BIST;
        public readonly PCIInterruptPIN InterruptPIN;

        public const ushort ConfigAddressPort = 0xCF8;
        public const ushort ConfigDataPort = 0xCFC;

        public readonly PCIBaseAddressBar[] BaseAddressBar;

        public byte InterruptLine { get; private set; }
        public PCICommand Command { get { return (PCICommand)ReadRegister16(0x04); } set { WriteRegister16(0x04, (ushort)value); } }

        /// <summary>
        /// Has this device been claimed by a driver
        /// </summary>
        public bool Claimed { get; set; }

        public PCIDevice(uint bus, uint slot, uint function)
        {
            this.bus = bus;
            this.slot = slot;
            this.function = function;

            VendorID = ReadRegister16((byte)Config.VendorID);
            DeviceID = ReadRegister16((byte)Config.DeviceID);

            BAR0 = ReadRegister32((byte)Config.BAR0);

            //Command = ReadRegister16((byte)Config.Command);
            //Status = ReadRegister16((byte)Config.Status);

            RevisionID = ReadRegister8((byte)Config.RevisionID);
            ProgIF = ReadRegister8((byte)Config.ProgIF);
            Subclass = ReadRegister8((byte)Config.SubClass);
            ClassCode = ReadRegister8((byte)Config.Class);
            SecondaryBusNumber = ReadRegister8((byte)Config.SecondaryBusNo);

            HeaderType = (PCIHeaderType)ReadRegister8((byte)Config.HeaderType);
            BIST = (PCIBist)ReadRegister8((byte)Config.BIST);
            InterruptPIN = (PCIInterruptPIN)ReadRegister8((byte)Config.InterruptPIN);
            InterruptLine = ReadRegister8((byte)Config.InterruptLine);

            if ((uint)VendorID == 0xFF && (uint)DeviceID == 0xFFFF)
            {
                DeviceExists = false;
            }
            else
            {
                DeviceExists = true;
            }
            if (HeaderType == PCIHeaderType.Normal)
            {
                BaseAddressBar = new PCIBaseAddressBar[6];
                BaseAddressBar[0] = new PCIBaseAddressBar(ReadRegister32(0x10));
                BaseAddressBar[1] = new PCIBaseAddressBar(ReadRegister32(0x14));
                BaseAddressBar[2] = new PCIBaseAddressBar(ReadRegister32(0x18));
                BaseAddressBar[3] = new PCIBaseAddressBar(ReadRegister32(0x1C));
                BaseAddressBar[4] = new PCIBaseAddressBar(ReadRegister32(0x20));
                BaseAddressBar[5] = new PCIBaseAddressBar(ReadRegister32(0x24));
            }
        }

        public void EnableDevice()
        {
            Command |= PCICommand.Master | PCICommand.IO | PCICommand.Memory;
        }

        /// <summary>
        /// Get header type.
        /// </summary>
        /// <param name="Bus">A bus.</param>
        /// <param name="Slot">A slot.</param>
        /// <param name="Function">A function.</param>
        /// <returns>ushort value.</returns>
        public static ushort GetHeaderType(ushort Bus, ushort Slot, ushort Function)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | 0xE & 0xFC;
            IOPort.Write32(ConfigAddressPort, xAddr);
            return (byte)(IOPort.Read32(ConfigDataPort) >> (0xE % 4 * 8) & 0xFF);
        }

        /// <summary>
        /// Get vendor ID.
        /// </summary>
        /// <param name="Bus">A bus.</param>
        /// <param name="Slot">A slot.</param>
        /// <param name="Function">A function.</param>
        /// <returns>UInt16 value.</returns>
        public static ushort GetVendorID(ushort Bus, ushort Slot, ushort Function)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | 0x0 & 0xFC;
            IOPort.Write32(ConfigAddressPort, xAddr);
            return (ushort)(IOPort.Read32(ConfigDataPort) >> (0x0 % 4 * 8) & 0xFFFF);
        }

        #region IOReadWrite
        /// <summary>
        /// Read register - 8-bit.
        /// </summary>
        /// <param name="aRegister">A register to read.</param>
        /// <returns>byte value.</returns>
        public byte ReadRegister8(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            return (byte)(IOPort.Read32(ConfigDataPort) >> (aRegister % 4 * 8) & 0xFF);
        }

        public void WriteRegister8(byte aRegister, byte value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            IOPort.Write8(ConfigDataPort, value);
        }

        /// <summary>
        /// Read register 16.
        /// </summary>
        /// <param name="aRegister">A register.</param>
        /// <returns>UInt16 value.</returns>
        public ushort ReadRegister16(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            return (ushort)(IOPort.Read32(ConfigDataPort) >> (aRegister % 4 * 8) & 0xFFFF);
        }

        /// <summary>
        /// Write register 16.
        /// </summary>
        /// <param name="aRegister">A register.</param>
        /// <param name="value">A value.</param>
        public void WriteRegister16(byte aRegister, ushort value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            IOPort.Write16(ConfigDataPort, value);
        }

        public uint ReadRegister32(byte aRegister)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            return IOPort.Read32(ConfigDataPort);
        }

        public void WriteRegister32(byte aRegister, uint value)
        {
            uint xAddr = GetAddressBase(bus, slot, function) | (uint)(aRegister & 0xFC);
            IOPort.Write32(ConfigAddressPort, xAddr);
            IOPort.Write32(ConfigDataPort, value);
        }
        #endregion

        /// <summary>
        /// Get address base.
        /// </summary>
        /// <param name="aBus">A bus.</param>
        /// <param name="aSlot">A slot.</param>
        /// <param name="aFunction">A function.</param>
        /// <returns>UInt32 value.</returns>
        protected static uint GetAddressBase(uint aBus, uint aSlot, uint aFunction)
        {
            return 0x80000000 | (aBus << 16) | ((aSlot & 0x1F) << 11) | ((aFunction & 0x07) << 8);
        }

        /// <summary>
        /// Enable memory.
        /// </summary>
        /// <param name="enable">bool value.</param>
        public void EnableMemory(bool enable)
        {
            ushort command = ReadRegister16(0x04);

            ushort flags = 0x0007;

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            WriteRegister16(0x04, command);
        }

        public void EnableBusMaster(bool enable)
        {
            ushort command = ReadRegister16(0x04);

            ushort flags = 1 << 2;

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            WriteRegister16(0x04, command);
        }

        public class DeviceClass
        {
            public static string GetDeviceString(PCIDevice device)
            {
                switch (device.VendorID)
                {
                    case 0x1022: //AMD
                        switch (device.DeviceID)
                        {
                            case 0x2000:
                                return "AMD PCnet LANCE PCI Ethernet Controller";
                            default:
                                return "AMD Unknown device";
                        }
                    case 0x104B: //Sony
                        switch (device.DeviceID)
                        {
                            case 0x1040:
                                return "Mylex BT958 SCSI Host Adaptor";
                            default:
                                return "Mylex Unknown device";
                        }
                    case 0x1234: //Bochs
                        switch (device.DeviceID)
                        {
                            case 0x1111:
                                return "Bochs BGA";
                            default:
                                return "Bochs Unknown device";
                        }
                    case 0x1274: //Ensoniq
                        switch (device.DeviceID)
                        {
                            case 0x1371:
                                return "Ensoniq AudioPCI";
                            default:
                                return "Ensoniq Unknown device";
                        }
                    case 0x15AD: //VMware
                        switch (device.DeviceID)
                        {
                            case 0x0405:
                                return "VMware NVIDIA 9500MGS";
                            case 0x0740:
                                return "Vmware Virtual Machine Communication Interface";
                            case 0x0770:
                                return "VMware Standard Enhanced PCI to USB Host Controller";
                            case 0x0790:
                                return "VMware 6.0 Virtual USB 2.0 Host Controller";
                            case 0x07A0:
                                return "VMware PCI Express Root Port";
                            default:
                                return "VMware Unknown device";
                        }
                    case 0x8086: //Intel
                        switch (device.DeviceID)
                        {
                            case 0x7190:
                                return "Intel 440BX/ZX AGPset Host Bridge";
                            case 0x7191:
                                return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                            case 0x7110:
                                return "Intel PIIX4/4E/4M ISA Bridge";
                            case 0x7111:
                                return "Intel PIIX4/82371AB/EB/MB IDE";
                            case 0x7112:
                                return "Intel PIIX4/4E/4M USB Interface";
                            case 0x7113:
                                return "Intel PIIX4/82371AB/EB/MB ACPI";
                            default:
                                return "Intel Unknown device";
                        }
                    case 0x80EE: //VirtualBox
                        switch (device.DeviceID)
                        {
                            case 0xBEEF:
                                return "VirtualBox Graphics Adapter";
                            case 0xCAFE:
                                return "VirtualBox Guest Service";
                            default:
                                return "VirtualBox Unknown device";
                        }
                    default:
                        return "Unknown device";
                }
            }

            public static string GetTypeString(PCIDevice device)
            {
                switch (device.ClassCode)
                {
                    case 0x00:
                        switch (device.Subclass)
                        {
                            case 0x01:
                                return "VGA-Compatible Device";
                            default:
                                return "0x00 Subclass";
                        }
                    case 0x01:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "SCSI Bus Controller";
                            case 0x01:
                                return "IDE Controller";
                            case 0x02:
                                return "Floppy Disk Controller";
                            case 0x03:
                                return "IPI Bus Controller";
                            case 0x04:
                                return "RAID Controller";
                            case 0x05:
                                return "ATA Controller";
                            case 0x06:
                                return "Serial ATA";
                            case 0x80:
                                return "Unknown Mass Storage Controller";
                            default:
                                return "Mass Storage Controller";
                        }
                    case 0x02:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Ethernet Controller";
                            case 0x01:
                                return "Token Ring Controller";
                            case 0x02:
                                return "FDDI Controller";
                            case 0x03:
                                return "ATM Controller";
                            case 0x04:
                                return "ISDN Controller";
                            case 0x05:
                                return "WorldFip Controller";
                            case 0x06:
                                return "PICMG 2.14 Multi Computing";
                            case 0x80:
                                return "Unknown Network Controller";
                            default:
                                return "Network Controller";
                        }
                    case 0x03:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "VGA-Compatible Controller";
                                    case 0x01:
                                        return "8512-Compatible Controller";
                                    default:
                                        return "Unknown Display Controller";
                                }
                            case 0x01:
                                return "XGA Controller";
                            case 0x02:
                                return "3D Controller (Not VGA-Compatible)";
                            case 0x80:
                                return "Unknown Display Controller";
                            default:
                                return "Display Controller";
                        }
                    case 0x04:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Video Device";
                            case 0x01:
                                return "Audio Device";
                            case 0x02:
                                return "Computer Telephony Device";
                            case 0x80:
                                return "Unknown Multimedia Controller";
                            default:
                                return "Multimedia Controller";
                        }
                    case 0x05:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "RAM Controller";
                            case 0x01:
                                return "Flash Controller";
                            case 0x80:
                                return "Unknown Memory Controller";
                            default:
                                return "Memory Controller";
                        }
                    case 0x06:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Host Bridge";
                            case 0x01:
                                return "ISA Bridge";
                            case 0x02:
                                return "EISA Bridge";
                            case 0x03:
                                return "MCA Bridge";
                            case 0x04:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "PCI-to-PCI Bridge";
                                    case 0x01:
                                        return "PCI-to-PCI Bridge (Subtractive Decode)";
                                    default:
                                        return "Unknown PCI-to-PCI Bridge";
                                }
                            case 0x05:
                                return "PCMCIA Bridge";
                            case 0x06:
                                return "NuBus Bridge";
                            case 0x07:
                                return "CardBus Bridge";
                            case 0x08:
                                return "RACEway Bridge";
                            case 0x09:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "PCI-to-PCI Bridge (Semi-Transparent, Primary)";
                                    case 0x01:
                                        return "PCI-to-PCI Bridge (Semi-Transparent, Secondary)";
                                    default:
                                        return "Unknown PCI-to-PCI Bridge";
                                }
                            case 0x0A:
                                return "InfiniBrand-to-PCI Host Bridge";
                            case 0x80:
                                return "Unknown Bridge Device";
                            default:
                                return "Bridge Device";
                        }
                    case 0x07:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic XT-Compatible Serial Controller";
                                    case 0x01:
                                        return "16450-Compatible Serial Controller";
                                    case 0x02:
                                        return "16550-Compatible Serial Controller";
                                    case 0x03:
                                        return "16650-Compatible Serial Controller";
                                    case 0x04:
                                        return "16750-Compatible Serial Controller";
                                    case 0x05:
                                        return "16850-Compatible Serial Controller";
                                    case 0x06:
                                        return "16950-Compatible Serial Controller";
                                    default:
                                        return "Simple Communication Controller";
                                }
                            case 0x01:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Parallel Port";
                                    case 0x01:
                                        return "Bi-Directional Parallel Port";
                                    case 0x02:
                                        return "ECP 1.X Compliant Parallel Port";
                                    case 0x03:
                                        return "IEEE 1284 Controller";
                                    case 0xFE:
                                        return "IEEE 1284 Target Device";
                                    default:
                                        return "Parallel Port";
                                }
                            case 0x02:
                                return "Multiport Serial Controller";
                            case 0x03:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic Modem";
                                    case 0x01:
                                        return "Hayes Compatible Modem (16450-Compatible Interface)";
                                    case 0x02:
                                        return "Hayes Compatible Modem (16550-Compatible Interface)";
                                    case 0x03:
                                        return "Hayes Compatible Modem (16650-Compatible Interface)";
                                    case 0x04:
                                        return "Hayes Compatible Modem (16750-Compatible Interface)";
                                    default:
                                        return "Modem";
                                }
                            case 0x04:
                                return "IEEE 488.1/2 (GPIB) Controller";
                            case 0x05:
                                return "Smart Card";
                            case 0x80:
                                return "Unknown Communications Device";
                            default:
                                return "Simple Communication Controller";
                        }
                    case 0x08:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic 8259 PIC";
                                    case 0x01:
                                        return "ISA PIC";
                                    case 0x02:
                                        return "EISA PIC";
                                    case 0x10:
                                        return "I/O APIC Interrupt Controller";
                                    case 0x20:
                                        return "I/O(x) APIC Interrupt Controller";
                                    default:
                                        return "PIC";
                                }
                            case 0x01:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic 8237 DMA Controller";
                                    case 0x01:
                                        return "ISA DMA Controller";
                                    case 0x02:
                                        return "EISA DMA Controller";
                                    default:
                                        return "DMA Controller";
                                }
                            case 0x02:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic 8254 System Timer";
                                    case 0x01:
                                        return "ISA System Timer";
                                    case 0x02:
                                        return "EISA System Timer";
                                    default:
                                        return "System Timer";
                                }
                            case 0x03:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Generic RTC Controller";
                                    case 0x01:
                                        return "ISA RTC Controller";
                                    default:
                                        return "RTC Controller";
                                }
                            case 0x04:
                                return "Generic PCI Hot-Plug Controller";
                            case 0x80:
                                return "Unknown System Peripheral";
                            default:
                                return "Base System Peripheral";
                        }
                    case 0x09:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Keyboard Controller";
                            case 0x01:
                                return "Digitizer";
                            case 0x02:
                                return "Mouse Controller";
                            case 0x03:
                                return "Scanner Controller";
                            case 0x04:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "Gameport Controller (Generic)";
                                    case 0x10:
                                        return "Gameport Controller (Legacy)";
                                    default:
                                        return "Gameport Controller";
                                }
                            case 0x80:
                                return "Unknown Input Controller";
                            default:
                                return "Input Device";
                        }
                    case 0x0A:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Generic Docking Station";
                            case 0x80:
                                return "Unknown Docking Station";
                            default:
                                return "Docking Station";
                        }
                    case 0x0B:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "386 Processor";
                            case 0x01:
                                return "486 Processor";
                            case 0x02:
                                return "Pentium Processor";
                            case 0x10:
                                return "Alpha Processor";
                            case 0x20:
                                return "PowerPC Processor";
                            case 0x30:
                                return "MIPS Processor";
                            case 0x40:
                                return "Co-Processor";
                            default:
                                return "Processor";
                        }
                    case 0x0C:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "IEEE 1394 Controller (FireWire)";
                                    case 0x10:
                                        return "IEEE 1394 Controller (1394 OpenHCI Spec)";
                                    default:
                                        return "IEEE Controller";
                                }
                            case 0x01:
                                return "ACCESS.bus";
                            case 0x02:
                                return "SSA";
                            case 0x03:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "USB (Universal Host Controller Spec)";
                                    case 0x10:
                                        return "USB (Open Host Controller Spec)";
                                    case 0x20:
                                        return "USB2 Host Controller (Intel Enhanced Host Controller Interface)";
                                    case 0x30:
                                        return "USB3 XHCI Controller";
                                    case 0x80:
                                        return "Unspecified USB Controller";
                                    case 0xFE:
                                        return "USB (Not Host Controller)";
                                    default:
                                        return "USB";
                                }
                            case 0x04:
                                return "Fibre Channel";
                            case 0x05:
                                return "SMBus";
                            case 0x06:
                                return "InfiniBand";
                            case 0x07:
                                switch (device.ProgIF)
                                {
                                    case 0x00:
                                        return "IPMI SMIC Interface";
                                    case 0x01:
                                        return "IPMI Kybd Controller Style Interface";
                                    case 0x02:
                                        return "IPMI Block Transfer Interface";
                                    default:
                                        return "IPMI SMIC Interface";
                                }
                            case 0x08:
                                return "SERCOS Interface Standard (IEC 61491)";
                            case 0x09:
                                return "CANbus";
                            default:
                                return "Serial Bus Controller";
                        }
                    case 0x0D:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "iRDA Compatible Controller";
                            case 0x01:
                                return "Consumer IR Controller";
                            case 0x10:
                                return "RF Controller";
                            case 0x11:
                                return "Bluetooth Controller";
                            case 0x12:
                                return "Broadband Controller";
                            case 0x20:
                                return "Ethernet Controller (802.11a)";
                            case 0x21:
                                return "Ethernet Controller (802.11b)";
                            case 0x80:
                                return "Unknown Wireless Controller";
                            default:
                                return "Wireless Controller";
                        }
                    case 0x0E:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Message FIFO";
                            default:
                                return "Intelligent I/O Controller";
                        }
                    case 0x0F:
                        switch (device.Subclass)
                        {
                            case 0x01:
                                return "TV Controller";
                            case 0x02:
                                return "Audio Controller";
                            case 0x03:
                                return "Voice Controller";
                            case 0x04:
                                return "Data Controller";
                            default:
                                return "Satellite Communication Controller";
                        }
                    case 0x10:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "Network and Computing Encrpytion/Decryption";
                            case 0x10:
                                return "Entertainment Encryption/Decryption";
                            case 0x80:
                                return "Unknown Encryption/Decryption";
                            default:
                                return "Encryption/Decryption Controller";
                        }
                    case 0x11:
                        switch (device.Subclass)
                        {
                            case 0x00:
                                return "DPIO Modules";
                            case 0x01:
                                return "Performance Counters";
                            case 0x10:
                                return "Communications Syncrhonization Plus Time and Frequency Test/Measurment";
                            case 0x20:
                                return "Management Card";
                            case 0x80:
                                return "Unknown Data Acquisition/Signal Processing Controller";
                            default:
                                return "Data Acquisition and Signal Processing Controller";
                        }
                    default:
                        return "Unknown device type";
                }
            }

        }

        private static string ToHex(uint aNumber, byte aBits)
        {
            return "0x" + aNumber.ToHex(aBits / 4);
        }
    }

    public class PCIBaseAddressBar
    {
        private ushort prefetchable, type;

        public PCIBaseAddressBar(uint raw)
        {
            IsIO = (raw & 0x01) == 1;

            if (IsIO)
            {
                BaseAddress = raw & 0xFFFFFFFC;
            }
            else
            {
                type = (ushort)((raw >> 1) & 0x03);
                prefetchable = (ushort)((raw >> 3) & 0x01);
                switch (type)
                {
                    case 0x00:
                        BaseAddress = raw & 0xFFFFFFF0;
                        break;
                    case 0x01:
                        BaseAddress = raw & 0xFFFFFFF0;
                        break;
                }
            }
        }

        public uint BaseAddress { get; }

        public bool IsIO { get; }
    }
}
