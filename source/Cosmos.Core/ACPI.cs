using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MACPI.Cosmos.HAL.PCIE
{
    public unsafe class ACPI
    {
        private static short SLP_TYPa;
        private static short SLP_TYPb;
        private static short SLP_EN;

        public static ACPI_FADT* FADT;
        public static ACPI_MADT* MADT;
        public static APIC_IO_APIC* IO_APIC;
        public static ACPI_HPET* HPET;
        public static MCFGHeader* MCFG;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ACPI_RSDP
        {
            public fixed sbyte Signature[8];
            public byte Checksum;
            public fixed sbyte OEMID[6];
            public byte Revision;
            public uint RsdtAddress;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACPI_HEADER
        {
            public fixed sbyte Signature[4];
            public uint Length;
            public byte Revision;
            public byte Checksum;
            public fixed byte OEMID[6];
            public fixed sbyte OEMTableID[8];
            public uint OEMRevision;
            public uint CreatorID;
            public uint CreatorRevision;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APIC_HEADER
        {
            public APIC_TYPE Type;
            public byte Length;
        }

        public enum APIC_TYPE : byte
        {
            LocalAPIC,
            IOAPIC,
            InterruptOverride
        }

        [StructLayout(LayoutKind.Sequential,Pack = 1)]
        public struct MCFGHeader
        {
            public ACPI_HEADER Header;
            public ulong Reserved;
            public MCFGEntry Entry0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MCFGEntry
        {
            public ulong BaseAddress;
            public ushort Segment;
            public byte StartBus;
            public byte EndBus;
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct APIC_LOCAL_APIC
        {
            public APIC_HEADER Header;
            public byte AcpiProcessorId;
            public byte ApicId;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APIC_IO_APIC
        {
            public APIC_HEADER Header;
            public byte IOApicId;
            public byte Reserved;
            public uint IOApicAddress;
            public uint GlobalSystemInterruptBase;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct APIC_INTERRUPT_OVERRIDE
        {
            public APIC_HEADER Header;
            public byte Bus;
            public byte Source;
            public uint Interrupt;
            public ushort Flags;
        }

        [StructLayout(LayoutKind.Sequential,Pack = 1)]
        public struct ACPI_HPET 
        {
            public ACPI_HEADER Header;
            public byte HardwareRevisionID;
            public byte Attribute;
            public ushort PCIVendorID;
            public ACPI_HPET_ADDRESS_STRUCTURE Addresses;
            public byte HPETNumber;
            public ushort MinimumTick;
            public byte PageProtection;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACPI_HPET_ADDRESS_STRUCTURE 
        {
            public byte AddressSpaceID;
            public byte RegisterBitWidth;
            public byte RegisterBitOffset;
            public byte Reserved;
            public ulong Address;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACPI_FADT
        {
            public ACPI_HEADER Header;

            public uint FirmwareCtrl;
            public uint Dsdt;

            public byte Reserved;

            public byte PreferredPowerManagementProfile;
            public ushort SCI_Interrupt;
            public uint SMI_CommandPort;
            public byte AcpiEnable;
            public byte AcpiDisable;
            public byte S4BIOS_REQ;
            public byte PSTATE_Control;
            public uint PM1aEventBlock;
            public uint PM1bEventBlock;
            public uint PM1aControlBlock;
            public uint PM1bControlBlock;
            public uint PM2ControlBlock;
            public uint PMTimerBlock;
            public uint GPE0Block;
            public uint GPE1Block;
            public byte PM1EventLength;
            public byte PM1ControlLength;
            public byte PM2ControlLength;
            public byte PMTimerLength;
            public byte GPE0Length;
            public byte GPE1Length;
            public byte GPE1Base;
            public byte CStateControl;
            public ushort WorstC2Latency;
            public ushort WorstC3Latency;
            public ushort FlushSize;
            public ushort FlushStride;
            public byte DutyOffset;
            public byte DutyWidth;
            public byte DayAlarm;
            public byte MonthAlarm;
            public byte Century;

            public ushort BootArchitectureFlags;

            public byte Reserved2;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ACPI_MADT
        {
            public ACPI_HEADER Header;
            public uint LocalAPICAddress;
            public uint Flags;
        }

        private static unsafe ACPI_RSDP* GetRSDP()
        {
            byte* p = (byte*)0xE0000;
            byte* end = (byte*)0xFFFFF;

            while (p < end)
            {
                ulong signature = *(ulong*)p;

                if (signature == 0x2052545020445352) // 'RSD PTR '
                {
                    return (ACPI_RSDP*)p;
                }

                p += 16;
            }

            return null;
        }

        public static void Shutdown()
        {
            IOPort.Write16((ushort)FADT->PM1aControlBlock, (ushort)(SLP_TYPa | SLP_EN));
            IOPort.Write16((ushort)FADT->PM1bControlBlock, (ushort)(SLP_TYPb | SLP_EN));
            CPU.Halt();
        }

        public static List<byte> LocalAPIC_CPUIDs;

        public static void Initialize()
        {
            FADT = null;
            MADT = null;
            IO_APIC = null;
            HPET = null;
            MCFG = null;

            LocalAPIC_CPUIDs = new List<byte>();
            ACPI_RSDP* rsdp = GetRSDP();
            //MMIO.Map(rsdp->RsdtAddress, ushort.MaxValue);
            ACPI_HEADER* hdr = (ACPI_HEADER*)rsdp->RsdtAddress;
            ACPI_HEADER* rsdt = (ACPI_HEADER*)rsdp->RsdtAddress;

            if (rsdt != null && *(uint*)rsdt == 0x54445352) //RSDT
            {
                uint* p = (uint*)(rsdt + 1);
                uint* end = (uint*)((byte*)rsdt + rsdt->Length);

                while (p < end)
                {
                    uint address = *p++;
                    ParseDT((ACPI_HEADER*)address);
                }
            }

            Console.WriteLine("[ACPI] ACPI Initialized");
        }

        private static void ParseDT(ACPI_HEADER* hdr)
        {
            if (*(uint*)hdr->Signature == 0x50434146)
            {
                FADT = (ACPI_FADT*)hdr;

                if (*(uint*)FADT->Dsdt == 0x54445344) //DSDT
                {
                    byte* S5Addr = (byte*)FADT->Dsdt + sizeof(ACPI_HEADER);
                    int dsdtLength = *((int*)FADT->Dsdt + 1) - sizeof(ACPI_HEADER);

                    while (0 < dsdtLength--)
                    {
                        if (*(uint*)S5Addr == 0x5f35535f) //_S5_
                            break;
                        S5Addr++;
                    }

                    if (dsdtLength > 0)
                    {
                        if ((*(S5Addr - 1) == 0x08 || (*(S5Addr - 2) == 0x08 && *(S5Addr - 1) == '\\')) && *(S5Addr + 4) == 0x12)
                        {
                            S5Addr += 5;
                            S5Addr += ((*S5Addr & 0xC0) >> 6) + 2;
                            if (*S5Addr == 0x0A)
                                S5Addr++;
                            SLP_TYPa = (short)(*(S5Addr) << 10);
                            S5Addr++;
                            if (*S5Addr == 0x0A)
                                S5Addr++;
                            SLP_TYPb = (short)(*(S5Addr) << 10);
                            SLP_EN = 1 << 13;

                            return;
                        }
                    }
                }
            }
            else if (*(uint*)hdr->Signature == 0x43495041)
            {
                MADT = (ACPI_MADT*)hdr;

                byte* p = (byte*)(MADT + 1);
                byte* end = (byte*)MADT + MADT->Header.Length;
                while (p < end)
                {
                    APIC_HEADER* header = (APIC_HEADER*)p;
                    APIC_TYPE type = header->Type;
                    byte length = header->Length;

                    if (type == APIC_TYPE.LocalAPIC)
                    {
                        APIC_LOCAL_APIC* pic = (APIC_LOCAL_APIC*)p;
                        if (((pic->Flags & 1) ^ ((pic->Flags >> 1) & 1)) == 1)
                        {
                            LocalAPIC_CPUIDs.Add(pic->ApicId);
                        }
                    }
                    else if (type == APIC_TYPE.IOAPIC)
                    {
                        APIC_IO_APIC* ioapic = (APIC_IO_APIC*)p;
                        if (IO_APIC == null)
                        {
                            IO_APIC = ioapic;
                        }
                    }
                    else if (type == APIC_TYPE.InterruptOverride)
                    {
                        APIC_INTERRUPT_OVERRIDE* ovr = (APIC_INTERRUPT_OVERRIDE*)p;
                    }

                    p += length;
                }
            }
            else if (*(uint*)hdr->Signature == 0x54455048) 
            {
                HPET = (ACPI_HPET*)hdr;
            }
            else if (*(uint*)hdr->Signature == 0x4746434D) 
            {
                MCFG = (MCFGHeader*)hdr;
            }
        }

        public static uint RemapIRQ(uint irq)
        {
            byte* p = (byte*)(MADT + 1);
            byte* end = (byte*)MADT + MADT->Header.Length;

            while (p < end)
            {
                APIC_HEADER* header = (APIC_HEADER*)p;
                APIC_TYPE type = header->Type;
                byte length = header->Length;

                if (type == APIC_TYPE.InterruptOverride)
                {
                    APIC_INTERRUPT_OVERRIDE* ovr = (APIC_INTERRUPT_OVERRIDE*)p;

                    if (ovr->Source == irq)
                    {
                        return ovr->Interrupt;
                    }
                }

                p += length;
            }

            return irq;
        }
    }
}