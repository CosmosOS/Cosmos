using Cosmos.Core;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    // Must be static, memory management requires this
    // Note we cannot allocate anything here, must wrap it very simply
    public static class MultiBoot
    {
        [StructLayout(LayoutKind.Explicit, Size = 88)]
        public struct Header
        {
            public const uint FLAGS_MEM = 0x00000001;
            public const uint FLAGS_BOOT = 0x00000002;
            public const uint FLAGS_CMDLINE = 0x00000004;
            public const uint FLAGS_MODULES = 0x00000008;
            public const uint FLAGS_AOUT = 0x00000010;
            public const uint FLAGS_ELF = 0x00000020;
            public const uint FLAGS_MMAP = 0x00000040;
            public const uint FLAGS_DRIVES = 0x00000080;
            public const uint FLAGS_CONFIG = 0x00000100;
            public const uint FLAGS_LOADER = 0x00000200;
            public const uint FLAGS_APM = 0x00000400;
            public const uint FLAGS_VBE = 0x00000800;

            [FieldOffset(0)]
            public uint flags;
            [FieldOffset(4)]
            public uint mem_lower; // Valid if (flags & 0x00000001)
            [FieldOffset(8)]
            public uint mem_upper; // Valid if (flags & 0x00000001)
            [FieldOffset(12)]
            public uint boot_device; // Valid if (flags & 0x00000002)
            [FieldOffset(16)]
            public uint cmdline; // Valid if (flags & 0x00000004)
            [FieldOffset(20)]
            public uint mods_count; // Valid if (flags & 0x00000008)
            [FieldOffset(24)]
            public uint mods_addr; // Valid if (flags & 0x00000008)
            [FieldOffset(28)]
            public AOUT aout; // Valid if (flags & 0x00000010)
            [FieldOffset(28)]
            public ELF elf; // Valid if (flags & 0x00000020)
            [FieldOffset(44)]
            public uint mmap_length; // Valid if (flags & 0x00000040)
            [FieldOffset(48)]
            public uint mmap_address; // Valid if (flags & 0x00000040)
            [FieldOffset(52)]
            public uint drives_length; // Valid if (flags & 0x00000080)
            [FieldOffset(56)]
            public uint drives_address; // Valid if (flags & 0x00000080)
            [FieldOffset(60)]
            public uint config_table; // Valid if (flags & 0x00000100)
            [FieldOffset(64)]
            public uint boot_loader_name; // Valid if (flags & 0x00000200)
            [FieldOffset(68)]
            public uint apm_table; // Valid if (flags & 0x00000400)
            [FieldOffset(72)]
            public uint vbe_control_info; // Valid if (flags & 0x00000800)
            [FieldOffset(76)]
            public uint vbe_mode_info; // Valid if (flags & 0x00000800)
            [FieldOffset(80)]
            public ushort vbe_mode; // Valid if (flags & 0x00000800)
            [FieldOffset(82)]
            public ushort vbe_interface_seg; // Valid if (flags & 0x00000800)
            [FieldOffset(84)]
            public ushort vbe_interface_off; // Valid if (flags & 0x00000800)
            [FieldOffset(86)]
            public ushort vbe_interface_len; // Valid if (flags & 0x00000800)
        }
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct Module
        {
            [FieldOffset(0)]
            public uint mod_start;
            [FieldOffset(4)]
            public uint mod_end;
            [FieldOffset(8)]
            public uint cmdline;
            [FieldOffset(12)]
            public uint reserved;
        }
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct AOUT
        {
            [FieldOffset(0)]
            public uint tabsize;
            [FieldOffset(4)]
            public uint strsize;
            [FieldOffset(8)]
            public uint address;
            [FieldOffset(12)]
            public uint reserved;
        }
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct ELF
        {
            [FieldOffset(0)]
            public uint num;
            [FieldOffset(4)]
            public uint size;
            [FieldOffset(8)]
            public uint address;
            [FieldOffset(12)]
            public uint shndx;
        }
        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public struct Memory
        {
            public const uint TYPE_RESERVED = 0;
            public const uint TYPE_AVAILABLE = 1;
            public const uint TYPE_ACPI_RECLAIMABLE = 2;
            public const uint TYPE_ACPI_HIBERNATION = 3;

            [FieldOffset(0)]
            public uint size;
            [FieldOffset(4)]
            public ulong address;
            [FieldOffset(12)]
            public ulong length;
            [FieldOffset(20)]
            public uint type;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct Drive
        {
            public const byte MODE_CHS = 0;
            public const byte MODE_LBA = 1;

            [FieldOffset(0)]
            public uint size;
            [FieldOffset(4)]
            public byte number;
            [FieldOffset(5)]
            public byte mode;
            [FieldOffset(6)]
            public ushort cylinders;
            [FieldOffset(8)]
            public byte heads;
            [FieldOffset(9)]
            public byte sectors;
        }
        [StructLayout(LayoutKind.Explicit, Size = 20)]
        public struct APM
        {
            [FieldOffset(0)]
            public ushort version;
            [FieldOffset(2)]
            public ushort cseg;
            [FieldOffset(4)]
            public uint offset;
            [FieldOffset(8)]
            public ushort cseg_16;
            [FieldOffset(10)]
            public ushort dseg;
            [FieldOffset(12)]
            public ushort flags;
            [FieldOffset(14)]
            public ushort cseg_len;
            [FieldOffset(16)]
            public ushort cseg_16_len;
            [FieldOffset(18)]
            public ushort dseg_len;
        }
    }
}
