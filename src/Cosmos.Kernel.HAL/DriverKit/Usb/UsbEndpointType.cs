// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>Transfer type of an endpoint, from bmAttributes bits 1:0.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum UsbEndpointType : byte
{
    /// <summary>A control endpoint.</summary>
    Control = 0,

    /// <summary>An isochronous endpoint.</summary>
    Isochronous = 1,

    /// <summary>A bulk endpoint.</summary>
    Bulk = 2,

    /// <summary>An interrupt endpoint.</summary>
    Interrupt = 3
}
