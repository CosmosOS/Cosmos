// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// Who defines a control request: the Type field of its bmRequestType,
/// bits 6:5 (USB 2.0 §9.3.1).
/// </summary>
internal enum UsbRequestKind
{
    /// <summary>A request USB 2.0 chapter 9 defines, such as GET_DESCRIPTOR.</summary>
    Standard,

    /// <summary>A request the interface's class specification defines, such as HID's SET_PROTOCOL.</summary>
    Class,

    /// <summary>A request the device's vendor defines.</summary>
    Vendor
}
