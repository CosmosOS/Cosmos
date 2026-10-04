// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Descriptor types (USB 2.0 section 9.4 table 9-5, hub class section
/// 11.23.2.1, USB 3.2 table 9-6 and section 10.15.2.1).
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum UsbDescriptorType : byte
{
    /// <summary>The device descriptor.</summary>
    Device = 0x01,

    /// <summary>A configuration descriptor, with the interface and endpoint descriptors that follow it.</summary>
    Configuration = 0x02,

    /// <summary>An interface descriptor.</summary>
    Interface = 0x04,

    /// <summary>An endpoint descriptor.</summary>
    Endpoint = 0x05,

    /// <summary>The hub descriptor of a USB 2.0 hub.</summary>
    Hub = 0x29,

    /// <summary>The hub descriptor of a SuperSpeed hub.</summary>
    SuperSpeedHub = 0x2A,

    /// <summary>The companion descriptor that follows a SuperSpeed endpoint descriptor.</summary>
    SuperSpeedEndpointCompanion = 0x30
}
