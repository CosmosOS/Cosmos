// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.System;

namespace Cosmos.Kernel.Drivers.Usb.Storage.UsbMassStorage;

/// <summary>
/// The USB mass storage driver over the driver kit: binds every interface
/// that carries SCSI commands over the Bulk-Only Transport, which USB
/// sticks, card readers and USB disks all do, opens its two bulk pipes,
/// asks for the highest LUN and publishes every logical unit with a medium
/// to the ring as <c>usb{index}</c>, the lowest number no unit present
/// uses, so a stick plugged back in gets its name back and two units never
/// share one. Everything it holds for one interface lives on a
/// <see cref="UsbMassStorageState"/> in <see cref="DeviceBinding.DriverState"/>.
/// <see cref="Probe"/> and <see cref="OnDetach"/> run in thread context on
/// the kit worker, serialized, so the unit table needs no lock.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class UsbMassStorageDriver : Driver
{
    // --- Constants ---

    /// <summary>bInterfaceSubClass: the SCSI transparent command set (USB MSC overview section 2).</summary>
    private const byte ScsiTransparentSubclass = 0x06;

    /// <summary>bInterfaceProtocol: Bulk-Only Transport (USB MSC overview section 3).</summary>
    private const byte BulkOnlyProtocol = 0x50;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new UsbMatch(interfaceClass: UsbClassCode.MassStorage, interfaceSubclass: ScsiTransparentSubclass, interfaceProtocol: BulkOnlyProtocol),
    ];

    /// <summary>The live units of every binding; probes and teardowns are serialized on the kit worker, so no lock.</summary>
    private readonly List<UsbMassStorageUnit> _units = new();

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(UsbMassStorageDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Opens the transport and publishes the units. Thread context on the
    /// kit worker; a declined or failed result makes the kit release
    /// everything acquired here, the pipe that opened included. The unit
    /// scan runs here with up to 5 s of TEST UNIT READY polls per unit, so
    /// the driver stage lengthens by that.
    /// </summary>
    /// <param name="binding">The mass storage interface's node and the kit facilities for it.</param>
    /// <returns>Bound with the units published, even when none was ready (the interface is this driver's); declined when storage support is compiled out or an endpoint is missing; failed when a pipe could not be opened or the ring refused a unit.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The ring's storage manager has to exist for a unit to go
        //    anywhere.
        if (!KernelFeatures.Storage)
        {
            return ProbeResult.Declined("storage support is compiled out");
        }

        // 2. The access and the two bulk endpoints.
        UsbAccess usb = binding.Node.Access<UsbAccess>();
        UsbEndpoint? bulkIn = usb.FindEndpoint(UsbEndpointType.Bulk, isIn: true);
        if (bulkIn is null)
        {
            return ProbeResult.Declined("no bulk IN endpoint");
        }

        UsbEndpoint? bulkOut = usb.FindEndpoint(UsbEndpointType.Bulk, isIn: false);
        if (bulkOut is null)
        {
            return ProbeResult.Declined("no bulk OUT endpoint");
        }

        // 3. The pipes; the unwind closes the one that opened when the
        //    second does not.
        if (!usb.OpenBulkPipe(binding, bulkIn, out UsbPipe? inPipe) || !usb.OpenBulkPipe(binding, bulkOut, out UsbPipe? outPipe))
        {
            return ProbeResult.Failed("could not open the bulk pipes");
        }

        // 4. The transport and the highest LUN (a STALL means 0).
        UsbBulkOnlyTransport transport = new(binding, usb, inPipe, outPipe, binding.CreateLock());
        byte maxLun = transport.GetMaxLun();
        UsbMassStorageState state = new(transport);
        state.MaxLun = maxLun;

        // 5. The units: one block device per unit with a medium, published
        //    one at a time; the consumer registers and scans the disk inside
        //    the publish, and a refused registration throws out of the probe
        //    as Failed with the consumer's reason. The unit joins the table
        //    only once the ring took it, so a refused one never holds its
        //    name; an index is computed before INQUIRY and is never recorded
        //    when Initialize or the publish fails. A throw also frees the
        //    names of the units this probe recorded before it: the unwind
        //    runs no OnDetach, and the state is not on the binding yet.
        for (byte lun = 0; lun <= maxLun; lun++)
        {
            UsbMassStorageUnit unit = new(state, transport, lun, FirstFreeIndex());
            if (!unit.Initialize())
            {
                continue;
            }

            try
            {
                binding.PublishBlockDevice(unit);
            }
            catch
            {
                RemoveUnits(state);
                throw;
            }

            _units.Add(unit);
            state.Add(unit);
        }

        // 6. The state, then the log; bound even with no unit ready.
        binding.DriverState = state;
        binding.Log($"{state.UnitCount} unit(s), max LUN {maxLun}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Marks the interface detached, so every unit's I/O fails, and frees
    /// the units' names: a stick plugged back in probes after this teardown
    /// completed and gets its name back. Thread context on the kit worker;
    /// the pipes are closed by the ledger and the units withdrawn by the kit.
    /// </summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not UsbMassStorageState state)
        {
            return;
        }

        state.Detached = true;
        RemoveUnits(state);
    }

    // --- Private methods ---

    /// <summary>Takes a state's units out of the driver-wide table, freeing their names. Thread context on the kit worker.</summary>
    /// <param name="state">The interface's state.</param>
    private void RemoveUnits(UsbMassStorageState state)
    {
        IReadOnlyList<UsbMassStorageUnit> units = state.Units;
        for (int i = 0; i < units.Count; i++)
        {
            _units.Remove(units[i]);
        }
    }

    /// <summary>Lowest name number no live unit uses, so a stick plugged back in gets its name back, and two units never share one. Thread context on the kit worker.</summary>
    private uint FirstFreeIndex()
    {
        for (uint index = 0; ; index++)
        {
            bool used = false;
            for (int i = 0; i < _units.Count && !used; i++)
            {
                used = _units[i].Index == index;
            }

            if (!used)
            {
                return index;
            }
        }
    }
}
