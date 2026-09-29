// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio-gpu driver (virtio specification section 5.7) over the
/// driver kit, 2D path only: creates the control queue on the kit's access,
/// allocates one page of scratch DMA for the commands and their responses,
/// connects the control queue's interrupt when the transport delivers one
/// and polls otherwise, asks the device for the first scanout's size,
/// allocates a DMA framebuffer of that size, creates one host 2D resource
/// backed by it, binds it to scanout 0 and publishes the display to the
/// ring; the ring's canvas draws into the framebuffer and every
/// <c>Flush</c> pushes the dirty rectangle through TRANSFER_TO_HOST_2D and
/// RESOURCE_FLUSH. No virgl: the guest renders, the host composites.
/// Everything it holds for one device lives on a
/// <see cref="VirtioGpuState"/> in <see cref="DeviceBinding.DriverState"/>.
/// The transport underneath (PCI on both architectures, MMIO on ARM64) is
/// invisible here. <see cref="Probe"/> and <see cref="OnDetach"/> run in
/// thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Graphics)]
public sealed class VirtioGpuDriver : Driver
{
    // --- Constants ---

    /// <summary>The features asked for: none; VIRTIO_F_VERSION_1 comes from the kit and the 2D path needs nothing else.</summary>
    private const uint RequestedFeatures = 0;

    /// <summary>The queue size asked for; the device's maximum caps it. Three descriptors are in use at most.</summary>
    private const ushort QueueSize = 16;

    /// <summary>Width assumed when the device reports no enabled scanout.</summary>
    private const int DefaultWidth = 1024;

    /// <summary>Height assumed when the device reports no enabled scanout.</summary>
    private const int DefaultHeight = 768;

    /// <summary>How long the detach hook waits after the reset, so DMA in flight lands before the framebuffer and the rings are freed.</summary>
    private const uint QuiesceMicroseconds = 100;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        VirtioMatch.DeviceType(VirtioDeviceType.Gpu),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(VirtioGpuDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the device up and publishes its display. Thread context on
    /// the kit worker; a failed result makes the kit release everything
    /// acquired here and reset the device through its hooks.
    /// </summary>
    /// <param name="binding">The virtio node and the kit facilities for it.</param>
    /// <returns>Bound with the display published; failed when the device did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The features.
        VirtioAccess dev = binding.Node.Access<VirtioAccess>();
        if (!dev.NegotiateFeatures(RequestedFeatures, out _))
        {
            return ProbeResult.Failed("the device rejected the feature set");
        }

        // 2. The control queue, and the cursor queue when the device has one.
        if (!dev.TryCreateQueue(binding, VirtioGpuState.ControlQueue, QueueSize, out Virtqueue? controlQueue))
        {
            return ProbeResult.Failed("no control queue");
        }

        if (!dev.TryCreateQueue(binding, VirtioGpuState.CursorQueue, QueueSize, out Virtqueue? cursorQueue))
        {
            binding.Log("no cursor queue; the cursor is not driven");
        }

        // 3. The scratch block: the command, the memory entry and the
        //    response, each at its offset; then the state, its lock and
        //    its event.
        DmaBuffer scratch = binding.AllocateDma(VirtioGpuState.PageBytes, VirtioGpuState.PageBytes);
        VirtioGpuState state = new(binding, dev, controlQueue, cursorQueue, scratch);
        binding.DriverState = state;
        state.Lock = binding.CreateLock();
        state.Event = binding.CreateEvent();

        // 4. The control queue's source, when the transport delivers one;
        //    the commands poll the used ring otherwise.
        bool connected = binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioGpuState.ControlQueue), state.OnInterrupt, out _);
        state.HasInterrupt = connected;
        state.IsPolling = !connected;

        // 5. DRIVER_OK before the first command: no notification before it.
        dev.SetDriverOk();

        // 6. The scanouts and the first one's size.
        uint scanouts = dev.ReadConfig32(VirtioGpuCmd.ConfigNumScanoutsOffset);
        if (scanouts == 0)
        {
            scanouts = 1;
        }

        state.ScanoutCount = (int)scanouts;
        if (!state.TryGetDisplayInfo(out int width, out int height))
        {
            if (state.IsFaulted)
            {
                return ProbeResult.Failed("the device did not answer GET_DISPLAY_INFO");
            }

            width = DefaultWidth;
            height = DefaultHeight;
        }

        // 7. The framebuffer, whole pages. The device's answer is bounded
        //    in TryGetDisplayInfo; the count is still taken in long so no
        //    truncated length can reach the DMA allocation.
        long frameBytes = (long)width * height * VirtioGpuState.BytesPerPixel;
        if (frameBytes > int.MaxValue - VirtioGpuState.PageBytes)
        {
            return ProbeResult.Failed("scanout too large");
        }

        int framebufferBytes = (int)((frameBytes + VirtioGpuState.PageBytes - 1) / VirtioGpuState.PageBytes * VirtioGpuState.PageBytes);
        DmaBuffer framebuffer = binding.AllocateDma(framebufferBytes, VirtioGpuState.PageBytes);
        state.SetGeometry(width, height, framebuffer);

        // 8. The resource, its backing and the scanout.
        if (!state.CreateScanoutResource())
        {
            return ProbeResult.Failed(Failure(state, "RESOURCE_CREATE_2D"));
        }

        if (!state.AttachFramebuffer())
        {
            return ProbeResult.Failed(Failure(state, "RESOURCE_ATTACH_BACKING"));
        }

        if (!state.SetScanout())
        {
            return ProbeResult.Failed(Failure(state, "SET_SCANOUT"));
        }

        // 9. The ring.
        state.Sink = binding.PublishDisplay(state);
        string wake = connected ? "interrupt" : "polling";
        binding.Log($"{width}x{height}, {scanouts} scanouts, {wake}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Resets the device so it stops scanning out before the kit frees the
    /// framebuffer and the rings, then waits a moment for DMA in flight
    /// (the status read-back is inside the reset). Thread context on the
    /// kit worker; nothing is written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its handles are already disconnected.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not VirtioGpuState || !reason.HardwarePresent)
        {
            return;
        }

        binding.Node.Access<VirtioAccess>().Reset();
        binding.Delay(QuiesceMicroseconds);
    }

    // --- Private methods ---

    /// <summary>The reason a command's failure fails the probe: a device that did not answer, or one that refused. Thread context.</summary>
    private static string Failure(VirtioGpuState state, string command) =>
        state.IsFaulted ? $"the device did not answer {command}" : $"{command} failed";
}
