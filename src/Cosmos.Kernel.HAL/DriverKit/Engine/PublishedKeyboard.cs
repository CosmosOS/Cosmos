// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Devices.Input;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// A keyboard a registered driver published, as the keyboard manager sees
/// it: a <see cref="KeyboardDevice"/> like the built-in ones, so the manager
/// wires its <see cref="KeyboardDevice.OnKeyPressed"/> the same way. The
/// driver reports through the <see cref="KeyboardReporter"/> in front of it;
/// nothing is polled. A USB driver's keyboard is withdrawn when its device
/// leaves the bus, and stays silent from then on.
/// </summary>
internal sealed class PublishedKeyboard : KeyboardDevice
{
    // The set 1 make codes of the three lock keys, which the lamps follow.
    private const byte CapsLockScanCode = 0x3A;
    private const byte NumLockScanCode = 0x45;
    private const byte ScrollLockScanCode = 0x46;

    /// <summary>What the driver asked to be told the lamps through, null for a keyboard with none.</summary>
    private readonly KeyboardLedHandler? _updateLeds;

    /// <summary>
    /// Set by the keyboard manager once it has wired
    /// <see cref="KeyboardDevice.OnKeyPressed"/>. A keyboard published by an
    /// attempt that was declined or failed never reaches the manager, so it
    /// is never enabled and its reports go nowhere.
    /// </summary>
    private volatile bool _enabled;

    /// <summary>
    /// Set when the kit withdrew the keyboard, as its USB device left the
    /// bus: every report is dropped from then on, and nothing enables it
    /// again.
    /// </summary>
    private volatile bool _withdrawn;

    /// <summary>
    /// The lamps, tracked here from the lock keys the driver reports, the
    /// way a PS/2 keyboard's driver tracks them: the manager asks for them
    /// to be set without saying what they are, since each keyboard carries
    /// its own.
    /// </summary>
    private KeyboardLeds _leds;

    /// <summary>True once the kit withdrew the keyboard; its reports go nowhere.</summary>
    internal bool IsWithdrawn => _withdrawn;

    /// <summary>Always false: the driver pushes each report, there is nothing to read.</summary>
    public override bool KeyAvailable => false;

    /// <summary>Puts a keyboard in front of <paramref name="updateLeds"/>, which is null for a driver that shows no lamps.</summary>
    internal PublishedKeyboard(KeyboardLedHandler? updateLeds)
    {
        _updateLeds = updateLeds;
    }

    /// <summary>Puts the lamps back out: the driver's device comes up with none lit.</summary>
    public override void Initialize() => _leds = KeyboardLeds.None;

    /// <summary>
    /// Lets reports through to <see cref="KeyboardDevice.OnKeyPressed"/>.
    /// The keyboard manager calls it once it is listening. A withdrawn
    /// keyboard stays silent.
    /// </summary>
    public override void Enable() => _enabled = !_withdrawn;

    /// <summary>Drops every report from now on, until <see cref="Enable"/>.</summary>
    public override void Disable() => _enabled = false;

    /// <summary>
    /// Shows the lamps the lock keys have left on. The manager calls it from
    /// inside the report that carried the lock key, so this runs in the
    /// driver's reporting context; a driver that has a transfer to make
    /// schedules it. A withdrawn keyboard shows nothing: its device is gone.
    /// </summary>
    public override void UpdateLeds()
    {
        if (_withdrawn)
        {
            return;
        }

        _updateLeds?.Invoke(_leds);
    }

    /// <summary>
    /// Silences the keyboard for good: its USB device left the bus. Called
    /// by the kit before it asks the keyboard manager to let go of it, so a
    /// report the driver still makes goes nowhere even meanwhile.
    /// </summary>
    internal void Withdraw()
    {
        _withdrawn = true;
        _enabled = false;
    }

    /// <summary>
    /// Hands one key change to the keyboard manager. IRQ-safe: interrupts
    /// are masked while it runs, so a report from a thread and one from an
    /// interrupt handler, this keyboard's or a built-in's, never interleave
    /// in the manager's key queue; and it allocates nothing, and calls
    /// through a delegate, not an interface. The lamps are tracked before
    /// the report goes on, because the manager toggles its lock state and
    /// asks for <see cref="UpdateLeds"/> inside that very call.
    /// </summary>
    internal void Report(byte scanCode, bool released)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!_enabled || _withdrawn)
            {
                return;
            }

            if (!released)
            {
                TrackLockKey(scanCode);
            }

            OnKeyPressed?.Invoke(scanCode, released);
        }
    }

    /// <summary>Mirrors the lock toggle the manager makes on the same key press.</summary>
    private void TrackLockKey(byte scanCode)
    {
        switch (scanCode)
        {
            case CapsLockScanCode:
                _leds ^= KeyboardLeds.CapsLock;
                break;
            case NumLockScanCode:
                _leds ^= KeyboardLeds.NumLock;
                break;
            case ScrollLockScanCode:
                _leds ^= KeyboardLeds.ScrollLock;
                break;
        }
    }
}
