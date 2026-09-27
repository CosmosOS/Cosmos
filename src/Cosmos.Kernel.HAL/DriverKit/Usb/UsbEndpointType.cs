// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>Transfer type of an endpoint, from bmAttributes bits 1:0 (USB 2.0 §9.6.6).</summary>
[Experimental(Experimentals.DriverKitDiagId)]
public enum UsbEndpointType : byte
{
    /// <summary>A control endpoint. The default control endpoint is reached through the context's ControlIn and ControlOut.</summary>
    Control = 0,

    /// <summary>An isochronous endpoint. The kit opens none in this version.</summary>
    Isochronous = 1,

    /// <summary>A bulk endpoint, opened through <see cref="UsbDeviceContext.TryOpenBulk"/>.</summary>
    Bulk = 2,

    /// <summary>An interrupt endpoint; an IN one is opened through <see cref="UsbDeviceContext.OpenInterruptIn"/>.</summary>
    Interrupt = 3
}
