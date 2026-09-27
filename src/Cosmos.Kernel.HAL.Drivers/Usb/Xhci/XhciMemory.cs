// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

/// <summary>
/// The pages the controller reads or writes by DMA, rings, contexts and
/// transfer buffers, allocated through the binding: during Probe for what
/// the controller needs to run, then, once Bound, for each device and
/// endpoint as it comes, and given back when the device leaves. Pages come
/// zeroed and page aligned, which covers every xHCI alignment rule (at most
/// 64 bytes, with no structure crossing a 64 KiB boundary), and below
/// 4 GiB for a controller that takes 32-bit addresses only.
/// </summary>
internal sealed class XhciMemory
{
    /// <summary>
    /// The page size the controller runs with, the one it must support
    /// (PAGESIZE bit 0), which is also the kit's allocation granule.
    /// </summary>
    internal const int PageSize = 4096;

    private readonly PciDeviceContext _context;

    /// <summary>Highest address the controller can reach: 4 GiB less one without HCCPARAMS1.AC64.</summary>
    private readonly ulong _maximumDeviceAddress;

    internal XhciMemory(PciDeviceContext context, bool is64BitCapable)
    {
        _context = context;
        _maximumDeviceAddress = is64BitCapable ? ulong.MaxValue : uint.MaxValue;
    }

    /// <summary>Allocates <paramref name="pageCount"/> zeroed pages. Probe, or thread context once Bound.</summary>
    /// <exception cref="InvalidOperationException">No memory the controller can reach is free; the kit logged why.</exception>
    internal DmaBuffer Allocate(int pageCount)
    {
        if (!_context.TryAllocateDma(pageCount * PageSize, _maximumDeviceAddress, out DmaBuffer? buffer))
        {
            throw new InvalidOperationException("No DMA memory the xHCI controller can reach is free.");
        }

        return buffer;
    }

    /// <summary>Gives pages back, once the controller references them no more. Thread context.</summary>
    internal void Free(DmaBuffer buffer) => _context.FreeDma(buffer);
}
