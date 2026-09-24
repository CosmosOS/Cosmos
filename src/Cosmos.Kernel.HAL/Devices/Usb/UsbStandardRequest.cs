// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Standard request codes (USB 2.0 §9.4 table 9-4). Hub-class requests
/// reuse the same codes with a class request type (§11.24.2); requests
/// only one class defines live with that class driver.
/// </summary>
internal enum UsbStandardRequest : byte
{
    GetStatus = 0x00,
    ClearFeature = 0x01,
    SetFeature = 0x03,
    GetDescriptor = 0x06,
    SetConfiguration = 0x09
}
