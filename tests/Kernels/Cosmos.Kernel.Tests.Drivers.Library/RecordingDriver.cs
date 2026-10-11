// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Base of every driver in the suite: counts the probes it was offered and
/// keeps the last detach reason, so a test asserts what the kit did to the
/// driver without reading the serial log. The manifest constructs one
/// instance per class, so the counters are per driver, not per device; a
/// driver that binds several nodes keeps its per-node state on
/// <see cref="DeviceBinding.DriverState"/>.
/// </summary>
public abstract class RecordingDriver : Driver
{
    private volatile int _probeCount;
    private volatile int _detachCount;
    private DetachReason _lastDetachReason;

    /// <summary>How many times the kit offered this driver a node.</summary>
    public int ProbeCount => _probeCount;

    /// <summary>How many bound devices were taken away from this driver.</summary>
    public int DetachCount => _detachCount;

    /// <summary>The reason handed to the most recent <see cref="OnDetach"/>; meaningful once <see cref="DetachCount"/> is positive.</summary>
    public DetachReason LastDetachReason => _lastDetachReason;

    /// <summary>Counts the offer, then lets the derived driver decide.</summary>
    /// <param name="binding">The device and the kit facilities for it.</param>
    public sealed override ProbeResult Probe(DeviceBinding binding)
    {
        _probeCount++;
        return ProbeCore(binding);
    }

    /// <summary>Records the reason, then lets the derived driver quiesce.</summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public sealed override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        _lastDetachReason = reason;
        _detachCount++;
        OnDetachCore(binding, reason);
    }

    /// <summary>The manifest's instance of <typeparamref name="TDriver"/>, or null when the manifest left it out.</summary>
    /// <typeparam name="TDriver">The driver class.</typeparam>
    public static TDriver? Find<TDriver>() where TDriver : Driver
    {
        IReadOnlyList<Driver> drivers = DriverRegistry.Drivers;
        for (int i = 0; i < drivers.Count; i++)
        {
            if (drivers[i] is TDriver driver)
            {
                return driver;
            }
        }

        return null;
    }

    /// <summary>The derived driver's probe.</summary>
    /// <param name="binding">The device and the kit facilities for it.</param>
    protected abstract ProbeResult ProbeCore(DeviceBinding binding);

    /// <summary>The derived driver's detach hook; nothing by default.</summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    protected virtual void OnDetachCore(DeviceBinding binding, DetachReason reason)
    {
    }
}
