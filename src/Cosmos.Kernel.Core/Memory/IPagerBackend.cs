// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// The architecture-specific half of <see cref="Pager"/>. Implemented once per
/// architecture and registered at boot by the platform initializer.
/// </summary>
/// <remarks>
/// Lives in <c>Cosmos.Kernel.Core</c> rather than in a HAL assembly because the
/// core assembly cannot reference <c>Cosmos.Kernel.Core.X64</c> /
/// <c>.ARM64</c> — those reference core, so the dependency cannot be reversed.
/// This is the same arrangement used by <c>IMsiBinder</c> (see
/// <c>MsiRouting</c>), where the contract is declared in core and the
/// implementation lives in the per-arch assembly.
/// <para>
/// Implementations speak their own page-table format and the shared native ABI
/// for the handful of instructions that have no managed equivalent; they never
/// see <see cref="PageFlags"/> except to translate it.
/// </para>
/// </remarks>
internal unsafe interface IPagerBackend
{
    /// <summary>
    /// True once the backend has registered itself. <see cref="Pager"/>
    /// refuses to build a descriptor before this, so a missing registration
    /// fails at the first call rather than at the first commit.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Hardware address of the page-table root the CPU is currently using, or 0
    /// when it cannot be read.
    /// </summary>
    /// <remarks>
    /// Reads CR3 (x64) or TTBR1_EL1 (ARM64) live. Backends must mask off the
    /// non-address bits — x64 CR3 carries a PCID in the low 12 and control
    /// flags in bits 63:62 — so the result is always a bare frame address.
    /// </remarks>
    ulong CurrentRootFrame();

    /// <summary>
    /// Root table for the lower half of the address space, or 0 when the
    /// architecture has a single root and it is already reported by
    /// <see cref="CurrentRootFrame"/>.
    /// </summary>
    /// <remarks>
    /// ARM64 splits the address space across two tables (TTBR0_EL1 and
    /// TTBR1_EL1), so a space that describes mappings in both halves needs
    /// both. x86-64 has one 4-level CR3 covering everything, and an
    /// implementation there may return 0 to mean "same as above".
    /// </remarks>
    ulong CurrentLowRootFrame();

    /// <summary>
    /// Installs every region of <paramref name="space"/> into the page tables
    /// rooted at <see cref="AddressSpaceDescriptor.RootFrame"/>.
    /// </summary>
    /// <remarks>
    /// May be called repeatedly on the same descriptor as regions are added.
    /// Must be safe against a concurrent
    /// <see cref="Translate(AddressSpaceDescriptor*, VirtualAddress, out PhysicalAddress)"/>
    /// only in the sense that the TLB flush at the end publishes the whole batch:
    /// a reader either sees the old mapping or the new one, never a torn entry.
    /// </remarks>
    void Commit(AddressSpaceDescriptor* space);

    /// <summary>
    /// Resolves a virtual address to physical within this descriptor's tables.
    /// Returns false when the address is unmapped or the walk hit a leaf that
    /// does not cover it.
    /// </summary>
    bool Translate(AddressSpaceDescriptor* space, VirtualAddress virt, out PhysicalAddress phys);

    /// <summary>
    /// Resolves a virtual address against the tables the CPU is currently
    /// using. Returns false when unmapped.
    /// </summary>
    /// <remarks>
    /// Distinct from the descriptor overload on purpose: this one must not
    /// assume it is walking the same root the caller last committed to. On a
    /// single-CPU kernel today those agree, but they stop agreeing the moment a
    /// second root is installed.
    /// </remarks>
    bool TranslateCurrent(VirtualAddress virt, out PhysicalAddress phys);
}