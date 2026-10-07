// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Platform-agnostic page attributes. These are a shared vocabulary, not either
/// architecture's encoding: each backend translates them into whatever its own
/// page tables express (x64 PTE/PDPDE bits + PAT, ARM64 descriptor bits + MAIR
/// index + SH), so no flag here should be read as a raw hardware bit.
/// </summary>
/// <remarks>
/// Only the low 16 bits are used; the width matches the field in
/// <see cref="AddressSpaceRegion"/>. Bits 16-31 are deliberately left free so a
/// second generation of attributes does not require changing the region layout.
/// </remarks>
[Flags]
internal enum PageFlags : ushort
{
    /// <summary>No attributes beyond <see cref="Present"/>. Backends may substitute their own defaults.</summary>
    None = 0,

    /// <summary>
    /// The mapping is valid. This is the one flag a caller cannot meaningfully
    /// omit: <see cref="Pager.Commit"/> installs every region with this set, and
    /// a region without it describes an entry that is already invalid.
    /// </summary>
    Present = 1 << 0,

    /// <summary>
    /// Writes are permitted. Maps to the x64 RW bit; on ARM64 a writable block
    /// also needs its AP[2] (EL0-writable) bit, which a backend sets only
    /// alongside <see cref="User"/>.
    /// </summary>
    Writable = 1 << 1,

    /// <summary>
    /// The mapping is reachable from EL0 (ring 3) as well as EL1.
    /// </summary>
    /// <remarks>
    /// Defined but not yet exercised: there is no ring-3 transition in the
    /// kernel today (every x64 IDT gate is DPL 0), so nothing can fault in from
    /// user mode to prove enforcement. Backends translate it now so the bit is
    /// already correct when a user transition lands; tests must not read a
    /// successful kernel-mode access as evidence that it works.
    /// </remarks>
    User = 1 << 2,

    /// <summary>
    /// Instruction fetch is permitted. Backends invert this into the negative
    /// encodings: x64 NX (bit 63) and ARM64 PXN + UXN.
    /// </summary>
    Executable = 1 << 3,

    /// <summary>
    /// The entry is global, so TLB invalidation that targets a context skips it.
    /// </summary>
    /// <remarks>
    /// Required for any mapping that must survive an address-space swap: a
    /// per-context flush would otherwise leave the kernel image and HHDM
    /// aliases untranslated under the incoming root. See
    /// <see cref="AddressSpaceDescriptor"/>.
    /// </remarks>
    Global = 1 << 4,

    /// <summary>
    /// Device memory: side-effecting, not prefetched, not speculatively read.
    /// Maps to x64 PCD+PWT (uncacheable via PAT) and ARM64 Device-nGnRnE.
    /// </summary>
    Device = 1 << 5,

    /// <summary>
    /// Normal cacheable memory (write-back). The default when neither
    /// <see cref="Device"/> nor <see cref="Uncached"/> is set.
    /// </summary>
    Cacheable = 1 << 6,

    /// <summary>
    /// Normal memory with caching disabled. Distinct from <see cref="Device"/>:
    /// coherent, but not cached. Maps to x64 PCD (PAT UC without PWT) and ARM64
    /// Normal Non-cacheable.
    /// </summary>
    Uncached = 1 << 7,
}