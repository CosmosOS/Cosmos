// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// What a control request is addressed to: the Recipient field of its
/// bmRequestType, bits 4:0 (USB 2.0 §9.3.1). For an interface or an
/// endpoint, the request's index names which one.
/// </summary>
internal enum UsbRecipient
{
    /// <summary>The device as a whole.</summary>
    Device,

    /// <summary>The interface whose number is in the request's index.</summary>
    Interface,

    /// <summary>The endpoint whose address is in the request's index.</summary>
    Endpoint,

    /// <summary>Something else the class defines, such as a hub's port.</summary>
    Other
}
