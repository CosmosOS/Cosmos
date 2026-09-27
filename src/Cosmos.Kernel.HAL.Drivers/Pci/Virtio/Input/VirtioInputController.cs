// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Rings;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Input;

/// <summary>
/// One virtio input device (virtio 1.2 §5.8): brings it up over the
/// transport, works out whether it is a keyboard or a pointer, and turns
/// the events it posts on the eventq into the reports of the keyboard or
/// the mouse it published to the kernel.
/// </summary>
/// <remarks>
/// <para>
/// One queue, reached from one context: the interrupt handler. Reporting a
/// key or a movement allocates nothing and takes no lock beyond masking
/// interrupts, so there is nothing to hand to a work item and the events
/// are drained in the handler itself, which is also what keeps typing
/// latency down to the interrupt.
/// </para>
/// <para>
/// The statusq, which a driver writes the lock lamps to, is not set up: the
/// device only ever shows them on a physical keyboard behind a passthrough,
/// and the keyboard is published without a lamp path rather than with one
/// that does nothing.
/// </para>
/// </remarks>
internal sealed class VirtioInputController
{
    /// <summary>
    /// Queues this driver sets up, which sizes the transport's doorbell
    /// table: the eventq alone. The statusq is queue 1, and never rung.
    /// </summary>
    internal const int QueueCount = 1;

    /// <summary>The eventq, queue 0, which the device posts input events on.</summary>
    private const ushort EventQueue = 0;

    /// <summary>
    /// Buffers wanted on the eventq, which the device lowers to what it
    /// offers. One event each, so 64 is a burst of a key press with its
    /// modifiers, or a flick of a mouse, without the handler having to run
    /// in between.
    /// </summary>
    private const int PreferredQueueLength = 64;

    // virtio_input_config (virtio 1.2 §5.8.4): a select/subselect pair the
    // driver writes, then the size and the payload of the answer.
    private const ulong SelectOffset = 0;
    private const ulong SubselectOffset = 1;
    private const ulong SizeOffset = 2;

    /// <summary>VIRTIO_INPUT_CFG_EV_BITS: select the bitmap of the codes the device reports for one event type.</summary>
    private const byte ConfigEventBits = 0x11;

    // MSI-X capability (PCI 3.0 §6.8.2), read to tell which interrupt path
    // the kit granted: the kit programs the capability, the driver only asks.
    private const byte MsiXCapabilityId = 0x11;
    private const ushort MsiXMessageControlOffset = 0x02;
    private const ushort MsiXEnableBit = 0x8000;

    private readonly PciDeviceContext _context;
    private readonly VirtioPciTransport _transport;

    // Set by Start, in the order it works through them. The kit arms the
    // interrupt handler only once Probe returned Bound, so by then the queue
    // and whichever of the two devices this is are set; the null checks
    // below are for the compiler, and for telling the two apart.
    private SplitVirtqueue? _events;
    private DmaBuffer? _buffers;
    private KeyboardReporter? _keyboard;
    private MouseReporter? _mouse;

    // A pointer's movement between two EV_SYN events: the device reports one
    // axis per event and the batch is only complete at the sync, so the
    // deltas add up here and go out as one report.
    private int _deltaX;
    private int _deltaY;
    private int _wheel;
    private MouseButtons _buttons;

    /// <summary>Takes over a device the transport has located the registers of.</summary>
    /// <param name="context">The binding attempt, which the controller allocates and publishes through.</param>
    /// <param name="transport">The device's registers, before the reset.</param>
    internal VirtioInputController(PciDeviceContext context, VirtioPciTransport transport)
    {
        _context = context;
        _transport = transport;
    }

