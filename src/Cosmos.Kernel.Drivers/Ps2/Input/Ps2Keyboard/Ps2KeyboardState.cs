// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Ps2;

namespace Cosmos.Kernel.Drivers.Ps2.Input.Ps2Keyboard;

/// <summary>
/// Everything <see cref="Ps2KeyboardDriver"/> holds for one bound keyboard
/// port, hung off <see cref="DeviceBinding.DriverState"/>, and the keyboard
/// it publishes: the port's access, the sink, the 0xE0 prefix pending
/// between two bytes and the counters the Drivers suite reads.
/// <see cref="OnInterrupt"/> runs in interrupt context, raised by the 8042
/// driver's delivery, and is allocation-free; <see cref="SetLeds"/> is
/// thread context, the ring's indicator work item on the kit worker.
/// </summary>
public sealed class Ps2KeyboardState : IKeyboard
{
    // --- Constants ---

    /// <summary>The byte the keyboard sends before an extended key's own code.</summary>
    private const byte ExtendedPrefix = 0xE0;

    /// <summary>The make code of the left Alt, and of the right Alt after the prefix.</summary>
    private const byte AltScanCode = 0x38;

    /// <summary>
    /// The code the ring's layouts expect for the right Alt, the value
    /// <c>ScanMapBase.RightAltScanCode</c> keeps and the USB and virtio key
    /// maps carry as the same private constant; no seam member exists for
    /// it. On the wire it is the extended form of the left Alt (E0 38),
    /// which every keyboard driver folds to this value, one set 1 assigns to
    /// no key.
    /// </summary>
    private const byte RightAltScanCode = 0x60;

    /// <summary>Bit 7 of a set 1 code: the key was released.</summary>
    private const byte ReleaseBit = 0x80;

    /// <summary>Device command: set the indicators; the byte follows.</summary>
    private const byte SetIndicators = 0xED;

    /// <summary>The bound on the indicator exchange.</summary>
    private const uint CommandTimeoutMilliseconds = 100;

    // --- Private fields ---

    private readonly Ps2Access _access;
    private bool _extendedPending;
    private volatile int _bytesReceived;
    private volatile int _keyEvents;
    private volatile int _ledWrites;
    private volatile byte _lastLedByte;
    private volatile bool _lastLedAcknowledged;

    // --- Constructor ---

    /// <summary>Takes the access and what the probe learned from the identify reply. Thread context, from the probe.</summary>
    /// <param name="access">The keyboard port's access.</param>
    /// <param name="isAtKeyboard">True when the keyboard answered Identify with nothing.</param>
    /// <param name="identityByte">The second identify byte of an MF2 keyboard, 0 for an AT keyboard.</param>
    internal Ps2KeyboardState(Ps2Access access, bool isAtKeyboard, byte identityByte)
    {
        _access = access;
        IsAtKeyboard = isAtKeyboard;
        IdentityByte = identityByte;
    }

    // --- IKeyboard ---

    /// <inheritdoc/>
    public string Name => "ps2-keyboard";

    /// <summary>
    /// Lights the indicators: 0xED then the byte with scroll lock in bit 0,
    /// num lock in bit 1 and caps lock in bit 2, which is
    /// <see cref="KeyboardLeds"/>' own bit order. Thread context, the ring's
    /// indicator work item on the kit worker; the outcome is recorded in
    /// <see cref="LastLedAcknowledged"/>, not logged. The two
    /// acknowledgements are consumed by the exchange, never parsed as scan
    /// codes.
    /// </summary>
    /// <param name="leds">The indicators to light.</param>
    public void SetLeds(KeyboardLeds leds)
    {
        byte value = (byte)(leds & (KeyboardLeds.ScrollLock | KeyboardLeds.NumLock | KeyboardLeds.CapsLock));
        _lastLedByte = value;
        _lastLedAcknowledged = _access.TryCommand(SetIndicators, [value], [], out _, CommandTimeoutMilliseconds);
        _ledWrites++;
    }

    // --- Properties the suites read ---

    /// <summary>The sink scan codes go to, set by the probe before scanning is enabled. Any context.</summary>
    public KeyboardSink? Sink { get; internal set; }

    /// <summary>True when the keyboard answered Identify with nothing. Any context.</summary>
    public bool IsAtKeyboard { get; }

    /// <summary>The second identify byte of an MF2 keyboard (0x41, 0xC1 or 0x83), 0 for an AT keyboard. Any context.</summary>
    public byte IdentityByte { get; }

    /// <summary>How many bytes the handler took off the port's ring. Any context.</summary>
    public int BytesReceived => _bytesReceived;

    /// <summary>How many key presses and releases the handler reported to the sink. Any context.</summary>
    public int KeyEvents => _keyEvents;

    /// <summary>How many times <see cref="SetLeds"/> wrote the indicators. Any context.</summary>
    public int LedWrites => _ledWrites;

    /// <summary>The last indicator byte <see cref="SetLeds"/> sent. Any context.</summary>
    public byte LastLedByte => _lastLedByte;

    /// <summary>Whether the keyboard acknowledged the last indicator exchange. Any context.</summary>
    public bool LastLedAcknowledged => _lastLedAcknowledged;

    // --- The handler ---

    /// <summary>Drains the port's ring and decodes each byte. Interrupt context, raised by the 8042 driver's delivery; allocation-free.</summary>
    /// <param name="context">The kit's handler context, unused.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the InterruptHandler shape; the ring is drained and the sink called, nothing is scheduled.")]
    internal void OnInterrupt(InterruptContext context)
    {
        while (_access.TryReceive(out byte value))
        {
            _bytesReceived++;
            Decode(value);
        }
    }

    /// <summary>
    /// Decodes one set 1 byte: 0x00 and 0xFF are ignored, the 0xE0 prefix is
    /// remembered for the code that follows, the release bit is folded out,
    /// and the extended Alt becomes the right Alt's code; every other
    /// extended key keeps its bare code, as the layouts list the Windows
    /// keys and the navigation cluster that way. Handler context.
    /// </summary>
    /// <param name="value">The byte.</param>
    private void Decode(byte value)
    {
        if (value == 0x00 || value == 0xFF)
        {
            return;
        }

        if (value == ExtendedPrefix)
        {
            _extendedPending = true;
            return;
        }

        bool extended = _extendedPending;
        _extendedPending = false;
        bool released = (value & ReleaseBit) != 0;
        byte scanCode = (byte)(value & ~ReleaseBit);
        if (extended && scanCode == AltScanCode)
        {
            scanCode = RightAltScanCode;
        }

        _keyEvents++;
        KeyboardSink? sink = Sink;
        sink?.Report(scanCode, released);
    }
}
