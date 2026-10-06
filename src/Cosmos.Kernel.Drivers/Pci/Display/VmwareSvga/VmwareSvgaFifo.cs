// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;

/// <summary>
/// The register and FIFO access of one VMware SVGA II adapter: the index
/// and value ports over BAR 0 through the kit's <see cref="RegisterWindow"/>,
/// the command FIFO over BAR 2 through a <see cref="DeviceRegion"/> viewed
/// as dwords, the FIFO initialisation with the SVGA3D negotiation, the mode
/// registers, the scanout enable shadow, the 2D commands (UPDATE, the
/// cursors) and the command reservation the SVGA3D layer builds on: a
/// command is placed contiguously when it fits before MAX and otherwise
/// appended dword by dword through the wrap, as the reference driver's
/// FIFOReserve and FIFOCommit do, so no dword between NEXT_CMD and MAX is
/// ever left for the host to parse. The port of the HAL's SVGA II driver
/// without its framebuffer half: the ring's canvas is the back buffer now.
/// Thread context; one caller at a time, as the ring's canvas is.
/// </summary>
internal sealed class VmwareSvgaFifo
{
    // --- Constants ---

    /// <summary>Byte offset of the index port in BAR 0.</summary>
    private const ulong IndexPortOffset = (ulong)IOPortOffset.Index;

    /// <summary>Byte offset of the value port in BAR 0, a 32-bit port at base + 1.</summary>
    private const ulong ValuePortOffset = (ulong)IOPortOffset.Value;

    /// <summary>Bytes in a FIFO dword.</summary>
    private const uint BytesPerDword = sizeof(uint);

    /// <summary>
    /// SVGA_FIFO_CAP_3D_HWVERSION_REVISED: the host publishes its 3D version
    /// in <see cref="Register3D.SVGA_FIFO_3D_HWVERSION_REVISED"/> rather than
    /// in the original register, which sits in guest-writable FIFO space.
    /// </summary>
    private const uint FifoCap3DHwVersionRevised = 1 << 8;

    /// <summary>SVGA3D_HWVERSION_WS65_B1 (2.0), the oldest SVGA3D the command layer targets. Anything below it counts as no 3D.</summary>
    private const uint MinimumHardwareVersion = 2u << 16;

    /// <summary>SVGA3D_HWVERSION_WS8_B1 (2.1), the SVGA3D version this driver declares to the host. The host clamps what it publishes to it.</summary>
    private const uint GuestHardwareVersion = (2u << 16) | 1u;

    /// <summary>The one cursor id the driver defines and shows.</summary>
    private const uint CursorId = 0;

    // --- Private fields ---

    private readonly RegisterWindow _registers;
    private readonly DeviceRegion _fifo;
    private uint _capabilities;
    private uint _fifoBytes;
    private bool _enabled;
    private bool _is3DNegotiated;
    private uint _svga3DVersion;

    // --- Constructor ---

    /// <summary>
    /// Takes the windows the probe mapped. Nothing is read or written until
    /// <see cref="Configure"/>. Thread context, from the probe.
    /// </summary>
    /// <param name="registers">BAR 0: the index port at offset 0 and the value port at offset 1.</param>
    /// <param name="fifo">BAR 2: the FIFO memory.</param>
    internal VmwareSvgaFifo(RegisterWindow registers, DeviceRegion fifo)
    {
        _registers = registers;
        _fifo = fifo;
    }

    // --- Properties ---

    /// <summary>The Capabilities register as the probe read it. Any context.</summary>
    internal uint Capabilities => _capabilities;

    /// <summary>The Enable register's shadow: seeded by <see cref="RecordEnabled"/> from the value the probe found, kept by <see cref="SetMode"/> and <see cref="SetEnabled"/>. Any context.</summary>
    internal bool IsEnabled => _enabled;

    /// <summary>True when the FIFO initialisation negotiated an SVGA3D version of at least <see cref="MinimumHardwareVersion"/>. Any context.</summary>
    internal bool Is3DNegotiated => _is3DNegotiated;

