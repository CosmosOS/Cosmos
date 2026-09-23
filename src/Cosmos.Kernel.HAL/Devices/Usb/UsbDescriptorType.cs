// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Descriptor types (USB 2.0 §9.4 table 9-5, hub class §11.23.2.1,
/// USB 3.2 §10.15.2.1).
/// </summary>
internal enum UsbDescriptorType : byte
{
    Device = 0x01,
    Configuration = 0x02,
    Interface = 0x04,
    Endpoint = 0x05,
    Hub = 0x29,
    SuperSpeedHub = 0x2A
}
