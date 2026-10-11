// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// One pipe of the display engine: its primary plane, which fetches a
/// surface through the translation table, its cursor, and its frame
/// counter. The probe reads what the firmware left in the plane; the
/// display flips its surface address and programs its cursor, the way
/// i915's <c>i9xx_plane.c</c>, <c>skl_universal_plane.c</c> and
/// <c>intel_cursor.c</c> do. The cursor's share of the display buffer and
/// its watermarks live in <c>IntelGraphicsPipe.Watermarks.cs</c>. Thread
/// context.
/// </summary>
internal sealed partial class IntelGraphicsPipe
{
    /// <summary>How long the refresh measurement counts frames over, in milliseconds.</summary>
    private const uint MeasureWindowMilliseconds = 200;

    /// <summary>The longest wait for the next frame, in milliseconds: a pipe slower than 10 Hz is not running.</summary>
    private const int FrameEdgeTimeoutMilliseconds = 100;

    /// <summary>Milliseconds in one second.</summary>
    private const int MillisecondsPerSecond = 1000;

    private readonly RegisterWindow _registers;
    private readonly IntelGraphicsGeneration _generation;
    private readonly uint _pipeOffset;
    private readonly uint _cursorOffset;

    /// <summary>The cursor control last written; all ones until the first write.</summary>
    private uint _cursorControl = uint.MaxValue;

    /// <summary>Which pipe this is: 0 for pipe A.</summary>
    internal int Index { get; }

    /// <summary>The plane's control register: DSPCNTR before Skylake, PLANE_CTL from Skylake on.</summary>
    internal uint Control => Read(IntelGraphicsRegisters.PlaneControl);

    /// <summary>Whether the primary plane is on.</summary>
    internal bool IsPlaneEnabled => (Control & IntelGraphicsRegisters.PlaneEnable) != 0;

    /// <summary>The surface address the plane is programmed with: the next frame once a flip is armed.</summary>
    internal ulong SurfaceAddress => Read(IntelGraphicsRegisters.PlaneSurface) & IntelGraphicsRegisters.SurfaceAddressMask;

    /// <summary>The surface address the plane is scanning out now: the frame on screen.</summary>
    internal ulong LiveSurfaceAddress => Read(IntelGraphicsRegisters.PlaneSurfaceLive) & IntelGraphicsRegisters.SurfaceAddressMask;

    /// <summary>The plane's width in pixels: PLANE_SIZE from Skylake on, the pipe's source size before.</summary>
    internal int Width => (int)((SizeRegister >> SizeWidthShift) & IntelGraphicsRegisters.HalfMask) + 1;

    /// <summary>The plane's height in lines.</summary>
    internal int Height => (int)((SizeRegister >> SizeHeightShift) & IntelGraphicsRegisters.HalfMask) + 1;

    /// <summary>The plane's left edge on the pipe: PLANE_POS from Skylake on, 0 before.</summary>
    internal int X => _generation.HasUniversalPlanes ? (int)(Read(IntelGraphicsRegisters.PlanePosition) & IntelGraphicsRegisters.HalfMask) : 0;

    /// <summary>The plane's top edge on the pipe.</summary>
    internal int Y => _generation.HasUniversalPlanes ? (int)(Read(IntelGraphicsRegisters.PlanePosition) >> IntelGraphicsRegisters.HighHalfShift) : 0;

    /// <summary>The surface pitch in bytes.</summary>
    internal int PitchBytes
    {
        get
        {
            uint stride = Read(IntelGraphicsRegisters.PlaneStride);
            return _generation.HasUniversalPlanes
                ? (int)(stride & IntelGraphicsRegisters.PlaneStrideMask) * IntelGraphicsRegisters.PlaneStrideUnit
                : (int)(stride & IntelGraphicsRegisters.DisplayStrideMask);
        }
    }

    /// <summary>The pipe's frame counter.</summary>
    internal uint FrameCount => Read(IntelGraphicsRegisters.PipeFrameCount);

    /// <summary>PLANE_SIZE from Skylake on; PIPESRC, the plane's size before Skylake, otherwise.</summary>
    private uint SizeRegister => Read(_generation.HasUniversalPlanes ? IntelGraphicsRegisters.PlaneSize : IntelGraphicsRegisters.PipeSourceSize);

    /// <summary>Where the width sits in <see cref="SizeRegister"/>: low in PLANE_SIZE, high in PIPESRC.</summary>
    private int SizeWidthShift => _generation.HasUniversalPlanes ? 0 : IntelGraphicsRegisters.HighHalfShift;

