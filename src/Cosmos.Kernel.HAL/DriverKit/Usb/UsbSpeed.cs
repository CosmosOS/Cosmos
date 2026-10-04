// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Bus speed of a USB device. The values are the xHCI default Protocol
/// Speed IDs (xHCI 1.2 section 7.2.2.1.1), which is what a root port
/// reports and what a Slot Context takes; another host controller
/// translates its own numbering to these.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum UsbSpeed : byte
{
    /// <summary>The speed is not known.</summary>
    Unknown = 0,

    /// <summary>Full-speed, 12 Mb/s.</summary>
    Full = 1,

    /// <summary>Low-speed, 1.5 Mb/s.</summary>
    Low = 2,

    /// <summary>High-speed, 480 Mb/s.</summary>
    High = 3,

    /// <summary>SuperSpeed, 5 Gb/s.</summary>
    Super = 4,

    /// <summary>SuperSpeedPlus, 10 Gb/s and above.</summary>
    SuperPlus = 5
}
