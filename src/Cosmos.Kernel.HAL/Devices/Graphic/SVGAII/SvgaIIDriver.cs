using System;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

/// <summary>
/// VMware SVGA II display driver. Binds to one <see cref="PciDevice"/> and owns
/// its register ports (BAR0), VRAM (BAR1) and command FIFO (BAR2). Implements
/// the <see cref="GraphicDevice"/> contract over a double-buffered framebuffer:
/// drawing lands in the back buffer at <see cref="FrameSize"/>, and
/// <see cref="Swap"/> copies it to the visible frame before submitting an
/// Update through the FIFO. The SVGA3D command layer is
/// <see cref="VMWareSVGAII3D"/>, built on top of this driver.
/// </summary>
internal unsafe class SvgaIIDriver : GraphicDevice
{
    /// <summary>
    /// Video memory block.
    /// </summary>
    public MemoryBlock VideoMemory { get; }

    /// <summary>
    /// FIFO memory block.
    /// </summary>
    private MemoryBlock _fifoMemory = null!;

    /// <summary>
    /// The bound PCI device.
    /// </summary>
    private readonly PciDevice _device;

    /// <summary>
    /// Base of the SVGA register I/O ports (BAR0): index at +0, value at +1.
    /// Both are 32-bit ports despite the 1-byte spacing.
    /// </summary>
    private readonly ushort _basePort;

    /// <summary>
    /// Height.
    /// </summary>
    private uint _height;

    /// <summary>
    /// Width.
    /// </summary>
    private uint _width;

    /// <summary>
    /// Depth in bytes per pixel.
    /// </summary>
    private uint _depth;

    /// <summary>
    /// Capabilities.
    /// </summary>
    public uint Capabilities { get; }

    public uint FrameSize { get; private set; }
    public uint FrameOffset { get; private set; }

    /// <summary>
    /// Whether the device supports the 32-bit alpha hardware cursor
    /// (composed host-side; QEMU's vmware-svga does not advertise it).
    /// </summary>
    public bool HasAlphaCursor => (Capabilities & (uint)Capability.AlphaCursor) != 0;

    /// <summary>
    /// SVGA_FIFO_CAP_3D_HWVERSION_REVISED: the host publishes its 3D version
    /// in <see cref="Register3D.SVGA_FIFO_3D_HWVERSION_REVISED"/> rather than
    /// in the original register, which sits in guest-writable FIFO space.
    /// </summary>
    private const uint FifoCap3DHwVersionRevised = 1 << 8;

    /// <summary>
    /// SVGA3D_HWVERSION_WS65_B1 (2.0), the oldest SVGA3D the command layer
    /// targets. Anything below it counts as no 3D.
    /// </summary>
    private const uint MinimumHardwareVersion = 2u << 16;

    /// <summary>
    /// SVGA3D_HWVERSION_WS8_B1 (2.1), the SVGA3D version this driver declares
    /// to the host. The host clamps what it publishes to it.
    /// </summary>
    private const uint GuestHardwareVersion = (2u << 16) | 1u;

    /// <summary>
    /// Whether the device negotiated SVGA3D support during FIFO initialization.
    /// </summary>
    public bool Is3DEnabled { get; private set; }

    /// <summary>
    /// Negotiated SVGA3D hardware version.
    /// </summary>
    public uint HW3DVer { get; private set; }

    public SvgaIIDriver(PciDevice device)
    {
        _device = device;
        _device.EnableMemory(true);
        _basePort = (ushort)_device.BaseAddressBar[0].BaseAddress;

        WriteRegister(Register.ID, (uint)ID.V2);
        if (ReadRegister(Register.ID) != (uint)ID.V2)
        {
            throw new Exception("VMware SVGA II device did not accept the version 2 protocol");
        }

        // FrameBufferStart is the physical VRAM BAR; Limine base revision >= 1
        // has no lower-half identity map, so CPU access goes through the HHDM.
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        uint fbPhys = ReadRegister(Register.FrameBufferStart);
        VideoMemory = new MemoryBlock(hhdmOffset + fbPhys, ReadRegister(Register.VRamSize));
        Capabilities = ReadRegister(Register.Capabilities);

        Serial.WriteString("[SVGAII] Init: ports 0x");
        Serial.WriteHex(_basePort);
        Serial.WriteString(", fb 0x");
        Serial.WriteHex(fbPhys);
        Serial.WriteString(", caps 0x");
        Serial.WriteHex(Capabilities);
        Serial.WriteString("\n");

        InitializeFIFO();
    }

