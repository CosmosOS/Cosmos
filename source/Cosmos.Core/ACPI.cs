using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    /// <summary>
    /// ACPI (Advanced Configuration and Power Interface) class.
    /// </summary>
    public unsafe class ACPI
    {
        /// <summary>
        /// MADT table struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MADT
        {
            public fixed byte Signature[4];   // 'APIC'
            public int Length;
            public byte Revision;
            public byte Checksum;
            public fixed byte OemId[6];
            public fixed byte OemTableId[8];
            public int OemRevision;
            public int CreatorId;
            public int CreatorRevision;
            public int LocalApicAddress;
            public int Flags;
        }

        /// <summary>
        /// MADT Entry Heady struct
        /// </summary>

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MADTEntryHeader
        {
            public byte EntryType;
            public byte Length;
        }

        /// <summary>
        /// MADT Entry Type struct
        /// </summary>

        public enum MADTEntryType : byte
        {
            ProcessorLocalApic = 0,
            IoApic = 1,
            InterruptSourceOverride = 2,
            NmiSource = 3,
            LocalApicNmi = 4,
            LocalApicAddressOverride = 5,
            IoSapic = 6,
            LocalSapic = 7,
            PlatformInterruptSources = 8,
            ProcessorLocalX2Apic = 9,
            LocalX2ApicNmi = 10
        }

        /// <summary>
        /// Processor Local Apic Entry struct
        /// </summary>

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProcessorLocalApicEntry
        {
            public MADTEntryHeader Header;
            public byte AcpiProcessorId;
            public byte ApicId;
            public int Flags;
        }

        /// <summary>
        /// IO Apic Entry struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IoApicEntry
        {
            public MADTEntryHeader Header;
            public byte IoApicId;
            public byte Reserved;
            public int IoApicAddress;
            public int GlobalSystemInterruptBase;
        }

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

        // New Port I/O
        /// <summary>
        /// IO port.
        /// </summary>
        private static ushort smiIO, pm1aIO, pm1bIO;

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
        /// FACP.
        /// </summary>
        private static byte* Facp = null;
        /// <summary>
        /// FACP struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FACP
        {
            /// <summary>
            /// Signature.
            /// </summary>
            public fixed byte Signature[4];
            /// <summary>
            /// Length.
            /// </summary>
            public int Length;

            /// <summary>
            /// Unused.
            /// </summary>
            public fixed byte unneded1[40 - 8];
            /// <summary>
            /// DSDT.
            /// </summary>
            public int* DSDT;
            /// <summary>
            /// Unused.
            /// </summary>
            public fixed byte unneded2[48 - 44];
            /// <summary>
            /// SMI CMD.
            /// </summary>
            public int* SMI_CMD;
            /// <summary>
            /// ACPI ENABLE.
            /// </summary>
            public byte ACPI_ENABLE;
            /// <summary>
            /// ACPI DISABLE.
            /// </summary>
            public byte ACPI_DISABLE;
            /// <summary>
            /// Unused.
            /// </summary>
            public fixed byte unneded3[64 - 54];
            /// <summary>
            /// PM1a CNT BLK.
            /// </summary>
            public int* PM1a_CNT_BLK;
            /// <summary>
            /// PM1b CNT BLK.
            /// </summary>
            public int* PM1b_CNT_BLK;
            /// <summary>
            /// Unused.
            /// </summary>
            public fixed byte unneded4[89 - 72];
            /// <summary>
            /// PM1 CNT LEN.
            /// </summary>
            public byte PM1_CNT_LEN;
        };

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
                sum += *check++;
            }

            return sum == 0;
        }
        /// <summary>
        /// Parse the MADT
        /// </summary>

        private static void ParseMADT(byte* madtPtr)
        {
            MADT* madt = (MADT*)madtPtr;

            byte* madtEnd = madtPtr + madt->Length;
            byte* entryPtr = madtPtr + sizeof(MADT);

            while (entryPtr < madtEnd)
            {
                MADTEntryHeader* entryHeader = (MADTEntryHeader*)entryPtr;

                switch ((MADTEntryType)entryHeader->EntryType)
                {
                    case MADTEntryType.ProcessorLocalApic:
                        ProcessorLocalApicEntry* localApic = (ProcessorLocalApicEntry*)entryPtr;
                        if ((localApic->Flags & 1) != 0) // CPU is enabled
                        {
                            Console.WriteLine($"Found CPU with APIC ID: {localApic->ApicId}");
                            // Here you can add this APIC ID to a list or process it as needed.
                        }
                        break;

                    case MADTEntryType.IoApic:
                        IoApicEntry* ioApic = (IoApicEntry*)entryPtr;
                        Console.WriteLine($"Found IO APIC with ID: {ioApic->IoApicId} at address: {ioApic->IoApicAddress:X}");
                        break;

                        // Handle other MADT entries if needed
                }

                entryPtr += entryHeader->Length;
            }
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

            IOPort.Write16(pm1aIO, (ushort)(SLP_TYPa | SLP_EN));

            if (PM1b_CNT != null)
            {
                IOPort.Write16(pm1bIO, (ushort)(SLP_TYPb | SLP_EN));
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
            byte* ptr = (byte*)RSDPAddress();
            int rsdtAddress = *(int*)(ptr + 16);

            if (rsdtAddress == 0)
            {
                return false;
            }

            byte* rsdt = (byte*)rsdtAddress;
            int entryCount = (*(int*)(rsdt + 4) - 36) / 4;

            for (int i = 0; i < entryCount; i++)
            {
                int entryAddress = *(int*)(rsdt + 36 + (i * 4));

                if (Compare("APIC", (byte*)entryAddress) == 0)
                {
                    ParseMADT((byte*)entryAddress);
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Enable ACPI.
        /// </summary>
        public static void Enable()
        {
            smiIO = ACPI_ENABLE;
        }

        /// <summary>
        /// Disable ACPI.
        /// </summary>
        public static void Disable()
        {
            smiIO = ACPI_DISABLE;
        }

        /// <summary>
        /// Get the RSDP address.
        /// </summary>
        /// <returns>uint value.</returns>
        private static unsafe uint RSDPAddress()
        {
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
            {
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                {
                    if (Check_RSD(addr))
                    {
                        return addr;
                    }
                }
            }

            uint ebda_address = *(uint*)0x040E;
            ebda_address = (ebda_address * 0x10) & 0x000fffff;

            for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
            {
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                {
                    return addr;
                }
            }

            return 0;
        }

        /// <summary>
        /// Check RSDT table
        /// </summary>
        /// <param name="ptr">A pointer to the RSDT</param>
        /// <returns>RSDT table address</returns>
        private static uint* acpiCheckRSDPtr(uint* ptr)
        {
            string sig = "RSD PTR ";
            var rsdp = (RSDPtr*)ptr;

            byte* bptr;
            byte check = 0;
            int i;

            if (Compare(sig, (byte*)rsdp) == 0)
            {
                bptr = (byte*)ptr;

                for (i = 0; i < 20; i++)
                {
                    check += *bptr;
                    bptr++;
                }

                if (check == 0)
                {
                    Compare("RSDT", (byte*)rsdp->RsdtAddress);

                    if (rsdp->RsdtAddress != 0)
                    {
                        return (uint*)rsdp->RsdtAddress;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get data from the FACP table.
        /// </summary>
        /// <param name="number">Index number of the data to get.
        /// <list type="bullet">
        /// <item>0 - ACPI ENABLE</item>
        /// <item>1 - ACPI DISABLE</item>
        /// <item>2 - PM1 CNT LEN</item>
        /// <item>other - 0</item>
        /// </list>
        /// </param>
        /// <returns>byte value.</returns>
        private static byte facpbget(int number)
        {
            switch (number)
            {
                case 0:
                    return *(Facp + 52);
                case 1:
                    return *(Facp + 53);
                case 2:
                    return *(Facp + 89);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Get pointer to the data on the FACP.
        /// </summary>
        /// <param name="number">Index number of the data to get.
        /// <list type="bullet">
        /// <item>0 - DSDT</item>
        /// <item>1 - SMI CMD</item>
        /// <item>2 - PM1a</item>
        /// <item>3 - PM1b</item>
        /// <item>other - null</item>
        /// </list>
        /// </param>
        /// <returns>int pointer.</returns>
        private static int* facpget(int number)
        {
            switch (number)
            {
                case 0:
                    return (int*)*(int*)(Facp + 40);
                case 1:
                    return (int*)*(int*)(Facp + 48);
                case 2:
                    return (int*)*(int*)(Facp + 64);
                case 3:
                    return (int*)*(int*)(Facp + 68);
                default:
                    return null;
            }
        }
    }
}
