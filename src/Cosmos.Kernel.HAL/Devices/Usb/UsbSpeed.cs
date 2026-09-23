// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Bus speed of a USB device. The values are the xHCI default Protocol
/// Speed IDs (xHCI 1.2 §7.2.2.1.1), which is what a root port reports and
/// what a Slot Context takes; another host controller translates.
/// </summary>
internal enum UsbSpeed : byte
{
    Unknown = 0,
    Full = 1,
    Low = 2,
    High = 3,
    Super = 4,
    SuperPlus = 5
}