    /// <summary>
    /// Brings the device up and publishes what it turned out to be: reset,
    /// features, the event types it reports, the queue, its buffers, then
    /// DRIVER_OK. Probe only.
    /// </summary>
    /// <returns>
    /// <see cref="ProbeResult.Bound"/> once the keyboard or the mouse is
    /// published; <see cref="ProbeResult.Declined"/> for an input device of
    /// a kind the kernel has no device for, such as a tablet;
    /// <see cref="ProbeResult.Failed"/> when a device of a kind it does have
    /// could not be driven, which the kit logged.
    /// </returns>
    internal ProbeResult Start()
    {
        if (!_transport.TryBegin())
        {
            return ProbeResult.Failed;
        }

        if (!_transport.TryNegotiateFeatures(VirtioPciTransport.FeatureVersion1, out ulong features))
        {
            return ProbeResult.Failed;
        }

        // The 64-bit ring addresses below come with VERSION_1; a device
        // offering neither it nor the legacy interface this driver declined
        // is one nothing here can drive.
        if ((features & VirtioPciTransport.FeatureVersion1) == 0)
        {
            _context.WriteLog("the device does not offer VIRTIO_F_VERSION_1");
            return ProbeResult.Failed;
        }

        // An absolute pointer is a tablet or a touchscreen, which the kernel
        // has no device for: declined, so a kernel's own driver may take it.
        // Checked before the relative axes, since a tablet reports buttons
        // and would otherwise read as a keyboard.
        if (ReportsEventType(InputEvent.TypeAbsolute))
        {
            _context.WriteLog("the device reports absolute axes, which no built-in driver handles");
            return ProbeResult.Declined;
        }

        bool pointer = ReportsEventType(InputEvent.TypeRelative);
        bool keys = ReportsEventType(InputEvent.TypeKey);
        if (!pointer && !keys)
        {
            _context.WriteLog("the device reports neither keys nor relative axes");
            return ProbeResult.Declined;
        }

        // Events sit in the used ring until something drains them, so a
        // device with no interrupt path of any kind is one whose keys would
        // never arrive. The kit polls the handler from the timer where it
        // cannot route a message, which is slower but works.
        if (!_context.TryRequestInterrupts(OnInterrupt))
        {
            _context.WriteLog("no MSI-X and no ticking timer to poll with");
            return ProbeResult.Failed;
        }

        // Only where the kit actually routed a message: a polled handler is
        // reached from the timer, and a vector the device raised into the
        // masked entry would be one nothing acknowledges.
        if (IsMsiXEnabled(_context.Function))
        {
            _transport.TryUseMessageVector();
        }

        // Before the queue is enabled, and so before DRIVER_OK lets the
        // device fetch a descriptor: bus mastering is what carries its reads
        // and writes, and the kit would otherwise only turn it on once Probe
        // returned Bound.
        _context.EnableBusMastering();

        if (!_transport.TryCreateQueue(EventQueue, PreferredQueueLength, out SplitVirtqueue? events))
        {
            return ProbeResult.Failed;
        }

        if (!_context.TryAllocateDma(events.Count * InputEvent.Size, ulong.MaxValue, out DmaBuffer? buffers))
        {
            _context.WriteLog("no DMA memory for the event buffers");
            return ProbeResult.Failed;
        }

        // Only once both exist, so the handler does not run against a
        // half-built queue if the device signals early.
        _events = events;
        _buffers = buffers;

        PostBuffers();

        // The device starts here, and only then is there any point ringing
        // its doorbell for the buffers posted above.
        _transport.Finish();
        _transport.Notify(EventQueue);

        // A device with relative axes is a mouse, whose buttons come as keys
        // too; one with keys alone is a keyboard.
        if (pointer)
        {
            _mouse = _context.PublishMouse();
        }
        else
        {
            _keyboard = _context.PublishKeyboard();
        }

        return ProbeResult.Bound;
    }

    /// <summary>
    /// The interrupt handler, and the whole event path. Interrupt context:
    /// no allocation, no throw, no string, no lock. The driver asked the
    /// device for one vector, which carries the queue and configuration
    /// changes alike, so any signal means the same thing: look at the ring.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        if (_events is not { } queue || _buffers is not { } buffers)
        {
            return;
        }

        bool posted = false;
        while (queue.TryTakeUsed(out int descriptor, out int written))
        {
            // A device that wrote less than an event is broken; reading the
            // rest of the buffer would report a key nobody pressed.
            if (written >= InputEvent.Size)
            {
                Handle(BufferOf(buffers, descriptor));
            }

            queue.Describe(descriptor, BufferAddressOf(buffers, descriptor), InputEvent.Size,
                VringDescriptor.DeviceWritable);
            queue.Offer(descriptor);
            posted = true;
        }

