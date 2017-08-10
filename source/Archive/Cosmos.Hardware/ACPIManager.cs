
using Cosmos.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Hardware2
{
    public static class ACPIManager
    {
        public static bool Enabled = false;
        public static bool Found = false;
        public static RSD Rsd;
        public static uint[] ACPITable;

        public static int Version
        {
            get
            {
                if (Found)
                    return Rsd.Revision;
                else
                    return 0;
            }
        }

        [StructLayout(LayoutKind.Explicit)]        public struct RSD
        {
            [FieldOffset(0)]            public int Signature0;
            [FieldOffset(4)]            public int Signature1;

            [FieldOffset(8)]            public byte Checksum;
            
            [FieldOffset(9)]            private byte OemID0;
            [FieldOffset(10)]           private byte OemID1;
            [FieldOffset(11)]           private byte OemID2;
            [FieldOffset(12)]           private byte OemID3;
            [FieldOffset(13)]           private byte OemID4;
            [FieldOffset(14)]           private byte OemID5;
            
            [FieldOffset(15)]           public byte Revision;
            [FieldOffset(16)]           public uint RsdtAddress;   
         
            public string Signature
            {
                get
                {
                    return ""; // Signature0.ToHex(8) + Signature1.ToHex(8);
                }
            }
            public string OemID
            {
                get
                {
                    return "" + (char)OemID0 + (char)OemID1 + (char)OemID2 + (char)OemID3 + (char)OemID4 + (char)OemID5;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]        public struct acpi_gen_regaddr
        {
            [FieldOffset(0)]            public byte space_id;
            [FieldOffset(1)]            public byte bit_width;
            [FieldOffset(2)]            public byte bit_offset;
            [FieldOffset(3)]            public byte resv;
            [FieldOffset(4)]            public uint addrl;
            [FieldOffset(8)]            public uint addrh;
        }

        [StructLayout(LayoutKind.Explicit)]        public struct acpi_table_header
        {
            [FieldOffset(0)]            public byte signature0;
            [FieldOffset(1)]            public byte signature1;
            [FieldOffset(2)]            public byte signature2;
            [FieldOffset(3)]            public byte signature3;

            [FieldOffset(4)]            public uint length;
            [FieldOffset(8)]            public byte revision;
            [FieldOffset(9)]            public byte checksum;

            [FieldOffset(10)]           byte oem_id0;
            [FieldOffset(11)]           byte oem_id1;
            [FieldOffset(12)]           byte oem_id2;
            [FieldOffset(13)]           byte oem_id3;
            [FieldOffset(14)]           byte oem_id4;
            [FieldOffset(15)]           byte oem_id5;

            [FieldOffset(16)]           byte oem_table_id0;
            [FieldOffset(17)]           byte oem_table_id1;
            [FieldOffset(18)]           byte oem_table_id2;
            [FieldOffset(19)]           byte oem_table_id3;
            [FieldOffset(20)]           byte oem_table_id4;
            [FieldOffset(21)]           byte oem_table_id5;
            [FieldOffset(22)]           byte oem_table_id6;
            [FieldOffset(23)]           byte oem_table_id7;

            [FieldOffset(24)]           public uint oem_revision;
            [FieldOffset(28)]           public uint asl_compiler_id;
            [FieldOffset(32)]           public uint asl_compiler_revision;


            public string Signature
            {
                get
                {
                    return "" + (char)signature0 + (char)signature1 + (char)signature2 + (char)signature3;
                }
            }
            public string OemID
            {
                get
                {
                    return "" + (char)oem_id0 + (char)oem_id1 + (char)oem_id2 + (char)oem_id3 + (char)oem_id4 + (char)oem_id5;
                }
            }
            public string OemTable
            {
                get
                {
                    return "" + (char)oem_table_id0 + (char)oem_table_id1 + (char)oem_table_id2 + (char)oem_table_id3 + (char)oem_table_id4 + (char)oem_table_id5 + (char)oem_table_id6 + (char)oem_table_id7;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Fadt
        {
            public acpi_table_header header;
            
            public uint firmware_ctrl;
            public uint dsdt;
            public byte model;
            public byte preferred_pm_profile;
            public ushort sci_int;
            public uint smi_cmd;
            public byte acpi_enable;
            public byte acpi_disable;
            public byte s4bios_req;
            public byte pstate_cnt;
            public uint pm1a_evt_blk;
            public uint pm1b_evt_blk;
            public uint pm1a_cnt_blk;
            public uint pm1b_cnt_blk;
            public uint pm2_cnt_blk;
            public uint pm_tmr_blk;
            public uint gpe0_blk;
            public uint gpe1_blk;
            public byte pm1_evt_len;
            public byte pm1_cnt_len;
            public byte pm2_cnt_len;
            public byte pm_tmr_len;
            public byte gpe0_blk_len;
            public byte gpe1_blk_len;
            public byte gpe1_base;
            public byte cst_cnt;
            public ushort p_lvl2_lat;
            public ushort p_lvl3_lat;
            public ushort flush_size;
            public ushort flush_stride;
            public byte duty_offset;
            public byte duty_width;
            public byte day_alrm;
            public byte mon_alrm;
            public byte century;
            public ushort iapc_boot_arch;
            public byte res2;
            public uint flags;
            public acpi_gen_regaddr reset_reg;
            public byte reset_value;
            public byte res3;
            public byte res4;
            public byte res5;
            public uint x_firmware_ctl_l;
            public uint x_firmware_ctl_h;
            public uint x_dsdt_l;
            public uint x_dsdt_h;
            public acpi_gen_regaddr x_pm1a_evt_blk;
            public acpi_gen_regaddr x_pm1b_evt_blk;
            public acpi_gen_regaddr x_pm1a_cnt_blk;
            public acpi_gen_regaddr x_pm1b_cnt_blk;
            public acpi_gen_regaddr x_pm2_cnt_blk;
            public acpi_gen_regaddr x_pm_tmr_blk;
            public acpi_gen_regaddr x_gpe0_blk;
            public acpi_gen_regaddr x_gpe1_blk;
        }

        public unsafe static void Init()
        {
            Console.Write("Looking for ACPI...");

            uint rsdp = RSDPAddress();
            
            if (rsdp != 0)
            {
                Rsd = *((RSD*)rsdp);


                Found = true;

                //Console.WriteLine("Found Version "  + Rsd.Revision + " (" + Rsd.OemID + ") @ " + rsdp.ToHex(8));

                acpi_table_header* Rsdt = (acpi_table_header*)Rsd.RsdtAddress;
                uint i = Rsdt->length;
                i = (i - 36) / 4;

                Console.WriteLine(i + " entrys found");

                ACPITable = new uint[i];

                uint rsdt = Rsd.RsdtAddress;
                for (int j = 0; j < i; j++)
                {
                    ACPITable[j] = *(uint*)(rsdt + 36 +j*4);
                    acpi_table_header* header = (acpi_table_header*)ACPITable[j];
                    
                        
                }
                Console.Read();

            }
            else
            {
                Console.WriteLine("Not Found");
            }

        }

        public static unsafe uint RSDPAddress()
        {
            Console.Write("searching for acpi...searching BIOS...");
            // check bios
            for (uint addr = 0xE0000; addr < 0x100000; addr += 4)
                if (CheckForRSDP(addr))
                    return addr;

            Console.Write("searching EBD...");

            // check extended bios
            uint ebda_address = *((uint*)0x040E);

            ebda_address = (ebda_address * 0x10) & 0x000fffff;

            for (uint addr = ebda_address; addr < ebda_address + 1024; addr += 4)
                if (CheckForRSDP(addr))
                    return addr;

            // not found
            return 0;
        }

        public static unsafe bool CheckForRSDP(uint addr)
        {
            // check signature
            byte* ch = (byte*)addr;

            if (*(ch++) != (byte)'R') return false;
            if (*(ch++) != (byte)'S') return false;
            if (*(ch++) != (byte)'D') return false;
            if (*(ch++) != (byte)' ') return false;
            if (*(ch++) != (byte)'P') return false;
            if (*(ch++) != (byte)'T') return false;
            if (*(ch++) != (byte)'R') return false;
            if (*(ch++) != (byte)' ') return false;

            // check checksum
            byte sum = 0;
            byte* check = (byte*)addr;

            for (int i = 0; i < 20; i++)
                sum += *(check++);

            return (sum == 0);
        }

        // checks for a given header and validates checksum
        public static unsafe bool acpiCheckHeader(int ptr, string sig)
        {
            byte* b = (byte*)ptr;

            for (int i = 0; i < sig.Length; i++)
                if (*(b++) != (byte)sig[i])
                    return false;

            byte* checkPtr = (byte*)ptr;
            int len = *(&ptr + 1);
            byte check = 0;

            while (0 < len--)
                check += *(checkPtr++);

            return (check == 0);
        }



        public static unsafe bool acpiEnable()
        {
            /*
           // check if acpi is enabled
           if ( (CPUBus.Read16((unsigned int) PM1a_CNT) & SCI_EN) == 0 )
           {
              // check if acpi can be enabled
              if (SMI_CMD != 0 && ACPI_ENABLE != 0)
              {
                 outb((unsigned int) SMI_CMD, ACPI_ENABLE); // send acpi enable command
                 // give 3 seconds time to enable acpi
                 int i;
                 for (i=0; i<300; i++ )
                 {
                    if ( (inw((unsigned int) PM1a_CNT) &SCI_EN) == 1 )
                       break;
                    sleep(10);
                 }
                 if (PM1b_CNT != 0)
                    for (; i<300; i++ )
                    {
                       if ( (inw((unsigned int) PM1b_CNT) &SCI_EN) == 1 )
                          break;
                       sleep(10);
                    }
                 if (i<300) {
                    wrstr("enabled acpi.\n");
                    return 0;
                 } else {
                    wrstr("couldn't enable acpi.\n");
                    return -1;
                 }
              } else {
                 wrstr("no known way to enable acpi.\n");
                 return -1;
              }
           } else {
              //wrstr("acpi was already enabled.\n");
              return 0;
           }*/
            return false;
        }

    }
}