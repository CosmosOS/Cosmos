// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Vogen;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// A virtual address, as an opaque value object.
/// </summary>
/// <remarks>
/// Exists to stop physical and virtual addresses being passed to each other by
/// accident — the two are both <see cref="ulong"/> and the compiler will happily
/// swap them. Construction goes through <c>From</c> so the intent is stated at
/// the call site; <c>Value</c> unwraps it for the arch-specific backends and
/// native imports.
/// <para>
/// This is an address, not a validated mapping.
/// <see cref="Pager.Translate(AddressSpaceDescriptor*, VirtualAddress, out PhysicalAddress)"/>
/// answers whether one is backed; <see cref="From"/> deliberately performs no
/// canonical-form check, because rejecting a not-yet-mapped higher-half address
/// would make it impossible to build the mapping for it in the first place.
/// </para>
/// </remarks>
[ValueObject(typeof(ulong))]
internal readonly partial struct VirtualAddress
{
    /// <summary>Wraps a raw address.</summary>
    /// <remarks>
    /// No range validation. Both the lower half (future user space) and the
    /// higher half (kernel, per <see cref="AddressSpace.KernelSpaceStart"/>) are
    /// representable, and so is the gap between them — deciding whether a given
    /// address is usable belongs to <see cref="Pager"/>, not to construction.
    /// </remarks>
    private static Validation Validate(ulong value) => Validation.Ok;
}