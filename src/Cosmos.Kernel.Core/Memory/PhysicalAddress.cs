// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Vogen;

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// A physical address, as an opaque value object.
/// </summary>
/// <remarks>
/// The counterpart to <see cref="VirtualAddress"/>. Keeping them distinct is the
/// point of both: a physical address reaches hardware (PCI BARs, the HHDM
/// alias, device-to-device DMA) and must not be confused with one that only
/// makes sense to the CPU's MMU.
/// <para>
/// Unlike <see cref="VirtualAddress"/>, a physical address <em>is</em> validated
/// for page alignment, because a region that is not 4 KiB aligned has no
/// meaningful page-table encoding on either architecture — x64 PTE/PDPDE/PD
/// physical fields and ARM64 block descriptors all drop the low bits. Catching
/// it here reports the caller's bug at the call site instead of silently
/// mapping the wrong frame.
/// </para>
/// </remarks>
[ValueObject(typeof(ulong))]
internal readonly partial struct PhysicalAddress
{
    /// <summary>
    /// Rejects a physical address that is not page-aligned, which cannot be
    /// encoded as a page-table entry on either architecture.
    /// </summary>
    /// <remarks>
    /// Reported as a validation failure rather than an
    /// <see cref="ArgumentOutOfRangeException"/>, so the throw site is the
    /// <c>From</c> call itself. A rejected value is a caller bug; there is no
    /// recovery path worth designing here.
    /// </remarks>
    private static Validation Validate(ulong value)
        => (value & (PageAllocator.PageSize - 1)) == 0
            ? Validation.Ok
            : Validation.Invalid("Physical address must be page-aligned.");

    /// <summary>
    /// The sentinel a failed translation reports: physical address 0.
    /// </summary>
    /// <remarks>
    /// Callers should prefer the <c>bool</c> return of
    /// <see cref="Pager.Translate(AddressSpaceDescriptor*, VirtualAddress, out PhysicalAddress)"/>
    /// over inspecting this. It exists so the <c>out</c> parameter is always
    /// assigned — <c>default</c> cannot be used because the type is validated.
    /// Physical 0 is page-aligned, so it constructs without validation failing,
    /// but it is not a meaningful frame: Limine's usable RAM never starts there.
    /// </remarks>
    public static PhysicalAddress None => From(0);
}