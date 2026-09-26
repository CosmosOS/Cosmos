// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// USB class driver for mass storage: binds every interface that carries
/// SCSI commands over the Bulk-Only Transport, which USB sticks, card
/// readers and USB disks all do, and exposes each logical unit with a
/// medium as a <see cref="UsbMassStorage"/> for the storage manager to
/// register, the way <see cref="Ahci.Ports"/> and
/// <see cref="Nvme.Namespaces"/> are. The units found at boot are read
/// from <see cref="Disks"/>; the ones plugged in or pulled out afterwards
/// are reported through <see cref="DiskAttached"/> and
/// <see cref="DiskDetached"/>.
/// </summary>
internal sealed class UsbMassStorageDriver : UsbClassDriver
{
    /// <summary>bInterfaceSubClass: the SCSI transparent command set (USB MSC overview §2).</summary>
    private const byte ScsiTransparentSubclass = 0x06;

    /// <summary>bInterfaceProtocol: Bulk-Only Transport (USB MSC overview §3).</summary>
    private const byte BulkOnlyProtocol = 0x50;

    /// <summary>
    /// The units present. Replaced on every change, never changed in place,
    /// so a thread reading <see cref="Disks"/> while the hot-plug thread
    /// adds or removes one still sees a whole list. Null rather than
    /// <c>[]</c> until the first one binds: an initializer would give this
    /// type a class constructor, and <see cref="TryBind"/> first runs while
    /// devices come up, before the scheduler has a current thread for the
    /// class-constructor lock to use.
    /// </summary>
    private static UsbMassStorage[]? s_disks;

    /// <summary>
    /// The driver's <see cref="Name"/>, which the driver kit also refuses as
    /// a registration name: the device list names an interface's owner by it.
    /// </summary>
    internal const string DriverName = "mass storage";

    public override string Name => DriverName;

    /// <summary>Every logical unit present, in enumeration order (empty before USB enumeration).</summary>
    public static IReadOnlyList<UsbMassStorage> Disks => s_disks ?? [];

    /// <summary>
    /// Called with every unit that becomes usable, after it joined
    /// <see cref="Disks"/>: on the boot path before anyone listens, then on
    /// the hot-plug thread.
    /// </summary>
    public static Action<UsbMassStorage>? DiskAttached { get; set; }

    /// <summary>
    /// Called on the hot-plug thread with every unit whose device was
    /// unplugged, after it left <see cref="Disks"/>. Its I/O already fails.
    /// </summary>
    public static Action<UsbMassStorage>? DiskDetached { get; set; }

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
            UsbMassStorage disk = new(transport, lun, FirstFreeIndex());
            if (disk.Initialize())
            {
                s_disks = With(disk);
                DiskAttached?.Invoke(disk);
            }
        }

        return true;
    }

    public override void Disconnect(UsbDevice device, UsbInterface usbInterface)
    {
        foreach (UsbMassStorage disk in Disks)
        {
            if (disk.Transport.Device != device || disk.Transport.InterfaceNumber != usbInterface.Number)
            {
                continue;
            }

            s_disks = Without(disk);
            Serial.WriteString("[USB storage] ");
            Serial.WriteString(disk.Name);
            Serial.WriteString(" removed\n");
            DiskDetached?.Invoke(disk);
        }
    }

    /// <summary>
    /// Lowest name number no present unit uses, so a stick plugged back in
    /// gets its name back, and two units never share one.
    /// </summary>
    private static uint FirstFreeIndex()
    {
        IReadOnlyList<UsbMassStorage> disks = Disks;
        for (uint index = 0; ; index++)
        {
            bool used = false;
            for (int i = 0; i < disks.Count && !used; i++)
            {
                used = disks[i].Index == index;
            }

            if (!used)
            {
                return index;
            }
        }
    }

    private static UsbMassStorage[] With(UsbMassStorage disk)
    {
        IReadOnlyList<UsbMassStorage> disks = Disks;
        UsbMassStorage[] result = new UsbMassStorage[disks.Count + 1];
        for (int i = 0; i < disks.Count; i++)
        {
            result[i] = disks[i];
        }

        result[disks.Count] = disk;
        return result;
    }

    private static UsbMassStorage[] Without(UsbMassStorage disk)
    {
        IReadOnlyList<UsbMassStorage> disks = Disks;
        List<UsbMassStorage> result = new(disks.Count);
        foreach (UsbMassStorage other in disks)
        {
            if (other != disk)
            {
                result.Add(other);
            }
        }

        return result.ToArray();
    }
}
