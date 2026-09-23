// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// USB class driver for mass storage: binds every interface that carries
/// SCSI commands over the Bulk-Only Transport, which USB sticks, card
/// readers and USB disks all do, and exposes each logical unit with a
/// medium as a <see cref="UsbMassStorage"/> for the storage manager to
/// register, the way <see cref="Ahci.Ports"/> and
/// <see cref="Nvme.Namespaces"/> are.
/// </summary>
internal sealed class UsbMassStorageDriver : UsbDriver
{
    /// <summary>bInterfaceSubClass: the SCSI transparent command set (USB MSC overview §2).</summary>
    private const byte ScsiTransparentSubclass = 0x06;

    /// <summary>bInterfaceProtocol: Bulk-Only Transport (USB MSC overview §3).</summary>
    private const byte BulkOnlyProtocol = 0x50;

    private static List<UsbMassStorage>? s_disks;

    public override string Name => "mass storage";

    /// <summary>Every logical unit bound so far, in enumeration order (empty before USB enumeration).</summary>
    public static IReadOnlyList<UsbMassStorage> Disks =>
        (IReadOnlyList<UsbMassStorage>?)s_disks ?? Array.Empty<UsbMassStorage>();

    public override bool TryBind(UsbDevice device, UsbInterface usbInterface)
    {
        if (usbInterface.Class != UsbClassCode.MassStorage
            || usbInterface.Subclass != ScsiTransparentSubclass
            || usbInterface.Protocol != BulkOnlyProtocol)
        {
            return false;
        }

        UsbEndpoint? bulkIn = usbInterface.FindEndpoint(UsbEndpointType.Bulk, isIn: true);
        UsbEndpoint? bulkOut = usbInterface.FindEndpoint(UsbEndpointType.Bulk, isIn: false);
        if (bulkIn is null || bulkOut is null)
        {
            return false;
        }

        UsbBulkOnlyTransport transport = new(device, usbInterface.Number, bulkIn, bulkOut);
        if (!transport.Open())
        {
            return false;
        }

        // The interface is this driver's from here on, even when no unit
        // has a medium (a card reader with its slots empty).
        byte maxLun = transport.GetMaxLun();
        for (byte lun = 0; lun <= maxLun; lun++)
        {
            UsbMassStorage disk = new(transport, lun, (uint)Disks.Count);
            if (disk.Initialize())
            {
                (s_disks ??= []).Add(disk);
            }
        }

        return true;
    }
}
