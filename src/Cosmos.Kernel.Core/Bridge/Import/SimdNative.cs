using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

internal static unsafe partial class SimdNative
{
    [LibraryImport("*", EntryPoint = "_simd_copy_16")]
    [SuppressGCTransition]
    public static partial void Copy16(byte* dest, byte* src);

    [LibraryImport("*", EntryPoint = "_simd_copy_32")]
    [SuppressGCTransition]
    public static partial void Copy32(byte* dest, byte* src);

    [LibraryImport("*", EntryPoint = "_simd_copy_64")]
    [SuppressGCTransition]
    public static partial void Copy64(byte* dest, byte* src);

    [LibraryImport("*", EntryPoint = "_simd_copy_128")]
    [SuppressGCTransition]
    public static partial void Copy128(byte* dest, byte* src);

    [LibraryImport("*", EntryPoint = "_simd_copy_128_blocks")]
    [SuppressGCTransition]
    public static partial void Copy128Blocks(byte* dest, byte* src, int blockCount);

    [LibraryImport("*", EntryPoint = "_simd_copy_nt_128_blocks")]
    [SuppressGCTransition]
    public static partial void CopyNT128Blocks(byte* dest, byte* src, int blockCount);

    [LibraryImport("*", EntryPoint = "_simd_fill_16_blocks")]
    [SuppressGCTransition]
    public static partial void Fill16Blocks(byte* dest, int value, int blockCount);
}
