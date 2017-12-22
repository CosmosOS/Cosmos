using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core.IOGroup;
using Cosmos.Common.Extensions;
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
            CocdMask = 0x0f,
            Start = 0x40,
            Capable = 0x80
        };

        public enum PCIInterruptPIN : byte
        {
            None = 0x00,
            INTA = 0x01,
            INTB = 0x02,
            INTC = 0x03,
            INTD = 0x04
        };

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

        public readonly ushort VendorID;
        public readonly ushort DeviceID;

        public readonly ushort Command;
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

        public PCIBaseAddressBar[] BaseAddressBar;

        protected static Core.IOGroup.PCI IO = new Core.IOGroup.PCI();

        public PCIDevice(uint bus, uint slot, uint function)
        {
            this.bus = bus;
            this.slot = slot;
            this.function = function;

            VendorID = ReadRegister16((byte)Config.VendorID);
            DeviceID = ReadRegister16((byte)Config.DeviceID);

            Command = ReadRegister16((byte)Config.Command);
            Status = ReadRegister16((byte)Config.Status);


            RevisionID = ReadRegister8((byte)Config.RevisionID);
            ProgIF = ReadRegister8((byte)Config.ProgIF);
            Subclass = ReadRegister8((byte)Config.SubClass);
            ClassCode = ReadRegister8((byte)Config.Class);
            SecondaryBusNumber = ReadRegister8((byte)Config.SecondaryBusNo);

            HeaderType = (PCIHeaderType)ReadRegister8((byte)Config.HeaderType);
            BIST = (PCIBist)ReadRegister8((byte)Config.BIST);
            InterruptPIN = (PCIInterruptPIN)ReadRegister8((byte)Config.InterruptPIN);

            DeviceExists = (uint)VendorID != 0xFFFF && (uint)DeviceID != 0xFFFF;
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

        public static ushort GetHeaderType(ushort Bus, ushort Slot, ushort Function)
        {
            UInt32 xAddr = GetAddressBase(Bus, Slot, Function) | 0xE & 0xFC;
            IO.ConfigAddressPort.DWord = xAddr;
            return (byte)(IO.ConfigDataPort.DWord >> ((0xE % 4) * 8) & 0xFF);
        }

        public static UInt16 GetVendorID(ushort Bus, ushort Slot, ushort Function)
        {
            UInt32 xAddr = GetAddressBase(Bus, Slot, Function) | 0x0 & 0xFC;
            IO.ConfigAddressPort.DWord = xAddr;
            return (UInt16)(IO.ConfigDataPort.DWord >> ((0x0 % 4) * 8) & 0xFFFF);
        }

        #region IOReadWrite
        protected byte ReadRegister8(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (byte)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFF);
        }

        protected void WriteRegister8(byte aRegister, byte value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Byte = value;
        }

        protected UInt16 ReadRegister16(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return (UInt16)(IO.ConfigDataPort.DWord >> ((aRegister % 4) * 8) & 0xFFFF);
        }

        protected void WriteRegister16(byte aRegister, ushort value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.Word = value;
        }

        protected UInt32 ReadRegister32(byte aRegister)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            return IO.ConfigDataPort.DWord;
        }

        protected void WriteRegister32(byte aRegister, uint value)
        {
            UInt32 xAddr = GetAddressBase(bus, slot, function) | ((UInt32)(aRegister & 0xFC));
            IO.ConfigAddressPort.DWord = xAddr;
            IO.ConfigDataPort.DWord = value;
        }
        #endregion

        protected static UInt32 GetAddressBase(uint aBus, uint aSlot, uint aFunction)
        {
            return 0x80000000 | (aBus << 16) | ((aSlot & 0x1F) << 11) | ((aFunction & 0x07) << 8);
        }

        public void EnableMemory(bool enable)
        {
            UInt16 command = ReadRegister16(0x04);

            UInt16 flags = 0x0007;

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            WriteRegister16(0x04, command);
        }

        public void EnableBusMaster(bool enable)
        {
            UInt16 command = ReadRegister16(0x04);

            UInt16 flags = (1 << 2);

            if (enable)
                command |= flags;
            else
                command &= (ushort)~flags;

            WriteRegister16(0x04, command);
        }

        public class DeviceClass
        {
            public static string GetString(PCIDevice device)
            {
                switch (device.VendorID)
                {
                    case 0x1022: //AMD
                        switch (device.DeviceID)
                        {
                            case 0x2000:
                                return "AMD PCnet LANCE PCI Ethernet Controller";
                        }
                        break;
                    case 0x104B: //Sony
                        switch (device.DeviceID)
                        {
                            case 0x1040:
                                return "Mylex BT958 SCSI Host Adaptor";
                        }
                        break;
                    case 0x1274: //Ensoniq
                        switch (device.DeviceID)
                        {
                            case 0x1371:
                                return "Ensoniq AudioPCI";
                        }
                        break;
                    case 0x15AD: //VMware
                        switch (device.DeviceID)
                        {
                            case 0x0405:
                                return "VMware NVIDIA 9500MGS";
                            case 0x0770:
                                return "VMware Standard Enhanced PCI to USB Host Controller";
                            case 0x0790:
                                return "VMware 6.0 Virtual USB 2.0 Host Controller";
                            case 0x07A0:
                                return "VMware PCI Express Root Port";
                        }
                        break;
                    case 0x8086: //Intel
                        switch (device.DeviceID)
                        {
                            case 0x7190:
                                return "Intel 440BX/ZX AGPset Host Bridge";
                            case 0x7191:
                                return "Intel 440BX/ZX AGPset PCI-to-PCI bridge";
                            case 0x7110:
                                return "Intel PIIX4/4E/4M ISA Bridge";
                            case 0x7112:
                                return "Intel PIIX4/4E/4M USB Interface";
                        }
                        break;
                }

                switch (device.ClassCode)
                {
                    //case 0x00:
                    //    return "Any device";
                    case 0x01:
                        return "Mass Storage Controller";
                    case 0x02:
                        return "Network Controller";
                    case 0x03:
                        return "Display Controller";
                    case 0x04:
                        return "Multimedia Controller";
                    case 0x05:
                        return "Memory Controller";
                    case 0x06:
                        return "Bridge Device";
                    case 0x07:
                        return "Simple Communication Controller";
                    case 0x08:
                        return "Base System Peripheral";
                    case 0x09:
                        return "Input Device";
                    case 0x0A:
                        return "Docking Station";
                    case 0x0B:
                        return "Processor";
                    case 0x0C:
                        return "Serial Bus Controller";
                    case 0x0D:
                        return "Wireless Controller";
                    case 0x0E:
                        return "Intelligent I/O Controller";
                    case 0x0F:
                        return "Satellite Communication Controller";
                    case 0x10:
                        return "Encryption/Decryption Controller";
                    case 0x11:
                        return "Data Acquisition and Signal Processing Controller";
                        //case 0xFF:
                        //    return "Unkown device";
                }
                return "ClassCode: " + device.ClassCode + "     Subclass: " + device.Subclass + "     ProgIF: " + device.ProgIF;
            }
        }

        private static string ToHex(uint aNumber, byte aBits)
        {
            return "0x" + aNumber.ToHex(aBits / 4);
        }
    }

    public class PCIBaseAddressBar
    {
        private uint baseAddress = 0;
        private ushort prefetchable = 0;
        private ushort type = 0;
        private bool isIO = false;

        public PCIBaseAddressBar(uint raw)
        {
            isIO = (raw & 0x01) == 1;

            if (isIO)
            {
                baseAddress = raw & 0xFFFFFFFC;
            }
            else
            {
                type = (ushort)((raw >> 1) & 0x03);
                prefetchable = (ushort)((raw >> 3) & 0x01);
                switch (type)
                {
                    case 0x00:
                        baseAddress = raw & 0xFFFFFFF0;
                        break;
                    case 0x01:
                        baseAddress = raw & 0xFFFFFFF0;
                        break;
                }
            }
        }

        public uint BaseAddress
        {
            get { return baseAddress; }
        }

        public bool IsIO
        {
            get { return isIO; }
        }
    }
}
