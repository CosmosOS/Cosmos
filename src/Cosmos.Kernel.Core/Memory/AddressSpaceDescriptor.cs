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
    public ulong RootFrame;

    /// <summary>
    /// x86 PCID assigned to this space, used to invalidate its TLB entries
    /// cheaply on switch. Zero means "unassigned": on hardware without INVPCID,
    /// and for a descriptor that never opts in, a root of 0 selects the
    /// legacy flush-everything behaviour. Populated by the backend, not by
    /// callers.
    /// </summary>
    public ulong Pcid;

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
    /// The slot after the last populated one, where
    /// <see cref="Pager.Add"/> writes next. Never addresses past
    /// <see cref="AddressSpaceRegionStore.MaxRegions"/> — the caller checks
    /// <see cref="RegionCount"/> against <see cref="MaxRegionCount"/> first and
    /// declines the add, so this relies on that contract rather than
    /// bounds-checking the write.
    /// </summary>
    /// <remarks>
    /// Returns by reference, so the write lands in the descriptor's inline
    /// storage without copying the element out and back. Requires a pointer to
    /// a descriptor, which is how every caller already holds one.
    /// </remarks>
    public ref AddressSpaceRegion NextRegion =>
        ref Regions[RegionCount < MaxRegionCount ? RegionCount : MaxRegionCount - 1];

    /// <summary>Capacity of <see cref="Regions"/>; see <see cref="AddressSpaceRegionStore.MaxRegions"/>.</summary>
    public const int MaxRegionCount = AddressSpaceRegionStore.MaxRegions;
}