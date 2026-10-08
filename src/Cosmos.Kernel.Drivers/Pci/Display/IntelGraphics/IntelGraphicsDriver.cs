// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// The display driver for the integrated graphics of Intel Core processors,
/// from the 2nd generation (Sandy Bridge, HD Graphics 2000 and 3000) to the
/// 14th (Raptor Lake, UHD and Iris Xe Graphics), over the driver kit. It does
/// not set modes: it keeps the timing the UEFI firmware programmed and takes
/// over its surface. It finds the pipe whose primary plane scans out a
/// linear 32-bit surface (registers in BAR 0), follows the surface through
/// the graphics translation table to the stolen memory the firmware put it
/// in, maps two more frames and two cursor images from the stolen memory
/// after it, reached by the CPU through the aperture (BAR 2), measures the
/// refresh rate on the pipe's frame counter, and publishes an
/// <see cref="IntelGraphicsState"/>: tear-free page flipping at the vertical
/// blank and a hardware cursor. The device id picks the register encodings,
/// as Linux's i915 tells the platforms apart. The firmware framebuffer sits
/// in BAR 2, so the kit retires it when this driver binds. Every decline
/// says what the probe found, which the shell's <c>drivers</c> command shows
/// on a machine without a serial port. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Graphics)]
public sealed class IntelGraphicsDriver : Driver
{
    // --- Constants ---

    /// <summary>PCI vendor id of Intel.</summary>
    private const ushort IntelVendorId = 0x8086;

    /// <summary>PCI base class of a display controller: VGA compatible, or other when a discrete GPU is primary.</summary>
    private const byte DisplayClassCode = 0x03;

    /// <summary>Index of the register and translation table window, BAR 0 (GTTMMADR).</summary>
    private const int RegistersBar = 0;

    /// <summary>Index of the aperture, BAR 2 (GMADR): the CPU's window onto GPU addresses.</summary>
    private const int ApertureBar = 2;

    /// <summary>The most frames laid out: the firmware's surface and two more.</summary>
    private const int MaxFrames = 3;

    /// <summary>Alignment of every frame and cursor image in GPU address space: i915's linear surface alignment from Skylake on, more than any earlier generation needs.</summary>
    private const ulong SurfaceAlignment = 256 * 1024;

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
        new PciMatch(vendorId: IntelVendorId, classCode: DisplayClassCode),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(IntelGraphicsDriver);

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
    /// <returns>Bound with the display published; declined, with what was found, when the device is not one this driver knows or the firmware left no surface it can take over.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The platform, from the device id.
        if (binding.Node.Identity is not PciIdentity identity)
        {
            return ProbeResult.Declined("the node is not a PCI function");
        }

        if (!IntelGraphicsDeviceIds.TryFind(identity.DeviceId, out IntelGraphicsPlatform platform))
        {
            return ProbeResult.Declined($"device {identity.DeviceId:X4} is not the integrated GPU of a Sandy Bridge to Raptor Lake Core processor");
        }

        IntelGraphicsGeneration generation = new(platform);

        // 2. The windows: registers and translation table in BAR 0, the aperture in BAR 2.
        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar registersBar = pci.Bars[RegistersBar];
        PciBar apertureBar = pci.Bars[ApertureBar];
        if (!registersBar.IsAssigned || registersBar.IsIo || registersBar.Length != generation.RegisterBarBytes)
        {
            return ProbeResult.Declined($"BAR {RegistersBar} is not the {generation.RegisterBarBytes / BytesPerMebibyte} MiB register and translation table window");
        }

        if (!apertureBar.IsAssigned || apertureBar.IsIo)
        {
            return ProbeResult.Declined($"BAR {ApertureBar} is not the aperture");
        }

        // 3. Decode and DMA back on at once: the kit's quiesce turned bus
        //    mastering off, and the display engine is fetching the scanout.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);
        RegisterWindow registers = binding.MapRegisters(RegistersBar);

        // 4. The pipe scanning out the firmware's surface, and any other pipe
        //    fetching the same surface (a cloned output).
        if (!TryFindScanout(binding, registers, generation, out IntelGraphicsPipe? scanout, out string refusal))
        {
            return ProbeResult.Declined(refusal);
        }

        int pitch = scanout.PitchBytes;
        IntelGraphicsPipe[] pipes = PipesScanning(registers, generation, scanout.SurfaceAddress, pitch);
        int width = 0;
        int height = 0;
        for (int i = 0; i < pipes.Length; i++)
        {
            width = Math.Max(width, pipes[i].Width);
            height = Math.Max(height, pipes[i].Height);
        }

        if (pitch < width * BytesPerPixel)
        {
            return ProbeResult.Declined($"pipe {scanout.Index} has a {width}x{height} plane over a surface {pitch} bytes wide");
        }

