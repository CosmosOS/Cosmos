// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers.Usb.Storage.UsbMassStorage;

/// <summary>
/// Everything <see cref="UsbMassStorageDriver"/> holds for one bound mass
/// storage interface, hung off <see cref="DeviceBinding.DriverState"/>: the
/// transport the units share, the units the probe published, the highest
/// LUN the device reported and whether the interface was detached. The unit
/// list changes on the kit worker only (the probe adds, nothing removes);
/// the detached flag is written by the detach hook and read by the units
/// from any thread.
/// </summary>
public sealed class UsbMassStorageState
{
    // --- Private fields ---

    private readonly UsbBulkOnlyTransport _transport;
    private readonly List<UsbMassStorageUnit> _units = new();
    private volatile bool _detached;
    private byte _maxLun;

    // --- Constructor ---

    /// <summary>Takes the transport the probe opened. Thread context, from the probe.</summary>
    /// <param name="transport">The interface's Bulk-Only Transport.</param>
    internal UsbMassStorageState(UsbBulkOnlyTransport transport)
    {
        _transport = transport;
    }

    // --- Properties the suites read ---

    /// <summary>How many logical units the probe published as block devices. Any context after the probe.</summary>
    public int UnitCount => _units.Count;

    /// <summary>The highest LUN number the device reported to GET MAX LUN (0 when it stalled the request). Any context after the probe.</summary>
    public byte MaxLun
    {
        get => _maxLun;
        internal set => _maxLun = value;
    }

    /// <summary>The units the probe published, in LUN order. Any context after the probe.</summary>
    public IReadOnlyList<UsbMassStorageUnit> Units => _units;

    /// <summary>True once the interface's binding was detached: every unit's I/O fails from then on. Any context.</summary>
    public bool Detached
    {
        get => _detached;
        internal set => _detached = value;
    }

    // --- Internal members ---

    /// <summary>The Bulk-Only Transport the units share.</summary>
    internal UsbBulkOnlyTransport Transport => _transport;

    /// <summary>Records a unit the ring took. Thread context, the probe.</summary>
    /// <param name="unit">The published unit.</param>
    internal void Add(UsbMassStorageUnit unit) => _units.Add(unit);
}