    /// <summary>The SVGA3D version the host published, 0 without 3D. Any context.</summary>
    internal uint Svga3DVersion => _svga3DVersion;

    /// <summary>True when the adapter composes a 32-bit alpha cursor on the host side. Any context.</summary>
    internal bool HasAlphaCursor => (_capabilities & (uint)Capability.AlphaCursor) != 0;

    /// <summary>The FIFO memory as bytes, for the SVGA3D layer's command bodies. Thread context.</summary>
    /// <exception cref="InvalidOperationException">The binding that mapped the FIFO was torn down.</exception>
    internal Span<byte> FifoBytes => _fifo.Span;

    // --- Registers ---

    /// <summary>
    /// Writes a register through a port window: the index, then the value.
    /// The static form serves the probe's version handshake, which runs
    /// before the FIFO is mapped. Thread context.
    /// </summary>
    /// <param name="registers">BAR 0: the index port at offset 0 and the value port at offset 1.</param>
    /// <param name="register">The register.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="InvalidOperationException">The binding that mapped the ports was torn down.</exception>
    internal static void WriteRegister(RegisterWindow registers, Register register, uint value)
    {
        registers.Write32(IndexPortOffset, (uint)register);
        registers.Write32(ValuePortOffset, value);
    }

    /// <summary>
    /// Reads a register through a port window: the index, then the value.
    /// The static form serves the probe's version handshake, which runs
    /// before the FIFO is mapped. Thread context.
    /// </summary>
    /// <param name="registers">BAR 0: the index port at offset 0 and the value port at offset 1.</param>
    /// <param name="register">The register.</param>
    /// <returns>The register's value.</returns>
    /// <exception cref="InvalidOperationException">The binding that mapped the ports was torn down.</exception>
    internal static uint ReadRegister(RegisterWindow registers, Register register)
    {
        registers.Write32(IndexPortOffset, (uint)register);
        return registers.Read32(ValuePortOffset);
    }

    /// <summary>Writes a register: the index, then the value. Thread context.</summary>
    /// <param name="register">The register.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="InvalidOperationException">The binding that mapped the ports was torn down.</exception>
    internal void WriteRegister(Register register, uint value) => WriteRegister(_registers, register, value);

    /// <summary>Reads a register: the index, then the value. Thread context.</summary>
    /// <param name="register">The register.</param>
    /// <returns>The register's value.</returns>
    /// <exception cref="InvalidOperationException">The binding that mapped the ports was torn down.</exception>
    internal uint ReadRegister(Register register) => ReadRegister(_registers, register);

    // --- FIFO registers ---

    /// <summary>Reads one of the four FIFO head registers. Thread context.</summary>
    /// <param name="register">The register, as a byte offset.</param>
    internal uint GetFifo(FIFO register) => ReadFifoDword((uint)register);

    /// <summary>Writes one of the four FIFO head registers. Thread context.</summary>
    /// <param name="register">The register, as a byte offset.</param>
    /// <param name="value">The value.</param>
    internal void SetFifo(FIFO register, uint value) => WriteFifoDword((uint)register, value);

    /// <summary>Reads a FIFO register slot, for the SVGA3D negotiation and the fence. Thread context.</summary>
    /// <param name="register">The slot, in dwords.</param>
    internal uint ReadFifo3D(Register3D register) => _fifo.As<uint>()[(int)register];

    /// <summary>Writes a FIFO register slot. Thread context.</summary>
    /// <param name="register">The slot, in dwords.</param>
    /// <param name="value">The value.</param>
    internal void WriteFifo3D(Register3D register, uint value)
    {
        _fifo.As<uint>()[(int)register] = value;
    }

    /// <summary>Reads a dword of FIFO memory. Thread context.</summary>
    /// <param name="byteOffset">The offset from the start of FIFO memory; the low two bits are dropped.</param>
    /// <exception cref="IndexOutOfRangeException">The offset is past the end of FIFO memory.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the FIFO was torn down.</exception>
    internal uint ReadFifoDword(uint byteOffset) => _fifo.As<uint>()[(int)(byteOffset / BytesPerDword)];

