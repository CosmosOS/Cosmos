// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Boot.Limine;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Address translation helpers for virtio DMA and register access.
/// Virtio devices perform DMA with guest physical addresses while the kernel
/// works with HHDM virtual addresses; register windows are accessed through
/// their HHDM alias so the per-arch device-memory attributes apply.
/// </summary>
internal static class VirtioDma
{
    /// <summary>
    /// Converts a kernel virtual address (HHDM) to a guest physical address for DMA.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong VirtToPhys(ulong virtAddr)
    {
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        if (hhdmOffset != 0 && virtAddr >= hhdmOffset)
        {
            return virtAddr - hhdmOffset;
        }

        return virtAddr;
    }

    /// <summary>
    /// Converts a physical MMIO address to its HHDM virtual address. On ARM64,
    /// DeviceMapper maps MMIO regions as Device memory in TTBR1, so register
    /// accesses must go through the HHDM alias — the raw physical address hits
    /// the cacheable identity mapping and writes never reach hardware.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong PhysToVirt(ulong phys)
    {
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        if (hhdmOffset != 0 && phys < hhdmOffset)
        {
            return phys + hhdmOffset;
        }

        return phys;
    }
}
