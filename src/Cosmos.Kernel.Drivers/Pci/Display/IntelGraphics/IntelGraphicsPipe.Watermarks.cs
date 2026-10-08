// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// What the cursor needs before it can fetch its image: room in the display
/// buffer and watermarks that keep that room filled. The firmware programs
/// them for the plane it put on screen, and rarely for a cursor it never
/// showed. From Skylake on the cursor gets a share of the pipe's display
/// buffer (DDB), cut from the end of the primary plane's when the firmware
/// gave it none, 32 blocks as i915's <c>skl_cursor_allocation</c> gives a
/// single pipe, with a level 0 watermark asking for all but one block of
/// it and the deeper levels off, which keeps the cursor's buffer full at
/// the cost of the deeper memory power states while it shows. Before
/// Skylake the cursor has its own FIFO, and only its level 0 watermark is
/// set when the firmware left it at 0, with the low power watermarks that
/// say nothing for the cursor turned off.
/// </summary>
internal sealed partial class IntelGraphicsPipe
{
    /// <summary>The display buffer blocks the cursor gets when the firmware gave it none.</summary>
    private const uint CursorBufferBlocks = 32;

    /// <summary>The fewest blocks the plane keeps once the cursor's share is cut from its own.</summary>
    private const uint MinPlaneBufferBlocks = 64;

    /// <summary>Bytes of one line of the widest cursor, which sets the line count of its watermark.</summary>
    private const uint WidestCursorLineBytes = 256 * sizeof(uint);

    /// <summary>
    /// Readies the cursor's display buffer and watermarks, once, before the
    /// cursor is first turned on. On Skylake and later, cutting the
    /// cursor's share from the plane's waits one frame, for the plane's
    /// smaller share to latch before the cursor is given the blocks.
    /// </summary>
    /// <param name="problem">Why the cursor cannot be used, when it cannot.</param>
    /// <returns>False when the plane's share is too small to give the cursor one, or the pipe stopped.</returns>
    internal bool TryPrepareCursorBuffer(out string problem)
    {
        problem = "";
        if (!_generation.HasUniversalPlanes)
        {
            PrepareLegacyCursorWatermark();
            return true;
        }

        uint cursorConfig = Read(IntelGraphicsRegisters.CursorBufferConfig);
        uint cursorBlocks = BufferBlocks(cursorConfig);
        if (cursorBlocks == 0)
        {
            uint planeConfig = Read(IntelGraphicsRegisters.PlaneBufferConfig);
            uint planeBlocks = BufferBlocks(planeConfig);
            if (planeBlocks < CursorBufferBlocks + MinPlaneBufferBlocks)
            {
                problem = $"pipe {Index}: the plane's {planeBlocks} display buffer blocks leave none for a cursor";
                return false;
            }

            uint planeStart = planeConfig & IntelGraphicsRegisters.BufferBlockMask;
            uint planeLast = (planeConfig >> IntelGraphicsRegisters.HighHalfShift) & IntelGraphicsRegisters.BufferBlockMask;
            uint newPlaneLast = planeLast - CursorBufferBlocks;
            TrimPlaneWatermarks(planeBlocks - CursorBufferBlocks);
            Write(IntelGraphicsRegisters.PlaneBufferConfig, (newPlaneLast << IntelGraphicsRegisters.HighHalfShift) | planeStart);

            // The plane's registers latch with its next surface write; the
            // same address again arms them without moving the scanout.
            Write(IntelGraphicsRegisters.PlaneSurface, Read(IntelGraphicsRegisters.PlaneSurface));
            if (!WaitForFrame())
            {
                problem = $"pipe {Index}: the frame counter stopped while the plane's display buffer shrank";
                return false;
            }

            cursorConfig = (planeLast << IntelGraphicsRegisters.HighHalfShift) | (newPlaneLast + 1);
            Write(IntelGraphicsRegisters.CursorBufferConfig, cursorConfig);
            cursorBlocks = CursorBufferBlocks;
        }

        WriteCursorWatermarks(cursorBlocks);
        return true;
    }

    /// <summary>
    /// Turns off every plane watermark level that would not fit a share of
    /// <paramref name="blocks"/>, and the levels above it: a level asking for
    /// as many blocks as the share holds is invalid. The transition and
    /// SAGV watermarks are checked the same way.
    /// </summary>
    private void TrimPlaneWatermarks(uint blocks)
    {
        bool fits = true;
        for (int level = 0; level < _generation.WatermarkLevelCount; level++)
        {
            uint register = IntelGraphicsRegisters.PlaneWatermark + (uint)level * sizeof(uint);
            fits = fits && WatermarkFits(Read(register), blocks);
            if (!fits)
            {
                Write(register, 0);
            }
        }

        TrimWatermark(IntelGraphicsRegisters.PlaneWatermarkTransition, blocks);
        if (_generation.HasSagvWatermarks)
        {
            TrimWatermark(IntelGraphicsRegisters.PlaneWatermarkSagv, blocks);
            TrimWatermark(IntelGraphicsRegisters.PlaneWatermarkSagvTransition, blocks);
        }
    }

