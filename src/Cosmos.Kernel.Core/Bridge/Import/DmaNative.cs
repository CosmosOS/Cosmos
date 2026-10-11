// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// DMA ordering barriers, one symbol per architecture:
/// Native.ARM64/Memory/DmaBarrier.s and Native.X64/Memory/DmaBarrier.s.
/// Wrapped by <see cref="Memory.DmaOrdering"/>.
/// </summary>
internal static partial class DmaNative
{
    [LibraryImport("*", EntryPoint = "_native_dma_rmb")]
    [SuppressGCTransition]
    public static partial void ReadBarrier();

    [LibraryImport("*", EntryPoint = "_native_dma_wmb")]
    [SuppressGCTransition]
    public static partial void WriteBarrier();
}
