// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// Diagnostic IDs of the experimental API seams exposed by
/// Cosmos.Kernel.HAL. An experimental API is usable today but carries no
/// compatibility promise; referencing one produces an error with the ID
/// below until the caller suppresses it, which is the caller's
/// acknowledgement of that contract.
/// </summary>
internal static class Experimentals
{
    /// <summary>
    /// The driver kit seam, which lets a kernel register its own PCI and USB
    /// class drivers. In Cosmos.Kernel.HAL.Drivers: <see cref="ProbeResult"/>,
    /// <see cref="DeviceContext"/>, <see cref="MmioRegion"/>,
    /// <see cref="PortRegion"/>, <see cref="DmaBuffer"/>,
    /// <see cref="DeviceInterruptHandler"/>, <see cref="DeviceWorkItem"/>,
    /// <see cref="DeviceEvent"/>, <see cref="IrqSafeLock"/>,
    /// <see cref="MouseReporter"/>, <see cref="MouseButtons"/>,
    /// <see cref="NetworkLink"/> and <see cref="NetworkTransmitHandler"/>. In
    /// Cosmos.Kernel.HAL.Drivers.Pci: <see cref="Pci.PciDriver"/>,
    /// <see cref="Pci.PciDriverRegistration"/>, <see cref="Pci.PciMatch"/>,
    /// <see cref="Pci.PciDeviceContext"/> and <see cref="Pci.PciFunction"/>.
    /// In Cosmos.Kernel.HAL.Drivers.Usb: <see cref="Usb.UsbDriver"/>,
    /// <see cref="Usb.UsbDriverRegistration"/>, <see cref="Usb.UsbMatch"/>,
    /// <see cref="Usb.UsbDeviceContext"/>, <see cref="Usb.UsbDeviceInfo"/>,
    /// <see cref="Usb.UsbInterfaceInfo"/>, <see cref="Usb.UsbEndpointInfo"/>,
    /// <see cref="Usb.UsbBulkPipe"/>, <see cref="Usb.UsbTransferResult"/>,
    /// <see cref="Usb.UsbReportHandler"/>, and the
    /// <see cref="Usb.UsbEndpointType"/>, <see cref="Usb.UsbTransferStatus"/>,
    /// <see cref="Usb.UsbDirection"/>, <see cref="Usb.UsbRequestKind"/> and
    /// <see cref="Usb.UsbRecipient"/> enums. Cosmos.Kernel.System adds the
    /// registration side under the same ID: its DriverManager and DeviceInfo,
    /// and the Kernel.RegisterDrivers hook. The engine behind the seam,
    /// Cosmos.Kernel.HAL.Drivers.Engine, stays internal.
    /// </summary>
    internal const string DriverKitDiagId = "COSMOS0003";
}
