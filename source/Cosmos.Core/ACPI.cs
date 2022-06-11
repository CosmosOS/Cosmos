using Cosmos.Core;
using Cosmos.Debug.Kernel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cosmos.Core
{
    /// <summary>
    /// ACPI (Advanced Configuration and Power Interface) class.
    /// </summary>
    public unsafe class ACPI
    {

        /// <summary>
        /// Debugger instance at the System ring, of the Global section.
        /// </summary>
        public static readonly Debugger mDebugger = new Debugger("System", "Global");

        /// <summary>
        /// RSD table struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct RSDPtr
        {
            /// <summary>
            /// Signature.
            /// </summary>
            public fixed byte Signature[8];

            /// <summary>
            /// CheckSum
            /// </summary>
            public byte CheckSum;

            /// <summary>
            /// OemID
            /// </summary>
            public fixed byte OemID[6];

            /// <summary>
            /// Revision
            /// </summary>
            public byte Revision;

            /// <summary>
            /// RSDT Address
            /// </summary>
            public int RsdtAddress;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AcpiHeader
        {
            /// <summary>
            /// Signature.
            /// </summary>
            public fixed byte Signature[4];

            /// <summary>
            /// Length.
            /// </summary>
            public uint Length;

            /// <summary>
            /// Revision.
            /// </summary>
            public byte Revision;

            /// <summary>
            /// Checksum.
            /// </summary>
            public byte Checksum;

            /// <summary>
            /// OEM ID.
            /// </summary>
            public fixed byte OEMID[6];

            /// <summary>
            /// OEM Table ID.
            /// </summary>
            public fixed byte OEMTableID[8];

            /// <summary>
            /// OEM Revision.
            /// </summary>
            public uint OEMRevision;

            /// <summary>
            /// CreatorID.
            /// </summary>
            public uint CreatorID;

            /// <summary>
            /// Creator Revision.
            /// </summary>
            public uint CreatorRevision;
        };

        /// <summary>
        /// FADT struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FADTPtr
        {
            /// <summary>
            /// ACPI Header.
            /// </summary>
            public AcpiHeader Header;

            /// <summary>
            /// Firmware Control.
            /// </summary>
            public uint FirmwareCtrl;

            /// <summary>
            /// DSDT Signature.
            /// </summary>
            public uint Dsdt;

            public byte Reserved;
            public byte PreferredPowerManagementProfile;
            public ushort SCI_Interrupt;
            public uint SMI_CommandPort;

            /// <summary>
            /// ACPI Enable.
            /// </summary>
            public byte AcpiEnable;

            /// <summary>
            /// ACPI Disable.
            /// </summary>
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

        /// <summary>
        /// MADT struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MADTPtr
        {
            /// <summary>
            /// ACPI Header.
            /// </summary>
            public AcpiHeader Header;

            /// <summary>
            /// Local APIC Address.
            /// </summary>
            public uint LocalAPICAddress;

            /// <summary>
            /// Flags.
            /// </summary>
            public uint Flags;
        }

        /// <summary>
        /// APIC Header struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ApicHeader
        {
            /// <summary>
            /// APIC Type.
            /// </summary>
            public ApicType Type;

            /// <summary>
            /// Length.
            /// </summary>
            public byte Length;
        }

        /// <summary>
        /// APIC Type enum.
        /// </summary>
        public enum ApicType : byte
        {
            LocalAPIC,
            IOAPIC,
            InterruptOverride
        }

        /// <summary>
        /// ApicLocalApic struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApicLocalApic
        {
            /// <summary>
            /// APIC Header.
            /// </summary>
            public ApicHeader Header;

            /// <summary>
            /// ACPI Processor ID.
            /// </summary>
            public byte AcpiProcessorId;

            /// <summary>
            /// APIC ID.
            /// </summary>
            public byte ApicId;

            /// <summary>
            /// APIC Flags.
            /// </summary>
            public uint Flags;
        }

        /// <summary>
        /// ApicIOApic struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ApicIOApic
        {
            /// <summary>
            /// APIC Header.
            /// </summary>
            public ApicHeader Header;

            /// <summary>
            /// APIC ID.
            /// </summary>
            public byte IOApicId;

            /// <summary>
            /// Reserved.
            /// </summary>
            public byte Reserved;

            /// <summary>
            /// IO APIC Base Address.
            /// </summary>
            public uint IOApicAddress;

            /// <summary>
            /// Global System Interrupt Base Address.
            /// </summary>
            public uint GlobalSystemInterruptBase;
        }

        /// <summary>
        /// ApicInterruptOverride struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApicInterruptOverride
        {
            /// <summary>
            /// APIC Header.
            /// </summary>
            public ApicHeader Header;

            /// <summary>
            /// Bus.
            /// </summary>
            public byte Bus;

            /// <summary>
            /// Source.
            /// </summary>
            public byte Source;

            /// <summary>
            /// Interrupt.
            /// </summary>
            public uint Interrupt;

            /// <summary>
            /// Floags.
            /// </summary>
            public ushort Flags;
        }

        // New Port I/O
        /// <summary>
        /// IO port.
        /// </summary>
        private static IOPort smiIO, pm1aIO, pm1bIO;

        // ACPI variables
        /// <summary>
        /// SMI CMD.
        /// </summary>
        private static int* SMI_CMD;
        /// <summary>
        /// ACPI ENABLE.
        /// </summary>
        private static byte ACPI_ENABLE;
        /// <summary>
        /// ACPI DISABLE.
        /// </summary>
        private static byte ACPI_DISABLE;
        /// <summary>
        /// PM1a CNT
        /// </summary>
        private static int* PM1a_CNT;
        /// <summary>
        /// PM1b CNT
        /// </summary>
        private static int* PM1b_CNT;
        /// <summary>
        /// SLP TYPa
        /// </summary>
        private static short SLP_TYPa;
        /// <summary>
        /// SLP TYPb
        /// </summary>
        private static short SLP_TYPb;
        /// <summary>
        /// SLP EN.
        /// </summary>
        private static short SLP_EN;
        /// <summary>
        /// PM1 CNT LEN1
        /// </summary>
        private static byte PM1_CNT_LEN;
        /// <summary>
        /// Global MADT.
        /// </summary>
        public static MADTPtr* MADT;
        /// <summary>
        /// Global IO APIC.
        /// </summary>
        public static ApicIOApic* IOAPIC;

        /// <summary>
        /// Check ACPI header.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        static int acpiCheckHeader(byte* ptr, string sig)
        {
            return Compare(sig, ptr);
        }

        /// <summary>
        /// Compare string to byte array.
        /// </summary>
        /// <param name="c1">String.</param>
        /// <param name="c2">Pointer to the head of the byte array.</param>
        /// <returns>0 - identical, -1 different.</returns>
        static int Compare(string c1, byte* c2)
        {
            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i]) { return -1; }
            }
            return 0;
        }

        /// <summary>
        /// Check RSD checksum.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>True if RSDT table checksum is good.</returns>
        static bool Check_RSD(uint address)
        {
            byte sum = 0;
            byte* check = (byte*)address;

            for (int i = 0; i < 20; i++)
            {
                sum += *(check++);
            }

            return (sum == 0);
        }

        /// <summary>
        /// Start the ACPI.
        /// </summary>
        /// <param name="initialize">Initialize the ACPI. (default = true)</param>
        /// <param name="enable">Enable the ACPI. (default = true)</param>
        public static void Start(bool initialize = true, bool enable = true)
        {
            if (initialize)
            {
                Init();
            }

            if (enable)
            {
                Enable();
            }
        }

        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown on IO error.</exception>
        public static void Shutdown()
        {
            Console.Clear();
            if (PM1a_CNT == null)
            {
                Init();
            }

            pm1aIO.Word = (ushort)(SLP_TYPa | SLP_EN);

            if (PM1b_CNT != null)
            {
                pm1bIO.Word = (ushort)(SLP_TYPb | SLP_EN);
            }

            CPU.Halt();
        }

        /// <summary>
        /// Reboot ACPI.
        /// Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public static void Reboot()
        {
            throw new NotImplementedException("ACPI Reset not implemented yet."); //TODO
        }

        /// <summary>
        /// Initialize the ACPI.
        /// </summary>
        /// <returns>true on success, false on failure.</returns>
        private static bool Init()
        {
            IOAPIC = null;
            var rsdp = RSDPAddress();
            byte* ptr = (byte*)rsdp;

            Global.mDebugger.Send("ACPI v" + rsdp->Revision);

            var rsdt = (AcpiHeader*)rsdp->RsdtAddress;
            ptr = (byte*)rsdt;

            uint* p = (uint*)(rsdt + 1);
            uint* end = (uint*)((byte*)rsdt + rsdt->Length);

            while (p < end)
            {
                uint address = *p++;

                ParseDT((AcpiHeader*)address);
            }

            return true;
        }

        private static void ParseDT(AcpiHeader *hdr)
        {
            var signature = Encoding.ASCII.GetString(hdr->Signature, 4);

            Global.mDebugger.Send(signature + " detected");

            if (signature == "FACP")
            {
                Global.mDebugger.Send("Parse FACP");

                var Fadt = (FADTPtr*)hdr;

                if (acpiCheckHeader((byte*)Fadt->Dsdt, "DSDT") == 0)
                {
                    byte* S5Addr = (byte*)Fadt->Dsdt + sizeof(AcpiHeader);
                    int dsdtLength = *((int*)Fadt->Dsdt + 1) - sizeof(AcpiHeader);

                    while (0 < dsdtLength--)
                    {
                        if (Compare("_S5_", S5Addr) == 0)
                        {
                            break;
                        }
                        S5Addr++;
                    }

                    if (dsdtLength > 0)
                    {
                        if ((*(S5Addr - 1) == 0x08 || (*(S5Addr - 2) == 0x08 && *(S5Addr - 1) == '\\')) && *(S5Addr + 4) == 0x12)
                        {
                            S5Addr += 5;
                            S5Addr += ((*S5Addr & 0xC0) >> 6) + 2;
                            if (*S5Addr == 0x0A)
                            {
                                S5Addr++;
                            }
                            SLP_TYPa = (short)(*(S5Addr) << 10);
                            S5Addr++;
                            if (*S5Addr == 0x0A)
                            {
                                S5Addr++;
                            }
                            SLP_TYPb = (short)(*(S5Addr) << 10);
                            SMI_CMD = (int*)Fadt->SMI_CommandPort;
                            ACPI_ENABLE = Fadt->AcpiEnable;
                            ACPI_DISABLE = Fadt->AcpiDisable;
                            PM1a_CNT = (int*)Fadt->PM1aControlBlock;
                            PM1b_CNT = (int*)Fadt->PM1bControlBlock;
                            PM1_CNT_LEN = Fadt->PM1ControlLength;
                            SLP_EN = 1 << 13;

                            smiIO = new IOPort((ushort)SMI_CMD);
                            pm1aIO = new IOPort((ushort)PM1a_CNT);
                            pm1bIO = new IOPort((ushort)PM1b_CNT);
                        }
                    }
                }
            }
            else if (signature == "APIC")
            {
                Global.mDebugger.Send("Parse APIC");

                MADT = (MADTPtr*)hdr;

                byte* p = (byte*)(MADT + 1);
                byte* end = (byte*)MADT + MADT->Header.Length;
                while (p < end)
                {
                    var header = (ApicHeader*)p;
                    var type = header->Type;
                    byte length = header->Length;

                    if (type == ApicType.LocalAPIC)
                    {
                        Global.mDebugger.Send("Parse local APIC");
                        var pic = (ApicLocalApic*)p;
                        Global.mDebugger.Send("Found APIC " + (ulong)pic->ApicId + " (Processor ID:" + pic->AcpiProcessorId + ")");
                    }
                    else if (type == ApicType.IOAPIC)
                    {
                        Global.mDebugger.Send("Parse IO APIC");
                        var ioapic = (ApicIOApic*)p;
                        if (IOAPIC == null)
                        {
                            IOAPIC = ioapic;
                        }
                        Global.mDebugger.Send("Found IO APIC " + (ulong)ioapic->IOApicId + " (Address:0x" + ((ulong)ioapic->IOApicAddress).ToString("X") + ", GSIB:" + (ulong)ioapic->GlobalSystemInterruptBase + ")");
                    }
                    else if (type == ApicType.InterruptOverride)
                    {
                        Global.mDebugger.Send("Parse Interrupt Override APIC");

                        var ovr = (ApicInterruptOverride*)p;

                        Global.mDebugger.Send("Found APIC Interrupt Override (Bus: " + ((ulong)ovr->Bus).ToString() + ", Source:" + ((ulong)ovr->Source).ToString() + ", Interrupt:0x" + ((ulong)ovr->Interrupt).ToString("X") + ", Flags:" + ((ulong)ovr->Flags).ToString() + ")");
                    }

                    p += length;
                }
            }
        }

        /// <summary>
        /// Enable ACPI.
        /// </summary>
        public static void Enable()
        {
            smiIO = new IOPort(ACPI_ENABLE);
        }

        /// <summary>
        /// Disable ACPI.
        /// </summary>
        public static void Disable()
        {
            smiIO = new IOPort(ACPI_DISABLE);
        }

        /// <summary>
        /// Get the RSDP address.
        /// </summary>
        /// <returns>uint value.</returns>
        private static unsafe RSDPtr *RSDPAddress()
        {
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
            {
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                {
                    if (Check_RSD(addr))
                    {
                        return (RSDPtr*)addr;
                    }
                }
            }

            uint ebda_address = *((uint*)0x040E);
            ebda_address = (ebda_address * 0x10) & 0x000fffff;

            for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
            {
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                {
                    return (RSDPtr*)addr;
                }
            }

            return null;
        }

        public static uint RemapIRQ(uint irq)
        {
            byte* p = (byte*)(MADT + 1);
            byte* end = (byte*)MADT + MADT->Header.Length;

            while (p < end)
            {
                var header = (ApicHeader*)p;
                var type = header->Type;
                byte length = header->Length;

                if (type == ApicType.InterruptOverride)
                {
                    var ovr = (ApicInterruptOverride*)p;

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