        // 5. Where the surface is: in the translation table, which the CPU
        //    reaches through the aperture, and in stolen memory behind it.
        ushort graphicsControl = pci.ReadConfig16(IntelGraphicsRegisters.GraphicsControl);
        IntelGraphicsGtt gtt = new(registers, generation, graphicsControl);
        DeviceRegion aperture = binding.MapRegion(ApertureBar, RegionCaching.WriteCombining);
        ulong surface = scanout.SurfaceAddress;
        ulong frameBytes = (ulong)pitch * (ulong)height;
        ulong reach = Math.Min(aperture.Length, gtt.Size);
        if (surface > reach || reach - surface < frameBytes)
        {
            return ProbeResult.Declined($"the surface at GPU address 0x{surface:X} runs past the {reach / BytesPerMebibyte} MiB the CPU reaches through the aperture");
        }

        if (!gtt.TryTranslate(surface, out ulong surfacePhysical, out ulong entryFlags))
        {
            return ProbeResult.Declined($"the translation table does not map the surface at GPU address 0x{surface:X}");
        }

        IntelGraphicsStolenMemory stolen = IntelGraphicsStolenMemory.Read(pci, registers, generation, graphicsControl);
        ulong stolenNext = 0;
        ulong stolenRoom = 0;
        if (IsStolenBlock(gtt, stolen, surface, surfacePhysical, frameBytes))
        {
            stolenNext = AlignUp(surfacePhysical + frameBytes, IntelGraphicsGtt.PageBytes);
            stolenRoom = stolen.UsableEnd > stolenNext ? stolen.UsableEnd - stolenNext : 0;
        }
        else
        {
            binding.Log($"the surface at 0x{surfacePhysical:X} is not one block of stolen memory; drawing into it without flipping, no hardware cursor");
        }

        // 6. The frames and cursor images: stolen memory after the surface,
        //    mapped after it in the table, inside what the aperture reaches.
        int refreshRate = scanout.MeasureRefreshRate(binding);
        ulong spareStart = AlignUp(surface + frameBytes, SurfaceAlignment);
        ulong spareReach = reach > spareStart ? reach - spareStart : 0;
        ulong frameStride = AlignUp(frameBytes, SurfaceAlignment);
        ulong stolenFrameBytes = AlignUp(frameBytes, IntelGraphicsGtt.PageBytes);
        int frameCount = FlipFrameCount(binding, scanout, refreshRate);
        while (frameCount > 1 && (stolenFrameBytes * (ulong)(frameCount - 1) > stolenRoom || frameStride * (ulong)(frameCount - 1) > spareReach))
        {
            frameCount--;
        }

        DeviceRegion[] frames = new DeviceRegion[frameCount];
        ulong[] frameAddresses = new ulong[frameCount];
        frames[0] = aperture.Slice(surface, frameBytes);
        frameAddresses[0] = surface;
        for (int i = 1; i < frameCount; i++)
        {
            ulong address = spareStart + frameStride * (ulong)(i - 1);
            gtt.Map(address, stolenNext + stolenFrameBytes * (ulong)(i - 1), frameBytes, entryFlags);
            frames[i] = aperture.Slice(address, frameBytes);
            frameAddresses[i] = address;
        }

        ulong cursorAddress = spareStart + frameStride * (ulong)(frameCount - 1);
        ulong cursorStolen = stolenNext + stolenFrameBytes * (ulong)(frameCount - 1);
        ulong cursorBytes = IntelGraphicsState.CursorImageBytes * CursorImageCount;
        bool hasCursor = stolenRoom >= cursorStolen - stolenNext + cursorBytes && spareReach >= cursorAddress - spareStart + cursorBytes;
        DeviceRegion[] cursorImages = hasCursor ? new DeviceRegion[CursorImageCount] : [];
        ulong[] cursorAddresses = hasCursor ? new ulong[CursorImageCount] : [];
        for (int i = 0; i < cursorImages.Length; i++)
        {
            ulong offset = IntelGraphicsState.CursorImageBytes * (ulong)i;
            gtt.Map(cursorAddress + offset, cursorStolen + offset, IntelGraphicsState.CursorImageBytes, entryFlags);
            cursorImages[i] = aperture.Slice(cursorAddress + offset, IntelGraphicsState.CursorImageBytes);
            cursorAddresses[i] = cursorAddress + offset;
        }

        // 7. The display, its spare frames cleared while the firmware's
        //    surface stays on screen, and the ring.
        DisplayMode mode = new(width, height, pitch, BitsPerPixel, refreshRate);
        IntelGraphicsState state = new(binding, pipes, mode, frames, frameAddresses, cursorImages, cursorAddresses);
        state.Prepare();
        binding.DriverState = state;
        binding.PublishDisplay(state);

