// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>Pointer buttons, as a set.</summary>
[Flags]
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum PointerButtons : byte
{
    /// <summary>No button.</summary>
    None = 0,

    /// <summary>The left (primary) button.</summary>
    Left = 1,

    /// <summary>The right (secondary) button.</summary>
    Right = 2,

    /// <summary>The middle button.</summary>
    Middle = 4,
}
