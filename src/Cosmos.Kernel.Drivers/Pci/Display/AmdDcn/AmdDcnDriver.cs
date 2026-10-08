// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// The display driver for AMD's DCN 3.1.5 display engine, the integrated
/// Radeon graphics of the Ryzen 7000 (Raphael, Dragon Range) and Ryzen 9000
/// (Granite Ridge) processors, over the driver kit. It does not set modes:
/// bringing up a panel link on DCN 3.1 takes the display microcontroller's
/// firmware and the power management firmware, so the driver keeps the
/// timing the UEFI firmware programmed and takes over its surface. It finds
/// the pipe scanning out a linear 32-bit surface (MMIO in BAR 5), locates
/// that surface in VRAM (BAR 0) through the display engine's view of the
/// VRAM aperture, lays two more frames and two cursor images out after it,
/// measures the refresh rate on the timing generator's frame counter, and
/// publishes an <see cref="AmdDcnState"/>: tear-free page flipping at the
/// vertical update and a hardware cursor. The firmware framebuffer sits in
/// BAR 0, so the kit retires it when this driver binds. Every decline says
/// what the probe found, which the shell's <c>drivers</c> command shows on a
/// machine without a serial port. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Graphics)]
public sealed class AmdDcnDriver : Driver
{
    // --- Constants ---

    /// <summary>PCI vendor id of AMD's graphics functions.</summary>
    private const ushort AmdVendorId = 0x1002;

    /// <summary>PCI device id of the Raphael integrated GPU (Ryzen 7000, desktop and Dragon Range).</summary>
    private const ushort RaphaelDeviceId = 0x164E;

    /// <summary>PCI device id of the Granite Ridge integrated GPU (Ryzen 9000 desktop), the same DCN 3.1.5.</summary>
    private const ushort GraniteRidgeDeviceId = 0x13C0;

    /// <summary>Index of the VRAM window, BAR 0.</summary>
    private const int VramBar = 0;

    /// <summary>Index of the register window, BAR 5.</summary>
    private const int RegistersBar = 5;

    /// <summary>The most frames laid out in VRAM: the firmware's surface and two more.</summary>
    private const int MaxFrames = 3;

    /// <summary>Alignment of every frame and cursor image in VRAM.</summary>
    private const ulong VramAlignment = 64 * 1024;

    /// <summary>The top of VRAM left untouched: the firmware keeps its discovery table and reserved areas there.</summary>
    private const ulong ReservedTopBytes = 16 * 1024 * 1024;

    /// <summary>The cursor images: two, so a new one is written while the pipe fetches the other.</summary>
    private const int CursorImageCount = 2;

    /// <summary>Bytes per pixel of a 32-bit surface.</summary>
    private const int BytesPerPixel = 4;

    /// <summary>Bits per pixel of a 32-bit surface.</summary>
    private const int BitsPerPixel = 32;

