// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Resources;

/// <summary>
/// Physically contiguous, zeroed memory a device reads and writes directly,
/// allocated through <see cref="DeviceBinding.AllocateDma"/>. The CPU sees it
/// through <see cref="Span"/>, the device through <see cref="PhysicalAddress"/>.
/// DMA is coherent on the machines the kernel runs on, so only ordering is
/// needed: <see cref="WriteBarrier"/> between filling a descriptor and the
/// store that hands it over, <see cref="ReadBarrier"/> between a flag the
/// device wrote and the data it guards. A <see cref="RegisterWindow"/> access
/// carries its own barriers. Managed arrays are not DMA memory: the pinned heap
/// is collected when nothing references an array, and a device holds no
/// reference. <see cref="Region"/> is the same memory as a
/// <see cref="DeviceRegion"/>, for a display whose framebuffer is DMA memory.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DmaBuffer : IKitResource
{
    /// <summary>The message of the exception every access throws once the pages are freed; the kit's ring code throws the same one.</summary>
    internal const string ReleasedMessage = "The binding that allocated this DMA buffer was torn down, and its memory freed.";

    /// <summary>The buffer's bytes, inside the pages freed on release.</summary>
    private readonly MemoryBlock _block;

    /// <summary>The pages the buffer was carved from, what the allocator gets back.</summary>
    private readonly MemoryBlock _pages;

    /// <summary>The one region over the buffer, created with it; invalidated by <see cref="Release"/>.</summary>
    private readonly DeviceRegion _region;

    /// <summary>Set once the pages are freed; every access throws from then on. Checked in every build.</summary>
    private volatile bool _released;

    /// <summary>The address the device uses for the buffer's first byte.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>Length in bytes, as requested.</summary>
    public int Length { get; }

    /// <summary>
    /// The buffer as a <see cref="DeviceRegion"/>, one instance for the
    /// buffer's lifetime, created with it. Throws once released, and the
    /// region itself throws from then on, so a driver that handed it out
    /// (an <see cref="IDisplay.Framebuffer"/> over DMA memory) never
    /// exposes freed pages. The region is not in the binding's ledger: the
    /// buffer's own release invalidates it. Any context; allocation-free.
    /// </summary>
    /// <exception cref="InvalidOperationException">The binding that allocated the buffer was torn down and its memory freed.</exception>
    public DeviceRegion Region
    {
        get
        {
            if (_released)
            {
                throw new InvalidOperationException(ReleasedMessage);
            }

            return _region;
        }
    }

    /// <summary>The buffer as the CPU reads and writes it. Allocation-free, so an interrupt handler may use it.</summary>
    /// <exception cref="InvalidOperationException">The binding that allocated the buffer was torn down and its memory freed.</exception>
    public Span<byte> Span
    {
        get
        {
            if (_released)
            {
                throw new InvalidOperationException(ReleasedMessage);
            }

            return _block.Span;
        }
    }

    /// <summary>True once the pages went back to the allocator: a volatile read, for the kit's ring code to refuse a ring access. Any context; allocation-free.</summary>
    internal bool IsReleased => _released;

    internal DmaBuffer(ulong address, ulong physicalAddress, int length, MemoryBlock pages)
    {
        _block = new MemoryBlock(address, (uint)length);
        _pages = pages;
        PhysicalAddress = physicalAddress;
        Length = length;
        _region = new DeviceRegion(address, (ulong)length, RegionCaching.Normal);
    }

    /// <summary>
    /// Orders loads from DMA memory: every load before it completes before
    /// any load after it. Use after reading a flag the device wrote and
    /// before reading what the flag says is valid.
    /// </summary>
    public static void ReadBarrier()
    {
        DmaOrdering.ReadBarrier();
    }

    /// <summary>
    /// Orders stores to DMA memory: every store before it is visible to the
    /// device before any store after it. Use between filling a descriptor and
    /// the store that hands it to the device.
    /// </summary>
    public static void WriteBarrier()
    {
        DmaOrdering.WriteBarrier();
    }

    /// <summary>Frees the pages and makes every later <see cref="Span"/> and <see cref="Region"/> access throw. Teardown only.</summary>
    internal void Release()
    {
        if (_released)
        {
            return;
        }

        _released = true;
        _region.Invalidate();
        PageAllocator.Free(_pages);
    }

    void IKitResource.Release()
    {
        Release();
    }
}