    /// <summary>
    /// Initialize FIFO.
    /// </summary>
    public void InitializeFIFO()
    {
        // MemStart is the physical FIFO BAR — same HHDM story as the VRAM BAR.
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        _fifoMemory = new MemoryBlock(hhdmOffset + ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
        _fifoMemory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
        _fifoMemory[(uint)FIFO.Max] = _fifoMemory.Size;
        _fifoMemory[(uint)FIFO.NextCmd] = _fifoMemory[(uint)FIFO.Min];
        _fifoMemory[(uint)FIFO.Stop] = _fifoMemory[(uint)FIFO.Min];

        DeclareGuestSvga3D();

        // No Register.Enable write here: the constructor runs InitializeFIFO
        // before any mode is set, and enabling the device with Width/Height
        // still unprogrammed wedges QEMU's display-refresh loop (main thread
        // at 100% holding the BQL — guest, monitor and gdbstub all freeze).
        // SetMode enables the device once the mode registers are valid.
        WriteRegister(Register.ConfigDone, 1);

        NegotiateSvga3D();
    }

    /// <summary>
    /// Tells the host which SVGA3D version this driver speaks. Runs before
    /// <see cref="Register.ConfigDone"/> hands the host the FIFO layout: the
    /// host answers with its own version only for a guest that declared one,
    /// and publishes 0 (which reads as "no 3D") for a guest that stayed
    /// silent.
    /// </summary>
    private void DeclareGuestSvga3D()
    {
        if (!HasSvga3DFifoRegisters() ||
            (ReadFifo3D(Register3D.SVGA_FIFO_CAPABILITIES) & FifoCap3DHwVersionRevised) == 0 ||
            _fifoMemory[(uint)FIFO.Min] <= ((uint)Register3D.SVGA_FIFO_GUEST_3D_HWVERSION << 2))
        {
            return;
        }

        WriteFifo3D(Register3D.SVGA_FIFO_GUEST_3D_HWVERSION, GuestHardwareVersion);
    }

    /// <summary>
    /// Reads back the SVGA3D version the host settled on. Negotiation lives
    /// with FIFO initialization (not in the 3D layer) because SetMode re-runs
    /// InitializeFIFO, which resets the FIFO registers it depends on.
    /// </summary>
    /// <remarks>
    /// Must run after <see cref="Register.ConfigDone"/>: the host publishes
    /// its read-only FIFO registers, the 3D hardware version among them, only
    /// once the guest has handed it the FIFO layout.
    /// </remarks>
    private void NegotiateSvga3D()
    {
        Is3DEnabled = false;
        HW3DVer = 0;

        if (!HasSvga3DFifoRegisters())
        {
            return;
        }

        // Which register carries the version is a FIFO capability, published
        // in the FIFO itself, not a device capability.
        uint fifoCapabilities = ReadFifo3D(Register3D.SVGA_FIFO_CAPABILITIES);

        HW3DVer = (fifoCapabilities & FifoCap3DHwVersionRevised) != 0
            ? ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION_REVISED)
            : ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION);

        Is3DEnabled = HW3DVer >= MinimumHardwareVersion;

