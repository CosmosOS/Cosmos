using System.Runtime.InteropServices;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Tag Framebuffer
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public unsafe readonly struct Framebuffer
    {
        /// <summary>
        /// Information about the tag.
        /// </summary>
        [FieldOffset(0)]
        public readonly MB2Tag Info;

        /// <summary>
        /// The video address for the frame buffer.
        /// </summary>
        [FieldOffset(8)]
        public readonly ulong Address;

        /// <summary>
        /// The pitch value of the frame buffer.
        /// </summary>
        [FieldOffset(16)]
        public readonly uint Pitch;

        /// <summary>
        /// The width value (in pixels) of the frame buffer
        /// </summary>
        [FieldOffset(20)]
        public readonly uint Width;

        /// <summary>
        /// The height value (in pixels) of the frame buffer
        /// </summary>
        [FieldOffset(24)]
        public readonly uint Height;

        /// <summary>
        /// The BPP (bits per pixel) of the frame buffer. E.g. 32-bit colors are 4 BPP.
        /// </summary>
        [FieldOffset(28)]
        public readonly byte Bpp;

        /// <summary>
        /// The type of frame buffer.
        /// </summary>
        [FieldOffset(29)]
        public readonly byte Type;

        /// <summary>
        /// Reserved.
        /// </summary>
        [FieldOffset(30)]
        public readonly ushort Reserved;
    }
}