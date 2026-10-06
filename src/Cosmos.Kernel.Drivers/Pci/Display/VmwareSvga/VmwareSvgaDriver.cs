// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;

/// <summary>
/// The VMware SVGA II display driver over the driver kit: binds the
/// adapter's PCI function, speaks the version 2 register protocol through
/// its index and value ports (BAR 0), maps its VRAM (BAR 1) and command
/// FIFO (BAR 2), initialises the FIFO with the SVGA3D negotiation, and
/// publishes the display to the ring, as a <see cref="VmwareSvga3DState"/>
/// when the host negotiated 3D and a <see cref="VmwareSvgaState"/>
/// otherwise. The firmware framebuffer sits in the VRAM window, so the kit
/// retires it when this driver binds. The ring's canvas is the back buffer
/// and draws into one frame of VRAM; every <c>Flush</c> is an UPDATE. The
/// SVGA3D command layer is <see cref="VmwareSvga3D"/>, reached through the
/// <see cref="VmwareSvgaCanvas3D"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Graphics)]
public sealed class VmwareSvgaDriver : Driver
{
    // --- Constants ---

    /// <summary>PCI vendor id of VMware.</summary>
    private const ushort VmwareVendorId = 0x15AD;

    /// <summary>PCI device id of the SVGA II adapter.</summary>
    private const ushort SvgaIIDeviceId = 0x0405;

    /// <summary>Index of the port range resource, BAR 0.</summary>
    private const int RegistersBar = 0;

    /// <summary>Index of the VRAM resource, BAR 1.</summary>
    private const int VramBar = 1;

    /// <summary>Index of the FIFO resource, BAR 2.</summary>
    private const int FifoBar = 2;

    /// <summary>Bytes in a mebibyte, for the log.</summary>
    private const uint BytesPerMebibyte = 1024 * 1024;

    /// <summary>Bytes in a kibibyte, for the log.</summary>
    private const uint BytesPerKibibyte = 1024;

    /// <summary>The shift of the major half of an SVGA3D version.</summary>
    private const int VersionMajorShift = 16;

    /// <summary>The mask of the minor half of an SVGA3D version.</summary>
    private const uint VersionMinorMask = 0xFFFF;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(vendorId: VmwareVendorId, deviceId: SvgaIIDeviceId),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(VmwareSvgaDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the adapter up and publishes its display. Thread context on
    /// the kit worker; a failed result makes the kit release everything
    /// acquired here.
    /// </summary>
    /// <param name="binding">The PCI node and the kit facilities for it.</param>
    /// <returns>Bound with the display published; declined on a platform without port I/O; failed when the adapter did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. Decode on: QEMU answers the FrameBufferStart and MemStart
        //    registers with the BAR addresses only while it is, and the
        //    kit's quiesce leaves the decode bits as firmware left them.
        PciAccess pci = binding.Node.Access<PciAccess>();
        pci.EnableIoSpace(true);
        pci.EnableMemorySpace(true);

        // 2. The ports and the version 2 protocol.
        RegisterWindow registers;
        try
        {
            registers = binding.MapRegisters(RegistersBar);
        }
        catch (PlatformNotSupportedException)
        {
            return ProbeResult.Declined("the adapter is programmed through port I/O, which this platform has not");
        }

        VmwareSvgaFifo.WriteRegister(registers, Register.ID, (uint)ID.V2);
        if (VmwareSvgaFifo.ReadRegister(registers, Register.ID) != (uint)ID.V2)
        {
            return ProbeResult.Failed("the adapter did not accept the version 2 protocol");
        }

        // 3. VRAM, the FIFO, and the registers that must agree with the BARs.
        DeviceRegion vram = binding.MapRegion(VramBar, RegionCaching.WriteCombining);
        DeviceRegion fifo = binding.MapRegion(FifoBar, RegionCaching.Device);
        VmwareSvgaFifo svga = new(registers, fifo);

        uint frameBufferStart = svga.ReadRegister(Register.FrameBufferStart);
        uint vramSize = svga.ReadRegister(Register.VRamSize);
        uint memStart = svga.ReadRegister(Register.MemStart);
        uint memSize = svga.ReadRegister(Register.MemSize);
        uint capabilities = svga.ReadRegister(Register.Capabilities);

        if (frameBufferStart != pci.Bars[VramBar].Base)
        {
            return ProbeResult.Failed($"register FrameBufferStart does not match BAR {VramBar}");
        }

        if (memStart != pci.Bars[FifoBar].Base)
        {
            return ProbeResult.Failed($"register MemStart does not match BAR {FifoBar}");
        }

        // 4. The FIFO: the head registers, the guest's 3D version, ConfigDone,
        //    the negotiation. No Enable write until a mode is programmed.
        uint fifoBytes = (uint)Math.Min(memSize, fifo.Length);
        svga.Configure(capabilities, fifoBytes);
        svga.InitializeFifo();

        // 5. The scanout as firmware left it. The Enable shadow is seeded
        //    from the register: a scanout the firmware enabled is flushed
        //    from the first UPDATE.
        int width = (int)svga.ReadRegister(Register.Width);
        int height = (int)svga.ReadRegister(Register.Height);
        int bitsPerPixel = (int)svga.ReadRegister(Register.BitsPerPixel);
        int pitch = (int)svga.ReadRegister(Register.BytesPerLine);
        uint frameOffset = svga.ReadRegister(Register.FrameBufferOffset);
        bool enabled = svga.ReadRegister(Register.Enable) == 1;
        svga.RecordEnabled(enabled);

        // 6. The state, 3D when negotiated, and the ring.
        VmwareSvgaState state = svga.Is3DNegotiated
            ? new VmwareSvga3DState(binding, svga, vram)
            : new VmwareSvgaState(binding, svga, vram);
        state.RecordFoundMode(width, height, bitsPerPixel, pitch, frameOffset, enabled);
        binding.DriverState = state;
        state.Sink = binding.PublishDisplay(state);

        string scanout = enabled ? "enabled" : "disabled";
        string svga3d = svga.Is3DNegotiated
            ? $"{svga.Svga3DVersion >> VersionMajorShift}.{svga.Svga3DVersion & VersionMinorMask}"
            : "none";
        binding.Log($"{width}x{height}x{bitsPerPixel} {scanout}, caps 0x{capabilities:X}, svga3d {svga3d}, vram {vramSize / BytesPerMebibyte} MiB, fifo {memSize / BytesPerKibibyte} KiB");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Turns the scanout off so the host stops reading VRAM the ring may
    /// still hold a region over. Thread context on the kit worker; the
    /// ports are still mapped here, and nothing is written when the
    /// hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its display is already withdrawn.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not VmwareSvgaState state || !reason.HardwarePresent)
        {
            return;
        }

        state.Fifo.SetEnabled(false);
    }
}