    /// <summary>Where the height sits in <see cref="SizeRegister"/>.</summary>
    private int SizeHeightShift => _generation.HasUniversalPlanes ? IntelGraphicsRegisters.HighHalfShift : 0;

    /// <summary>A view of pipe <paramref name="index"/> over BAR 0.</summary>
    /// <param name="registers">BAR 0.</param>
    /// <param name="generation">The engine's generation.</param>
    /// <param name="index">The pipe, 0 to <see cref="IntelGraphicsGeneration.PipeCount"/> exclusive.</param>
    internal IntelGraphicsPipe(RegisterWindow registers, IntelGraphicsGeneration generation, int index)
    {
        _registers = registers;
        _generation = generation;
        Index = index;
        _pipeOffset = (uint)index * IntelGraphicsRegisters.PipeStride;
        _cursorOffset = (uint)index * generation.CursorStride;
    }

    /// <summary>
    /// Why the plane's surface is not one this driver can draw into, or null
    /// when it is: 32-bit XRGB, linear, unrotated, starting at the surface's
    /// first byte. Reads only.
    /// </summary>
    internal string? Unsupported()
    {
        uint control = Control;
        if (!_generation.HasUniversalPlanes)
        {
            if ((control & IntelGraphicsRegisters.DisplayFormatMask) != IntelGraphicsRegisters.DisplayFormatXrgb8888)
            {
                return $"the pixel format (DSPCNTR 0x{control:X8}) is not 32-bit XRGB";
            }

            if ((control & IntelGraphicsRegisters.DisplayTiled) != 0)
            {
                return "the surface is X-tiled, not linear";
            }

            if ((control & IntelGraphicsRegisters.DisplayRotate180) != 0)
            {
                return "the surface is rotated";
            }
        }
        else
        {
            uint formatMask = _generation.DisplayVersion >= 11 ? IntelGraphicsRegisters.PlaneFormatMask : IntelGraphicsRegisters.PlaneFormatMaskGen9;
            if ((control & formatMask) != IntelGraphicsRegisters.PlaneFormatXrgb8888)
            {
                return $"the pixel format (PLANE_CTL 0x{control:X8}) is not 32-bit XRGB";
            }

            if ((control & IntelGraphicsRegisters.PlaneOrderRgbx) != 0)
            {
                return "the surface is XBGR, red in the low byte";
            }

            if ((control & IntelGraphicsRegisters.PlaneTiledMask) != 0)
            {
                return $"the surface is tiled (PLANE_CTL 0x{control:X8}), not linear";
            }

            uint mirror = _generation.DisplayVersion >= 11 ? IntelGraphicsRegisters.PlaneFlipHorizontal : 0;
            if ((control & (IntelGraphicsRegisters.PlaneRotateMask | mirror)) != 0)
            {
                return "the surface is rotated or mirrored";
            }
        }

        uint offset = Read(_generation.HasPlaneOffsetXY ? IntelGraphicsRegisters.PlaneOffset : IntelGraphicsRegisters.PlaneLinearOffset);
        if (offset != 0)
        {
            return $"the plane starts at offset 0x{offset:X} into its surface";
        }

        return null;
    }

    /// <summary>Makes a written surface address take effect at the next vertical blank rather than mid-frame.</summary>
    internal void SetVerticalFlip()
    {
        uint control = Control;
        if ((control & IntelGraphicsRegisters.PlaneAsyncFlip) != 0)
        {
            Write(IntelGraphicsRegisters.PlaneControl, control & ~IntelGraphicsRegisters.PlaneAsyncFlip);
        }
    }

    /// <summary>
    /// Programs the surface address. The write arms the plane's
    /// double-buffered registers, which latch at the next vertical blank.
    /// </summary>
    /// <param name="address">The surface's GPU address, page aligned.</param>
    internal void ProgramSurface(ulong address)
    {
        Write(IntelGraphicsRegisters.PlaneSurface, (uint)address & IntelGraphicsRegisters.SurfaceAddressMask);
    }

    /// <summary>
    /// The cursor control for an image of <paramref name="mode"/>: the mode,
    /// the pipe's gamma and colour conversion enables as the plane has them
    /// (before Ice Lake), and the platform's fixed bits, as i915's
    /// <c>i9xx_cursor_ctl</c> builds it.
    /// </summary>
    /// <param name="mode">One of the ARGB cursor modes.</param>
    internal uint CursorControlFor(uint mode)
    {
        uint control = mode;
        if (_generation.CursorFollowsPipeColor)
        {
            uint plane = Control;
            uint csc = _generation.HasUniversalPlanes ? IntelGraphicsRegisters.PlanePipeCscEnable : IntelGraphicsRegisters.DisplayPipeCscEnable;
            if ((plane & IntelGraphicsRegisters.PlanePipeGammaEnable) != 0)
            {
                control |= IntelGraphicsRegisters.CursorPipeGammaEnable;
            }

            if ((plane & csc) != 0)
            {
                control |= IntelGraphicsRegisters.CursorPipeCscEnable;
            }
        }

        if (_generation.CursorDisablesTrickleFeed)
        {
            control |= IntelGraphicsRegisters.CursorTrickleFeedDisable;
        }

        if (_generation.CursorNeedsArbitrationSlot)
        {
            control |= IntelGraphicsRegisters.CursorOneArbitrationSlot;
        }

        return control;
    }

