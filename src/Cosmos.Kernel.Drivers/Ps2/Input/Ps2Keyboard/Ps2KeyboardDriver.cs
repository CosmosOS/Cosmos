// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Ps2;

namespace Cosmos.Kernel.Drivers.Ps2.Input.Ps2Keyboard;

/// <summary>
/// The PS/2 keyboard over the driver kit: binds the 8042 driver's keyboard
/// port node, resets and identifies the device through the port's
/// <see cref="Ps2Access"/>, publishes a keyboard named <c>ps2-keyboard</c>,
/// then enables scanning; its handler drains the port's ring and reports
/// set 1 scan codes to the keyboard sink, folding the 0xE0 prefix and the
/// release bit as the old handler did, and <see cref="Ps2KeyboardState.SetLeds"/>
/// lights the indicators with 0xED, which the old driver never did. State on
/// a <see cref="Ps2KeyboardState"/> in <see cref="DeviceBinding.DriverState"/>.
/// <see cref="Probe"/> and <see cref="OnDetach"/> run in thread context on
/// the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Keyboard)]
public sealed class Ps2KeyboardDriver : Driver
{
    // --- Constants ---

    /// <summary>Device command: reset and self test; the completion byte follows the acknowledgement.</summary>
    private const byte Reset = 0xFF;

    /// <summary>Device command: identify; an MF2 keyboard answers two bytes, an AT keyboard none.</summary>
    private const byte Identify = 0xF2;

    /// <summary>Device command: set the indicators; the byte follows.</summary>
    private const byte SetIndicators = 0xED;

    /// <summary>Device command: enable scanning.</summary>
    private const byte EnableScanning = 0xF4;

    /// <summary>Device command: disable scanning.</summary>
    private const byte DisableScanning = 0xF5;

    /// <summary>The self test's completion byte when it passed; 0xFC and 0xFD are its failure codes.</summary>
    private const byte SelfTestPassed = 0xAA;

    /// <summary>The first identify byte of an MF2 keyboard.</summary>
    private const byte IdentifyMf2 = 0xAB;

    /// <summary>The second identify byte of an MF2 keyboard behind a translating controller.</summary>
    private const byte Mf2Translated = 0x41;

    /// <summary>The second identify byte some MF2 keyboards answer behind a translating controller.</summary>
    private const byte Mf2TranslatedAlternate = 0xC1;

    /// <summary>The second identify byte of an MF2 keyboard with translation off.</summary>
    private const byte Mf2Untranslated = 0x83;

    /// <summary>The bound on the reset: a keyboard's self test takes up to 750 ms on real hardware.</summary>
    private const uint ResetTimeoutMilliseconds = 1000;

    /// <summary>The bound on every other exchange.</summary>
    private const uint CommandTimeoutMilliseconds = 100;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        Ps2Match.Port(Ps2Port.Keyboard),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(Ps2KeyboardDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Resets and identifies the keyboard, publishes it and enables
    /// scanning. Thread context on the kit worker; a declined or failed
    /// result makes the kit release everything acquired here, the published
    /// keyboard included.
    /// </summary>
    /// <param name="binding">The keyboard port's node and the kit facilities for it.</param>
    /// <returns>Bound with the keyboard published and scanning; declined when no byte reaches the port unattended, when nothing answers the reset or when the device is not a keyboard; failed when the keyboard failed its self test or refused a command it must accept.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The access.
        Ps2Access ps2 = binding.Node.Access<Ps2Access>();

        // 2. A keyboard bound on a controller that neither interrupts nor
        //    drains would never deliver a key after this probe.
        if (!ps2.DeliversUnattended)
        {
            return ProbeResult.Declined("no interrupt and no timer to poll with");
        }

        // 3. The reset: the access hands a failure code through as the
        //    completion byte.
        Span<byte> reply = stackalloc byte[2];
        if (!ps2.TryCommand(Reset, [], reply[..1], out int count, ResetTimeoutMilliseconds))
        {
            return ProbeResult.Declined("no keyboard on the port");
        }

        if (count != 1 || reply[0] != SelfTestPassed)
        {
            return ProbeResult.Failed("the keyboard did not pass its self test");
        }

        // 3b. A reset leaves scanning enabled, and a key held during boot
        //     would otherwise put a scan code in the identify reply.
        ps2.TryCommand(DisableScanning, [], [], out _, CommandTimeoutMilliseconds);

        // 4. Identify: nothing from an AT keyboard, ab 41, ab c1 or ab 83
        //    from an MF2 one; a mouse on the port answers its own id and is
        //    left unbound.
        if (!ps2.TryCommand(Identify, [], reply, out count, CommandTimeoutMilliseconds))
        {
            return ProbeResult.Failed("identify was not acknowledged");
        }

        bool isAt = count == 0;
        if (!isAt)
        {
            bool isMf2 = count == 2 && reply[0] == IdentifyMf2
                && (reply[1] == Mf2Translated || reply[1] == Mf2TranslatedAlternate || reply[1] == Mf2Untranslated);
            if (!isMf2)
            {
                return ProbeResult.Declined("not a keyboard");
            }
        }

        // 5. Indicators off; a keyboard that refuses it still types.
        ps2.TryCommand(SetIndicators, [0x00], [], out _, CommandTimeoutMilliseconds);

        // 6. The state.
        Ps2KeyboardState state = new(ps2, isAt, isAt ? (byte)0 : reply[1]);
        binding.DriverState = state;

        // 6b. The ring holds what the exchanges above did not consume (a
        //     held key's scan codes before the scanning was disabled, a
        //     mouse's id byte after its 0xAA): emptied here, so the handler
        //     never reports a byte from before it connected.
        while (ps2.TryReceive(out _))
        {
        }

        // 7. The port's source; the kit's refuses only when already connected.
        if (!binding.TryRequestInterrupt(binding.Node.Interrupts[0], state.OnInterrupt, out _))
        {
            return ProbeResult.Failed("the port's interrupt source could not be connected");
        }

        // 8. The ring, before scanning is enabled, so a scan code never finds
        //    a null sink.
        state.Sink = binding.PublishKeyboard(state);

        // 9. Scanning; a failure here unwinds the published keyboard.
        if (!ps2.TryCommand(EnableScanning, [], [], out _, CommandTimeoutMilliseconds))
        {
            return ProbeResult.Failed("enable scanning was not acknowledged");
        }

        // 10.
        binding.Log(isAt ? "AT keyboard (no identify reply), scanning" : $"MF2 keyboard (id ab {reply[1]:x2}), scanning");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Disables scanning when the hardware is still there. Thread context
    /// on the kit worker; the handle is already disconnected, so the
    /// acknowledgement reaches the exchange through the access and nothing
    /// is raised.
    /// </summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (reason.HardwarePresent)
        {
            Ps2Access ps2 = binding.Node.Access<Ps2Access>();
            ps2.TryCommand(DisableScanning, [], [], out _, CommandTimeoutMilliseconds);
        }
    }
}