        string cursor = hasCursor ? "hardware cursor" : "no room for a cursor";
        binding.Log(
            $"{generation.Name}, display {generation.DisplayVersion}: {width}x{height}x{BitsPerPixel} @ {refreshRate} Hz on {PipeList(pipes)}, " +
            $"surface 0x{surface:X} at 0x{surfacePhysical:X}, stolen {stolen.Size / BytesPerMebibyte} MiB at 0x{stolen.Base:X} " +
            $"({(stolen.UsableEnd - stolen.Base) / BytesPerMebibyte} MiB usable), aperture {aperture.Length / BytesPerMebibyte} MiB, " +
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
        if (binding.DriverState is not IntelGraphicsState state || !reason.HardwarePresent)
        {
            return;
        }

        state.Release();
    }

    // --- Private methods ---

    /// <summary>
    /// The lowest pipe whose primary plane scans out a surface this driver
    /// can draw into, each enabled plane logged on the way. Reads only.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="registers">BAR 0.</param>
    /// <param name="generation">The engine's generation.</param>
    /// <param name="scanout">The pipe, when one qualifies.</param>
    /// <param name="refusal">Why none qualifies: the first unsupported plane's problem, or that none is on.</param>
    /// <returns>False when no pipe qualifies.</returns>
    private static bool TryFindScanout(DeviceBinding binding, RegisterWindow registers, IntelGraphicsGeneration generation, [NotNullWhen(true)] out IntelGraphicsPipe? scanout, out string refusal)
    {
        scanout = null;
        refusal = (registers.Read32(IntelGraphicsRegisters.VgaControl) & IntelGraphicsRegisters.VgaDisplayDisable) == 0
            ? "the display engine is in VGA mode; boot through UEFI so the firmware leaves a linear surface on screen"
            : "no pipe is scanning out a plane: the firmware left the display engine off";
        bool refused = false;
        for (int i = 0; i < generation.PipeCount; i++)
        {
            IntelGraphicsPipe pipe = new(registers, generation, i);
            if (!pipe.IsPlaneEnabled)
            {
                continue;
            }

            binding.Log(
                $"pipe {i}: surface 0x{pipe.SurfaceAddress:X}, plane {pipe.Width}x{pipe.Height} at {pipe.X},{pipe.Y}, " +
                $"pitch {pipe.PitchBytes}, control 0x{pipe.Control:X8}");

            string? problem = pipe.Unsupported();
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

    /// <summary>Every pipe whose plane fetches the surface at <paramref name="surface"/> with the same pitch, lowest first.</summary>
    private static IntelGraphicsPipe[] PipesScanning(RegisterWindow registers, IntelGraphicsGeneration generation, ulong surface, int pitch)
    {
        int count = 0;
        IntelGraphicsPipe[] found = new IntelGraphicsPipe[generation.PipeCount];
        for (int i = 0; i < generation.PipeCount; i++)
        {
            IntelGraphicsPipe pipe = new(registers, generation, i);
            if (pipe.IsPlaneEnabled && pipe.SurfaceAddress == surface && pipe.PitchBytes == pitch && pipe.Unsupported() is null)
            {
                found[count++] = pipe;
            }
        }

        return found.AsSpan(0, count).ToArray();
    }

    /// <summary>
    /// Whether the surface is one block of stolen memory: its first and last
    /// pages map to stolen pages as far apart as they are in GPU address
    /// space. Only then is the stolen memory after it known to be free.
    /// </summary>
    private static bool IsStolenBlock(IntelGraphicsGtt gtt, IntelGraphicsStolenMemory stolen, ulong surface, ulong surfacePhysical, ulong frameBytes)
    {
        ulong lastPage = (frameBytes - 1) & ~(IntelGraphicsGtt.PageBytes - 1);
        return stolen.Contains(surfacePhysical, frameBytes)
            && gtt.TryTranslate(surface + lastPage, out ulong lastPhysical, out _)
            && lastPhysical == surfacePhysical + lastPage;
    }

    /// <summary>
    /// How many frames to flip between, before stolen memory is counted: one
    /// when a flip could not be seen to latch (a frame counter that does not
    /// move), three otherwise.
    /// </summary>
    private static int FlipFrameCount(DeviceBinding binding, IntelGraphicsPipe scanout, int refreshRate)
    {
        if (refreshRate == 0)
        {
            binding.Log($"pipe {scanout.Index}: the frame counter does not move; drawing into the firmware's surface without flipping");
            return 1;
        }

        return MaxFrames;
    }

    /// <summary>The pipes as the log names them: <c>pipe 0</c>, <c>pipes 0+1</c>.</summary>
    private static string PipeList(IntelGraphicsPipe[] pipes)
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
