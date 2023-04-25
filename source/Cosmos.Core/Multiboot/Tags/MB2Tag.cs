using System.Runtime.InteropServices;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Base Tag
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public unsafe readonly struct MB2Tag
    {
        [FieldOffset(0)]
        public readonly uint Type;
        [FieldOffset(4)]
        public readonly uint Size;
    }
}