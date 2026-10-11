// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// Everything <see cref="AmdDcnDriver"/> holds for one bound display engine,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the display it
/// publishes. The mode is the one the firmware programmed: the driver keeps
/// its timing and takes over its surface. Up to three frames sit in VRAM:
/// frame 0 is the surface the firmware scans out, the others follow it.
/// <see cref="Framebuffer"/> is the frame the ring draws next, one that is
/// neither on screen nor queued, and <see cref="Flush"/> queues it as a flip
/// at the next vertical update, so the screen never shows a frame being
/// drawn and a present never waits while a third frame is free. The first
/// flip is waited for: when it does not latch, the display falls back to
/// drawing into frame 0 directly. The ring's canvas writes whole frames,
/// which is what makes flipping between frames that hold different older
/// contents correct. The cursor half lives in <c>AmdDcnState.Cursor.cs</c>.
/// Thread context; one caller at a time, as the ring's canvas is.
/// </summary>
internal sealed partial class AmdDcnState : IDisplay, IHardwareCursor
{
    // --- Constants ---

    /// <summary>The longest wait for a flip to latch, in milliseconds: several frames at any refresh rate a panel runs.</summary>
    private const int FlipTimeoutMilliseconds = 100;

    /// <summary>Milliseconds in one second.</summary>
    private const int MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly AmdDcnPipe[] _pipes;
    private readonly DeviceRegion[] _frames;
    private readonly ulong[] _frameAddresses;
    private int _back;
    private int _pending;
    private bool _flipsVerified;

    // --- Constructor ---

    /// <summary>
    /// Takes what the probe found and laid out. Thread context, from the
    /// probe; <see cref="Prepare"/> readies the frames before the display is
    /// published.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="pipes">The pipes scanning out the firmware's surface, one unless the firmware split it.</param>
    /// <param name="mode">The firmware's mode, with the measured refresh rate.</param>
    /// <param name="frames">The frames in VRAM, frame 0 the firmware's surface.</param>
    /// <param name="frameAddresses">The GPU address of each frame.</param>
    /// <param name="cursorImages">The two cursor images in VRAM, or none when VRAM has no room.</param>
    /// <param name="cursorAddresses">The GPU address of each cursor image.</param>
    internal AmdDcnState(DeviceBinding binding, AmdDcnPipe[] pipes, DisplayMode mode, DeviceRegion[] frames, ulong[] frameAddresses, DeviceRegion[] cursorImages, ulong[] cursorAddresses)
    {
        _binding = binding;
        _pipes = pipes;
        Mode = mode;
        _frames = frames;
        _frameAddresses = frameAddresses;
        FrameCount = frames.Length;
        _cursorImages = cursorImages;
        _cursorAddresses = cursorAddresses;
    }

    // --- IDisplay ---

    /// <inheritdoc/>
    public string Name => "amd-dcn";

    /// <summary>The firmware's mode, with the pitch of its surface and the measured refresh rate. Any context.</summary>
    public DisplayMode Mode { get; }

    /// <summary>The frame the ring draws next: off screen and not queued, or frame 0 without flipping. Any context.</summary>
    public DeviceRegion? Framebuffer => _frames[_back];

    /// <summary>
    /// Queues the frame just drawn as a flip at the next vertical update, and
    /// picks the next frame to draw. The rectangle is not used: the ring's
    /// canvas writes whole frames. The first flip is waited for, and the
    /// display drops to one frame when it does not latch. Thread context.
    /// </summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height)
    {
        if (FrameCount == 1)
        {
            return;
        }

        int presented = _back;
        ProgramScanout(_frameAddresses[presented]);
        _pending = presented;

        if (!_flipsVerified)
        {
            if (!WaitForScanout(_frameAddresses[presented]))
            {
                ProgramScanout(_frameAddresses[0]);
                FrameCount = 1;
                _back = 0;
                _pending = 0;
                _binding.Log("the first flip did not latch; drawing into the firmware's surface without flipping");
                return;
            }

            _flipsVerified = true;
        }

        _back = NextBackFrame();
    }

    // --- Internal properties ---

    /// <summary>How many frames are in use: 3 or 2 while flipping, 1 without. Any context.</summary>
    internal int FrameCount { get; private set; }

    // --- Internal methods ---

    /// <summary>
    /// Readies the frames before the display is published: flips are made to
    /// wait for the vertical update, and every frame but the one on screen is
    /// cleared, so a frame the ring has not drawn yet shows black rather than
    /// stale VRAM. The screen itself is left alone until the ring presents,
    /// so the boot log the firmware's surface shows stays visible. Thread
    /// context, from the probe.
    /// </summary>
    internal void Prepare()
    {
        if (FrameCount == 1)
        {
            return;
        }

        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i].SetVerticalFlip();
        }

        for (int i = 1; i < FrameCount; i++)
        {
            _frames[i].Span.Clear();
        }

        _back = 1;
        _pending = 0;
    }

    /// <summary>
    /// Leaves the hardware as the firmware had it: the cursor off and frame
    /// 0 on screen. Thread context, from the detach, while the registers are
    /// still mapped.
    /// </summary>
    internal void Release()
    {
        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i].SetCursorEnabled(false);
        }

        if (FrameCount > 1 && _pending != 0)
        {
            ProgramScanout(_frameAddresses[0]);
        }
    }

    // --- Private methods ---

    /// <summary>Writes the surface address on every pipe of the surface.</summary>
    private void ProgramScanout(ulong address)
    {
        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i].ProgramSurfaceAddress(address);
        }
    }

    /// <summary>
    /// A frame neither on screen nor queued. With three frames one always
    /// exists; with two, or when the frame on screen cannot be told, the
    /// queued flip is waited for first.
    /// </summary>
    private int NextBackFrame()
    {
        int front = FrameAt(_pipes[0].EarliestInUseAddress);
        if (front >= 0)
        {
            for (int i = 0; i < FrameCount; i++)
            {
                if (i != _pending && i != front)
                {
                    return i;
                }
            }
        }

        WaitForScanout(_frameAddresses[_pending]);
        return (_pending + 1) % FrameCount;
    }

    /// <summary>The frame at a GPU address, or -1.</summary>
    private int FrameAt(ulong address)
    {
        address &= AmdDcnPipe.AddressMask;
        for (int i = 0; i < FrameCount; i++)
        {
            if (_frameAddresses[i] == address)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Spins until every pipe scans out <paramref name="address"/>, up to <see cref="FlipTimeoutMilliseconds"/>.</summary>
    /// <returns>False on timeout.</returns>
    private bool WaitForScanout(ulong address)
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * FlipTimeoutMilliseconds / MillisecondsPerSecond;
        for (int i = 0; i < _pipes.Length; i++)
        {
            AmdDcnPipe pipe = _pipes[i];
            while (pipe.IsFlipPending || (pipe.EarliestInUseAddress & AmdDcnPipe.AddressMask) != address)
            {
                if (Stopwatch.GetTimestamp() > deadline)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
