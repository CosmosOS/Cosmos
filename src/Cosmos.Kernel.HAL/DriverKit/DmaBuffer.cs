// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

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
/// reference.
/// </summary>
internal sealed unsafe class DmaBuffer : IKitResource
{
    /// <summary>Virtual address of the first byte, inside the pages freed on release.</summary>
    private readonly ulong _address;

    /// <summary>First byte of the pages the buffer was carved from, what the allocator gets back.</summary>
    private readonly ulong _pagesAddress;

    /// <summary>Set once the pages are freed; every access throws from then on. Checked in every build.</summary>
    private volatile bool _released;

    /// <summary>The address the device uses for the buffer's first byte.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>Length in bytes, as requested.</summary>
    public int Length { get; }

    internal DmaBuffer(ulong address, ulong physicalAddress, int length, ulong pagesAddress)
    {
        _address = address;
        _pagesAddress = pagesAddress;
        PhysicalAddress = physicalAddress;
        Length = length;
    }

    /// <summary>The buffer as the CPU reads and writes it. Allocation-free, so an interrupt handler may use it.</summary>
    /// <exception cref="InvalidOperationException">The binding that allocated the buffer was torn down and its memory freed.</exception>
    public Span<byte> Span
    {
        get
        {
            if (_released)
            {
                throw new InvalidOperationException("The binding that allocated this DMA buffer was torn down, and its memory freed.");
            }

            return new Span<byte>((void*)_address, Length);
        }
    }

    /// <summary>
    /// Orders loads from DMA memory: every load before it completes before
    /// any load after it. Use after reading a flag the device wrote and
    /// before reading what the flag says is valid.
    /// </summary>
    public static void ReadBarrier() => DmaOrdering.ReadBarrier();

    /// <summary>
    /// Orders stores to DMA memory: every store before it is visible to the
    /// device before any store after it. Use between filling a descriptor and
    /// the store that hands it to the device.
    /// </summary>
    public static void WriteBarrier() => DmaOrdering.WriteBarrier();

    /// <summary>Frees the pages and makes every later <see cref="Span"/> throw. Teardown only.</summary>
    internal void Release()
    {
        if (_released)
        {
            return;
        }

        _released = true;
        PageAllocator.Free((void*)_pagesAddress);
    }

    void IKitResource.Release() => Release();
}
