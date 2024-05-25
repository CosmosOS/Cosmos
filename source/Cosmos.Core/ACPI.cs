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
        public struct RSDPtr
        {
            public fixed byte Signature[8];
            public byte CheckSum;
            public fixed byte OemID[6];
            public byte Revision;
            public uint RsdtAddress;
        };

        /// <summary>
        /// RSDT table struct.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RSDT
        {
            public fixed byte Signature[4];
            public uint Length;
            public byte Revision;
            public byte Checksum;
            public fixed byte OemID[6];
            public fixed byte OemTableID[8];
            public uint OemRevision;
            public uint CreatorID;
            public uint CreatorRevision;
            public fixed uint Entry[1]; // Placeholder for the start of the array of pointers to other tables
        }

        // New Port I/O
        private static ushort smiIO, pm1aIO, pm1bIO;

        // ACPI variables
        private static int* SMI_CMD;
        private static byte ACPI_ENABLE;
        private static byte ACPI_DISABLE;
        private static int* PM1a_CNT;
        private static int* PM1b_CNT;
        private static short SLP_TYPa;
        private static short SLP_TYPb;
        private static short SLP_EN;
        private static byte PM1_CNT_LEN;

        private static byte* Facp = null;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FACP
        {
            public fixed byte Signature[4];
            public int Length;
            public fixed byte unneded1[40 - 8];
            public int* DSDT;
            public fixed byte unneded2[48 - 44];
            public int* SMI_CMD;
            public byte ACPI_ENABLE;
            public byte ACPI_DISABLE;
            public fixed byte unneded3[64 - 54];
            public int* PM1a_CNT_BLK;
            public int* PM1b_CNT_BLK;
            public fixed byte unneded4[89 - 72];
            public byte PM1_CNT_LEN;
        };

        static int Compare(string c1, byte* c2)
        {
            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i]) { return -1; }
            }
            return 0;
        }

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

        public static void Reboot()
        {
            throw new NotImplementedException("ACPI Reset not implemented yet.");
        }

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

        public static void Enable()
        {
            smiIO = ACPI_ENABLE;
        }

        public static void Disable()
        {
            smiIO = ACPI_DISABLE;
        }

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

        public static RSDT* FindRSDT()
        {
            uint rsdpAddress = RSDPAddress();
            if (rsdpAddress == 0)
            {
                return null;
            }

            RSDPtr* rsdp = (RSDPtr*)rsdpAddress;
            uint rsdtAddress = rsdp->RsdtAddress;

            if (rsdtAddress == 0)
            {
                return null;
            }

            return (RSDT*)rsdtAddress;
        }
    }
}