    /// <summary>Bytes in a mebibyte.</summary>
    private const ulong BytesPerMebibyte = 1024 * 1024;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(vendorId: AmdVendorId, deviceId: RaphaelDeviceId),
        new PciMatch(vendorId: AmdVendorId, deviceId: GraniteRidgeDeviceId),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(AmdDcnDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Takes over the firmware's scanout and publishes the display. Nothing
    /// on the display engine is written before every check passed. Thread
    /// context on the kit worker; a declined result makes the kit release
    /// everything acquired here and give the function back as firmware left it.
    /// </summary>
    /// <param name="binding">The PCI node and the kit facilities for it.</param>
    /// <returns>Bound with the display published; declined, with what was found, when the firmware left no surface this driver can take over.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The windows: registers in BAR 5, VRAM in BAR 0.
        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar registersBar = pci.Bars[RegistersBar];
        PciBar vramBar = pci.Bars[VramBar];
        if (!registersBar.IsAssigned || registersBar.IsIo || registersBar.Length < AmdDcnRegisters.WindowBytes)
        {
            return ProbeResult.Declined($"BAR {RegistersBar} is not a register window of {AmdDcnRegisters.WindowBytes} bytes");
        }

        if (!vramBar.IsAssigned || vramBar.IsIo)
        {
            return ProbeResult.Declined($"BAR {VramBar} is not a VRAM window");
        }

        // 2. Decode and DMA back on at once: the kit's quiesce turned bus
        //    mastering off, and the display engine is fetching the scanout.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);
        RegisterWindow registers = binding.MapRegisters(RegistersBar);

        // 3. A legacy boot leaves the VGA engine on screen, not a surface.
        if ((registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment1, AmdDcnRegisters.VgaControl)) & AmdDcnRegisters.VgaModeEnable) != 0)
        {
            return ProbeResult.Declined("the display engine is in VGA mode; boot through UEFI so the firmware leaves a linear surface on screen");
        }

        // 4. The pipe scanning out the firmware's surface, and any other pipe
        //    fetching the same surface (a split or a cloned output).
        if (!TryFindScanout(binding, registers, out AmdDcnPipe? scanout, out string refusal))
        {
            return ProbeResult.Declined(refusal);
        }

        AmdDcnPipe[] pipes = PipesScanning(registers, scanout.SurfaceAddress);
        int width = 0;
        int height = 0;
        for (int i = 0; i < pipes.Length; i++)
        {
            width = Math.Max(width, pipes[i].ViewportX + pipes[i].ViewportWidth);
            height = Math.Max(height, pipes[i].ViewportY + pipes[i].ViewportHeight);
        }

        int pitch = scanout.PitchPixels * BytesPerPixel;
        if (width == 0 || height == 0 || scanout.PitchPixels < width)
        {
            return ProbeResult.Declined($"pipe {scanout.Index} has a {width}x{height} viewport over a surface {scanout.PitchPixels} pixels wide");
        }

        // 5. Where the surface sits in VRAM: its offset into the aperture the
        //    display engine maps VRAM at, which BAR 0 maps from its start.
        uint fbBase = registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, AmdDcnRegisters.FbLocationBase)) & AmdDcnRegisters.FbLocationMask;
        uint fbTop = registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, AmdDcnRegisters.FbLocationTop)) & AmdDcnRegisters.FbLocationMask;
        ulong apertureStart = (ulong)fbBase << AmdDcnRegisters.FbLocationShift;
        ulong apertureEnd = (((ulong)fbTop + 1) << AmdDcnRegisters.FbLocationShift) - 1;
        ulong surface = scanout.SurfaceAddress;
        if (fbTop < fbBase || surface < apertureStart || surface > apertureEnd)
        {
            return ProbeResult.Declined($"the surface at 0x{surface:X} is outside the VRAM aperture 0x{apertureStart:X}-0x{apertureEnd:X}");
        }

        ulong surfaceOffset = surface - apertureStart;
        ulong frameBytes = (ulong)pitch * (ulong)height;
        DeviceRegion vram = binding.MapRegion(VramBar, RegionCaching.WriteCombining);
        if (surfaceOffset > vram.Length || vram.Length - surfaceOffset < frameBytes)
        {
            return ProbeResult.Declined($"the surface at VRAM offset 0x{surfaceOffset:X} is past the {vram.Length / BytesPerMebibyte} MiB the CPU sees through BAR {VramBar}");
        }

        // 6. The frames and cursor images after the surface, inside what the
        //    CPU sees, what the aperture maps and the VRAM the firmware sized.
        uint memorySize = registers.Read32(AmdDcnRegisters.Offset(0, AmdDcnRegisters.ConfigMemorySize));
        ulong vramBytes = memorySize == 0 || memorySize == uint.MaxValue ? vram.Length : memorySize * BytesPerMebibyte;
        ulong limit = Math.Min(Math.Min(vram.Length, vramBytes), apertureEnd - apertureStart + 1);
        limit = limit > ReservedTopBytes ? limit - ReservedTopBytes : 0;

        AmdDcnTimingGenerator timingGenerator = new(registers, scanout.TimingGenerator);
        int refreshRate = timingGenerator.MeasureRefreshRate(binding);
        ulong frameStride = AlignUp(frameBytes, VramAlignment);
        int frameCount = FlipFrameCount(binding, pipes, timingGenerator, refreshRate);
        while (frameCount > 1 && surfaceOffset + frameStride * (ulong)frameCount > limit)
        {
            frameCount--;
        }

        DeviceRegion[] frames = new DeviceRegion[frameCount];
        ulong[] frameAddresses = new ulong[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            frames[i] = vram.Slice(surfaceOffset + frameStride * (ulong)i, frameBytes);
            frameAddresses[i] = surface + frameStride * (ulong)i;
        }

        ulong cursorOffset = surfaceOffset + frameStride * (ulong)frameCount;
        bool hasCursor = cursorOffset + AmdDcnState.CursorImageBytes * CursorImageCount <= limit;
        DeviceRegion[] cursorImages = hasCursor ? new DeviceRegion[CursorImageCount] : [];
        ulong[] cursorAddresses = hasCursor ? new ulong[CursorImageCount] : [];
        for (int i = 0; i < cursorImages.Length; i++)
        {
            ulong offset = cursorOffset + AmdDcnState.CursorImageBytes * (ulong)i;
            cursorImages[i] = vram.Slice(offset, AmdDcnState.CursorImageBytes);
            cursorAddresses[i] = surface + (offset - surfaceOffset);
        }

        // 7. The display, its spare frames cleared while the firmware's
        //    surface stays on screen, and the ring.
        DisplayMode mode = new(width, height, pitch, BitsPerPixel, refreshRate);
        AmdDcnState state = new(binding, pipes, mode, frames, frameAddresses, cursorImages, cursorAddresses);
        state.Prepare();
        binding.DriverState = state;
        binding.PublishDisplay(state);

        string cursor = hasCursor ? "hardware cursor" : "no room for a cursor";
        binding.Log(
            $"{width}x{height}x{BitsPerPixel} @ {refreshRate} Hz on {PipeList(pipes)} (otg {timingGenerator.Index}, active {timingGenerator.ActiveWidth}x{timingGenerator.ActiveHeight}), " +
            $"surface 0x{surface:X} at VRAM offset 0x{surfaceOffset:X}, vram {vramBytes / BytesPerMebibyte} MiB, BAR {VramBar} {vram.Length / BytesPerMebibyte} MiB, " +
            $"{frameCount} frames, {cursor}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Hides the cursor and puts the firmware's surface back on screen.
    /// Thread context on the kit worker; the registers are still mapped
    /// here, and nothing is written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its display is already withdrawn.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not AmdDcnState state || !reason.HardwarePresent)
        {
            return;
        }

        state.Release();
    }

    // --- Private methods ---

    /// <summary>
    /// The lowest active pipe whose surface this driver can draw into, each
    /// active pipe logged on the way. Reads only.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="registers">The MMIO BAR.</param>
    /// <param name="scanout">The pipe, when one qualifies.</param>
    /// <param name="refusal">Why none qualifies: the first unsupported pipe's problem, or that none is active.</param>
    /// <returns>False when no pipe qualifies.</returns>
    private static bool TryFindScanout(DeviceBinding binding, RegisterWindow registers, [NotNullWhen(true)] out AmdDcnPipe? scanout, out string refusal)
    {
        scanout = null;
        refusal = "no pipe is scanning out: the firmware left the display engine off";
        bool refused = false;
        for (int i = 0; i < AmdDcnRegisters.PipeCount; i++)
        {
            AmdDcnPipe pipe = new(registers, i);
            if (!pipe.IsActive)
            {
                continue;
            }

            binding.Log(
                $"pipe {i}: surface 0x{pipe.SurfaceAddress:X}, viewport {pipe.ViewportWidth}x{pipe.ViewportHeight} at {pipe.ViewportX},{pipe.ViewportY}, " +
                $"pitch {pipe.PitchPixels}, format {pipe.PixelFormat}, swizzle {pipe.SwizzleMode}, vmid {pipe.Vmid}, otg {pipe.TimingGenerator}");

            string? problem = Unsupported(registers, pipe);
            if (problem is not null)
            {
                if (!refused)
                {
                    refusal = $"pipe {i}: {problem}";
                    refused = true;
                }

                continue;
            }

            scanout ??= pipe;
        }

        return scanout is not null;
    }

    /// <summary>Why a pipe's surface is not one this driver can draw into, or null when it is.</summary>
    private static string? Unsupported(RegisterWindow registers, AmdDcnPipe pipe)
    {
        if (pipe.PixelFormat != AmdDcnRegisters.PixelFormat32Bit)
        {
            return $"pixel format {pipe.PixelFormat} is not 32-bit RGB";
        }

        if (pipe.SwizzleMode != 0)
        {
            return $"swizzle mode {pipe.SwizzleMode} is not linear";
        }

        if (pipe.IsCompressedOrEncrypted)
        {
            return "the surface is compressed or encrypted";
        }

        if (pipe.IsRotatedOrMirrored)
        {
            return "the surface is rotated or mirrored";
        }

        if (pipe.Vmid != 0)
        {
            return $"the surface is mapped through GPU VM context {pipe.Vmid}";
        }

        int timingGenerator = pipe.TimingGenerator;
        if (timingGenerator >= AmdDcnRegisters.PipeCount || !new AmdDcnTimingGenerator(registers, timingGenerator).IsEnabled)
        {
            return $"its timing generator {timingGenerator} is not running";
        }

        return null;
    }

    /// <summary>Every active pipe fetching the surface at <paramref name="surface"/>, lowest first.</summary>
    private static AmdDcnPipe[] PipesScanning(RegisterWindow registers, ulong surface)
    {
        int count = 0;
        AmdDcnPipe[] found = new AmdDcnPipe[AmdDcnRegisters.PipeCount];
        for (int i = 0; i < AmdDcnRegisters.PipeCount; i++)
        {
            AmdDcnPipe pipe = new(registers, i);
            if (pipe.IsActive && pipe.SurfaceAddress == surface && Unsupported(registers, pipe) is null)
            {
                found[count++] = pipe;
            }
        }

        return found.AsSpan(0, count).ToArray();
    }

    /// <summary>
    /// How many frames to flip between, before VRAM is counted: one when a
    /// flip could not latch (a held update lock, a generator whose frame
    /// counter does not move), three otherwise.
    /// </summary>
    private static int FlipFrameCount(DeviceBinding binding, AmdDcnPipe[] pipes, AmdDcnTimingGenerator timingGenerator, int refreshRate)
    {
        if (refreshRate == 0)
        {
            binding.Log($"otg {timingGenerator.Index}: the frame counter does not move; drawing into the firmware's surface without flipping");
            return 1;
        }

        if (timingGenerator.IsUpdateLocked)
        {
            binding.Log($"otg {timingGenerator.Index}: the update lock is held; drawing into the firmware's surface without flipping");
            return 1;
        }

        for (int i = 0; i < pipes.Length; i++)
        {
            if (pipes[i].IsSurfaceUpdateLocked)
            {
                binding.Log($"pipe {pipes[i].Index}: the surface update lock is held; drawing into the firmware's surface without flipping");
                return 1;
            }
        }

        return MaxFrames;
    }

    /// <summary>The pipes as the log names them: <c>pipe 0</c>, <c>pipes 0+1</c>.</summary>
    private static string PipeList(AmdDcnPipe[] pipes)
    {
        string list = pipes.Length == 1 ? "pipe " : "pipes ";
        for (int i = 0; i < pipes.Length; i++)
        {
            list = i == 0 ? $"{list}{pipes[i].Index}" : $"{list}+{pipes[i].Index}";
        }

        return list;
    }

    /// <summary>Rounds <paramref name="value"/> up to a multiple of <paramref name="alignment"/>, a power of two.</summary>
    private static ulong AlignUp(ulong value, ulong alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }
}
