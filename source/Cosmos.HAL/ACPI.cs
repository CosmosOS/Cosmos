using Cosmos.Core;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.HAL
{
        public unsafe class ACPI
        {
            private static int* SMI_CMD;
            private static byte ACPI_ENABLE;
            private static byte ACPI_DISABLE;
            private static int* PM1a_CNT;
            private static int* PM1b_CNT;
            private static short SLP_TYPa;
            private static short SLP_TYPb;
            private static short SLP_EN;
            private static short SCI_EN;
            private static byte PM1_CNT_LEN;


            static int Compare(string c1, byte* c2)
            {

                for (int i = 0; i < c1.Length; i++)
                {
                    if (c1[i] != (char)c2[i]) { return -1; }
                }
                return 0;
            }
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct RSDPtr
            {
                public fixed byte Signature[8];
                public byte CheckSum;
                public fixed byte OemID[6];
                public byte Revision;
                public int RsdtAddress;
            };


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
            static byte* Facp = null;
            static int* facpget(int number)
            {

                if (number == 0) { return (int*)*((int*)(Facp + 40)); }
                else if (number == 1) { return (int*)*((int*)(Facp + 48)); }
                else if (number == 2) { return (int*)*((int*)(Facp + 64)); }
                else if (number == 3) { return (int*)*((int*)(Facp + 68)); }
                else { return null; }

            }
            static byte facpbget(int number)
            {
                if (number == 0) { return *(Facp + 52); }
                else if (number == 1) { return *(Facp + 53); }
                else if (number == 2) { return *(Facp + 89); }
                else return 0;
            }
            // check if the given address has a valid header
            static uint* acpiCheckRSDPtr(uint* ptr)
            {
                string sig = "RSD PTR ";
                RSDPtr* rsdp = (RSDPtr*)ptr;
                byte* bptr;
                byte check = 0;
                int i;

                if (Compare(sig, (byte*)rsdp) == 0)
                {
                    // check checksum rsdpd
                    bptr = (byte*)ptr;
                    for (i = 0; i < 20; i++)
                    {
                        check += *bptr;
                        bptr++;
                    }
                    //Console.WriteLine("0x" + check.ToHex());
                    // found valid rsdpd   
                    if (check == 0)
                    {
                        //string str = rsdp->RsdtAddress.ToHex();
                        //Console.WriteLine("seen"); Console.WriteLine(str);
                        Compare("RSDT", (byte*)rsdp->RsdtAddress);
                        if (rsdp->RsdtAddress != 0)
                            return (uint*)rsdp->RsdtAddress;
                    }
                }
                Console.WriteLine("Unable to find RSDT. ACPI not available");
                return null;
            }

            static unsafe uint RSDPAddress()
            {

                // check bios
                for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
                    if (Compare("RSD PTR ", (byte*)addr) == 0)
                        if (Check_RSD(addr))
                            return addr;
                // check extended bios
                uint ebda_address = *((uint*)0x040E);

                ebda_address = (ebda_address * 0x10) & 0x000fffff;

                for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
                    if (Compare("RSD PTR ", (byte*)addr) == 0)
                        return addr;

                // not found
                return 0;
            }
            // checks for a given header and validates checksum
            static int acpiCheckHeader(byte* ptr, string sig)
            {
                return Compare(sig, ptr);
            }

            static bool Check_RSD(uint address)
            {
                byte sum = 0;
                byte* check = (byte*)address;

                for (int i = 0; i < 20; i++)
                    sum += *(check++);

                return (sum == 0);
            }
            private static Cosmos.Core.IOPort smiIO, pm1aIO, pm1bIO;
            public static int Enable()
            {
                // check if acpi is enabled

                if (pm1aIO.Word == 0)
                {
                    // check if acpi can be enabled
                    if (SMI_CMD != null && ACPI_ENABLE != 0)
                    {
                        smiIO.Byte = ACPI_ENABLE;
                        //CPUBus.Write8((byte)SMI_CMD, ACPI_ENABLE); // send acpi enable command
                        // give 3 seconds time to enable acpi
                        int i;
                        for (i = 0; i < 300; i++)
                        {
                            if ((pm1aIO.Word & 1) == 1)
                                //if ((CPUBus.Read16((ushort)PM1a_CNT) & SCI_EN) == 1)
                                break;

                        }
                        if (PM1b_CNT != null)
                            for (; i < 300; i++)
                            {
                                //if ((CPUBus.Read16((ushort)PM1b_CNT) & SCI_EN) == 1)
                                if ((pm1bIO.Word & 1) == 1)
                                    break;
                            }
                        if (i < 300)
                        {

                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("Couldn't enable ACPI.");
                            return -1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No known way to enable ACPI.");
                        return -1;
                    }
                }
                else
                {

                    return 0;
                }
            }
            public static void Disable()
            {
                smiIO.Byte = ACPI_DISABLE;
            }
            public static int Init()
            {
                byte* ptr = (byte*)RSDPAddress(); int addr = 0;

                for (int i = 19; i >= 16; i--)
                {
                    addr += (*((byte*)ptr + i));
                    addr = (i == 16) ? addr : addr << 8;
                }

                ptr = (byte*)addr;
                ptr += 4; addr = 0;
                for (int i = 3; i >= 0; i--)
                {
                    addr += (*((byte*)ptr + i));
                    addr = (i == 0) ? addr : addr << 8;
                }
                int length = addr;

                ptr -= 4;
                // check if address is correct  ( if acpi is available on this pc )
                if (ptr != null && acpiCheckHeader((byte*)ptr, "RSDT") == 0)
                {
                    addr = 0;
                    // the RSDT contains an unknown number of pointers to acpi tables

                    int entrys = length;
                    entrys = (entrys - 36) / 4;
                    ptr += 36;   // skip header information
                    byte* yeuse;

                    while (0 < entrys--)
                    {
                        for (int i = 3; i >= 0; i--)
                        {
                            addr += (*((byte*)ptr + i));
                            addr = (i == 0) ? addr : addr << 8;
                        }
                        yeuse = (byte*)addr;
                        // check if the desired table is reached
                        Facp = (byte*)yeuse;
                        if (Compare("FACP", Facp) == 0)
                        {
                            if (acpiCheckHeader((byte*)facpget(0), "DSDT") == 0)
                            {
                                // search the \_S5 package in the DSDT
                                byte* S5Addr = (byte*)facpget(0) + 36; // skip header
                                int dsdtLength = *(facpget(0) + 1) - 36;
                                while (0 < dsdtLength--)
                                {
                                    if (Compare("_S5_", (byte*)S5Addr) == 0)
                                        break;
                                    S5Addr++;
                                }
                                // check if \_S5 was found
                                if (dsdtLength > 0)
                                {
                                    // check for valid AML structure
                                    if ((*(S5Addr - 1) == 0x08 || (*(S5Addr - 2) == 0x08 && *(S5Addr - 1) == '\\')) && *(S5Addr + 4) == 0x12)
                                    {
                                        S5Addr += 5;
                                        S5Addr += ((*S5Addr & 0xC0) >> 6) + 2;   // calculate PkgLength size

                                        if (*S5Addr == 0x0A)
                                            S5Addr++;   // skip byteprefix
                                        SLP_TYPa = (short)(*(S5Addr) << 10);
                                        S5Addr++;

                                        if (*S5Addr == 0x0A)
                                            S5Addr++;   // skip byteprefix
                                        SLP_TYPb = (short)(*(S5Addr) << 10);

                                        SMI_CMD = facpget(1);

                                        ACPI_ENABLE = facpbget(0);
                                        ACPI_DISABLE = facpbget(1);

                                        PM1a_CNT = facpget(2);
                                        PM1b_CNT = facpget(3);

                                        PM1_CNT_LEN = facpbget(3);

                                        SLP_EN = 1 << 13;
                                        SCI_EN = 1;
                                        smiIO = new IOPort((ushort)SMI_CMD);
                                        pm1aIO = new IOPort((ushort)PM1a_CNT);
                                        pm1bIO = new IOPort((ushort)PM1b_CNT);
                                        return 0;
                                    }
                                    else
                                    {
                                        Console.WriteLine("\\_S5 parse error.\n");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("\\_S5 not present.\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine("DSDT invalid.\n");
                            }
                        }
                        ptr += 4;
                    }
                    Console.WriteLine("No valid FACP present.\n");
                }
                else
                {
                    Console.WriteLine("No ACPI.\n");
                }

                return -1;

            }

            public static void Reboot()
            {
                Core.CPU.DisableInterrupts();
                byte good = 0x02;
                while ((good & 0x02) != 0)
                    good = Inb(0x64);
                Outb(0x64, 0xFE);

                Cosmos.Core.Global.CPU.Halt();
            }

            static Cosmos.Core.IOPort io = new Cosmos.Core.IOPort(0);
            static int PP = 0, D = 0;
            public static void Outb(ushort port, byte data)
            {
                if (io.Port != port)
                    io = new Cosmos.Core.IOPort(port);
                io.Byte = data;
                PP = port;
                D = data;

            }
            public static byte Inb(ushort port)
            {
                if (io.Port != port)
                    io = new Cosmos.Core.IOPort(port);
                return io.Byte;

            }

            public static void Shutdown()
            {
                // send the shutdown command
                Console.Clear();
                if (PM1a_CNT == null) Init();
                if (pm1aIO != null)
                {
                    pm1aIO.Word = (ushort)(SLP_TYPa | SLP_EN);
                    if (PM1b_CNT != null)
                        pm1bIO.Word = (ushort)(SLP_TYPb | SLP_EN);

                }
                //  Console.WriteLine("It is now safe to turn off your computer");
            }
        }
}
