using Cosmos.Debug.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cosmos.Core
{
    /// <summary>
    /// PCI IRQ Routing information.
    /// </summary>
    public class IrqRouting
    {
        /// <summary>
        /// The address of the PCI device.
        /// </summary>
        public uint Address;

        /// <summary>
        /// The PCI pin number of the device.
        /// </summary>
        public byte Pin;

        /// <summary>
        /// Source.
        /// </summary>
        public byte Source;

        /// <summary>
        /// Source Index.
        /// </summary>
        public byte SourceIndex;
    }

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

        public static uint DSDTLenght = 0;

        /// <summary>
        /// PCI IRQ Routing Table.
        /// </summary>
        public static List<IrqRouting> IrqRoutingTable;

        /// <summary>
        /// PCI IRQ Routing Table.
        /// </summary>
        public static List<byte> LocalApicCpus;

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

            IOPort.Write16((ushort)PM1a_CNT, (ushort)(SLP_TYPa | SLP_EN));

            if (PM1b_CNT != null)
            {
                IOPort.Write16((ushort)PM1b_CNT, (ushort)(SLP_TYPb | SLP_EN));
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
                IOPort.Write16((ushort)FADT->ResetReg.Address, ResetValue);
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
            IrqRoutingTable = new List<IrqRouting>();
            LocalApicCpus = new List<byte>();

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

            if (LocalApicCpus.Count > 0)
            {
                Global.mDebugger.Send("Found " + LocalApicCpus.Count + " CPUs via MADT.");
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

        private static void ParseS5()
        {
            byte* S5Addr = (byte*)FADT->Dsdt;

            while (0 < DSDTLenght--)
            {
                if (Compare("_S5_", S5Addr) == 0)
                {
                    break;
                }
                S5Addr++;
            }

            if (DSDTLenght > 0)
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

                    Global.mDebugger.Send("SLP_TYPa=" + SLP_TYPa);
                    Global.mDebugger.Send("SLP_TYPb=" + SLP_TYPb);
                }
            }
        }

        private static void ParsePRT()
        {
            /*
            if (DSDTLenght > 0)
            {
                var dsdtBlock = new MemoryBlock08(FADT->Dsdt + (uint)sizeof(AcpiHeader), SdtLength - (uint)sizeof(AcpiHeader));

                Stream stream = new MemoryStream(dsdtBlock.ToArray());

                Global.mDebugger.Send("Create parser...");

                var root = new Parser(stream);

                Global.mDebugger.Send("Parse first node...");

                var node = root.Parse();
                foreach (var item in node.Nodes)
                {
                    Global.mDebugger.Send("Node: " + item.Name);
                }
            }*/
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


                if (acpiCheckHeader((byte*)FADT->Dsdt, "DSDT") == 0)
                {
                    uint dsdtAddress = FADT->Dsdt;
                    uint dsdtLength = (uint)(*((int*)FADT->Dsdt + 1) - sizeof(AcpiHeader));

                    var dsdtHeader = new MemoryBlock08(dsdtAddress, 36);
                    var _reader = new BinaryReader(new MemoryStream(dsdtHeader.ToArray()));

                    ReadHeader(_reader);

                    Global.mDebugger.Send("Parsing _S5...");

                    ParseS5();

                    Global.mDebugger.Send("Parsing _PRT...");

                    ParsePRT();
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
                        var pic = (ApicLocalApic*)p;

                        if (((pic->Flags & 1) ^ ((pic->Flags >> 1) & 1)) != 0)
                        {
                            LocalApicCpus.Add(pic->ApicId);

                            Global.mDebugger.Send("Found APIC " + (ulong)pic->ApicId + " (Processor ID:" + pic->AcpiProcessorId + ")");
                        }
                    }
                    else if (type == ApicType.IOAPIC)
                    {
                        var ioapic = (ApicIOApic*)p;
                        if (IOAPIC == null)
                        {
                            IOAPIC = ioapic;
                        }
                        Global.mDebugger.Send("Found IO APIC " + (ulong)ioapic->IOApicId + " (Address:0x" + ((ulong)ioapic->IOApicAddress).ToString("X") + ", GSIB:" + (ulong)ioapic->GlobalSystemInterruptBase + ")");
                    }
                    else if (type == ApicType.InterruptOverride)
                    {
                        var ovr = (ApicInterruptOverride*)p;

                        Global.mDebugger.Send("Found APIC Interrupt Override (Bus: " + ((ulong)ovr->Bus).ToString() + ", Source:" + ((ulong)ovr->Source).ToString() + ", Interrupt:0x" + ((ulong)ovr->Interrupt).ToString("X") + ", Flags:" + ((ulong)ovr->Flags).ToString() + ")");
                    }

                    p += length;
                }
            }
        }

        /*
        private static void PopulateNode(ParseNode op)
        {
            //Recursive function does a null reference exception trick the matrice with a Stack and iterative function
            var sthack = new Stack<ParseNode>();

            sthack.Push(op);

            while (sthack.Count != 0)
            {
                ParseNode current = sthack.Pop();

                if (current.Arguments.Count > 0)
                {
                    SearchPackage(current);
                }

                if (current != null)
                {
                    for (int i = current.Nodes.Count - 1; i >= 0; i--)
                    {
                        sthack.Push(current.Nodes[i]);
                    }
                }
            }
        }

        
        private static void SearchPackage(ParseNode op)
        {
            for (int x = 0; x < op.Op.ParseArgs.Length; x++)
            {
                if (op.Op.ParseArgs[x] == ParseArgFlags.DataObjectList || op.Op.ParseArgs[x] == ParseArgFlags.TermList || op.Op.ParseArgs[x] == ParseArgFlags.ObjectList)
                    continue;

                if (op.Arguments[x].ToString() == "Package")
                {
                    Global.mDebugger.Send("Package found!");

                    //var arg = (ParseNode)op.Arguments[x];

                    /*
                    for (int y = 0; y < arg.Nodes.Count; y++)
                    {
                        List<ParseNode> package = arg.Nodes[y].Nodes;

                        var irqRouting = new IrqRouting()
                        {
                            Address = (uint)package[0].ConstantValue,
                            Pin = (byte)package[1].ConstantValue,
                            Source = (byte)package[2].ConstantValue,
                            SourceIndex = (byte)package[3].ConstantValue
                        };

                        IrqRoutingTable.Add(irqRouting);
                    }
                    
                }
            }
        }*/

        /// <summary>
        /// Enable ACPI.
        /// </summary>
        public static void Enable()
        {
        }

        /// <summary>
        /// Disable ACPI.
        /// </summary>
        public static void Disable()
        {
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