        Serial.WriteString("[SVGAII] SVGA3D: fifocaps 0x");
        Serial.WriteHex(fifoCapabilities);
        Serial.WriteString(" hw 0x");
        Serial.WriteHex(ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION));
        Serial.WriteString(" rev 0x");
        Serial.WriteHex(ReadFifo3D(Register3D.SVGA_FIFO_3D_HWVERSION_REVISED));
        Serial.WriteString(" caps0 0x");
        Serial.WriteHex(ReadFifo3D(Register3D.SVGA_FIFO_3D_CAPS));
        Serial.WriteString(Is3DEnabled ? " enabled\n" : " disabled\n");
    }

    /// <summary>
    /// Whether the device carries the FIFO registers the SVGA3D negotiation
    /// reads and writes at all.
    /// </summary>
    private bool HasSvga3DFifoRegisters()
    {
        return (Capabilities & (uint)Capability.ExtendedFifo) != 0 &&
               (Capabilities & (uint)Capability.Cap3D) != 0 &&
               _fifoMemory[(uint)FIFO.Min] > ((uint)Register3D.SVGA_FIFO_3D_HWVERSION << 2);
    }

    /// <summary>
    /// Read a FIFO register slot (used by the SVGA3D layer for fences and
    /// hardware-version negotiation).
    /// </summary>
    public uint ReadFifo3D(Register3D reg)
    {
        return _fifoMemory[(uint)reg << 2];
    }

    /// <summary>
    /// Write a FIFO register slot.
    /// </summary>
    public void WriteFifo3D(Register3D reg, uint value)
    {
        _fifoMemory[(uint)reg << 2] = value;
    }

    /// <summary>
    /// Set video mode.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="depth">Depth.</param>
    public void SetMode(uint width, uint height, uint depth = 32)
    {
        // Depth is color depth in bytes.
        _depth = depth / 8;
        _width = width;
        _height = height;
        WriteRegister(Register.Width, width);
        WriteRegister(Register.Height, height);
        WriteRegister(Register.BitsPerPixel, depth);
        Enable();
        InitializeFIFO();

        // One frame, computed from the mode — NOT Register.FrameBufferSize.
        // QEMU reports the whole VRAM there (16 MiB), and FrameSize doubles as
        // the back-buffer offset: trusting the register sends every Clear/blit
        // 16 MiB past the VRAM BAR, straight over the FIFO BAR and into
        // unmapped space.
        FrameSize = width * height * _depth;
        FrameOffset = ReadRegister(Register.FrameBufferOffset);
    }

    /// <summary>
    /// Write register.
    /// </summary>
    /// <param name="register">A register.</param>
    /// <param name="value">A value.</param>
    public void WriteRegister(Register register, uint value)
    {
        PlatformHAL.PortIO.WriteDWord((ushort)(_basePort + (byte)IOPortOffset.Index), (uint)register);
        PlatformHAL.PortIO.WriteDWord((ushort)(_basePort + (byte)IOPortOffset.Value), value);
    }

    /// <summary>
    /// Read register.
    /// </summary>
    /// <param name="register">A register.</param>
    /// <returns>uint value.</returns>
    public uint ReadRegister(Register register)
    {
        PlatformHAL.PortIO.WriteDWord((ushort)(_basePort + (byte)IOPortOffset.Index), (uint)register);
        return PlatformHAL.PortIO.ReadDWord((ushort)(_basePort + (byte)IOPortOffset.Value));
    }

    /// <summary>
    /// Get FIFO.
    /// </summary>
    /// <param name="cmd">FIFO command.</param>
    /// <returns>uint value.</returns>
    public uint GetFIFO(FIFO cmd) => _fifoMemory[(uint)cmd];

    /// <summary>
    /// Set FIFO.
    /// </summary>
    /// <param name="cmd">Command.</param>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public uint SetFIFO(FIFO cmd, uint value) => _fifoMemory[(uint)cmd] = value;

    /// <summary>
    /// Wait for FIFO.
    /// </summary>
    public void WaitForFifo()
    {
        WriteRegister(Register.Sync, 1);
        while (ReadRegister(Register.Busy) != 0) { }
    }

    /// <summary>
    /// Write to FIFO.
    /// </summary>
    /// <param name="value">Value to write.</param>
    public void WriteToFifo(uint value)
    {
        if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4 && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min) ||
            GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop))
        {
            WaitForFifo();
        }

        SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
        SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

        if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
        {
            SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
        }
    }

    /// <summary>
    /// Reserve a contiguous command area in the FIFO.
    /// </summary>
    public void* ReserveFIFO(uint bytes)
    {
        uint next = GetFIFO(FIFO.NextCmd);
        uint stop = GetFIFO(FIFO.Stop);
        uint min = GetFIFO(FIFO.Min);
        uint max = GetFIFO(FIFO.Max);

        uint space;
        if (next >= stop)
        {
            space = (max - next) + (stop - min);
        }
        else
        {
            space = stop - next;
        }

        // Wait if not enough contiguous space
        while (space < bytes)
        {
            WaitForFifo(); // give the SVGA device time to consume FIFO
            next = GetFIFO(FIFO.NextCmd);
            stop = GetFIFO(FIFO.Stop);
            if (next >= stop)
            {
                space = (max - next) + (stop - min);
            }
            else
            {
                space = stop - next;
            }
        }

        // Make sure contiguous region fits before end of buffer
        if (next + bytes > max)
        {
            // wrap to beginning of buffer
            SetFIFO(FIFO.NextCmd, min);
            next = min;
        }

        void* ptr = (void*)(_fifoMemory.Base + next);

        // Advance NEXT_CMD
        uint newNext = next + bytes;
        SetFIFO(FIFO.NextCmd, (newNext == max ? min : newNext));

        return ptr;
    }

    /// <summary>
    /// Update FIFO.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public void Update(uint x, uint y, uint width, uint height)
    {
        WriteToFifo((uint)FIFOCommand.Update);
        WriteToFifo(x);
        WriteToFifo(y);
        WriteToFifo(width);
        WriteToFifo(height);
        WaitForFifo();
    }

    /// <summary>
    /// Initializes the SVGA device. Bring-up happens in the constructor and the
    /// mode is programmed by <see cref="SetMode"/>.
    /// </summary>
    public override void Initialize()
    {
    }

    /// <summary>
    /// Byte offset of a pixel within one frame.
    /// </summary>
    private uint GetPointByteOffset(int x, int y)
    {
        return (uint)(((y * (int)_width) + x) * (int)_depth);
    }

    public override void DrawPixel(uint color, int x, int y)
    {
        VideoMemory[GetPointByteOffset(x, y) + FrameSize] = color;
    }

    public override uint GetPixel(int x, int y)
    {
        return VideoMemory[GetPointByteOffset(x, y) + FrameSize];
    }

    public override void ClearScreen(uint color)
    {
        // MemoryBlock.Fill counts dwords, not bytes.
        VideoMemory.Fill(FrameSize, FrameSize / 4, color);
    }

    /// <summary>
    /// Fill a back-buffer row. Same shape as <see cref="GopDriver.ClearVRAM"/>:
    /// byte offset within the frame, count in dwords.
    /// </summary>
    public void ClearVRAM(int aStart, int aCount, int value)
    {
        VideoMemory.Fill((int)FrameSize + aStart, aCount, value);
    }

    public override void GetVRAM(int sourceByteOffset, int[] dest, int destIndex, int count)
    {
        // MemoryBlock.Get counts bytes; the contract's count is pixels.
        VideoMemory.Get((int)FrameSize + sourceByteOffset, dest, destIndex, count * (int)_depth);
    }

    /// <summary>
    /// Swap the back buffer to the visible frame and submit an Update.
    /// </summary>
    public override void Swap()
    {
        VideoMemory.MoveDown(FrameOffset, FrameSize, FrameSize);
        Update(0, 0, _width, _height);
    }

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region of the back buffer.
    /// </summary>
    public override void CopyBuffer(ReadOnlyMemory<uint> pixels, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || width <= 0 || height <= 0)
        {
            return;
        }

        if (x >= (int)_width || y >= (int)_height)
        {
            return;
        }

        int clampedWidth = Math.Min(width, (int)_width - x);
        int clampedHeight = Math.Min(height, (int)_height - y);

        fixed (uint* src = pixels.Span)
        {
            for (int row = 0; row < clampedHeight; row++)
            {
                byte* dst = (byte*)(VideoMemory.Base + FrameSize + GetPointByteOffset(x, y + row));
                MemoryOp.MemCopy(dst, (byte*)(src + row * width), clampedWidth * (int)_depth);
            }
        }
    }

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region of the back buffer (int version for image data).
    /// </summary>
    public override void CopyBuffer(ReadOnlyMemory<int> pixels, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || width <= 0 || height <= 0)
        {
            return;
        }

        if (x >= (int)_width || y >= (int)_height)
        {
            return;
        }

        int clampedWidth = Math.Min(width, (int)_width - x);
        int clampedHeight = Math.Min(height, (int)_height - y);

        fixed (int* src = pixels.Span)
        {
            for (int row = 0; row < clampedHeight; row++)
            {
                byte* dst = (byte*)(VideoMemory.Base + FrameSize + GetPointByteOffset(x, y + row));
                MemoryOp.MemCopy(dst, (byte*)(src + row * width), clampedWidth * (int)_depth);
            }
        }
    }

    /// <summary>
    /// Copy rectangle (accelerated RECT_COPY on the visible frame).
    /// </summary>
    /// <param name="x">Source X coordinate.</param>
    /// <param name="y">Source Y coordinate.</param>
    /// <param name="newX">Destination X coordinate.</param>
    /// <param name="newY">Destination Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <exception cref="NotImplementedException">Thrown if VMWare SVGA 2 has no rectangle copy capability</exception>
    public void Copy(uint x, uint y, uint newX, uint newY, uint width, uint height)
    {
        if ((Capabilities & (uint)Capability.RectCopy) != 0)
        {
            WriteToFifo((uint)FIFOCommand.RECT_COPY);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(newX);
            WriteToFifo(newY);
            WriteToFifo(width);
            WriteToFifo(height);
        }
        else
        {
            throw new NotImplementedException("VMWareSVGAII Copy()");
        }
    }

    /// <summary>
    /// Fill rectangle.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="color">Color.</param>
    public void Fill(uint x, uint y, uint width, uint height, uint color)
    {
        if ((Capabilities & (uint)Capability.RectFill) != 0)
        {
            WriteToFifo((uint)FIFOCommand.RECT_FILL);
            WriteToFifo(color);
            WriteToFifo(x);
            WriteToFifo(y);
            WriteToFifo(width);
            WriteToFifo(height);
        }
        else
        {
            if ((Capabilities & (uint)Capability.RectCopy) != 0)
            {
                // fill first line and copy it to all other
                uint xTarget = x + width;
                uint yTarget = y + height;

                for (uint xTmp = x; xTmp < xTarget; xTmp++)
                {
                    DrawPixel(color, (int)xTmp, (int)y);
                }
                // refresh first line for copy process
                Update(x, y, width, 1);
                for (uint yTmp = y + 1; yTmp < yTarget; yTmp++)
                {
                    Copy(x, y, x, yTmp, width, 1);
                }
            }
            else
            {
                uint xTarget = x + width;
                uint yTarget = y + height;
                uint dwordsPerRow = (width * _depth) / 4;

                for (uint yTmp = y; yTmp < yTarget; yTmp++)
                {
                    VideoMemory.Fill(FrameSize + GetPointByteOffset((int)x, (int)yTmp), dwordsPerRow, color);
                }
                Update(x, y, width, height);
            }
        }
    }

    /// <summary>
    /// Define cursor.
    /// </summary>
    public void DefineCursor()
    {
        WriteToFifo((uint)FIFOCommand.DEFINE_CURSOR);
        WriteToFifo(0); // ID
        WriteToFifo(0); // Hotspot X
        WriteToFifo(0); // Hotspot Y
        WriteToFifo(2);
        WriteToFifo(2);
        WriteToFifo(1);
        WriteToFifo(1);

        for (int i = 0; i < 4; i++)
        {
            WriteToFifo(0);
        }

        for (int i = 0; i < 4; i++)
        {
            WriteToFifo(0xFFFFFF);
        }
    }

    /// <summary>
    /// Define alpha cursor. <paramref name="data"/> is width×height premultiplied
    /// 32-bit BGRA pixels.
    /// </summary>
    public void DefineAlphaCursor(uint hotspotX, uint hotspotY, uint width, uint height, int[] data)
    {
        WriteToFifo((uint)FIFOCommand.DEFINE_ALPHA_CURSOR);
        WriteToFifo(0); // ID
        WriteToFifo(hotspotX);
        WriteToFifo(hotspotY);
        WriteToFifo(width);
        WriteToFifo(height);

        for (int i = 0; i < data.Length; i++)
        {
            WriteToFifo((uint)data[i]);
        }
    }

    /// <summary>
    /// Enable the SVGA Driver, only needed after Disable() has been called.
    /// </summary>
    public void Enable()
    {
        WriteRegister(Register.Enable, 1);
    }

    /// <summary>
    /// Disable the SVGA Driver, returns to text mode.
    /// </summary>
    public void Disable()
    {
        WriteRegister(Register.Enable, 0);
    }

    /// <summary>
    /// Sets the cursor position and draws it. CursorOn is written last — that is
    /// the write that latches the new ID/X/Y on the host. (The old gen2 code also
    /// read-modify-wrote Register.CursorCount, but 0x0C is BYTES_PER_LINE in the
    /// v2 register map — it was bumping the pitch register on every mouse move.)
    /// </summary>
    /// <param name="visible">Visible.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public void SetCursor(bool visible, uint x, uint y)
    {
        WriteRegister(Register.CursorID, 0);
        WriteRegister(Register.CursorX, x);
        WriteRegister(Register.CursorY, y);
        WriteRegister(Register.CursorOn, (uint)(visible ? 1 : 0));
    }
}
