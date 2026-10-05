// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Ps2;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The PS/2 mouse over the driver kit: binds the 8042 driver's auxiliary
/// port node, resets the device, sets its defaults, knocks the IntelliMouse
/// sequence for the wheel, publishes a pointer named <c>ps2-mouse</c> and
/// enables data reporting; its handler assembles three- or four-byte
/// packets from the port's ring and reports each as one relative movement.
/// State on a <see cref="Ps2MouseState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Mouse)]
public sealed class Ps2MouseDriver : Driver
{
    // --- Constants ---

    /// <summary>Device command: reset and self test; the completion byte and the id follow the acknowledgement.</summary>
    private const byte Reset = 0xFF;

    /// <summary>Device command: set the defaults (sample rate 100, resolution 4, scaling 1:1, reporting off).</summary>
    private const byte SetDefaults = 0xF6;

    /// <summary>Device command: set the sample rate; the rate follows.</summary>
    private const byte SetSampleRate = 0xF3;

    /// <summary>Device command: identify; the id byte follows the acknowledgement.</summary>
    private const byte Identify = 0xF2;

    /// <summary>Device command: enable data reporting.</summary>
    private const byte EnableReporting = 0xF4;

    /// <summary>Device command: disable data reporting.</summary>
    private const byte DisableReporting = 0xF5;

    /// <summary>The self test's completion byte when it passed; 0xFC and 0xFD are its failure codes.</summary>
    private const byte SelfTestPassed = 0xAA;

    /// <summary>The id of a standard three-byte mouse, which follows the completion byte.</summary>
    private const byte StandardMouseId = 0x00;

    /// <summary>The id of a wheel mouse after the knock.</summary>
    private const byte WheelMouseId = 0x03;

    /// <summary>The bound on the reset.</summary>
    private const uint ResetTimeoutMilliseconds = 1000;

    /// <summary>The bound on every other exchange.</summary>
    private const uint CommandTimeoutMilliseconds = 100;

    /// <summary>The IntelliMouse knock: three sample rates in this order, then an identify.</summary>
    private static ReadOnlySpan<byte> KnockRates => [200, 100, 80];

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        Ps2Match.Port(Ps2Port.Auxiliary),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(Ps2MouseDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Resets the mouse, knocks for the wheel, publishes it and enables
    /// reporting. Thread context on the kit worker; a declined or failed
    /// result makes the kit release everything acquired here, the published
    /// pointer included.
    /// </summary>
    /// <param name="binding">The auxiliary port's node and the kit facilities for it.</param>
    /// <returns>Bound with the pointer published and reporting; declined when no byte reaches the port unattended, when nothing answers the reset or when the device is not a mouse; failed when the mouse failed its self test or refused a command it must accept.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The access.
        Ps2Access ps2 = binding.Node.Access<Ps2Access>();

        // 2. A mouse bound on a controller that neither interrupts nor
        //    drains would never deliver a packet after this probe.
        if (!ps2.DeliversUnattended)
        {
            return ProbeResult.Declined("no interrupt and no timer to poll with");
        }

        // 3. The reset: a mouse follows its 0xAA with its id byte 0x00; a
        //    keyboard sends 0xAA alone and is left unbound.
        Span<byte> reply = stackalloc byte[2];
        if (!ps2.TryCommand(Reset, [], reply, out int count, ResetTimeoutMilliseconds))
        {
            return ProbeResult.Declined("no mouse on the port");
        }

        if (count == 0 || reply[0] != SelfTestPassed)
        {
            return ProbeResult.Failed("the mouse did not pass its self test");
        }

        if (count != 2 || reply[1] != StandardMouseId)
        {
            return ProbeResult.Declined("not a mouse");
        }

        // 4. The defaults.
        if (!ps2.TryCommand(SetDefaults, [], [], out _, CommandTimeoutMilliseconds))
        {
            return ProbeResult.Failed("set defaults was not acknowledged");
        }

        // 5. The knock; a knock that fails leaves a standard mouse, and the
        //    sample rate stays at the last value accepted.
        bool knocked = true;
        ReadOnlySpan<byte> rates = KnockRates;
        for (int i = 0; i < rates.Length; i++)
        {
            if (!ps2.TryCommand(SetSampleRate, [rates[i]], [], out _, CommandTimeoutMilliseconds))
            {
                knocked = false;
            }
        }

        bool identified = ps2.TryCommand(Identify, [], reply[..1], out count, CommandTimeoutMilliseconds) && count == 1;
        bool hasWheel = knocked && identified && reply[0] == WheelMouseId;

        // 5b. The knock and the id must agree before the packet length is
        //     fixed: an IntelliMouse switches to 4-byte packets when the
        //     third rate is accepted, before the identify is sent, so an
        //     identify that confirmed neither id (not answered, or answered
        //     something else), or a wheel id without every rate
        //     acknowledged, leaves the protocol unknown. A reset puts the
        //     device back to 3-byte packets (every mouse reverts its id on
        //     0xFF, where 0xF6 does not), then the defaults again.
        bool agreed = identified && (hasWheel || reply[0] == StandardMouseId);
        if (!agreed)
        {
            if (!ps2.TryCommand(Reset, [], reply, out count, ResetTimeoutMilliseconds)
                || count != 2 || reply[0] != SelfTestPassed || reply[1] != StandardMouseId)
            {
                return ProbeResult.Failed("the reset after an unconfirmed knock was not answered");
            }

            if (!ps2.TryCommand(SetDefaults, [], [], out _, CommandTimeoutMilliseconds))
            {
                return ProbeResult.Failed("set defaults was not acknowledged");
            }
        }

        // 6. The state.
        Ps2MouseState state = new(ps2, hasWheel);
        binding.DriverState = state;

        // 6b. The ring holds what the exchanges above did not consume (a
        //     stray byte would cost a resync drop, or misalign the first
        //     packet): emptied here, so the handler never collects a byte
        //     from before it connected.
        while (ps2.TryReceive(out _))
        {
        }

        // 7. The port's source; the kit's refuses only when already connected.
        if (!binding.TryRequestInterrupt(binding.Node.Interrupts[0], state.OnInterrupt, out _))
        {
            return ProbeResult.Failed("the port's interrupt source could not be connected");
        }

        // 8. The ring, before reporting is enabled, so a packet never finds
        //    a null sink.
        state.Sink = binding.PublishPointer(state);

        // 9. Reporting; a failure here unwinds the published pointer.
        if (!ps2.TryCommand(EnableReporting, [], [], out _, CommandTimeoutMilliseconds))
        {
            return ProbeResult.Failed("enable data reporting was not acknowledged");
        }

        // 10.
        binding.Log(hasWheel ? "wheel mouse (id 03), 4-byte packets, reporting" : "standard mouse (id 00), 3-byte packets, reporting");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Disables data reporting when the hardware is still there. Thread
    /// context on the kit worker; the handle is already disconnected, so the
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
            ps2.TryCommand(DisableReporting, [], [], out _, CommandTimeoutMilliseconds);
        }
    }
}
