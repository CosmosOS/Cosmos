// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// One output timing generator (OTG) of the display engine, read only: the
/// driver keeps the timing the firmware programmed and reads it for the
/// mode, the frame counter for the refresh rate, and the update lock that
/// would hold a flip back. Thread context.
/// </summary>
internal sealed class AmdDcnTimingGenerator
{
    /// <summary>How long the refresh measurement counts frames over, in milliseconds.</summary>
    private const uint MeasureWindowMilliseconds = 200;

    /// <summary>The longest wait for the next frame, in milliseconds: a generator slower than 10 Hz is not running.</summary>
    private const int FrameEdgeTimeoutMilliseconds = 100;

    /// <summary>Milliseconds in one second.</summary>
    private const int MillisecondsPerSecond = 1000;

    private readonly RegisterWindow _registers;
    private readonly uint _stride;

    /// <summary>Which generator this is, 0 to <see cref="AmdDcnRegisters.PipeCount"/> exclusive.</summary>
    internal int Index { get; }

    /// <summary>Whether the generator runs: OTG_MASTER_EN.</summary>
    internal bool IsEnabled => (Read(AmdDcnRegisters.OtgControl) & AmdDcnRegisters.OtgMasterEnable) != 0;

    /// <summary>Whether the generator holds its double-buffered registers: OTG_MASTER_UPDATE_LOCK.</summary>
    internal bool IsUpdateLocked => (Read(AmdDcnRegisters.OtgMasterUpdateLock) & AmdDcnRegisters.OtgUpdateLocked) != 0;

    /// <summary>The active width in pixels: horizontal blank start minus blank end.</summary>
    internal int ActiveWidth => BlankSpan(AmdDcnRegisters.OtgHorizontalBlank);

    /// <summary>The active height in lines: vertical blank start minus blank end.</summary>
    internal int ActiveHeight => BlankSpan(AmdDcnRegisters.OtgVerticalBlank);

    /// <summary>The 24-bit frame counter.</summary>
    internal uint FrameCount => Read(AmdDcnRegisters.OtgFrameCount) & AmdDcnRegisters.OtgFrameCountMask;

    /// <summary>A view of generator <paramref name="index"/> over the MMIO window.</summary>
    /// <param name="registers">The MMIO BAR.</param>
    /// <param name="index">The generator, 0 to <see cref="AmdDcnRegisters.PipeCount"/> exclusive.</param>
    internal AmdDcnTimingGenerator(RegisterWindow registers, int index)
    {
        _registers = registers;
        Index = index;
        _stride = (uint)index * AmdDcnRegisters.TimingGeneratorStride;
    }

    /// <summary>
    /// Counts frames over about <see cref="MeasureWindowMilliseconds"/>,
    /// timed from one frame edge to another with the TSC, and returns the
    /// refresh rate rounded to the hertz. Spins on the frame counter at both
    /// edges and sleeps through the window between them. Thread context.
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

        long frames = (last - first) & AmdDcnRegisters.OtgFrameCountMask;
        long elapsed = end - start;
        return (int)((frames * Stopwatch.Frequency + elapsed / 2) / elapsed);
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

    /// <summary>A blank register's start minus its end: the active span.</summary>
    private int BlankSpan(uint register)
    {
        uint value = Read(register);
        int start = (int)(value & AmdDcnRegisters.OtgTimingMask);
        int end = (int)((value >> AmdDcnRegisters.OtgBlankEndShift) & AmdDcnRegisters.OtgTimingMask);
        return start - end;
    }

    private uint Read(uint register)
    {
        return _registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, register + _stride));
    }
}
