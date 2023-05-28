using System.Runtime.InteropServices;

namespace Cosmos.Core.Multiboot.Tags
{
    /// <summary>
    /// Tag EFI64
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe readonly struct EFI64
    {
        [FieldOffset(0)]
        public readonly MB2Tag Info;
        [FieldOffset(8)]
        public readonly ulong Address;
    }
}