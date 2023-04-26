using System.Runtime.InteropServices;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Tag BasicMemoryInformation
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe readonly struct BasicMemoryInformation
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
}