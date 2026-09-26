// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// The buttons held down in a <see cref="MouseReporter.Report"/>. The values
/// are the bits of a HID boot mouse report's first byte, so a USB driver can
/// cast that byte's low three bits straight to this type.
/// </summary>
[Flags]
internal enum MouseButtons
{
    /// <summary>No button is held.</summary>
    None = 0,

    /// <summary>The left, primary button.</summary>
    Left = 1,

    /// <summary>The right, secondary button.</summary>
    Right = 2,

    /// <summary>The middle button, often the wheel pressed down.</summary>
    Middle = 4
}