    /// <summary>Writes a dword of FIFO memory. Thread context.</summary>
    /// <param name="byteOffset">The offset from the start of FIFO memory; the low two bits are dropped.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="IndexOutOfRangeException">The offset is past the end of FIFO memory.</exception>
    /// <exception cref="InvalidOperationException">The binding that mapped the FIFO was torn down.</exception>
    internal void WriteFifoDword(uint byteOffset, uint value)
    {
        _fifo.As<uint>()[(int)(byteOffset / BytesPerDword)] = value;
    }

    // --- Initialisation ---

    /// <summary>
    /// Records what the probe read before the first FIFO initialisation:
    /// the Capabilities register and the FIFO's usable size (the MemSize
    /// register, clamped to the mapped region). Thread context, from the
    /// probe.
    /// </summary>
    /// <param name="capabilities">The Capabilities register.</param>
    /// <param name="fifoBytes">The FIFO's usable size in bytes.</param>
    internal void Configure(uint capabilities, uint fifoBytes)
    {
        _capabilities = capabilities;
        _fifoBytes = fifoBytes;
    }

    /// <summary>
    /// Seeds the Enable register's shadow from the value the probe read,
    /// so a scanout the firmware enabled is flushed from the start: without
    /// it <see cref="IsEnabled"/> would read false against a register
    /// holding 1 and every UPDATE would be dropped. Thread context, from
    /// the probe.
    /// </summary>
    /// <param name="enabled">Whether the Enable register read 1.</param>
    internal void RecordEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    /// <summary>
    /// Initialises the FIFO: MIN past the register slots, MAX at the FIFO's
    /// end, NEXT_CMD and STOP at MIN, the guest's SVGA3D version declared,
    /// ConfigDone, then the SVGA3D negotiation read back. No Enable write:
    /// enabling the adapter with the mode registers unprogrammed wedges
    /// QEMU's display refresh; <see cref="SetMode"/> enables it once the
    /// registers are valid. Thread context.
    /// </summary>
    internal void InitializeFifo()
    {
        uint min = (uint)Register.FifoNumRegisters * BytesPerDword;
        SetFifo(FIFO.Min, min);
        SetFifo(FIFO.Max, _fifoBytes);
        SetFifo(FIFO.NextCmd, min);
        SetFifo(FIFO.Stop, min);

        DeclareGuestSvga3D();
        WriteRegister(Register.ConfigDone, 1);
        NegotiateSvga3D();
    }

    /// <summary>
    /// Programs a mode: the width, height and depth registers, the Enable
    /// register, then the FIFO initialisation again, which the mode change
    /// resets. The caller reads the pitch and the frame offset afterwards.
    /// Thread context.
    /// </summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="bitsPerPixel">Bits per pixel.</param>
    internal void SetMode(uint width, uint height, uint bitsPerPixel)
    {
        WriteRegister(Register.Width, width);
        WriteRegister(Register.Height, height);
        WriteRegister(Register.BitsPerPixel, bitsPerPixel);
        SetEnabled(true);
        InitializeFifo();
    }

    /// <summary>Writes the Enable register and keeps its shadow. Thread context.</summary>
    /// <param name="enabled">True to enable the scanout and the FIFO consumption, false to stop both.</param>
    internal void SetEnabled(bool enabled)
    {
        WriteRegister(Register.Enable, enabled ? 1u : 0u);
        _enabled = enabled;
    }

    // --- FIFO commands ---

    /// <summary>
    /// Asks the host to consume the FIFO and waits until it is idle. Spins
    /// forever on a disabled adapter, which consumes nothing: callers gate
    /// on <see cref="IsEnabled"/>. Thread context.
    /// </summary>
    internal void WaitForFifo()
    {
        WriteRegister(Register.Sync, 1);
        while (ReadRegister(Register.Busy) != 0)
        {
        }
    }

