// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// One contiguous mapping: <paramref cref="PageCount"/> pages starting at
/// <paramref cref="Virt"/> in the virtual space and <paramref cref="Phys"/> in
/// physical memory.
/// </summary>
/// <remarks>
/// Deliberately not a Vogen value object. It does not wrap a single primitive,
/// so the primitive-obsession protection Vogen gives (you cannot pass a virtual
/// address where a physical one belongs) does not apply, and a region is only
/// meaningful as part of an <see cref="AddressSpaceDescriptor"/> — it is never
/// compared, ordered, or parsed on its own.
/// <para>
/// A plain mutable struct rather than a <c>readonly</c> one, so
/// <see cref="Pager"/> can fill a slot in place.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct AddressSpaceRegion
{
    /// <summary>First virtual address covered by the mapping.</summary>
    public VirtualAddress Virt;

    /// <summary>First physical address covered by the mapping. Page-aligned.</summary>
    public PhysicalAddress Phys;

    /// <summary>
    /// Length in pages. Zero is a legal value: it describes an empty region and
    /// installs nothing, which lets a caller reserve an address range before
    /// committing to a length.
    /// </summary>
    public ulong PageCount;

    /// <summary>Platform-agnostic page attributes. See <see cref="PageFlags"/>.</summary>
    public PageFlags Flags;
}