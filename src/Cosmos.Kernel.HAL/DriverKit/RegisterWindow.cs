// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A window of device registers, mapped through <see cref="DeviceBinding.MapRegisters"/>.
/// Each accessor is one access of its width, in program order, with the
/// ordering the architecture needs: a write is preceded by a DMA write
/// barrier, so the device sees every earlier store to DMA memory before the
/// register write that tells it to look, and a read is followed by a DMA read
/// barrier, so no later load from DMA memory runs ahead of the register that
/// said the data is there. Over a memory window the accesses are device-memory
/// loads and stores; over a port range on x64 they are <c>in</c> and
/// <c>out</c>, and the same driver code works over either. Offsets are checked
/// against the window's bounds, and over a memory window against the access
/// width's alignment; ports are byte addressed, so a port range accepts any
/// offset. The accessors neither allocate nor block, so an interrupt handler
/// may use them.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class RegisterWindow : IKitResource
{
    private readonly ulong _address;
    private readonly bool _isPortRange;

    /// <summary>Set by the binding's teardown; every access throws from then on. Checked in every build.</summary>
    private volatile bool _invalidated;

    /// <summary>Length of the window in bytes (or ports). An access of n bytes is valid at any multiple of n up to Length minus n over a memory window, and at any offset up to Length minus n over a port range.</summary>
    public ulong Length { get; }

    internal RegisterWindow(ulong address, ulong length, bool isPortRange)
    {
        _address = address;
        _isPortRange = isPortRange;
        Length = length;
    }

    /// <summary>Reads the byte register at <paramref name="offset"/>.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public byte Read8(ulong offset)
    {
        ulong address = AddressOf(offset, sizeof(byte));
        byte value = _isPortRange ? Native.IO.Read8((ushort)address) : Native.MMIO.Read8(address);
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 16-bit register at <paramref name="offset"/>, a multiple of 2 over a memory window; any offset over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public ushort Read16(ulong offset)
    {
        ulong address = AddressOf(offset, sizeof(ushort));
        ushort value = _isPortRange ? Native.IO.Read16((ushort)address) : Native.MMIO.Read16(address);
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 32-bit register at <paramref name="offset"/>, a multiple of 4 over a memory window; any offset over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public uint Read32(ulong offset)
    {
        ulong address = AddressOf(offset, sizeof(uint));
        uint value = _isPortRange ? Native.IO.Read32((ushort)address) : Native.MMIO.Read32(address);
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Reads the 64-bit register at <paramref name="offset"/>, a multiple of 8, in one access. Not available over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    /// <exception cref="NotSupportedException">The window is a port range, which has no 64-bit access.</exception>
    public ulong Read64(ulong offset)
    {
        ThrowIfPortRange();
        ulong value = Native.MMIO.Read64(AddressOf(offset, sizeof(ulong)));
        DmaOrdering.ReadBarrier();
        return value;
    }

    /// <summary>Writes <paramref name="value"/> to the byte register at <paramref name="offset"/>.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <param name="value">Value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public void Write8(ulong offset, byte value)
    {
        ulong address = AddressOf(offset, sizeof(byte));
        DmaOrdering.WriteBarrier();
        if (_isPortRange)
        {
            Native.IO.Write8((ushort)address, value);
        }
        else
        {
            Native.MMIO.Write8(address, value);
        }
    }

    /// <summary>Writes <paramref name="value"/> to the 16-bit register at <paramref name="offset"/>, a multiple of 2 over a memory window; any offset over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <param name="value">Value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public void Write16(ulong offset, ushort value)
    {
        ulong address = AddressOf(offset, sizeof(ushort));
        DmaOrdering.WriteBarrier();
        if (_isPortRange)
        {
            Native.IO.Write16((ushort)address, value);
        }
        else
        {
            Native.MMIO.Write16(address, value);
        }
    }

    /// <summary>Writes <paramref name="value"/> to the 32-bit register at <paramref name="offset"/>, a multiple of 4 over a memory window; any offset over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <param name="value">Value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    public void Write32(ulong offset, uint value)
    {
        ulong address = AddressOf(offset, sizeof(uint));
        DmaOrdering.WriteBarrier();
        if (_isPortRange)
        {
            Native.IO.Write32((ushort)address, value);
        }
        else
        {
            Native.MMIO.Write32(address, value);
        }
    }

    /// <summary>Writes <paramref name="value"/> to the 64-bit register at <paramref name="offset"/>, a multiple of 8, in one access. Not available over a port range.</summary>
    /// <param name="offset">Offset from the start of the window.</param>
    /// <param name="value">Value to write.</param>
    /// <exception cref="ArgumentOutOfRangeException">The access runs past the window or is misaligned.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the window was torn down.</exception>
    /// <exception cref="NotSupportedException">The window is a port range, which has no 64-bit access.</exception>
    public void Write64(ulong offset, ulong value)
    {
        ThrowIfPortRange();
        ulong address = AddressOf(offset, sizeof(ulong));
        DmaOrdering.WriteBarrier();
        Native.MMIO.Write64(address, value);
    }

    /// <summary>
    /// Makes every later access throw. Called by the binding's teardown, so a
    /// driver still holding the window gets an exception instead of writing
    /// to a device that is no longer its own.
    /// </summary>
    internal void Invalidate() => _invalidated = true;

    void IKitResource.Release() => Invalidate();

    private void ThrowIfPortRange()
    {
        if (_isPortRange)
        {
            throw new NotSupportedException("A port range has no 64-bit access.");
        }
    }

    /// <summary>
    /// Address (or port) of an access of <paramref name="size"/> bytes at
    /// <paramref name="offset"/>, after checking that the window is still
    /// valid, that the access fits in it and, over a memory window, that it
    /// is naturally aligned.
    /// </summary>
    private ulong AddressOf(ulong offset, ulong size)
    {
        if (_invalidated)
        {
            throw new InvalidOperationException("The binding that mapped this register window was torn down.");
        }

        // Written as a remainder rather than offset + size > Length, which an
        // offset near ulong.MaxValue would wrap past.
        if (offset >= Length || Length - offset < size)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The access runs past the end of the register window.");
        }

        // A misaligned memory access is split or refused differently by each
        // architecture and device, so it is refused here on all of them. x86
        // port I/O is byte addressed (the SVGA value port sits at base + 1
        // and is a 32-bit port), so a port range accepts any offset.
        if (!_isPortRange && (offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset is not a multiple of the access width.");
        }

        return _address + offset;
    }
}
