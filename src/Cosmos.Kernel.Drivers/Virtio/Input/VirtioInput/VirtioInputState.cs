// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers.Virtio.Input.VirtioInput;

/// <summary>
/// Everything <see cref="VirtioInputDriver"/> holds for one bound device,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the keyboard or
/// pointer it publishes: the access, the event queue and its buffers, the
/// sink, the drain work item and the counters the Virtio suite reads. Only
/// the drain touches the queue, on the kit worker, so no lock is needed;
/// <see cref="OnInterrupt"/> runs in interrupt context and only counts and
/// schedules the drain. Events are read through the DMA span, never a
/// pointer.
/// </summary>
public sealed class VirtioInputState : IKeyboard, IPointer
{
    /// <summary>The event queue's index (eventq).</summary>
    internal const ushort EventQueue = 0;

    /// <summary>The status queue's index (statusq); not driven, so the keyboard's indicators are not lit.</summary>
    internal const ushort StatusQueue = 1;

    /// <summary>Bytes of one event: type, code and value (virtio 5.8.6).</summary>
    internal const int EventBytes = 8;

    /// <summary>How many event buffers are posted at once.</summary>
    internal const int PostedEvents = 32;

    /// <summary>EV_SYN: the end of a batch of events.</summary>
    internal const ushort EventTypeSync = 0;

    /// <summary>EV_KEY: a key or button changed state.</summary>
    internal const ushort EventTypeKey = 1;

    /// <summary>EV_REL: a relative axis moved.</summary>
    internal const ushort EventTypeRelative = 2;

    /// <summary>REL_X: horizontal movement.</summary>
    private const ushort RelativeX = 0;

    /// <summary>REL_Y: vertical movement.</summary>
    private const ushort RelativeY = 1;

    /// <summary>REL_WHEEL: wheel movement.</summary>
    private const ushort RelativeWheel = 8;

    /// <summary>BTN_LEFT.</summary>
    private const ushort ButtonLeft = 0x110;

    /// <summary>BTN_RIGHT.</summary>
    private const ushort ButtonRight = 0x111;

    /// <summary>BTN_MIDDLE.</summary>
    private const ushort ButtonMiddle = 0x112;

    /// <summary>Offset of the 16-bit event type within an event.</summary>
    private const int TypeOffset = 0;

    /// <summary>Offset of the 16-bit event code within an event.</summary>
    private const int CodeOffset = 2;

    /// <summary>Offset of the 32-bit event value within an event.</summary>
    private const int ValueOffset = 4;

    private readonly DeviceBinding _binding;
    private readonly VirtioAccess _access;
    private readonly Virtqueue _eventQueue;
    private readonly DmaBuffer _events;
    private readonly bool _isMouse;
    private KeyboardSink? _keyboardSink;
    private PointerSink? _pointerSink;
    private WorkItem? _drainWork;
    private int _deltaX;
    private int _deltaY;
    private int _wheel;
    private PointerButtons _buttons;
    private bool _pending;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile int _eventsProcessed;
    private volatile int _interruptCount;

    /// <summary>Takes the access, the queue and the buffers the probe acquired; the event slots are already posted. Thread context, from the probe.</summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="access">The kit's access to the device.</param>
    /// <param name="eventQueue">Queue 0, descriptors 0 to <see cref="PostedEvents"/> - 1 posted with their slots.</param>
    /// <param name="events">One slot of <see cref="EventBytes"/> per posted descriptor.</param>
    /// <param name="isMouse">True when the device reports relative events and is published as a pointer.</param>
    internal VirtioInputState(DeviceBinding binding, VirtioAccess access, Virtqueue eventQueue, DmaBuffer events, bool isMouse)
    {
        _binding = binding;
        _access = access;
        _eventQueue = eventQueue;
        _events = events;
        _isMouse = isMouse;
    }

    /// <inheritdoc/>
    public string Name => _isMouse ? "virtio-mouse" : "virtio-keyboard";

    /// <summary>True when the device is published as a pointer, false as a keyboard. Any context.</summary>
    public bool IsMouse => _isMouse;

    /// <summary>How many events the drain read and handled. Any context.</summary>
    public int EventsProcessed => _eventsProcessed;

    /// <summary>How many times the queue's source ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>True when the event queue's source is connected to <see cref="OnInterrupt"/>. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when the kit runs the drain periodically because the event queue has no interrupt. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>The access, for the driver's detach hook. Any context.</summary>
    internal VirtioAccess Access => _access;

    /// <summary>The sink keys go to; set by the probe when a keyboard is published, null for a pointer.</summary>
    internal KeyboardSink? KeyboardSink
    {
        get => _keyboardSink;
        set => _keyboardSink = value;
    }

    /// <summary>The sink movement goes to; set by the probe when a pointer is published, null for a keyboard.</summary>
    internal PointerSink? PointerSink
    {
        get => _pointerSink;
        set => _pointerSink = value;
    }

    /// <summary>The work item running <see cref="Drain"/>, which the handler schedules; set by the probe before the source is connected.</summary>
    internal WorkItem? DrainWork
    {
        get => _drainWork;
        set => _drainWork = value;
    }

