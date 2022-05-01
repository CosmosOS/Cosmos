/*
* PROJECT:          Cosmos Development
* CONTENT:          Multiboot2 class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
* RESOURCES:        https://www.gnu.org/software/grub/manual/multiboot2/multiboot.html
*/

using IL2CPU.API.Attribs;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Core
{
    /// <summary>
    /// Multiboot2 class. Used for multiboot parsing.
    /// </summary>
    public unsafe class Multiboot2
    {
        /// <summary>
        /// Base Tag
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        internal unsafe readonly struct Mb2Tag
        {
            [FieldOffset(0)]
            public readonly uint Type;
            [FieldOffset(4)]
            public readonly uint Size;
        }

        /// <summary>
        /// Tag BasicMemoryInformation
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal unsafe readonly struct Mb2TagBasicMemoryInformation
        {
            [FieldOffset(0)]
            public readonly uint Type;
            [FieldOffset(4)]
            public readonly uint Size;
            [FieldOffset(8)]
            public readonly uint MemLower;
            [FieldOffset(12)]
            public readonly uint MemUpper;
        }

        /// <summary>
        /// Tag MemoryMap
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 40)]
        internal unsafe readonly struct Mb2TagMemoryMap
        {
            [FieldOffset(0)]
            public readonly uint Type;
            [FieldOffset(4)]
            public readonly uint Size;
            [FieldOffset(8)]
            public readonly uint EntrySize;
            [FieldOffset(12)]
            public readonly uint EntryVersion;
            [FieldOffset(16)]
            public readonly RawMemoryMapBlock MemoryMapEntries;
        }

        /// <summary>
        /// Tag Framebuffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 784)]
        internal unsafe readonly struct Mb2TagVbeInfo
        {
            [FieldOffset(0)]
            public readonly Mb2Tag Info;
            [FieldOffset(8)]
            public readonly ushort VbeMode;
            [FieldOffset(10)]
            public readonly ushort VbeInterfaceSeg;
            [FieldOffset(12)]
            public readonly ushort VbeInterfaceOff;
            [FieldOffset(14)]
            public readonly ushort VbeInterfaceLen;
            [FieldOffset(16)]
            public readonly VBE.ControllerInfo VbeControlInfo; //512
            [FieldOffset(528)]
            public readonly VBE.ModeInfo VbeModeInfo; //256
        }

        /// <summary>
        /// Tag Framebuffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal unsafe readonly struct Mb2TagFramebuffer
        {
            [FieldOffset(0)]
            public readonly Mb2Tag Info;
            [FieldOffset(8)]
            public readonly ulong Address;
            [FieldOffset(16)]
            public readonly uint Pitch;
            [FieldOffset(20)]
            public readonly uint Width;
            [FieldOffset(24)]
            public readonly uint Height;
            [FieldOffset(28)]
            public readonly byte Bpp;
            [FieldOffset(29)]
            public readonly byte Type;
            [FieldOffset(30)]
            public readonly ushort Reserved;
        }

        /// <summary>
        /// Tag EFI64
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal unsafe readonly struct Mb2TagEFI64
        {
            [FieldOffset(0)]
            public readonly Mb2Tag Info;
            [FieldOffset(8)]
            public readonly ulong Address;
        }

        internal static Mb2TagBasicMemoryInformation* BasicMemoryInformation { get; set; }
        internal static Mb2TagMemoryMap* MemoryMap { get; set; }
        internal static Mb2TagVbeInfo* VbeInfo { get; set; }
        internal static Mb2TagFramebuffer* Framebuffer { get; set; }
        internal static Mb2TagEFI64* EFI64 { get; set; }

        private static bool mInitialized = false;

        /// /// <summary>
        /// Parse multiboot2 structure
        /// </summary>
        public static void Init()
        {
            if (!mInitialized)
            {
                mInitialized = true;

                var MbAddress = (IntPtr)GetMBIAddress();

                Mb2Tag* tag;

                for (tag = (Mb2Tag*)(MbAddress + 8); tag->Type != 0; tag = (Mb2Tag*)((byte*)tag + ((tag->Size + 7) & ~7)))
                {
                    switch (tag->Type)
                    {
                        case 4:
                            BasicMemoryInformation = (Mb2TagBasicMemoryInformation*)tag;
                            break;
                        case 6:
                            MemoryMap = (Mb2TagMemoryMap*)tag;
                            break;
                        case 7:
                            VbeInfo = (Mb2TagVbeInfo*)tag;
                            break;
                        /*case 8:
                            Framebuffer = (Mb2TagFramebuffer*)tag;
                            break;
                        case 12:
                            EFI64 = (Mb2TagEFI64*)tag;
                            break;*/
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get MemLower
        /// </summary>
        /// <returns>MemLower</returns>
        public static uint GetMemLower()
        {
            return BasicMemoryInformation->MemLower;
        }

        /// <summary>
        /// Get MemUpper
        /// </summary>
        /// <returns>MemUpper</returns>
        public static uint GetMemUpper()
        {
            return BasicMemoryInformation->MemUpper;
        }

        /// <summary>
        /// Checks if Multiboot returned a memory map
        /// </summary>
        /// <returns>True if is available, false if not</returns>
        public static bool MemoryMapExists()
        {
            if (MemoryMap != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// /// <summary>
        /// Get Multiboot address. Plugged.
        /// </summary>
        /// <returns>The Multiboot Address</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint GetMBIAddress() => throw null;
    }

    /// <summary>
    /// VBE class.
    /// </summary>
    public unsafe static class VBE
    {
        /// /// <summary>
        /// Check in Multiboot if framebuffer is available
        /// </summary>
        /// <returns>True if is available, false if not</returns>
        public static bool IsAvailable()
        {
            if (Multiboot2.VbeInfo != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// /// <summary>
        /// Get VBE Modeinfo structure
        /// </summary>
        public static ModeInfo getModeInfo()
        {
            return Multiboot2.VbeInfo->VbeModeInfo;
        }

        /// /// <summary>
        /// Get VBE Modeinfo structure
        /// </summary>
        public static ControllerInfo getControllerInfo()
        {
            return Multiboot2.VbeInfo->VbeControlInfo;
        }

        /// /// <summary>
        /// Get the linear frame buffer address from VBE ModeInfo structure
        /// </summary>
        /// <returns>the offset in an uint</returns>
        public static uint getLfbOffset()
        {
            return getModeInfo().framebuffer;
        }

        /// <summary>
        /// Controller info struct.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 512)]
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
