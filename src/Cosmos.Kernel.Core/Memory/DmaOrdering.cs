// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Orders CPU accesses to coherent DMA memory the way a device observes
/// them: a completion flag against the fields it guards, or a descriptor
/// against the store that hands it over. ARM64 issues <c>dmb oshld</c> /
/// <c>dmb oshst</c>, in the Outer Shareable domain where DMA masters
/// observe memory; x64 is TSO and issues nothing, but each call stays an
/// opaque native call so the compiler cannot move a memory access across
/// it. Neither allocates, blocks or dispatches through an interface, so
/// both are safe in an ISR.
/// </summary>
internal static class DmaOrdering
{
    /// <summary>
    /// Read barrier: loads before it complete before any load or store
    /// after it. Use after reading a device-written flag (a phase or used
    /// bit) and before reading what that flag says is valid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadBarrier() => DmaNative.ReadBarrier();

    /// <summary>
    /// Write barrier: stores before it become visible before any store
    /// after it. Use after filling a descriptor in DMA memory and before
    /// the DMA-memory store that publishes it to the device.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBarrier() => DmaNative.WriteBarrier();
}
