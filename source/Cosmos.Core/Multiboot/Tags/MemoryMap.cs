using System.Runtime.InteropServices;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Tag MemoryMap
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public unsafe readonly struct MemoryMap
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
}