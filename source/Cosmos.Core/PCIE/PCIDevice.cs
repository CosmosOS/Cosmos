using System.Collections.Generic;
using System;
using Cosmos.Core;

namespace Cosmos.Core.PCIE
{
    public class PCIDevice
    {
        public ushort Bus;
        public ushort Slot;
        public ushort Function;
        public ushort VendorID;

        public ushort DeviceID;

        public byte ClassID;
        public byte SubClassID;
        public byte ProgIF;
        public byte IRQ;

        public uint Bar0;
        public uint Bar1;
        public uint Bar2;
        public uint Bar3;
        public uint Bar4;
        public uint Bar5;

        //PCI Express
        public ushort Segment;
        public bool IsPCIEDevice;

        public void WriteRegister(ushort Register, ushort Value)
        {
            PCI.WriteRegister16(Bus, Slot, Function, (byte)Register, (ushort)(ReadRegister(Register) | Value));
        }

        public ushort ReadRegister(ushort Register)
        {
            return PCI.ReadRegister16(Bus, Slot, Function, (byte)Register);
        }
    }

    public static unsafe class PCI
    {
        public static List<PCIDevice> Devices;

        public static PCIDevice GetDevice(ushort VendorID, ushort DeviceID)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                if (
                    Devices[i] != null &&
                    Devices[i].VendorID == VendorID &&
                    Devices[i].DeviceID == DeviceID
                    )
                {
                    return Devices[i];
                }
            }
            return null;
        }

        public static PCIDevice GetDevice(byte ClassID, byte SubClassID, byte ProgIF)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                if (
                    Devices[i] != null &&
                    Devices[i].ClassID == ClassID &&
                    Devices[i].SubClassID == SubClassID &&
                    Devices[i].ProgIF == ProgIF
                    )
                {
                    return Devices[i];
                }
            }
            return null;
        }

        public static void Initialise()
        {
            Devices = new List<PCIDevice>();
            if ((GetHeaderType(0x0, 0x0, 0x0) & 0x80) == 0)
            {
                CheckBus(0);
            }
            else
            {
                for (ushort fn = 0; fn < 8; fn++)
                {
                    if (GetVendorID(0x0, 0x0, fn) != 0xFFFF)
                        break;

                    CheckBus(fn);
                }
            }

            PCIExpress.Initialize();

            Console.Write("[PCI] PCI Initialized. ");
            Console.Write(((ulong)Devices.Count).ToString());
            Console.WriteLine(" Devices");
        }

        public static void CheckBus(ushort Bus)
        {
            for (ushort slot = 0; slot < 32; slot++)
            {
                ushort vendorID = GetVendorID(Bus, slot, 0);
                if (vendorID == 0xFFFF)
                {
                    continue;
                }

                PCIDevice device = new PCIDevice();
                device.Bus = Bus;
                device.Slot = slot;
                device.Function = 0;
                device.VendorID = vendorID;
                device.Segment = 0;
                device.IsPCIEDevice = false;

                device.Bar0 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x10);
                device.Bar1 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x14);
                device.Bar2 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x18);
                device.Bar3 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x1C);
                device.Bar4 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x20);
                device.Bar5 = ReadRegister32(device.Bus, device.Slot, device.Function, 0x24);

                device.ClassID = ReadRegister8(device.Bus, device.Slot, device.Function, 11);
                device.SubClassID = ReadRegister8(device.Bus, device.Slot, device.Function, 10);
                device.ProgIF = ReadRegister8(device.Bus, device.Slot, device.Function, 9);
                device.IRQ = (byte)(0x20 + ReadRegister8(device.Bus, device.Slot, device.Function, 60));

                device.DeviceID = ReadRegister16(device.Bus, device.Slot, device.Function, 2);

                Devices.Add(device);

                if (device.ClassID == 0x06 && device.SubClassID == 0x04)
                {
                    CheckBus(ReadRegister8(device.Bus, device.Slot, device.Function, 25));
                }
            }
        }

        public static void WriteRegister32(ushort Bus, ushort Slot, ushort Function, byte aRegister, uint Value)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | ((uint)(aRegister & 0xFC));
            IOPort.Write32(0xCF8, xAddr);
            IOPort.Write32(0xCFC, Value);
        }

        public static uint ReadRegister32(ushort Bus, ushort Slot, ushort Function, byte aRegister)
        {
            uint xAddr = PCI.GetAddressBase(Bus, Slot, Function) | ((uint)(aRegister & 0xFC));
            IOPort.Write32(0xCF8, xAddr);
            return IOPort.Read32(0xCFC);
        }

        public static ushort ReadRegister16(ushort Bus, ushort Slot, ushort Function, byte aRegister)
        {
            uint xAddr = PCI.GetAddressBase(Bus, Slot, Function) | ((uint)(aRegister & 0xFC));
            IOPort.Write32(0xCF8, xAddr);
            return (ushort)(IOPort.Read32(0xCFC) >> ((aRegister % 4) * 8) & 0xFFFF);
        }

        public static byte ReadRegister8(ushort Bus, ushort Slot, ushort Function, byte aRegister)
        {
            uint xAddr = PCI.GetAddressBase(Bus, Slot, Function) | ((uint)(aRegister & 0xFC));
            IOPort.Write32(0xCF8, xAddr);
            return ((byte)(IOPort.Read32(0xCFC) >> ((aRegister % 4) * 8) & 0xFF));
        }

        public static void WriteRegister16(ushort Bus, ushort Slot, ushort Function, byte aRegister, ushort Value)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | ((uint)(aRegister & 0xFC));
            IOPort.Write32(0xCF8, xAddr);
            IOPort.Write16(0xCFC, Value);
        }

        public static ushort GetVendorID(ushort Bus, ushort Slot, ushort Function)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | 0x0 & 0xFC;
            IOPort.Write32(0xCF8, xAddr);
            return (ushort)(IOPort.Read32(0xCFC) >> ((0x0 % 4) * 8) & 0xFFFF);
        }

        public static ushort GetHeaderType(ushort Bus, ushort Slot, ushort Function)
        {
            uint xAddr = GetAddressBase(Bus, Slot, Function) | 0xE & 0xFC;
            IOPort.Write32(0xCF8, xAddr);
            return (byte)(IOPort.Read32(0xCFC) >> ((0xE % 4) * 8) & 0xFF);
        }

        public static uint GetAddressBase(ushort Bus, uint Slot, uint Function)
        {
            return (uint)(0x80000000 | (Bus << 16) | ((Slot & 0x1F) << 11) | ((Function & 0x07) << 8));
        }
    }
}