// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Standard request codes (USB 2.0 section 9.4 table 9-4). Hub-class
/// requests reuse the same codes with a class request type (section
/// 11.24.2); requests only one class defines live with that class driver.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum UsbStandardRequest : byte
{
    /// <summary>GET_STATUS.</summary>
    GetStatus = 0x00,

    /// <summary>CLEAR_FEATURE.</summary>
    ClearFeature = 0x01,

    /// <summary>SET_FEATURE.</summary>
    SetFeature = 0x03,

    /// <summary>GET_DESCRIPTOR.</summary>
    GetDescriptor = 0x06,

    /// <summary>SET_CONFIGURATION.</summary>
    SetConfiguration = 0x09
}
