// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A keyboard a driver published through
/// <see cref="DeviceContext.PublishKeyboard()"/>: what the driver reports
/// through it reaches the kernel's key queue exactly as a built-in
/// keyboard's keys do. Reports reach the keyboard manager once the driver's
/// Probe returned Bound; a report made before that, through the reporter of
/// an attempt that was declined or failed, or once the kit withdrew the
/// keyboard because its USB device left the bus, is dropped.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public sealed class KeyboardReporter
{
    /// <summary>
    /// The code to report for the right Alt key. On the wire it is the
    /// extended form of the left Alt (E0 38), and dropping the prefix as
    /// every other extended key does would make the two Alt keys one key; a
    /// layout maps this code to AltGr or to right Alt to say which it is.
    /// Set 1 assigns nothing to 0x60.
    /// </summary>
    public const byte RightAltScanCode = 0x60;

    private readonly PublishedKeyboard _keyboard;

    /// <summary>The adapter the kit delivers to the keyboard manager, and withdraws from it when a USB device leaves.</summary>
    internal PublishedKeyboard Device => _keyboard;

    /// <summary>Puts a reporter in front of <paramref name="keyboard"/>, the adapter the kit delivers to the keyboard manager.</summary>
    internal KeyboardReporter(PublishedKeyboard keyboard)
    {
        _keyboard = keyboard;
    }

    /// <summary>
    /// Reports one key going down or coming back up. Any context: it
    /// allocates nothing and masks interrupts only while it hands the report
    /// on, so the driver's interrupt handler may call it, and so may a
    /// thread. It throws nothing of its own.
    /// </summary>
    /// <param name="scanCode">
    /// The key's PS/2 set 1 make code, with the 0xE0 prefix of an extended
    /// key dropped, except for the right Alt key, which is
    /// <see cref="RightAltScanCode"/>. A driver whose device speaks another
    /// code set translates; 0 reports nothing and is the value to use for a
    /// key with no set 1 code.
    /// </param>
    /// <param name="released">True when the key came up, false when it went down.</param>
    public void Report(byte scanCode, bool released) => _keyboard.Report(scanCode, released);
}
