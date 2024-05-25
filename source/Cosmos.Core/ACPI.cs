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
        public struct DescriptionHeader
        {
            public fixed byte Signature[4];
            public uint Length;
            public byte Revision;
            public byte Checksum;
            public fixed byte OEMID[6];
            public fixed byte OEMTableID[8];
            public uint OEMRevision;
            public uint CreatorID;
            public uint CreatorRevision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct APICHeader
        {
            public byte Type;
            public byte Length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LocalAPIC
        {
            public byte Type;
            public byte Length;
            public byte ProcessorID;
            public byte APICID;
            public uint Flags;
        }

        /// <summary>
        /// New Port I/O
        /// </summary>
        private static ushort smiIO, pm1aIO, pm1bIO;

        /// <summary>
        /// ACPI variables
        /// </summary>
        private static int* SMI_CMD;
        private static byte ACPI_ENABLE;
        private static byte ACPI_DISABLE;
        private static int* PM1a_CNT;
        private static int* PM1b_CNT;
        private static short SLP_TYPa;
        private static short SLP_TYPb;
        private static short SLP_EN;
        private static byte PM1_CNT_LEN;

        /// <summary>
        /// FACP
        /// </summary>
        private static byte* Facp = null;

        /// <summary>
        /// Check ACPI header
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        static int acpiCheckHeader(byte* ptr, string sig)
        {
            return Compare(sig, ptr);
        }

        /// <summary>
        /// Compare string to byte array
        /// </summary>
        /// <param name="c1">String</param>
        /// <param name="c2">Pointer to the head of the byte array</param>
        /// <returns>0 - identical, -1 different</returns>
        static int Compare(string c1, byte* c2)
        {
            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i]) { return -1; }
            }
            return 0;
        }

        /// <summary>
        /// Check RSD checksum
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <returns>True if RSDT table checksum is good</returns>
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
        /// Start the ACPI
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
        /// Shutdown the ACPI
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown on IO error</exception>
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
        /// Reboot ACPI
        /// Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public static void Reboot()
        {
            throw new NotImplementedException("ACPI Reset not implemented yet."); //TODO
        }

        /// <summary>
        /// Initialize the ACPI
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        private static bool Init()
        {
            byte* ptr = (byte*)RSDPAddress();
            int addr = 0;

            for (int i = 19; i >= 16; i--)
            {
                addr += *(ptr + i);
                addr = i == 16 ? addr : addr << 8;
            }

            ptr = (byte*)addr;
            ptr += 4; addr = 0;

            for (int i = 3; i >= 0; i--)
            {
                addr += *(ptr + i);
                addr = i == 0 ? addr : addr << 8;
            }

            int length = addr;
            ptr -= 4;

            if (ptr != null && acpiCheckHeader(ptr, "RSDT") == 0)
            {
                addr = 0;
                int entrys = length;
                entrys = (entrys - 36) / 4;
                ptr += 36;
                byte* yeuse;

                while (0 < entrys--)
                {
                    for (int i = 3; i >= 0; i--)
                    {
                        addr += *(ptr + i);
                        addr = i == 0 ? addr : addr << 8;
                    }

                    yeuse = (byte*)addr;
                    Facp = yeuse;

                    if (acpiCheckHeader((byte*)facpget(0), "DSDT") == 0)
                    {
                        byte* S5Addr = (byte*)facpget(0) + 36;
                        int dsdtLength = *(facpget(0) + 1) - 36;

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
                                SLP_TYPa = (short)(*S5Addr << 10);
                                S5Addr++;
                                if (*S5Addr == 0x0A)
                                {
                                    S5Addr++;
                                }
                                SLP_TYPb = (short)(*S5Addr << 10);
                                SMI_CMD = facpget(1);
                                ACPI_ENABLE = facpbget(0);
                                ACPI_DISABLE = facpbget(1);
                                PM1a_CNT = facpget(2);
                                PM1b_CNT = facpget(3);
                                PM1_CNT_LEN = facpbget(3);
                                SLP_EN = 1 << 13;

                                smiIO = (ushort)SMI_CMD;
                                pm1aIO = (ushort)PM1a_CNT;
                                pm1bIO = (ushort)PM1b_CNT;

                                return true;
                            }
                        }
                    }
                    ptr += 4;
                }
            }

            return false;
        }

        /// <summary>
        /// Enable ACPI
        /// </summary>
        public static void Enable()
        {
            smiIO = ACPI_ENABLE;
        }

        /// <summary>
        /// Disable ACPI
        /// </summary>
        public static void Disable()
        {
            smiIO = ACPI_DISABLE;
        }

        /// <summary>
        /// Get the RSDP address
        /// </summary>
        /// <returns>uint value</returns>
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
        /// Get data from the FACP table
        /// </summary>
        /// <param name="number">Index number of the data to get
        /// <list type="bullet">
        /// <item>0 - ACPI ENABLE</item>
        /// <item>1 - ACPI DISABLE</item>
        /// <item>2 - PM1 CNT LEN</item>
        /// <item>other - 0</item>
        /// </list>
        /// </param>
        /// <returns>byte value</returns>
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
        /// Get pointer to the data on the FACP
        /// </summary>
        /// <param name="number">Index number of the data to get
        /// <list type="bullet">
        /// <item>0 - DSDT</item>
        /// <item>1 - SMI CMD</item>
        /// <item>2 - PM1a</item>
        /// <item>3 - PM1b</item>
        /// <item>other - null</item>
        /// </list>
        /// </param>
        /// <returns>int pointer</returns>
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

        /// <summary>
        /// Get the address of the RSDT
        /// </summary>
        /// <returns>Address of the RSDT or 0 if not found</returns>
        public static uint GetRSDTAddress()
        {
            uint rsdpAddr = RSDPAddress();
            if (rsdpAddr == 0)
            {
                return 0;
            }

            var rsdp = (RSDPtr*)rsdpAddr;
            return (uint)rsdp->RsdtAddress;
        }

        /// <summary>
        /// Get the contents of the RSDT
        /// </summary>
        /// <returns>Pointer to the RSDT contents or null if not found</returns>
        public static byte* GetRSDTContents()
        {
            uint rsdtAddr = GetRSDTAddress();
            if (rsdtAddr == 0)
            {
                return null;
            }

            return (byte*)rsdtAddr;
        }

        /// <summary>
        /// Count the number of local APIC entries in the APIC table
        /// </summary>
        /// <returns>Number of local APIC entries</returns>
        public static int CountLocalAPICEntries()
        {
            byte* rsdtContents = GetRSDTContents();
            if (rsdtContents == null)
            {
                return 0;
            }

            var rsdtHeader = (DescriptionHeader*)rsdtContents;
            int entriesCount = (rsdtHeader->Length - sizeof(DescriptionHeader)) / 4;
            uint* tableEntries = (uint*)(rsdtContents + sizeof(DescriptionHeader));

            for (int i = 0; i < entriesCount; i++)
            {
                byte* tablePtr = (byte*)tableEntries[i];
                if (Compare("APIC", tablePtr) == 0)
                {
                    return CountAPICEntries(tablePtr);
                }
            }

            return 0;
        }

        /// <summary>
        /// Count the number of APIC entries
        /// </summary>
        /// <param name="apicTable">Pointer to the APIC table</param>
        /// <returns>Number of APIC entries</returns>
        private static int CountAPICEntries(byte* apicTable)
        {
            var apicHeader = (DescriptionHeader*)apicTable;
            int length = (int)apicHeader->Length;
            int count = 0;
            byte* ptr = apicTable + sizeof(DescriptionHeader);

            while (ptr < apicTable + length)
            {
                var header = (APICHeader*)ptr;
                if (header->Type == 0) // Type 0 indicates a Processor Local APIC
                {
                    count++;
                }
                ptr += header->Length;
            }

            return count;
        }
    }
}
