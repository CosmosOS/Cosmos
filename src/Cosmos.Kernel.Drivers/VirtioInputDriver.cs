// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;
using Cosmos.Kernel.System;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio-input driver (virtio specification section 5.8) over the
/// driver kit: asks the device which event types it reports, creates the
/// event queue on the kit's access, posts a batch of event buffers,
/// connects the queue's source when the transport delivers it and polls
/// otherwise, then publishes a keyboard (key events only) or a pointer
/// (key and relative events) to the ring. The same device type is either,
/// so the driver carries no feature and checks
/// <see cref="KernelFeatures.Keyboard"/> or <see cref="KernelFeatures.Mouse"/>
/// in the probe once it knows which one the device is. Everything it holds
/// for one device lives on a <see cref="VirtioInputState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver]
public sealed class VirtioInputDriver : Driver
{
    /// <summary>VIRTIO_INPUT_CFG_EV_BITS: the select value that asks for the event codes of one type.</summary>
    private const byte ConfigSelectEventBits = 0x11;

    /// <summary>Offset of the select byte in the configuration space.</summary>
    private const uint SelectOffset = 0;

    /// <summary>Offset of the subsel byte in the configuration space: the event type asked about.</summary>
    private const uint SubselOffset = 1;

    /// <summary>Offset of the size byte in the configuration space: bytes of the answer, 0 when the type is not reported.</summary>
    private const uint SizeOffset = 2;

    /// <summary>The queue size asked for; the device's maximum caps it.</summary>
    private const ushort QueueSize = 64;

    /// <summary>Alignment of the event buffer area: one page.</summary>
    private const int EventAlignment = 4096;

    /// <summary>Period of the drain when the event queue has no interrupt.</summary>
    private const uint PollPeriodMilliseconds = 20;

    private readonly DeviceMatch[] _matches =
    [
        VirtioMatch.DeviceType(VirtioDeviceType.Input),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(VirtioInputDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Brings the device up and publishes it as a keyboard or a pointer.
    /// Thread context on the kit worker; a declined or failed result makes
    /// the kit release everything acquired here and reset the device
    /// through its hooks.
    /// </summary>
    /// <param name="binding">The virtio node and the kit facilities for it.</param>
    /// <returns>Bound with the device published; declined when the device reports no keys or its kind is compiled out; failed when it did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. No device-specific feature.
        VirtioAccess dev = binding.Node.Access<VirtioAccess>();
        if (!dev.NegotiateFeatures(0, out _))
        {
            return ProbeResult.Failed("the device rejected the feature set");
        }

        // 2. Which event types the device reports, and whether the kernel
        //    carries the manager for what that makes it. Each switch is
        //    tested alone: a compound guard does not fold in Debug IL.
        bool supportsKey = ReportsEventType(dev, VirtioInputState.EventTypeKey);
        bool supportsRelative = ReportsEventType(dev, VirtioInputState.EventTypeRelative);
        if (!supportsKey)
        {
            return ProbeResult.Declined("no key events");
        }

        bool isMouse = supportsRelative;
        if (isMouse)
        {
            if (!KernelFeatures.Mouse)
            {
                return ProbeResult.Declined("mouse support is compiled out");
            }
        }
        else
        {
            if (!KernelFeatures.Keyboard)
            {
                return ProbeResult.Declined("keyboard support is compiled out");
            }
        }

        // 3. The event queue and its posted buffers, not yet notified.
        if (!dev.TryCreateQueue(binding, VirtioInputState.EventQueue, QueueSize, out Virtqueue? eventQueue))
        {
            return ProbeResult.Failed("no event queue");
        }

        DmaBuffer events = binding.AllocateDma(VirtioInputState.PostedEvents * VirtioInputState.EventBytes, EventAlignment);
        for (int i = 0; i < VirtioInputState.PostedEvents; i++)
        {
            if (!eventQueue.TryAllocateDescriptor(out ushort slot))
            {
                return ProbeResult.Failed("the event queue ran out of descriptors while posting");
            }

            ulong physical = events.PhysicalAddress + (ulong)(slot * VirtioInputState.EventBytes);
            eventQueue.SetDescriptor(slot, physical, VirtioInputState.EventBytes, VirtqueueDescriptorFlags.Write);
            eventQueue.Submit(slot);
        }

        // 4. The state and the drain; scheduling it once tells whether a
        //    worker exists to run it.
        VirtioInputState state = new(binding, dev, eventQueue, events, isMouse);
        binding.DriverState = state;
        WorkItem drain = binding.CreateWorkItem(state.Drain);
        state.DrainWork = drain;
        if (!drain.Schedule())
        {
            return ProbeResult.Declined("no kit worker to run the drain on");
        }

        // 5. The queue's source, or the periodic drain.
        bool hasInterrupt = binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioInputState.EventQueue), state.OnInterrupt, out _);
        state.HasInterrupt = hasInterrupt;
        bool polling = false;
        if (!hasInterrupt)
        {
            polling = binding.TrySchedulePeriodic(PollPeriodMilliseconds, drain);
            if (!polling)
            {
                return ProbeResult.Declined("no interrupt and no timer to poll with");
            }
        }

        state.IsPolling = polling;

        // 6. DRIVER_OK, the kick that starts the event queue, then the ring.
        dev.SetDriverOk();
        eventQueue.Notify();
        if (isMouse)
        {
            state.PointerSink = binding.PublishPointer(state);
        }
        else
        {
            state.KeyboardSink = binding.PublishKeyboard(state);
        }

        string kind = isMouse ? "pointer" : "keyboard";
        string wake = hasInterrupt ? "interrupt" : $"polling every {PollPeriodMilliseconds} ms";
        binding.Log($"{kind}, {wake}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Resets the device so it stops before the kit frees the event
    /// buffers. Thread context on the kit worker; nothing is written when
    /// the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its handles are already disconnected.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not VirtioInputState || !reason.HardwarePresent)
        {
            return;
        }

        binding.Node.Access<VirtioAccess>().Reset();
    }

    /// <summary>Asks the configuration space whether the device reports events of <paramref name="eventType"/>. Thread context.</summary>
    private static bool ReportsEventType(VirtioAccess dev, ushort eventType)
    {
        dev.WriteConfig8(SelectOffset, ConfigSelectEventBits);
        dev.WriteConfig8(SubselOffset, (byte)eventType);
        return dev.ReadConfig8(SizeOffset) > 0;
    }
}
