// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// Physically contiguous, zeroed memory a device reads and writes by DMA,
/// allocated through a device context. The CPU sees it through
/// <see cref="Span"/>, the device through <see cref="DeviceAddress"/>. DMA is
/// assumed coherent: no cache maintenance is needed, only ordering, which the
/// register accessors provide around each MMIO access and
/// <see cref="ReadBarrier"/> and <see cref="WriteBarrier"/> provide between
/// two accesses to DMA memory.
/// </summary>
internal sealed class DmaBuffer
{
    /// <summary>Virtual address of the buffer's first byte.</summary>
    private readonly ulong _address;

    /// <summary>Set once the attempt that allocated the buffer is torn down and its pages are freed.</summary>
    private bool _released;

    /// <summary>
    /// The address the device uses for the buffer's first byte: the physical
    /// address today, an IOMMU address once the kit programs one.
    /// </summary>
    public ulong DeviceAddress { get; }

    /// <summary>Size of the buffer in bytes, as requested.</summary>
    public int Length { get; }

    /// <summary>The buffer as the CPU reads and writes it. Allocation-free, so an interrupt handler may use it.</summary>
    /// <exception cref="InvalidOperationException">The binding attempt that allocated the buffer was torn down, and its memory freed.</exception>
    public unsafe Span<byte> Span
    {
        get
        {
            if (_released)
            {
                throw new InvalidOperationException("The binding attempt that allocated this buffer was torn down, and its memory freed.");
            }

            return new Span<byte>((void*)_address, Length);
        }
    }

    internal DmaBuffer(ulong address, ulong deviceAddress, int length)
    {
        _address = address;
        DeviceAddress = deviceAddress;
        Length = length;
    }

    /// <summary>
    /// Orders loads from DMA memory: every load before it completes before
    /// any load after it. Use after reading a device-written flag, such as a
    /// descriptor's done bit, and before reading what the flag says is valid,
    /// when no register read comes in between. Allocation-free.
    /// </summary>
    public static void ReadBarrier() => DmaOrdering.ReadBarrier();

    /// <summary>
    /// Orders stores to DMA memory: every store before it is visible to the
    /// device before any store after it. Use between filling a descriptor and
    /// the store that hands it to the device, when no register write comes in
    /// between. Allocation-free.
    /// </summary>
    public static void WriteBarrier() => DmaOrdering.WriteBarrier();

    /// <summary>
    /// Frees the buffer's pages and makes <see cref="Span"/> throw. Called
    /// when the binding attempt that allocated it is torn down, once the
    /// device can no longer master into it.
    /// </summary>
    internal unsafe void Release()
    {
        _released = true;
        PageAllocator.Free((void*)_address);
    }
}
