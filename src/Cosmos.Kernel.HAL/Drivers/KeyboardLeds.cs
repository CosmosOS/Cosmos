// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// The lock lamps a keyboard shows, as handed to a
/// <see cref="KeyboardLedHandler"/>. The values are the bits of a HID boot
/// keyboard's output report (HID 1.11 appendix B.1), so a USB driver can
/// cast the set straight to the byte it sends.
/// </summary>
[Flags]
[Experimental(Experimentals.DriverKitDiagId)]
public enum KeyboardLeds
{
    /// <summary>No lamp is lit.</summary>
    None = 0,

    /// <summary>Num Lock is on.</summary>
    NumLock = 1,

    /// <summary>Caps Lock is on.</summary>
    CapsLock = 2,

    /// <summary>Scroll Lock is on.</summary>
    ScrollLock = 4
}