    /// <summary>Nothing: the status queue is not driven, so the indicators stay as they are. Thread context, from the ring.</summary>
    /// <param name="leds">The indicators asked for; ignored.</param>
    public void SetLeds(KeyboardLeds leds)
    {
    }

    /// <summary>The queue's handler: counts the interrupt and schedules the drain. Interrupt context; allocation-free.</summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        _interruptCount++;
        WorkItem? drain = _drainWork;
        if (drain is not null)
        {
            context.Schedule(drain);
        }
    }

    /// <summary>
    /// The drain: takes every used event, reads it from its slot through the
    /// DMA span (an element shorter than an event is skipped), reports a key
    /// to the keyboard sink or folds a movement or button into the pending
    /// report and sends it on the sync event, re-posts the slot, and kicks
    /// the queue once when anything was taken. Idempotent, so the source and
    /// the periodic schedule can both run it. Thread context on the kit
    /// worker.
    /// </summary>
    internal void Drain()
    {
        bool taken = false;
        while (_eventQueue.TryTakeUsed(out ushort id, out uint length))
        {
            taken = true;
            if (id >= PostedEvents)
            {
                // Out of contract: the device returned a descriptor that was
                // never posted, so there is no slot to read or re-post.
                continue;
            }

            if (length >= EventBytes)
            {
                ReadOnlySpan<byte> slot = _events.Span.Slice(id * EventBytes, EventBytes);
                ushort type = MemoryMarshal.Read<ushort>(slot.Slice(TypeOffset));
                ushort code = MemoryMarshal.Read<ushort>(slot.Slice(CodeOffset));
                uint value = MemoryMarshal.Read<uint>(slot.Slice(ValueOffset));
                Handle(type, code, value);
                _eventsProcessed++;
            }

            ulong physical = _events.PhysicalAddress + (ulong)(id * EventBytes);
            _eventQueue.SetDescriptor(id, physical, EventBytes, VirtqueueDescriptorFlags.Write);
            _eventQueue.Submit(id);
        }

        if (taken)
        {
            _eventQueue.Notify();
        }
    }

    /// <summary>Routes one event to the keyboard or the pointer path. Thread context on the kit worker.</summary>
    private void Handle(ushort type, ushort code, uint value)
    {
        if (!_isMouse)
        {
            if (type == EventTypeKey)
            {
                ReportKey(VirtioInputKeyMap.ToScanCode(code), value == 0);
            }

            return;
        }

        switch (type)
        {
            case EventTypeRelative:
                AccumulateMovement(code, (int)value);
                break;
            case EventTypeKey:
                SetButton(code, value != 0);
                break;
            case EventTypeSync:
                if (_pending)
                {
                    ReportPointer();
                }

                break;
        }
    }

    /// <summary>Folds one relative axis event into the pending report. Thread context on the kit worker.</summary>
    private void AccumulateMovement(ushort code, int delta)
    {
        switch (code)
        {
            case RelativeX:
                _deltaX += delta;
                _pending = true;
                break;
            case RelativeY:
                _deltaY += delta;
                _pending = true;
                break;
            case RelativeWheel:
                _wheel += delta;
                _pending = true;
                break;
        }
    }

    /// <summary>Folds one button event into the pending report. Thread context on the kit worker.</summary>
    private void SetButton(ushort code, bool pressed)
    {
        PointerButtons button = code switch
        {
            ButtonLeft => PointerButtons.Left,
            ButtonRight => PointerButtons.Right,
            ButtonMiddle => PointerButtons.Middle,
            _ => PointerButtons.None,
        };
        if (button == PointerButtons.None)
        {
            return;
        }

        _buttons = pressed ? _buttons | button : _buttons & ~button;
        _pending = true;
    }

    /// <summary>Sends the pending report to the pointer sink and clears the deltas; the buttons persist. Thread context on the kit worker.</summary>
    private void ReportPointer()
    {
        int deltaX = _deltaX;
        int deltaY = _deltaY;
        int wheel = _wheel;
        _deltaX = 0;
        _deltaY = 0;
        _wheel = 0;
        _pending = false;
        PointerSink? sink = _pointerSink;
        if (sink is null)
        {
            return;
        }

        try
        {
            sink.ReportRelative(deltaX, deltaY, _buttons, wheel);
        }
        catch (Exception exception)
        {
            _binding.Log($"a pointer report threw: {exception.Message}");
        }
    }

    /// <summary>
    /// Hands one key to the keyboard sink, fencing whatever the ring does
    /// with it: the kit cancels a work item that throws, which would stop
    /// every later drain, so a report that throws is logged and the walk
    /// goes on. Thread context on the kit worker.
    /// </summary>
    private void ReportKey(byte scanCode, bool released)
    {
        KeyboardSink? sink = _keyboardSink;
        if (sink is null)
        {
            return;
        }

        try
        {
            sink.Report(scanCode, released);
        }
        catch (Exception exception)
        {
            _binding.Log($"a key report threw: {exception.Message}");
        }
    }
}
