// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A window of memory-mapped device registers, such as a mapped PCI memory
/// BAR. Every access is checked against the window's bounds and against the
/// natural alignment of its size, and is ordered like Linux's
/// <c>writel</c>/<c>readl</c>: a write is preceded by a DMA write barrier, so
/// the device sees every earlier store to DMA memory before the register
/// write that tells it to look, and a read is followed by a DMA read
/// barrier, so no later load from DMA memory runs ahead of the register read
/// that said the data is there. The accessors neither allocate nor block and
/// may run in an interrupt handler; a bad offset throws, which halts the
/// kernel there.
/// </summary>
internal sealed class MmioRegion
{
    /// <summary>Virtual address of the window's first byte.</summary>
    private readonly ulong _address;

    /// <summary>Set when the attempt that mapped the window is torn down; every access throws from then on.</summary>
    private bool _invalidated;

    /// <summary>Size of the window in bytes. An access of n bytes is valid at any multiple of n up to Length minus n.</summary>
    public ulong Length { get; }

    internal MmioRegion(ulong address, ulong length)
    {
        _address = address;
        Length = length;
    }

    /// <summary>Reads the byte at <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public byte Read8(ulong offset)
    {
        byte value = Native.MMIO.Read8(AddressOf(offset, sizeof(byte)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 16-bit register at <paramref name="offset"/>, a multiple of 2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public ushort Read16(ulong offset)
    {
        ushort value = Native.MMIO.Read16(AddressOf(offset, sizeof(ushort)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 32-bit register at <paramref name="offset"/>, a multiple of 4.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public uint Read32(ulong offset)
    {
        uint value = Native.MMIO.Read32(AddressOf(offset, sizeof(uint)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 64-bit register at <paramref name="offset"/>, a multiple of 8, in one access.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public ulong Read64(ulong offset)
    {
        ulong value = Native.MMIO.Read64(AddressOf(offset, sizeof(ulong)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Writes <paramref name="value"/> to the byte at <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write8(ulong offset, byte value)
    {
        ulong address = AddressOf(offset, sizeof(byte));
        DmaOrdering.WriteBarrier();
        Native.MMIO.Write8(address, value);
    }

    /// <summary>Writes <paramref name="value"/> to the 16-bit register at <paramref name="offset"/>, a multiple of 2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write16(ulong offset, ushort value)
    {
        ulong address = AddressOf(offset, sizeof(ushort));
        DmaOrdering.WriteBarrier();
        Native.MMIO.Write16(address, value);
    }

    /// <summary>Writes <paramref name="value"/> to the 32-bit register at <paramref name="offset"/>, a multiple of 4.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write32(ulong offset, uint value)
    {
        ulong address = AddressOf(offset, sizeof(uint));
        DmaOrdering.WriteBarrier();
        Native.MMIO.Write32(address, value);
    }

    /// <summary>Writes <paramref name="value"/> to the 64-bit register at <paramref name="offset"/>, a multiple of 8, in one access.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write64(ulong offset, ulong value)
    {
        ulong address = AddressOf(offset, sizeof(ulong));
        DmaOrdering.WriteBarrier();
        Native.MMIO.Write64(address, value);
    }

    /// <summary>
    /// Makes every later access throw. Called when the binding attempt that
    /// mapped the window is torn down, so a driver still holding the region
    /// gets an exception rather than writing to a device that is no longer
    /// its own.
    /// </summary>
    internal void Invalidate() => _invalidated = true;

    /// <summary>
    /// Virtual address of an access of <paramref name="size"/> bytes at
    /// <paramref name="offset"/>, after checking that the window is still
    /// valid, that the access fits in it and that it is naturally aligned.
    /// </summary>
    private ulong AddressOf(ulong offset, ulong size)
    {
        if (_invalidated)
        {
            throw new InvalidOperationException("The binding attempt that mapped this region was torn down.");
        }

        // Written as a remainder rather than offset + size > Length, which
        // an offset near ulong.MaxValue would wrap past.
        if (offset >= Length || Length - offset < size)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The access runs past the end of the region.");
        }

        // A misaligned access is split or refused differently by each
        // architecture and device, so it is refused here on all of them.
        if ((offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The offset is not a multiple of the access size.");
        }

        return _address + offset;
    }
}
