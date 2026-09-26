// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A window of device registers in I/O port space, such as a mapped PCI I/O
/// BAR. Only x64 has port space: on other architectures the kit never
/// creates one. Checked and ordered like <see cref="MmioRegion"/>, so a
/// driver can move between the two kinds of BAR without rethinking its
/// barriers; the accessors neither allocate nor block and may run in an
/// interrupt handler.
/// </summary>
internal sealed class PortRegion
{
    /// <summary>First port of the window.</summary>
    private readonly ushort _basePort;

    /// <summary>Set when the attempt that mapped the window is torn down; every access throws from then on.</summary>
    private bool _invalidated;

    /// <summary>Number of ports in the window. An access of n bytes is valid at any multiple of n up to Length minus n.</summary>
    public ushort Length { get; }

    internal PortRegion(ushort basePort, ushort length)
    {
        _basePort = basePort;
        Length = length;
    }

    /// <summary>Reads the byte at <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public byte Read8(ushort offset)
    {
        byte value = Native.IO.Read8(PortOf(offset, sizeof(byte)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 16-bit register at <paramref name="offset"/>, a multiple of 2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public ushort Read16(ushort offset)
    {
        ushort value = Native.IO.Read16(PortOf(offset, sizeof(ushort)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 32-bit register at <paramref name="offset"/>, a multiple of 4.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public uint Read32(ushort offset)
    {
        uint value = Native.IO.Read32(PortOf(offset, sizeof(uint)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Writes <paramref name="value"/> to the byte at <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write8(ushort offset, byte value)
    {
        ushort port = PortOf(offset, sizeof(byte));
        DmaOrdering.WriteBarrier();
        Native.IO.Write8(port, value);
    }

    /// <summary>Writes <paramref name="value"/> to the 16-bit register at <paramref name="offset"/>, a multiple of 2.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write16(ushort offset, ushort value)
    {
        ushort port = PortOf(offset, sizeof(ushort));
        DmaOrdering.WriteBarrier();
        Native.IO.Write16(port, value);
    }

    /// <summary>Writes <paramref name="value"/> to the 32-bit register at <paramref name="offset"/>, a multiple of 4.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt that mapped the window was torn down.</exception>
    public void Write32(ushort offset, uint value)
    {
        ushort port = PortOf(offset, sizeof(uint));
        DmaOrdering.WriteBarrier();
        Native.IO.Write32(port, value);
    }

    /// <summary>
    /// Makes every later access throw. Called when the binding attempt that
    /// mapped the window is torn down, so a driver still holding the region
    /// gets an exception rather than writing to a device that is no longer
    /// its own.
    /// </summary>
    internal void Invalidate() => _invalidated = true;

    /// <summary>
    /// Port of an access of <paramref name="size"/> bytes at
    /// <paramref name="offset"/>, after checking that the window is still
    /// valid, that the access fits in it and that it is naturally aligned.
    /// </summary>
    private ushort PortOf(ushort offset, int size)
    {
        if (_invalidated)
        {
            throw new InvalidOperationException("The binding attempt that mapped this region was torn down.");
        }

        if (offset >= Length || Length - offset < size)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The access runs past the end of the region.");
        }

        if ((offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The offset is not a multiple of the access size.");
        }

        return (ushort)(_basePort + offset);
    }
}
