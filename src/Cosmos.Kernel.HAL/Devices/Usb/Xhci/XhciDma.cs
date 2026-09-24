// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// Page allocation for structures the controller reads or writes by DMA:
/// rings, contexts and transfer buffers. Pages come zeroed and page
/// aligned, which covers every xHCI alignment rule (at most 64 bytes, with
/// no structure crossing a 64 KiB boundary).
/// </summary>
internal static unsafe class XhciDma
{
    public const int PageSize = (int)PageAllocator.PageSize;

    private static Action? s_barrier;

    /// <summary>
    /// Resolves the platform DMA barrier once, from thread context. The
    /// barrier runs in the interrupt handler too, and calling it through
    /// <see cref="Interfaces.IPlatformInitializer"/> there would be an
    /// interface dispatch, whose first resolution may allocate.
    /// </summary>
    public static void Initialize()
    {
        if (s_barrier is null && PlatformHAL.Initializer is { } initializer)
        {
            s_barrier = initializer.DmaBarrier;
        }
    }

    /// <summary>
    /// Orders ring and context stores before the MMIO store that hands them
    /// to the controller, and controller-written memory before the reads that
    /// follow. Safe from interrupt context.
    /// </summary>
    public static void Barrier() => s_barrier?.Invoke();

    /// <summary>Allocates zeroed pages and returns their virtual address.</summary>
    /// <exception cref="InvalidOperationException">The page allocator is out of memory.</exception>
    public static byte* AllocPages(ulong pageCount, out ulong physicalAddress)
    {
        byte* pages = (byte*)PageAllocator.AllocPages(PageType.Unmanaged, pageCount, true);
        if (pages == null)
        {
            throw new InvalidOperationException("[xHCI] Out of memory for DMA structures");
        }

        physicalAddress = PageAllocator.VirtualToPhysical((ulong)pages);
        return pages;
    }

    public static void Free(void* pages) => PageAllocator.Free(pages);
}
