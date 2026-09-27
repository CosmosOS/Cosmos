// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.BulkOnly;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage;

/// <summary>
/// The built-in USB mass storage driver: binds every interface that carries
/// SCSI commands over the Bulk-Only Transport, which USB sticks, card
/// readers and USB disks all do, and publishes each logical unit with a
/// medium to the storage manager, named <c>usb0</c>, <c>usb1</c>, ... by
/// the lowest number no unit present uses, so a stick plugged back in gets
/// its name back. The kit takes a unit's disk back out of the storage
/// manager when its device leaves the bus.
/// </summary>
/// <remarks>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreateUsbMassStorageRegistrations"/>, in a kernel
/// built with storage and USB support. Its name, <c>mass storage</c>, is
/// the owner the interfaces it binds record, and a name the kit refuses to
/// a kernel's own registration.
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class MassStorageDriver : UsbDriver
{
    private const string DriverName = "mass storage";

    // A mass storage interface (class 08h) of the SCSI transparent command
    // set (subclass 06h) over the Bulk-Only Transport (protocol 50h), as the
    // USB Mass Storage Class overview names them: the UFI, CBI and UAS
    // interfaces speak other protocols.
    private const byte MassStorageClass = 0x08;
    private const byte ScsiTransparentSubclass = 0x06;
    private const byte BulkOnlyProtocol = 0x50;

    // Every unit present, across every interface bound, which is what
    // names them. Probes and removals run one at a time: the driver pass
    // runs before the USB hot-plug thread starts, and that thread runs the
    // rest. Null until the first unit, rather than an initializer, which
    // would give this type a class constructor.
    private static List<LogicalUnit>? s_units;

    // The units this instance published, whose names its Remove gives back.
    private readonly List<LogicalUnit> _units = [];

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private MassStorageDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>mass storage</c> and one match, the mass storage class with the
    /// SCSI transparent subclass and the Bulk-Only protocol, so every such
    /// interface is offered to it and no other is. Any context: it only
    /// allocates the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per interface it binds through it.</returns>
    public static UsbDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new MassStorageDriver(), UsbMatch.Interface(MassStorageClass, ScsiTransparentSubclass, BulkOnlyProtocol));

    /// <inheritdoc />
    protected override ProbeResult Probe(UsbDeviceContext context)
    {
        UsbInterfaceInfo usbInterface = context.Interface;
        if (!usbInterface.TryFindEndpoint(UsbEndpointType.Bulk, UsbDirection.In, out UsbEndpointInfo bulkInEndpoint)
            || !usbInterface.TryFindEndpoint(UsbEndpointType.Bulk, UsbDirection.Out, out UsbEndpointInfo bulkOutEndpoint))
        {
            context.WriteLog("no bulk IN and bulk OUT endpoint pair");
            return ProbeResult.Declined;
        }

        if (!context.TryOpenBulk(bulkInEndpoint, out UsbBulkPipe? bulkIn)
            || !context.TryOpenBulk(bulkOutEndpoint, out UsbBulkPipe? bulkOut))
        {
            return ProbeResult.Failed;
        }

        BulkOnlyTransport transport = new(context, bulkIn, bulkOut, context.CreateEvent());
        byte maxLun = transport.GetMaxLun();
        try
        {
            for (byte lun = 0; lun <= maxLun; lun++)
            {
                LogicalUnit unit = new(context, transport, lun, FirstFreeIndex());
                if (!unit.Initialize())
                {
                    continue;
                }

                context.PublishBlockDevice(unit);
                _units.Add(unit);
                (s_units ??= []).Add(unit);
            }
        }
        catch (Exception)
        {
            // The kit calls Remove for a bound interface only, and a probe
            // that throws fails: the names its units took go back here.
            ReleaseNames();
            throw;
        }

        // The interface is this driver's from here on, even when no unit
        // has a medium (a card reader with its slots empty).
        return ProbeResult.Bound;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The kit already took the units' disks back out of the storage
    /// manager; what is left is their names, which a unit plugged in later
    /// can take.
    /// </remarks>
    protected override void Remove(UsbDeviceContext context) => ReleaseNames();

    /// <summary>Lowest name number no unit present uses, so two units never share a name.</summary>
    private static uint FirstFreeIndex()
    {
        for (uint index = 0; ; index++)
        {
            if (!IsIndexUsed(index))
            {
                return index;
            }
        }
    }

    private static bool IsIndexUsed(uint index)
    {
        if (s_units is not { } units)
        {
            return false;
        }

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].Index == index)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives back the names of the units this instance published.</summary>
    private void ReleaseNames()
    {
        if (s_units is { } units)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                units.Remove(_units[i]);
            }
        }

        _units.Clear();
    }
}
