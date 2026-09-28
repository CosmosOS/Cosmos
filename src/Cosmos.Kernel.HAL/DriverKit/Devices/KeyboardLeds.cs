// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>Keyboard indicator lights.</summary>
[Flags]
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum KeyboardLeds : byte
{
    /// <summary>No indicator.</summary>
    None = 0,

    /// <summary>Scroll Lock.</summary>
    ScrollLock = 1,

    /// <summary>Num Lock.</summary>
    NumLock = 2,

    /// <summary>Caps Lock.</summary>
    CapsLock = 4,
}