    /// <summary>Appends one dword at NEXT_CMD, waiting for room when the FIFO is full and wrapping at MAX. Thread context.</summary>
    /// <param name="value">The dword.</param>
    internal void WriteToFifo(uint value)
    {
        uint next = GetFifo(FIFO.NextCmd);
        uint max = GetFifo(FIFO.Max);
        uint min = GetFifo(FIFO.Min);
        uint stop = GetFifo(FIFO.Stop);
        if ((next == max - BytesPerDword && stop == min) || next + BytesPerDword == stop)
        {
            WaitForFifo();
        }

        WriteFifoDword(next, value);
        next += BytesPerDword;
        SetFifo(FIFO.NextCmd, next == max ? min : next);
    }

    /// <summary>Appends a command's bytes at NEXT_CMD dword by dword, waiting for room and wrapping at MAX as <see cref="WriteToFifo(uint)"/> does: the commit of a command <see cref="TryReserveFifo"/> could not place contiguously. Thread context.</summary>
    /// <param name="bytes">The command, a multiple of 4 bytes long.</param>
    internal void WriteToFifo(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<uint> dwords = MemoryMarshal.Cast<byte, uint>(bytes);
        for (int i = 0; i < dwords.Length; i++)
        {
            WriteToFifo(dwords[i]);
        }
    }

    /// <summary>
    /// Reserves a contiguous command area of <paramref name="bytes"/> at
    /// NEXT_CMD and advances NEXT_CMD past it, by the reference driver's
    /// FIFOReserve rule: the area is placed when it ends before MAX (or at
    /// MAX with STOP past MIN, so the wrapped NEXT_CMD differs from STOP)
    /// and, with STOP ahead of NEXT_CMD, ends before STOP, leaving at least
    /// one dword between NEXT_CMD and STOP, since NEXT_CMD equal to STOP
    /// reads as an empty FIFO. The host is waited for while the FIFO holds
    /// too little room in all. NEXT_CMD is never jumped to MIN: the host
    /// consumes the ring from STOP through MAX and on from MIN, so a jump
    /// would leave the dwords up to MAX for it to parse as commands. A
    /// command that would straddle MAX is not placed; the caller appends it
    /// through <see cref="WriteToFifo(ReadOnlySpan{byte})"/>
    /// instead. Thread context.
    /// </summary>
    /// <param name="bytes">The area's size in bytes, a multiple of 4.</param>
    /// <param name="offset">The byte offset of the area from the start of FIFO memory when placed.</param>
    /// <returns>True with the area reserved; false when the command must be appended through the wrap.</returns>
    /// <exception cref="InvalidOperationException">The command is at least as large as the FIFO and could never be consumed whole.</exception>
    internal bool TryReserveFifo(uint bytes, out uint offset)
    {
        uint min = GetFifo(FIFO.Min);
        uint max = GetFifo(FIFO.Max);
        if (bytes >= max - min)
        {
            throw new InvalidOperationException($"command of {bytes} bytes does not fit the {max - min} byte FIFO");
        }

        while (true)
        {
            uint next = GetFifo(FIFO.NextCmd);
            uint stop = GetFifo(FIFO.Stop);
            if (next >= stop)
            {
                if (next + bytes < max || (next + bytes == max && stop > min))
                {
                    break;
                }

                if (FreeSpace(next, stop, min, max) > bytes)
                {
                    offset = 0;
                    return false;
                }
            }
            else if (next + bytes < stop)
            {
                break;
            }

            WaitForFifo();
        }

        offset = GetFifo(FIFO.NextCmd);
        uint newNext = offset + bytes;
        SetFifo(FIFO.NextCmd, newNext == max ? min : newNext);
        return true;
    }

    /// <summary>UPDATE: makes a rectangle of the frame visible, then waits for the host to consume it. Thread context.</summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    internal void Update(uint x, uint y, uint width, uint height)
    {
        WriteToFifo((uint)FIFOCommand.Update);
        WriteToFifo(x);
        WriteToFifo(y);
        WriteToFifo(width);
        WriteToFifo(height);
        WaitForFifo();
    }

