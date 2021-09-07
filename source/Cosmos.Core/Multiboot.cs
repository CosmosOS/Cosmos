using IL2CPU.API.Attribs;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    /// <summary>
    /// Multiboot class. Used for Multiboot parsing.
    /// </summary>
    public class Multiboot
    {
        /// /// <summary>
        /// Get Multiboot address. Plugged.
        /// </summary>
        /// <returns>The Multiboot Address</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint GetMBIAddress() => throw null;

        /// <summary>
        /// Header struct.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 88)]
        public unsafe struct Header
        {
            /// <summary>
            /// Flags.
            /// </summary>
            [FieldOffset(0)]
            public uint Flags;
            /// <summary>
            /// Lower memory amount.
            /// </summary>
            [FieldOffset(4)]
            public uint mem_lower;
            /// <summary>
            /// Upper memory amount.
            /// </summary>
            [FieldOffset(8)]
            public uint mem_upper;
            /// <summary>
            /// Boot device.
            /// </summary>
            [FieldOffset(12)]
            public uint boot_device;
            /// <summary>
            /// CMD line.
            /// </summary>
            [FieldOffset(16)]
            public uint cmdline;
            /// <summary>
            /// Modules count.
            /// </summary>
            [FieldOffset(20)]
            public uint mods_count;
            /// <summary>
            /// Modules address.
            /// </summary>
            [FieldOffset(24)]
            public uint mods_addr;
            /// <summary>
            /// Symbol table.
            /// </summary>
            [FieldOffset(28)]
            public fixed uint syms[4];
            /// <summary>
            /// Memory map length.
            /// </summary>
            [FieldOffset(44)]
            public uint memMapLength;
            /// <summary>
            /// Memory map address.
            /// </summary>
            [FieldOffset(48)]
            public uint memMapAddress;
            /// <summary>
            /// Drives list length.
            /// </summary>
            [FieldOffset(52)]
            public uint drivesLength;
            /// <summary>
            /// Drives list address.
            /// </summary>
            [FieldOffset(56)]
            public uint drivesAddress;
            /// <summary>
            /// ROM config table.
            /// </summary>
            [FieldOffset(60)]
            public uint configTable;
            /// <summary>
            /// APM table.
            /// </summary>
            [FieldOffset(68)]
            public uint apmTable;
            /// <summary>
            /// VBE control info.
            /// </summary>
            [FieldOffset(72)]
            public uint vbeControlInfo;
            /// <summary>
            /// VBE mode info.
            /// </summary>
            [FieldOffset(76)]
            public uint vbeModeInfo;
            /// <summary>
            /// VBE mode.
            /// </summary>
            [FieldOffset(80)]
            public uint vbeMode;
            /// <summary>
            /// VBE interface segment.
            /// </summary>
            [FieldOffset(82)]
            public uint vbeInterfaceSeg;
            /// <summary>
            /// VBE interface offset.
            /// </summary>
            [FieldOffset(84)]
            public uint vbeInterfaceOff;
            /// <summary>
            /// VBE interface length.
            /// </summary>
            [FieldOffset(86)]
            public uint vbeInterfaceLength;
        }
    }

    /// <summary>
    /// VBE class.
    /// </summary>
    public unsafe static class VBE
    {

        static uint VBEINFO_PRESENT = (1 << 11);

        /// /// <summary>
        /// Check in Multiboot if VBE is available
        /// </summary>
        /// <returns>True if is available, false if not</returns>
        public static bool IsAvailable()
        {
            if ((Bootstrap.MultibootHeader->Flags & VBEINFO_PRESENT) == 0)
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


        /// <summary>
        /// Controller info struct.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 36)]
        public struct ControllerInfo
        {
            /// <summary>
            /// VBE signature.
            /// </summary>
            [FieldOffset(0)]
            public uint vbeSignature;
            /// <summary>
            /// VBE version.
            /// </summary>
            [FieldOffset(4)]
            public ushort vbeVersion;
            /// <summary>
            /// OEM string pointer.
            /// </summary>
            [FieldOffset(6)]
            public uint oemStringPtr;
            /// <summary>
            /// Capabilities.
            /// </summary>
            [FieldOffset(10)]
            public uint capabilities;
            /// <summary>
            /// Video mode pointer.
            /// </summary>
            [FieldOffset(14)]
            public uint videoModePtr;
            /// <summary>
            /// Total memory.
            /// </summary>
            [FieldOffset(18)]
            public ushort totalmemory;
            /// <summary>
            /// OEM software revision.
            /// </summary>
            [FieldOffset(20)]
            public ushort oemSoftwareRev;
            /// <summary>
            /// OEM vendor name pointer.
            /// </summary>
            [FieldOffset(24)]
            public uint oemVendorNamePtr;
            /// <summary>
            /// OEM product name pointer.
            /// </summary>
            [FieldOffset(28)]
            public uint oemProductNamePtr;
            /// <summary>
            /// OEM product revision pointer.
            /// </summary>
            [FieldOffset(32)]
            public uint oemProductRevPtr;
        }

        /// <summary>
        /// Mode info struct.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 256)]
        public struct ModeInfo
        {
            /// <summary>
            /// Attributes.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(0)]
            public ushort attributes; // deprecated, only bit 7 should be of interest to you, and it indicates the mode supports a linear frame buffer.
            /// <summary>
            /// Window A.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(2)]
            public byte window_a; // deprecated
            /// <summary>
            /// Window B.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(3)]
            public byte window_b; // deprecated
            /// <summary>
            /// Granularity
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(4)]
            public ushort granularity; // deprecated; used while calculating bank numbers
            /// <summary>
            /// Window size.
            /// </summary>
            [FieldOffset(6)]
            public ushort window_size;
            /// <summary>
            /// Segment A.
            /// </summary>
            [FieldOffset(8)]
            public ushort segment_a;
            /// <summary>
            /// Segment B.
            /// </summary>
            [FieldOffset(10)]
            public ushort segment_b;
            /// <summary>
            /// Window function pointer.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(12)]
            public uint win_func_ptr; // deprecated; used to switch banks from protected mode without returning to real mode
            /// <summary>
            /// Pitch - number of bytes per horizontal line.
            /// </summary>
            [FieldOffset(16)]
            public ushort pitch; // number of bytes per horizontal line
            /// <summary>
            /// Width. (in pixels)
            /// </summary>
            [FieldOffset(18)]
            public ushort width; // width in pixels
            /// <summary>
            /// Height. (in pixels)
            /// </summary>
            [FieldOffset(20)]
            public ushort height; // height in pixels
            /// <summary>
            /// W char.
            /// </summary>
            /// <remarks>Unused.</remarks>
            [FieldOffset(22)]
            public byte w_char; // unused...
            /// <summary>
            /// Y char.
            /// </summary>
            /// <remarks>Unused.</remarks>
            [FieldOffset(23)]
            public byte y_char; // ...
            /// <summary>
            /// Planes.
            /// </summary>
            [FieldOffset(24)]
            public byte planes;
            /// <summary>
            /// Bits per pixel.
            /// </summary>
            [FieldOffset(25)]
            public byte bpp; // bits per pixel in this mode
            /// <summary>
            /// Banks.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(26)]
            public byte banks; // deprecated; total number of banks in this mode
            /// <summary>
            /// Memory model.
            /// </summary>
            [FieldOffset(27)]
            public byte memory_model;
            /// <summary>
            /// Bank size.
            /// </summary>
            /// <remarks>Deprecated.</remarks>
            [FieldOffset(28)]
            public byte bank_size; // deprecated; size of a bank, almost always 64 KB but may be 16 KB...
            /// <summary>
            /// Image pages.
            /// </summary>
            [FieldOffset(29)]
            public byte image_pages;
            /// <summary>
            /// Reserved.
            /// </summary>
            [FieldOffset(30)]
            public byte reserved0;
            /// <summary>
            /// Red mask.
            /// </summary>
            [FieldOffset(31)]
            public byte red_mask;
            /// <summary>
            /// Red position.
            /// </summary>
            [FieldOffset(32)]
            public byte red_position;
            /// <summary>
            /// Green mask.
            /// </summary>
            [FieldOffset(33)]
            public byte green_mask;
            /// <summary>
            /// Green position.
            /// </summary>
            [FieldOffset(34)]
            public byte green_position;
            /// <summary>
            /// Blue mask.
            /// </summary>
            [FieldOffset(35)]
            public byte blue_mask;
            /// <summary>
            /// Blue position.
            /// </summary>
            [FieldOffset(36)]
            public byte blue_position;
            /// <summary>
            /// Reserved mask.
            /// </summary>
            [FieldOffset(37)]
            public byte reserved_mask;
            /// <summary>
            /// Reserved position.
            /// </summary>
            [FieldOffset(38)]
            public byte reserved_position;
            /// <summary>
            /// Direct color attributes.
            /// </summary>
            [FieldOffset(39)]
            public byte direct_color_attributes;
            /// <summary>
            /// Frame buffer.
            /// </summary>
            [FieldOffset(40)]
            public uint framebuffer; // physical address of the linear frame buffer; write here to draw to the screen
            /// <summary>
            /// Off screen memory offset.
            /// </summary>
            [FieldOffset(44)]
            public uint off_screen_mem_off;
            /// <summary>
            /// Off screen memory size.
            /// </summary>
            [FieldOffset(48)]
            public ushort off_screen_mem_size; // size of memory in the framebuffer but not being displayed on the screen
            /// <summary>
            /// Reserved.
            /// </summary>
            [FieldOffset(50)]
            public fixed byte reserved1[206];
        }
    }
}