        if (posted)
        {
            _transport.Notify(EventQueue);
        }
    }

    /// <summary>Turns one event into a report of whichever device this is.</summary>
    /// <param name="input">The event the device wrote, <see cref="InputEvent.Size"/> bytes.</param>
    private void Handle(ReadOnlySpan<byte> input)
    {
        ushort type = InputEvent.ReadType(input);

        if (_keyboard is { } keyboard)
        {
            if (type != InputEvent.TypeKey)
            {
                return;
            }

            // Value 2 is the device repeating a key held down, which counts
            // as another press, as it does on a PS/2 keyboard.
            byte scanCode = ScanCodeMap.ToScanCode(InputEvent.ReadCode(input));
            if (scanCode != 0)
            {
                keyboard.Report(scanCode, InputEvent.ReadValue(input) == 0);
            }

            return;
        }

        if (_mouse is not { } mouse)
        {
            return;
        }

        switch (type)
        {
            case InputEvent.TypeRelative:
                Accumulate(InputEvent.ReadCode(input), InputEvent.ReadValue(input));
                break;

            case InputEvent.TypeKey:
                Hold(InputEvent.ReadCode(input), InputEvent.ReadValue(input) != 0);
                break;

            case InputEvent.TypeSync:
                mouse.Report(_deltaX, _deltaY, _wheel, _buttons);
                _deltaX = 0;
                _deltaY = 0;
                _wheel = 0;
                break;
        }
    }

    /// <summary>Adds one axis event to the movement the next sync reports.</summary>
    /// <param name="axis">The REL_* code.</param>
    /// <param name="value">How far it moved.</param>
    private void Accumulate(ushort axis, int value)
    {
        switch (axis)
        {
            case InputEvent.RelativeX:
                _deltaX += value;
                break;
            case InputEvent.RelativeY:
                _deltaY += value;
                break;
            case InputEvent.RelativeWheel:
                // evdev counts a wheel turned away from the user as
                // positive, the kit as negative.
                _wheel -= value;
                break;
        }
    }

    /// <summary>Records a button going down or coming up, which the next sync reports.</summary>
    /// <param name="button">The BTN_* code; anything else is a key a pointer has no use for.</param>
    /// <param name="pressed">True when it went down.</param>
    private void Hold(ushort button, bool pressed)
    {
        MouseButtons held = button switch
        {
            InputEvent.ButtonLeft => MouseButtons.Left,
            InputEvent.ButtonRight => MouseButtons.Right,
            InputEvent.ButtonMiddle => MouseButtons.Middle,
            _ => MouseButtons.None
        };

        if (pressed)
        {
            _buttons |= held;
        }
        else
        {
            _buttons &= ~held;
        }
    }

    /// <summary>Points every descriptor of the eventq at its buffer and offers it to the device.</summary>
    private void PostBuffers()
    {
        if (_events is not { } queue || _buffers is not { } buffers)
        {
            return;
        }

        while (queue.TryAllocateDescriptor(out int descriptor))
        {
            queue.Describe(descriptor, BufferAddressOf(buffers, descriptor), InputEvent.Size,
                VringDescriptor.DeviceWritable);
            queue.Offer(descriptor);
        }
    }

    /// <summary>
    /// Whether the device reports any code of <paramref name="type"/>, asked
    /// through the configuration window's select and subselect pair: the
    /// answer's size is zero for a type the device never sends.
    /// </summary>
    /// <param name="type">One of <see cref="InputEvent"/>'s type constants.</param>
    /// <returns>True when the device reports that type.</returns>
    private bool ReportsEventType(ushort type)
    {
        _transport.WriteDeviceConfig8(SelectOffset, ConfigEventBits);
        _transport.WriteDeviceConfig8(SubselectOffset, (byte)type);
        return _transport.ReadDeviceConfig8(SizeOffset) > 0;
    }

    /// <summary>
    /// Whether the kit routed the function's interrupts through MSI-X, read
    /// from the capability it programmed, rather than polled from the timer.
    /// </summary>
    private static bool IsMsiXEnabled(PciFunction function) =>
        function.TryFindCapability(MsiXCapabilityId, out ushort capability)
        && (function.ReadConfig16((ushort)(capability + MsiXMessageControlOffset)) & MsiXEnableBit) != 0;

    /// <summary>The buffer descriptor <paramref name="index"/> owns, in the driver's own memory.</summary>
    private static Span<byte> BufferOf(DmaBuffer buffers, int index) =>
        buffers.Span.Slice(index * InputEvent.Size, InputEvent.Size);

    /// <summary>The same buffer, as the device addresses it.</summary>
    private static ulong BufferAddressOf(DmaBuffer buffers, int index) =>
        buffers.DeviceAddress + (ulong)(index * InputEvent.Size);
}