    /// <summary>DEFINE_ALPHA_CURSOR: uploads the cursor image, premultiplied 32-bit pixels row by row. Thread context.</summary>
    /// <param name="hotspotX">The hotspot's column within the image.</param>
    /// <param name="hotspotY">The hotspot's row within the image.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="pixels">The pixels, exactly width times height of them: the caller has checked the count, so the header and the payload agree.</param>
    internal void DefineAlphaCursor(uint hotspotX, uint hotspotY, uint width, uint height, ReadOnlySpan<uint> pixels)
    {
        WriteToFifo((uint)FIFOCommand.DEFINE_ALPHA_CURSOR);
        WriteToFifo(CursorId);
        WriteToFifo(hotspotX);
        WriteToFifo(hotspotY);
        WriteToFifo(width);
        WriteToFifo(height);
        for (int i = 0; i < pixels.Length; i++)
        {
            WriteToFifo(pixels[i]);
        }
    }

    /// <summary>
    /// Moves the cursor and shows or hides it through the cursor registers.
    /// CursorOn is written last: that is the write that latches the new id
    /// and position on the host. Thread context.
    /// </summary>
    /// <param name="visible">True to show the cursor.</param>
    /// <param name="x">The hotspot's column on the display.</param>
    /// <param name="y">The hotspot's row on the display.</param>
    internal void SetCursor(bool visible, uint x, uint y)
    {
        WriteRegister(Register.CursorID, CursorId);
        WriteRegister(Register.CursorX, x);
        WriteRegister(Register.CursorY, y);
        WriteRegister(Register.CursorOn, visible ? 1u : 0u);
    }

    // --- Private methods ---

    /// <summary>The bytes free between NEXT_CMD and STOP, counting the wrap.</summary>
    private static uint FreeSpace(uint next, uint stop, uint min, uint max) =>
        next >= stop ? (max - next) + (stop - min) : stop - next;

    /// <summary>
    /// Tells the host which SVGA3D version this driver speaks. Runs before
    /// ConfigDone hands the host the FIFO layout: the host answers with its
    /// own version only for a guest that declared one, and publishes 0
    /// (which reads as no 3D) for a guest that stayed silent.
    /// </summary>
    private void DeclareGuestSvga3D()
    {
        if (!HasSvga3DFifoRegisters())
        {
            return;
        }

        if ((ReadFifo3D(Register3D.SVGA_FIFO_CAPABILITIES) & FifoCap3DHwVersionRevised) == 0)
        {
            return;
        }

        if (GetFifo(FIFO.Min) <= (uint)Register3D.SVGA_FIFO_GUEST_3D_HWVERSION * BytesPerDword)
        {
            return;
        }

        WriteFifo3D(Register3D.SVGA_FIFO_GUEST_3D_HWVERSION, GuestHardwareVersion);
    }

    /// <summary>
    /// Reads back the SVGA3D version the host settled on. Must run after
    /// ConfigDone: the host publishes its read-only FIFO registers, the 3D
    /// hardware version among them, only once the guest has handed it the
    /// FIFO layout. Which register carries the version is a FIFO
    /// capability, published in the FIFO itself.
    /// </summary>
    private void NegotiateSvga3D()
    {
        _is3DNegotiated = false;
        _svga3DVersion = 0;
        if (!HasSvga3DFifoRegisters())
        {
            return;
        }

        uint fifoCapabilities = ReadFifo3D(Register3D.SVGA_FIFO_CAPABILITIES);
        _svga3DVersion = (fifoCapabilities & FifoCap3DHwVersionRevised) != 0
            ? ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION_REVISED)
            : ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION);
        _is3DNegotiated = _svga3DVersion >= MinimumHardwareVersion;
    }

    /// <summary>Whether the adapter carries the FIFO registers the SVGA3D negotiation reads and writes at all.</summary>
    private bool HasSvga3DFifoRegisters()
    {
        if ((_capabilities & (uint)Capability.ExtendedFifo) == 0)
        {
            return false;
        }

        if ((_capabilities & (uint)Capability.Cap3D) == 0)
        {
            return false;
        }

        return GetFifo(FIFO.Min) > (uint)Register3D.SVGA_FIFO_3D_HWVERSION * BytesPerDword;
    }
}
