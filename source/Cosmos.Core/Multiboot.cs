using IL2CPU.API.Attribs;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    /// <summary>
    /// Used for Multiboot parsing
    /// </summary>
    public class Multiboot
    {
        /// /// <summary>
        /// Get Multiboot address. Plugged.
        /// </summary>
        /// <returns>The Multiboot Address</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint GetMBIAddress() => throw null;

        [StructLayout(LayoutKind.Explicit, Size = 88)]
        public unsafe struct Header
        {
            [FieldOffset(0)]
            public uint Flags;
            [FieldOffset(4)]
            public uint mem_lower;
            [FieldOffset(8)]
            public uint mem_upper;
            [FieldOffset(12)]
            public uint boot_device;
            [FieldOffset(16)]
            public uint cmdline;
            [FieldOffset(20)]
            public uint mods_count;
            [FieldOffset(24)]
            public uint mods_addr;
            [FieldOffset(28)]
            public fixed uint syms[4];
            [FieldOffset(44)]
            public uint memMapLength;
            [FieldOffset(48)]
            public uint memMapAddress;
            [FieldOffset(52)]
            public uint drivesLength;
            [FieldOffset(56)]
            public uint drivesAddress;
            [FieldOffset(60)]
            public uint configTable;
            [FieldOffset(68)]
            public uint apmTable;
            [FieldOffset(72)]
            public uint vbeControlInfo;
            [FieldOffset(76)]
            public uint vbeModeInfo;
            [FieldOffset(80)]
            public uint vbeMode;
            [FieldOffset(82)]
            public uint vbeInterfaceSeg;
            [FieldOffset(84)]
            public uint vbeInterfaceOff;
            [FieldOffset(86)]
            public uint vbeInterfaceLength;
        }
    }

    public unsafe static class VBE
    {

        static uint VBEINFO_PRESENT = (1 << 11);

        /// /// <summary>
        /// Check in Multiboot if VBE is available
        /// </summary>
        /// <returns>True if is available, false if not</returns>
        public static bool IsAvailable()
        {
            if ((Bootstrap.header->Flags & VBEINFO_PRESENT) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// /// <summary>
        /// Get VBE Modeinfo structure
        /// </summary>
        public static ModeInfo getModeInfo()
        {
            return *Bootstrap.modeinfo;
        }

        /// /// <summary>
        /// Get the linear frame buffer address from VBE ModeInfo structure
        /// </summary>
        /// <returns>the offset in an uint</returns>
        public static uint getLfbOffset()
        {
            return Bootstrap.modeinfo->framebuffer;
        }

        [StructLayout(LayoutKind.Explicit, Size = 36)]
        public struct ControllerInfo
        {
            [FieldOffset(0)]
            public uint vbeSignature;
            [FieldOffset(4)]
            public ushort vbeVersion;
            [FieldOffset(6)]
            public uint oemStringPtr;
            [FieldOffset(10)]
            public uint capabilities;
            [FieldOffset(14)]
            public uint videoModePtr;
            [FieldOffset(18)]
            public ushort totalmemory;
            [FieldOffset(20)]
            public ushort oemSoftwareRev;
            [FieldOffset(24)]
            public uint oemVendorNamePtr;
            [FieldOffset(28)]
            public uint oemProductNamePtr;
            [FieldOffset(32)]
            public uint oemProductRevPtr;
        }

        [StructLayout(LayoutKind.Explicit, Size = 256)]
        public struct ModeInfo
        {
            [FieldOffset(0)]
            public ushort attributes; // deprecated, only bit 7 should be of interest to you, and it indicates the mode supports a linear frame buffer.
            [FieldOffset(2)]
            public byte window_a; // deprecated
            [FieldOffset(3)]
            public byte window_b; // deprecated
            [FieldOffset(4)]
            public ushort granularity; // deprecated; used while calculating bank numbers
            [FieldOffset(6)]
            public ushort window_size;
            [FieldOffset(8)]
            public ushort segment_a;
            [FieldOffset(10)]
            public ushort segment_b;
            [FieldOffset(12)]
            public uint win_func_ptr; // deprecated; used to switch banks from protected mode without returning to real mode
            [FieldOffset(16)]
            public ushort pitch; // number of bytes per horizontal line
            [FieldOffset(18)]
            public ushort width; // width in pixels
            [FieldOffset(20)]
            public ushort height; // height in pixels
            [FieldOffset(22)]
            public byte w_char; // unused...
            [FieldOffset(23)]
            public byte y_char; // ...
            [FieldOffset(24)]
            public byte planes;
            [FieldOffset(25)]
            public byte bpp; // bits per pixel in this mode
            [FieldOffset(26)]
            public byte banks; // deprecated; total number of banks in this mode
            [FieldOffset(27)]
            public byte memory_model;
            [FieldOffset(28)]
            public byte bank_size; // deprecated; size of a bank, almost always 64 KB but may be 16 KB...
            [FieldOffset(29)]
            public byte image_pages;
            [FieldOffset(30)]
            public byte reserved0;
            [FieldOffset(31)]
            public byte red_mask;
            [FieldOffset(32)]
            public byte red_position;
            [FieldOffset(33)]
            public byte green_mask;
            [FieldOffset(34)]
            public byte green_position;
            [FieldOffset(35)]
            public byte blue_mask;
            [FieldOffset(36)]
            public byte blue_position;
            [FieldOffset(37)]
            public byte reserved_mask;
            [FieldOffset(38)]
            public byte reserved_position;
            [FieldOffset(39)]
            public byte direct_color_attributes;
            [FieldOffset(40)]
            public uint framebuffer; // physical address of the linear frame buffer; write here to draw to the screen
            [FieldOffset(44)]
            public uint off_screen_mem_off;
            [FieldOffset(48)]
            public ushort off_screen_mem_size; // size of memory in the framebuffer but not being displayed on the screen
            [FieldOffset(50)]
            public fixed byte reserved1[206];
        }
    }
}
