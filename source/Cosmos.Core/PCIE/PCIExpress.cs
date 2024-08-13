using System;
using System.Runtime.InteropServices;
using PCIDevice = Cosmos.Core.PCIE.PCIDevice;

namespace Cosmos.Core.PCIE
{
    public static unsafe class PCIExpress
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public ushort VendorID;
            public ushort DeviceID;
            public ushort Command;
            public ushort Status;
            public byte RevisionID;
            public byte ProgIF;
            public byte SubClass;
            public byte ClassID;
            public byte CachelineSize;
            public byte LatencyTimer;
            public byte HeaderType;
            public byte BIST;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DeviceHeader
        {
            public Header Header;
            public uint Bar0;
            public uint Bar1;
            public uint Bar2;
            public uint Bar3;
            public uint Bar4;
            public uint Bar5;
            public uint CardbusCisPtr;
            public ushort SubSystemVendorID;
            public ushort SubSystemID;
            public uint ExpRomBaseAddr;
            public byte CapabPtr;
            public byte Reserved0;
            public ushort Reserved1;
            public uint Reserved2;
            public byte InterruptLine;
            public byte InterruptPin;
            public byte MinGrid;
            public byte MaxLatency;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BridgeHeader
        {
            public Header Header;
            public uint Bar0;
            public uint Bar1;
            public byte PrimaryBus;
            public byte SecondBus;
            public byte SubBus;
            public byte SecLatTimer;
            public byte IOBase;
            public byte IOLimit;
            public ushort SecStatus;
            public ushort MemoryBase;
            public ushort MemLimit;
            public ushort PrefMembase;
            public ushort PrefMemlimit;
            public uint PrefBaseUp;
            public uint PrefLimitUp;
            public ushort IOBaseUp;
            public ushort IOLimitUp;
            public byte CapabPtr;
            public byte Reserved0;
            public ushort Reserved1;
            public uint ExpRomBase;
            public byte InterruptLine;
            public byte InterruptPin;
            public ushort BridgeCtrl;
        };

        public static void Initialize()
        {
            if (ACPI.MCFG == null) return;

            var numEntries = (ACPI.MCFG->Header.Length - sizeof(ACPI.MCFGHeader) + sizeof(ACPI.MCFGEntry)) / sizeof(ACPI.MCFGEntry);
            Console.WriteLine(numEntries + " PCIE entries");
            Console.ReadKey();
            for (int i = 0; i < numEntries; i++)
            {
                ACPI.MCFGEntry* Entries = &ACPI.MCFG->Entry0;
                for (var bus = Entries->StartBus; bus < Entries->EndBus; bus++)
                {
                    PCIExpress.CheckBus(Entries->BaseAddress, bus, Entries->Segment);
                }
            }

            Console.WriteLine(numEntries + " PCIE entries");

        }

        public static void CheckBus(ulong BaseAddress, byte Bus, ushort Segment)
        {
            ulong BusAddress = BaseAddress + (ulong)(Bus << 20);

            Header* Header0 = (Header*)BusAddress;
            if (Header0->DeviceID == 0 || Header0->DeviceID == 0xFFFF)return;

            for (byte Slot = 0; Slot < 32; Slot++)
            {
                ulong DeviceAddress = BusAddress + (ulong)(Slot << 15);

                Header* Header1 = (Header*)DeviceAddress;
                if (Header1->DeviceID == 0 || Header1->DeviceID == 0xFFFF) return;

                for (byte Func = 0; Func < 8; Func++)
                {
                    ulong FuncAddress = DeviceAddress + (ulong)(Func << 12);

                    DeviceHeader* Dev = (DeviceHeader*)FuncAddress;
                    if (Dev->Header.DeviceID == 0 || Dev->Header.DeviceID == 0xFFFF) return;

                    PCIDevice device = new PCIDevice();
                    device.Segment = Segment;
                    device.Bus = Bus;
                    device.Slot = Slot;
                    device.Function = Func;
                    device.VendorID = Dev->Header.VendorID;
                    device.IsPCIEDevice = true;

                    device.ClassID = Dev->Header.ClassID;
                    device.SubClassID = Dev->Header.SubClass;
                    device.ProgIF = Dev->Header.ProgIF;

                    device.DeviceID = Dev->Header.DeviceID;

                    if (device.ClassID == 0x06 && device.SubClassID == 0x04)
                    {
                        BridgeHeader* Bri = (BridgeHeader*)Dev;

                        device.IRQ = (byte)(Bri->InterruptLine + 0x20);
                        device.Bar0 = Bri->Bar0;
                        device.Bar1 = Bri->Bar1;
                        device.Bar2 = 0;
                        device.Bar3 = 0;
                        device.Bar4 = 0;
                        device.Bar5 = 0;

                        PCI.CheckBus(Bri->SecondBus);
                    }
                    else
                    {
                        device.IRQ = (byte)(Dev->InterruptLine + 0x20);
                        device.Bar0 = Dev->Bar0;
                        device.Bar1 = Dev->Bar1;
                        device.Bar2 = Dev->Bar2;
                        device.Bar3 = Dev->Bar3;
                        device.Bar4 = Dev->Bar4;
                        device.Bar5 = Dev->Bar5;
                    }

                    Console.WriteLine($"[PCI Express {device.Bus}:{device.Slot}:{device.Function}] {device.VendorID} {device.ClassID}");

                    PCI.Devices.Add(device);
                }
            }
        }
    }
}