    /// <summary>
    /// Programs the cursor: its control when it changed, its position, then
    /// its base, whose write arms the update for the next vertical blank,
    /// the order i915's <c>i9xx_cursor_update_arm</c> writes them in.
    /// </summary>
    /// <param name="control">The control, from <see cref="CursorControlFor"/>, or 0 to turn the cursor off.</param>
    /// <param name="x">The image's left edge on the pipe; may be negative.</param>
    /// <param name="y">The image's top edge on the pipe; may be negative.</param>
    /// <param name="address">The image's GPU address, page aligned.</param>
    internal void ProgramCursor(uint control, int x, int y, ulong address)
    {
        if (control != _cursorControl)
        {
            if (_generation.HasCursorFbcControl)
            {
                WriteCursor(IntelGraphicsRegisters.CursorFbcControl, 0);
            }

            WriteCursor(IntelGraphicsRegisters.CursorControl, control);
            _cursorControl = control;
        }

        WriteCursor(IntelGraphicsRegisters.CursorPosition, EncodeCursorPosition(x, y));
        WriteCursor(IntelGraphicsRegisters.CursorBase, (uint)address & IntelGraphicsRegisters.SurfaceAddressMask);
    }

    /// <summary>
    /// Counts frames over about <see cref="MeasureWindowMilliseconds"/>,
    /// timed from one frame edge to another with the TSC, and returns the
    /// refresh rate rounded to the hertz. Spins on the frame counter at both
    /// edges and sleeps through the window between them.
    /// </summary>
    /// <param name="binding">The device's binding, which sleeps through the window.</param>
    /// <returns>The refresh rate in hertz, or 0 when the counter did not move.</returns>
    internal int MeasureRefreshRate(DeviceBinding binding)
    {
        if (!TryWaitForFrameEdge(out uint first, out long start))
        {
            return 0;
        }

        binding.Sleep(MeasureWindowMilliseconds);

        if (!TryWaitForFrameEdge(out uint last, out long end) || end <= start)
        {
            return 0;
        }

        long frames = last - first;
        long elapsed = end - start;
        return (int)((frames * Stopwatch.Frequency + elapsed / 2) / elapsed);
    }

    /// <summary>Spins until the frame counter changes: the next vertical blank, when the armed registers latch.</summary>
    /// <returns>False when no frame went by within <see cref="FrameEdgeTimeoutMilliseconds"/>.</returns>
    internal bool WaitForFrame()
    {
        return TryWaitForFrameEdge(out _, out _);
    }

    /// <summary>Spins until the frame counter changes, and reads the TSC as it does.</summary>
    private bool TryWaitForFrameEdge(out uint count, out long timestamp)
    {
        uint previous = FrameCount;
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * FrameEdgeTimeoutMilliseconds / MillisecondsPerSecond;
        while (true)
        {
            count = FrameCount;
            timestamp = Stopwatch.GetTimestamp();
            if (count != previous)
            {
                return true;
            }

            if (timestamp > deadline)
            {
                return false;
            }
        }
    }

    /// <summary>CURPOS: each coordinate as a sign bit and a 15-bit magnitude.</summary>
    private static uint EncodeCursorPosition(int x, int y)
    {
        uint position = (uint)Math.Abs(x) & IntelGraphicsRegisters.CursorPositionMask;
        position |= ((uint)Math.Abs(y) & IntelGraphicsRegisters.CursorPositionMask) << IntelGraphicsRegisters.HighHalfShift;
        if (x < 0)
        {
            position |= IntelGraphicsRegisters.CursorPositionXSign;
        }

        if (y < 0)
        {
            position |= IntelGraphicsRegisters.CursorPositionYSign;
        }

        return position;
    }

    private uint Read(uint register)
    {
        return _registers.Read32(register + _pipeOffset);
    }

    private void Write(uint register, uint value)
    {
        _registers.Write32(register + _pipeOffset, value);
    }

    private void WriteCursor(uint register, uint value)
    {
        _registers.Write32(register + _cursorOffset, value);
    }
}
