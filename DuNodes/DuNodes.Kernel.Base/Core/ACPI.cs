/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Runtime.InteropServices;
using Cosmos.Core;

namespace DuNodes.Kernel.Base.Core
{
    // You'll have so much fun while reading that code xP
    public static unsafe class ACPI
    {
        internal static int* SMI_CMD;
        internal static byte ACPI_ENABLE;
        internal static byte ACPI_DISABLE;
        internal static int* PM1a_CNT;
        internal static int* PM1b_CNT;
        internal static short SLP_TYPa;
        internal static short SLP_TYPb;
        internal static short SLP_EN;
        internal static short SCI_EN;
        internal static byte PM1_CNT_LEN;
        internal static int Compare(string c1, byte* c2)
        {

            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != (char)c2[i]) { return -1; }
            }
            return 0;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct RSDPtr
        {
            internal fixed byte Signature[8];
            internal byte CheckSum;
            internal fixed byte OemID[6];
            internal byte Revision;
            internal int RsdtAddress;
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct FACP
        {
            internal fixed byte Signature[4];
            internal int Length;
            internal fixed byte unneded1[40 - 8];
            internal int* DSDT;
            internal fixed byte unneded2[48 - 44];
            internal int* SMI_CMD;
            internal byte ACPI_ENABLE;
            internal byte ACPI_DISABLE;
            internal fixed byte unneded3[64 - 54];
            internal int* PM1a_CNT_BLK;
            internal int* PM1b_CNT_BLK;
            internal fixed byte unneded4[89 - 72];
            internal byte PM1_CNT_LEN;
        };
        internal static byte* Facp = null;
        internal static int* facpget(int number)
        {

            if (number == 0) { return (int*)*((int*)(Facp + 40)); }
            else if (number == 1) { return (int*)*((int*)(Facp + 48)); }
            else if (number == 2) { return (int*)*((int*)(Facp + 64)); }
            else if (number == 3) { return (int*)*((int*)(Facp + 68)); }
            else { return null; }
        }
        internal static byte facpbget(int number)
        {
            if (number == 0) { return *(Facp + 52); }
            else if (number == 1) { return *(Facp + 53); }
            else if (number == 2) { return *(Facp + 89); }
            else return 0;
        }
        internal static uint* acpiCheckRSDPtr(uint* ptr)
        {
            string sig = "RSD PTR ";
            RSDPtr* rsdp = (RSDPtr*)ptr;
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
                        return (uint*)rsdp->RsdtAddress;
                }
            }
            return null;
        }
        internal static unsafe uint RSDPAddress()
        {
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                    if (Check_RSD(addr))
                        return addr;
            uint ebda_address = *((uint*)0x040E);
            ebda_address = (ebda_address * 0x10) & 0x000fffff;
            for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
                if (Compare("RSD PTR ", (byte*)addr) == 0)
                    return addr;
            return 0;
        }
        internal static int acpiCheckHeader(byte* ptr, string sig)
        {
            return Compare(sig, ptr);
        }
        internal static bool Check_RSD(uint address)
        {
            byte sum = 0;
            byte* check = (byte*)address;
            for (int i = 0; i < 20; i++)
                sum += *(check++);
            return (sum == 0);
        }
        internal static Cosmos.Core.IOPort smiIO, pm1aIO, pm1bIO;
        internal static bool Enable()
        {
            if (pm1aIO.Word == 0)
            {
                if (SMI_CMD != null && ACPI_ENABLE != 0)
                {
                    smiIO.Byte = ACPI_ENABLE;
                    int i;
                    for (i = 0; i < 300; i++)
                    {
                        if ((pm1aIO.Word & 1) == 1)
                            break;
                    }
                    if (PM1b_CNT != null)
                        for (; i < 300; i++)
                        {
                            if ((pm1bIO.Word & 1) == 1)
                                break;
                        }
                    if (i < 300) return true;
                    else return false;
                }
                else return false;
            }
            else return true;
        }
        internal static void Disable() { smiIO.Byte = ACPI_DISABLE; }
        internal static bool Init()
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
            if (ptr != null && acpiCheckHeader((byte*)ptr, "RSDT") == 0)
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
                        addr += (*((byte*)ptr + i));
                        addr = (i == 0) ? addr : addr << 8;
                    }
                    yeuse = (byte*)addr;
                    Facp = (byte*)yeuse;
                    if (Compare("FACP", Facp) == 0)
                    {
                        if (acpiCheckHeader((byte*)facpget(0), "DSDT") == 0)
                        {
                            byte* S5Addr = (byte*)facpget(0) + 36;
                            int dsdtLength = *(facpget(0) + 1) - 36;
                            while (0 < dsdtLength--)
                            {
                                if (Compare("_S5_", (byte*)S5Addr) == 0)
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
                                    return true;
                                }
                            }
                        }
                    }
                    ptr += 4;
                }
            }
            return false;
        }
        /// <summary>
        /// Shutdown
        /// </summary>
        public static void Shutdown()
        {
            Console.Console.Clear();
            if (PM1a_CNT == null) Init();
            if (pm1aIO != null)
            {
                pm1aIO.Word = (ushort)(SLP_TYPa | SLP_EN);
                if (PM1b_CNT != null)
                    pm1bIO.Word = (ushort)(SLP_TYPb | SLP_EN);
            }
            Console.Console.WriteLine("Its now safe to turn off the computer.");
        }
        /// <summary>
        /// Reboot
        /// </summary>
        public static void Reboot()
        {
            byte good = 0x02;
            while ((good & 0x02) != 0)
                good = IO.PortIO.inb(0x64);
            IO.PortIO.outb(0x64, 0xFE);
            Cosmos.Core.Global.CPU.Halt();
        }
    }
}
