// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Fixed-capacity region store for an <see cref="AddressSpaceDescriptor"/>.
/// </summary>
/// <remarks>
/// <see cref="AddressSpaceDescriptor"/> is allocated with
/// <c>Heap.Heap.Alloc</c> and handed back to callers as a pointer, so the
/// descriptor's regions must live inline — there is no object header and no
/// indirection to grow through. <c>[InlineArray]</c> gives a real span over
/// <see cref="AddressSpaceRegion"/> backed by a single field, which is the
/// idiom already used for the GC handle store
/// (<c>GCHandleStoreList</c>, GCHandleManager.cs).
/// </remarks>
[InlineArray(MaxRegions)]
internal struct AddressSpaceRegionStore
{
    /// <summary>
    /// Maximum regions per address space. Fixed by design: the store is inline,
    /// so raising this raises the size of every descriptor allocation.
    /// </summary>
    public const int MaxRegions = 64;

    private AddressSpaceRegion _element0;
}

/// <summary>
/// A platform-agnostic description of an address space: a root table plus the
/// regions installed into it. Created by <see cref="Pager.Create"/>, filled via
/// <see cref="Pager.Add"/>, and applied to real page tables by
/// <see cref="Pager.Commit"/>.
/// </summary>
/// <remarks>
/// This is the kernel's own description of a space; nothing on the hardware side
/// consumes it directly. Each backend translates a committed descriptor into its
/// architecture's page-table format, so the same descriptor can be committed on
/// either architecture without change.
/// <para>
/// Mutable and heap-allocated rather than a managed class: it is reached
/// through raw pointers from the arch backends and native code, and the kernel
/// heap is non-moving, so a pointer to one stays valid for its lifetime.
/// </para>
/// </remarks>
internal unsafe struct AddressSpaceDescriptor
{
    /// <summary>
    /// Physical address of the root table, or zero when this descriptor has not
    /// been committed and therefore names no tables yet.
    /// </summary>
    /// <remarks>
    /// This is the root for whichever half the address being translated falls
    /// in; <see cref="LowRootFrame"/> holds the other one. On x86-64 a single
    /// 4-level CR3 covers the whole address space, so both fields are the same
    /// value there. On ARM64 the two halves are separate tables (TTBR0_EL1 and
    /// TTBR1_EL1) and a space needs both: the kernel heap is identity-mapped in
    /// the lower half while the HHDM and the kernel image live in the upper.
    /// </remarks>
    public ulong RootFrame;

    /// <summary>
    /// Root table for the lower half of the address space. Equal to
    /// <see cref="RootFrame"/> when the architecture has a single root (x86-64).
    /// </summary>
    public ulong LowRootFrame;

    /// <summary>Number of populated slots in <see cref="Regions"/>.</summary>
    public int RegionCount;

    /// <summary>
    /// Inline region storage. Only the first <see cref="RegionCount"/> entries
    /// are meaningful; the rest are scratch and must not be read.
    /// </summary>
    public AddressSpaceRegionStore Regions;

    /// <summary>
    /// True once <see cref="RootFrame"/> refers to allocated tables, i.e.
    /// <see cref="Pager.Commit"/> has run at least once. Distinguishes "never
    /// committed" from "committed with a root of zero", which <see cref="RootFrame"/>
    /// alone cannot express.
    /// </summary>
    public bool IsCommitted;

    /// <summary>
    /// The slot after the last populated one, where <see cref="Pager.Add"/>
    /// writes next.
    /// </summary>
    /// <remarks>
    /// <see cref="Pager.Add"/> checks <see cref="RegionCount"/> against
    /// <see cref="MaxRegionCount"/> and declines the add before this is
    /// reached, so the index is in range on every real call. The clamp below is
    /// a backstop against escaping the descriptor's own allocation — which would
    /// corrupt whatever the heap placed next — not a bounds check: a caller that
    /// bypassed <see cref="Pager.Add"/> would overwrite the last region rather
    /// than be caught. Returns by reference, so the write lands in the
    /// descriptor's inline storage without copying the element out and back.
    /// Requires a pointer to a descriptor, which is how every caller already
    /// holds one.
    /// </remarks>
    public ref AddressSpaceRegion NextRegion =>
        ref Regions[RegionCount < MaxRegionCount ? RegionCount : MaxRegionCount - 1];

    /// <summary>Capacity of <see cref="Regions"/>; see <see cref="AddressSpaceRegionStore.MaxRegions"/>.</summary>
    public const int MaxRegionCount = AddressSpaceRegionStore.MaxRegions;
}