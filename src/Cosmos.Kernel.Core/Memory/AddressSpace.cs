// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// Virtual address-space layout constants shared across the kernel.
/// </summary>
internal static class AddressSpace
{
    /// <summary>
    /// Lowest canonical higher-half address (48-bit virtual addressing) — the start of
    /// kernel space. Every valid kernel pointer (MethodTable, code, stack, HHDM alias)
    /// is at or above this bound; any lower value read from a heap word is data, not a
    /// pointer. Limine also places the Higher Half Direct Map at this offset by default
    /// (see <see cref="PageAllocator.DefaultHhdmOffset"/>).
    /// </summary>
    public const ulong KernelSpaceStart = 0xFFFF800000000000UL;

    /// <summary>
    /// Bits that are sign-extended in a 48-bit canonical virtual address (bits [63:48]).
    /// A higher-half (kernel) address has all of them set: <c>(addr &amp; mask) == mask</c>.
    /// </summary>
    public const ulong KernelSpaceCanonicalMask = 0xFFFF000000000000UL;

    /// <summary>
    /// Base of the kernel-image window (top 2 GiB of the canonical higher half, matching the
    /// linker scripts); addresses at or above it are kernel-image mappings, not HHDM aliases.
    /// </summary>
    public const ulong KernelImageWindow = 0xFFFFFFFF80000000UL;
}
