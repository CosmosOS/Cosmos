using System;
using System.Runtime.InteropServices;

namespace Kernel
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Mb2Tag
    {
        public readonly uint Type;
        public readonly uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Mb2TagFramebufferCommon
    {
        public readonly Mb2Tag Info;
        public readonly ulong Address;
        public readonly uint Pitch;
        public readonly uint Width;
        public readonly uint Height;
        public readonly byte Bpp;
        public readonly byte Type;
        public readonly ushort Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Mb2TagFramebuffer
    {
        public readonly Mb2TagFramebufferCommon Common;
        //TODO: Add other framebuffer info
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Mb2TagEFI64
    {
        public readonly Mb2Tag Info;
        public readonly ulong Address;
    }

    public static unsafe class Multiboot2
    {
        public static Mb2TagFramebuffer* TagFramebuffer;
        public static Mb2TagEFI64* TagEFI64;
        public static bool BootServicesTerminated = true;

        public static void Parse(IntPtr MbAddress)
        {
            Mb2Tag *tag;

            for (tag = (Mb2Tag*)(MbAddress + 8); tag->Type != 0; tag = (Mb2Tag*)((uint)tag + Align(tag->Size, 8)))
            {
                switch (tag->Type)
                {
                    case 8:
                        TagFramebuffer = (Mb2TagFramebuffer*)tag;
                        break;
                    case 12:
                        TagEFI64 = (Mb2TagEFI64*)tag;
                        break;
                    case 18:
                        BootServicesTerminated = false;
                        break;
                    default:
                        break;
                }
            }
        }

        private static uint Align(uint Value, uint Alignment)
        {
            return ((Value) + (((Alignment) - (Value)) & ((Alignment) - 1)));
        }
    }
}