    private void TrimWatermark(uint register, uint blocks)
    {
        if (!WatermarkFits(Read(register), blocks))
        {
            Write(register, 0);
        }
    }

    /// <summary>
    /// The cursor's watermarks for a share of <paramref name="blocks"/>:
    /// level 0 (and the SAGV one, on display 13) on, asking for all but one
    /// block with the lines the widest cursor fills them with; the deeper
    /// levels and the transition watermarks off. They latch with the
    /// cursor's next base write.
    /// </summary>
    private void WriteCursorWatermarks(uint blocks)
    {
        uint wanted = blocks - 1;
        uint lines = 0;
        if (_generation.Watermark0HasLines)
        {
            lines = Math.Min(wanted * IntelGraphicsRegisters.BufferBlockBytes / WidestCursorLineBytes, _generation.MaxWatermarkLines);
        }

        uint level0 = IntelGraphicsRegisters.WatermarkEnable
            | ((lines & IntelGraphicsRegisters.WatermarkLinesMask) << IntelGraphicsRegisters.WatermarkLinesShift)
            | (wanted & IntelGraphicsRegisters.WatermarkBlocksMask);
        Write(IntelGraphicsRegisters.CursorWatermark, level0);
        for (int level = 1; level < _generation.WatermarkLevelCount; level++)
        {
            Write(IntelGraphicsRegisters.CursorWatermark + (uint)level * sizeof(uint), 0);
        }

        Write(IntelGraphicsRegisters.CursorWatermarkTransition, 0);
        if (_generation.HasSagvWatermarks)
        {
            Write(IntelGraphicsRegisters.CursorWatermarkSagv, level0);
            Write(IntelGraphicsRegisters.CursorWatermarkSagvTransition, 0);
        }
    }

    /// <summary>
    /// Before Skylake: gives the cursor a level 0 watermark, the most the
    /// register holds, when the firmware left it at 0, and first turns off
    /// the low power levels that give the cursor nothing, from the deepest
    /// down, as i915's <c>ilk_write_wm_values</c> turns them off. The low
    /// power watermarks are shared by every pipe; turning one off again is
    /// harmless.
    /// </summary>
    private void PrepareLegacyCursorWatermark()
    {
        int firstEmpty = IntelGraphicsRegisters.LowPowerWatermarkCount;
        for (int level = IntelGraphicsRegisters.LowPowerWatermarkCount - 1; level >= 0; level--)
        {
            uint value = _registers.Read32(LowPowerWatermarkRegister(level));
            if ((value & IntelGraphicsRegisters.LowPowerWatermarkEnable) != 0 && (value & IntelGraphicsRegisters.WatermarkCursorMask) == 0)
            {
                firstEmpty = level;
            }
        }

        for (int level = IntelGraphicsRegisters.LowPowerWatermarkCount - 1; level >= firstEmpty; level--)
        {
            uint register = LowPowerWatermarkRegister(level);
            uint value = _registers.Read32(register);
            _registers.Write32(register, value & ~IntelGraphicsRegisters.LowPowerWatermarkEnable);
        }

        uint pipeRegister = Index switch
        {
            0 => IntelGraphicsRegisters.PipeWatermark0A,
            1 => IntelGraphicsRegisters.PipeWatermark0B,
            _ => IntelGraphicsRegisters.PipeWatermark0C,
        };

        uint level0 = _registers.Read32(pipeRegister);
        if ((level0 & IntelGraphicsRegisters.WatermarkCursorMask) == 0)
        {
            _registers.Write32(pipeRegister, level0 | _generation.CursorWatermark0Max);
        }
    }

    /// <summary>Blocks in a buffer configuration: its inclusive end minus its start plus one, 0 when it holds none.</summary>
    private static uint BufferBlocks(uint config)
    {
        uint start = config & IntelGraphicsRegisters.BufferBlockMask;
        uint last = (config >> IntelGraphicsRegisters.HighHalfShift) & IntelGraphicsRegisters.BufferBlockMask;
        return config == 0 || last < start ? 0 : last - start + 1;
    }

    /// <summary>Whether a watermark is off, or asks for fewer blocks than a share of <paramref name="blocks"/> holds.</summary>
    private static bool WatermarkFits(uint watermark, uint blocks)
    {
        return (watermark & IntelGraphicsRegisters.WatermarkEnable) == 0 || (watermark & IntelGraphicsRegisters.WatermarkBlocksMask) < blocks;
    }

    private static uint LowPowerWatermarkRegister(int level)
    {
        return IntelGraphicsRegisters.LowPowerWatermark1 + (uint)level * sizeof(uint);
    }
}
