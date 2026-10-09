using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class ARM64MmioNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_read_byte")]
    [SuppressGCTransition]
    public static partial byte ReadByte(ulong address);

    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_read_word")]
    [SuppressGCTransition]
    public static partial ushort ReadWord(ulong address);

    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_read_dword")]
    [SuppressGCTransition]
    public static partial uint ReadDWord(ulong address);

    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_write_byte")]
    [SuppressGCTransition]
    public static partial void WriteByte(ulong address, byte value);

    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_write_word")]
    [SuppressGCTransition]
    public static partial void WriteWord(ulong address, ushort value);

    [LibraryImport("*", EntryPoint = "_native_arm64_mmio_write_dword")]
    [SuppressGCTransition]
    public static partial void WriteDWord(ulong address, uint value);
}
