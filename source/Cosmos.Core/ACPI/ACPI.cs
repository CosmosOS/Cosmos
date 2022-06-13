using ACPILib.AML;
using ACPILib.Parser2;
using Cosmos.Debug.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
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
        public struct FADTPtr
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

            // 12 public byte structure; see below for details
            public GenericAddressStructure ResetReg;

            public byte ResetValue;
            public byte Reserved3;
            public byte Reserved34;
            public byte Reserved35;

            // 64bit pointers - Available on ACPI 2.0+
            public ulong X_FirmwareControl;
            public ulong X_Dsdt;

            public GenericAddressStructure X_PM1aEventBlock;
            public GenericAddressStructure X_PM1bEventBlock;
            public GenericAddressStructure X_PM1aControlBlock;
            public GenericAddressStructure X_PM1bControlBlock;
            public GenericAddressStructure X_PM2ControlBlock;
            public GenericAddressStructure X_PMTimerBlock;
            public GenericAddressStructure X_GPE0Block;
            public GenericAddressStructure X_GPE1Block;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GenericAddressStructure
        {
            public byte AddressSpace;
            public byte BitWidth;
            public byte BitOffset;
            public byte AccessSize;
            public ulong Address;
        };

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
        private static IOPort smiIO, pm1aIO, pm1bIO, ResetRegister;

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
        /// Reset value to write into reset register when you need to reboot
        /// </summary>
        private static byte ResetValue;
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
        /// FADT table
        /// </summary>
        public static FADTPtr* FADT;

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
            for (var i = 0; i < c1.Length; i++)
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
            var check = (byte*)address;

            for (var i = 0; i < 20; i++)
            {
                sum += *check++;
            }

            return sum == 0;
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
            if (PM1a_CNT == null)
            {
                Init();
            }

            var header = FADT->Header;
            if (header.Revision >= 2 && (FADT->Flags & (1 << 10)) != 0)
            {
                ResetRegister.Byte = ResetValue;
            }
            else
            {
                throw new Exception("Hardware does not support ACPI reboot.");
            }

            throw new Exception("ACPI reboot failed.");
        }

        /// <summary>
        /// Initialize the ACPI.
        /// </summary>
        /// <returns>true on success, false on failure.</returns>
        private static bool Init()
        {
            IOAPIC = null;
            var rsdp = RSDPAddress();
            var ptr = (byte*)rsdp;

            Global.mDebugger.Send("ACPI v" + rsdp->Revision);

            var rsdt = (AcpiHeader*)rsdp->RsdtAddress;
            ptr = (byte*)rsdt;

            var p = (uint*)(rsdt + 1);
            var end = (uint*)((byte*)rsdt + rsdt->Length);

            while (p < end)
            {
                var address = *p++;

                ParseDT((AcpiHeader*)address);
            }

            return true;
        }

        private static uint SdtLength = 0;

        private static void ReadHeader(BinaryReader _reader)
        {
            Global.mDebugger.Send("SDT header:");

            //Signature
            Global.mDebugger.Send("\tSignature: " + Encoding.ASCII.GetString(_reader.ReadBytes(4)));

            //Length
            SdtLength = _reader.ReadUInt32();
            Global.mDebugger.Send("\tLendth: " + SdtLength.ToString());

            //Revision
            Global.mDebugger.Send("\tRevision: " + _reader.ReadByte().ToString());

            //Checksum
            Global.mDebugger.Send("\tChecksum: " + _reader.ReadByte().ToString());

            //OEM ID
            Global.mDebugger.Send("\tOEM ID: " + Encoding.ASCII.GetString(_reader.ReadBytes(6)));

            //OEMTableID
            Global.mDebugger.Send("\tOEMTableID: " + Encoding.ASCII.GetString(_reader.ReadBytes(8)));

            //OEMRevision
            Global.mDebugger.Send("\tOEMRevision: " + _reader.ReadUInt32().ToString());

            //OEMRevision
            Global.mDebugger.Send("\tCreatorID: " + _reader.ReadUInt32().ToString());

            //OEMRevision
            Global.mDebugger.Send("\tCreatorRevision: " + _reader.ReadUInt32().ToString());
        }

        private static string ValueToString(object val)
        {
            if (val == null)
                return "null";

            if (val is string)
                return "\"" + val.ToString() + "\"";

            if (val is byte)
                return "0x" + ((byte)val).ToString("X2");

            if (val.GetType().IsArray)
            {
                Array ar = (Array)val;

                string rt = "";

                for (int x = 0; x < ar.Length; x++)
                    rt += ValueToString(ar.GetValue(x)) + (x < ar.Length - 1 ? ", " : string.Empty);

                return rt;
            }

            if (val is ParseNode)
            {
                ParseNode node = (ParseNode)val;

                if (node.ConstantValue != null)
                    return ValueToString(node.ConstantValue);
            }

            return val.ToString();
        }

        private static void PopulateNode(ParseNode op, int depth)
        {
            Global.mDebugger.Send("=========== DEPTH " + depth + " ===========");

            if (!string.IsNullOrEmpty(op.Name))
                Global.mDebugger.Send(op.Name);

            if (op.Op != null)
            {
                Global.mDebugger.Send("OpCode = " + op.Op.ToString());
                Global.mDebugger.Send("Start = " + op.Start.ToString());
                Global.mDebugger.Send("Length = " + op.Length.ToString());
                Global.mDebugger.Send("End = " + op.End.ToString());
                if (op.ConstantValue != null)
                {
                    Global.mDebugger.Send("Value = " + ValueToString(op.ConstantValue));
                }
            }

            if (op.Arguments.Count > 0)
            {
                for (int x = 0; x < op.Op.ParseArgs.Length; x++)
                {
                    if (op.Op.ParseArgs[x] == ParseArgFlags.DataObjectList || op.Op.ParseArgs[x] == ParseArgFlags.TermList || op.Op.ParseArgs[x] == ParseArgFlags.ObjectList)
                        continue;

                    Global.mDebugger.Send(ValueToString(op.Arguments[x]) + " (" + op.Op.ParseArgs[x].ToString() + ")");
                }
            }

            if (op.Nodes.Count > 0)
            {
                foreach (ParseNode ch in op.Nodes)
                {
                    PopulateNode(ch, depth + 1);
                }
            }
        }

        private static void ParseDT(AcpiHeader* hdr)
        {
            var signature = Encoding.ASCII.GetString(hdr->Signature, 4);

            Global.mDebugger.Send(signature + " detected");

            if (signature == "FACP")
            {
                Global.mDebugger.Send("Parse FACP");

                FADT = (FADTPtr*)hdr;

                SMI_CMD = (int*)FADT->SMI_CommandPort;
                ACPI_ENABLE = FADT->AcpiEnable;
                ACPI_DISABLE = FADT->AcpiDisable;
                PM1a_CNT = (int*)FADT->PM1aControlBlock;
                PM1b_CNT = (int*)FADT->PM1bControlBlock;
                PM1_CNT_LEN = FADT->PM1ControlLength;
                SLP_EN = 1 << 13;

                smiIO = new IOPort((ushort)SMI_CMD);
                pm1aIO = new IOPort((ushort)PM1a_CNT);
                pm1bIO = new IOPort((ushort)PM1b_CNT);
                ResetRegister = new IOPort((ushort)FADT->ResetReg.Address);

                if (acpiCheckHeader((byte*)FADT->Dsdt, "DSDT") == 0)
                {
                    uint dsdtAddress = FADT->Dsdt;
                    uint dsdtLength = (uint)(*((int*)FADT->Dsdt + 1) - sizeof(AcpiHeader));

                    var dsdtHeader = new MemoryBlock08(dsdtAddress, 36);
                    var _reader = new BinaryReader(new MemoryStream(dsdtHeader.ToArray()));

                    ReadHeader(_reader);

                    var dsdtBlock = new MemoryBlock08(dsdtAddress, SdtLength);

                    Stream stream = new MemoryStream(dsdtBlock.ToArray());

                    //var root = new Parser(stream).Parse();

                    //PopulateNode(root, 0);
                }
            }
            else if (signature == "APIC")
            {
                Global.mDebugger.Send("Parse APIC");

                MADT = (MADTPtr*)hdr;

                var p = (byte*)(MADT + 1);
                var end = (byte*)MADT + MADT->Header.Length;
                while (p < end)
                {
                    var header = (ApicHeader*)p;
                    var type = header->Type;
                    var length = header->Length;

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
        private static unsafe RSDPtr* RSDPAddress()
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

            var ebda_address = *(uint*)0x040E;
            ebda_address = ebda_address * 0x10 & 0x000fffff;

            for (var addr = ebda_address; addr < ebda_address + 1024; addr += 4)
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
            var p = (byte*)(MADT + 1);
            var end = (byte*)MADT + MADT->Header.Length;

            while (p < end)
            {
                var header = (ApicHeader*)p;
                var type = header->Type;
                var length = header->Length;

                if (type == ApicType.InterruptOverride)
                {
                    var ovr = (ApicInterruptOverride*)p;

                    if (ovr->Source == irq)
                    {
                        Global.mDebugger.Send("IRQ" + irq + " remapped to IRQ" + ovr->Interrupt);

                        return ovr->Interrupt;
                    }
                }

                p += length;
            }

            return irq;
        }
    }
}
