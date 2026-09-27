// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

/// <summary>
/// A mapped BAR as this driver reads and writes it: dwords at byte offsets,
/// filled and copied in bulk. The adapter's two big windows are its
/// framebuffer and its command FIFO, and both are worked a scanline or a
/// command at a time, which the kit's <see cref="MmioRegion"/> accessors are
/// the wrong shape for — one barrier per pixel would make a screen clear
/// three quarters of a million ordered writes.
/// </summary>
/// <remarks>
/// The pointer is taken once, from <see cref="MmioRegion.GetSpan"/>, which
/// checks it against the window; every access here is then checked against
/// <see cref="Size"/> alone, as the driver's was before the kit owned the
/// mapping. Ordering is the caller's: the FIFO is handed to the host by a
/// register write, and <see cref="MmioRegion.Write32"/> carries the barrier
/// that publishes everything written here first.
/// </remarks>
internal readonly unsafe struct DeviceMemory
{
    private readonly uint* _base;

    /// <summary>Size of the window in bytes.</summary>
    internal uint Size { get; }

    /// <summary>True when the window was never mapped.</summary>
    internal bool IsEmpty => _base is null;

    /// <summary>
    /// Takes the whole of <paramref name="region"/>, or as much of it as a
    /// span can address.
    /// </summary>
    internal DeviceMemory(MmioRegion region, uint size)
    {
        Size = size;
        _base = size == 0
            ? null
            : (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(region.GetSpan(0, (int)size)));
    }

    /// <summary>The dword at <paramref name="byteOffset"/>, which must be a multiple of 4.</summary>
    internal uint this[uint byteOffset]
    {
        get
        {
            ThrowIfPastDword(byteOffset);
            return _base[byteOffset >> 2];
        }

        set
        {
            ThrowIfPastDword(byteOffset);
            _base[byteOffset >> 2] = value;
        }
    }

    /// <summary>Writes <paramref name="count"/> dwords of <paramref name="value"/> from <paramref name="byteOffset"/>.</summary>
    internal void Fill(uint byteOffset, uint count, uint value)
    {
        Span(byteOffset, count).Fill(value);
    }

    /// <summary>Writes <paramref name="count"/> dwords of <paramref name="value"/> from <paramref name="byteOffset"/>.</summary>
    internal void Fill(int byteOffset, int count, int value)
    {
        if (byteOffset < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        }

        Span((uint)byteOffset, (uint)count).Fill((uint)value);
    }

    /// <summary>
    /// The <paramref name="count"/> dwords at <paramref name="byteOffset"/>,
    /// for a caller copying a run in or out in one go.
    /// </summary>
    internal Span<uint> Span(uint byteOffset, uint count)
    {
        ThrowIfPastDword(byteOffset, count * sizeof(uint));
        return new Span<uint>(_base + (byteOffset >> 2), (int)count);
    }

    /// <summary>
    /// Copies <paramref name="count"/> bytes from <paramref name="byteOffset"/>
    /// into <paramref name="dest"/> at <paramref name="destIndex"/>, counted in
    /// ints.
    /// </summary>
    internal void Get(int byteOffset, int[] dest, int destIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(dest);
        if (byteOffset < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        }

        ThrowIfPast((uint)byteOffset, (uint)count);
        ReadOnlySpan<byte> source = new((byte*)_base + byteOffset, count);
        source.CopyTo(MemoryMarshal.AsBytes(dest.AsSpan(destIndex)));
    }

    /// <summary>
    /// Copies <paramref name="count"/> bytes within the window, from
    /// <paramref name="source"/> to <paramref name="destination"/>, which is
    /// how the back buffer reaches the visible frame.
    /// </summary>
    internal void Move(uint destination, uint source, uint count)
    {
        ThrowIfPast(destination, count);
        ThrowIfPast(source, count);
        Span<byte> window = new((byte*)_base, (int)Size);
        window.Slice((int)source, (int)count).CopyTo(window[(int)destination..]);
    }

    /// <summary>
    /// A byte pointer <paramref name="byteOffset"/> into the window, for the
    /// row-at-a-time blits that copy straight from a caller's pixel buffer.
    /// </summary>
    internal byte* BytePointerTo(uint byteOffset, uint length)
    {
        ThrowIfPast(byteOffset, length);
        return (byte*)_base + byteOffset;
    }

    /// <summary>
    /// Checks a dword access: bounds, plus the multiple-of-4 the
    /// <c>&gt;&gt; 2</c> in the accessors would otherwise swallow.
    /// </summary>
    private void ThrowIfPastDword(uint byteOffset, uint length = sizeof(uint))
    {
        ThrowIfPast(byteOffset, length);

        if ((byteOffset & 3) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset), "The offset is not a multiple of 4.");
        }
    }

    private void ThrowIfPast(uint byteOffset, uint length = sizeof(uint))
    {
        if (_base is null)
        {
            throw new InvalidOperationException("The adapter's memory window was never mapped.");
        }

        if (byteOffset > Size || Size - byteOffset < length)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset), "The access runs past the end of the window.");
        }
    }

    /// <summary>
    /// A pointer <paramref name="byteOffset"/> into the window, for the FIFO
    /// command builders, which write a struct in place and hand the reservation
    /// on. Checked once, here; the caller stays inside the bytes it reserved.
    /// </summary>
    internal void* PointerTo(uint byteOffset, uint length)
    {
        ThrowIfPastDword(byteOffset, length);
        return _base + (byteOffset >> 2);
    }
}
