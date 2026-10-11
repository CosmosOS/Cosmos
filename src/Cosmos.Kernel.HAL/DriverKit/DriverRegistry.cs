// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The drivers this kernel carries, in manifest order. The generated
/// manifest fills it before the kernel starts; the engine reads it for every
/// arbitration. Manifest position is the last arbitration key, so the order
/// here is the order drivers are offered a node when priority and
/// specificity tie.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public static class DriverRegistry
{
    private static Driver[] s_drivers = [];

    /// <summary>The registered drivers, in manifest order.</summary>
    public static IReadOnlyList<Driver> Drivers => s_drivers;

    /// <summary>
    /// Registers a driver. Called by the generated manifest, in its order,
    /// before <see cref="DriverEngine.Start"/>.
    /// </summary>
    /// <param name="driver">The driver instance the manifest constructed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="driver"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The engine has started; the manifest is fixed from then on.</exception>
    public static void Register(Driver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);

        if (DriverEngine.IsStarted)
        {
            throw new InvalidOperationException("Drivers are registered by the manifest before the kernel starts; the registry is fixed once the driver engine has started.");
        }

        Driver[] drivers = new Driver[s_drivers.Length + 1];
        Array.Copy(s_drivers, drivers, s_drivers.Length);
        drivers[s_drivers.Length] = driver;
        s_drivers = drivers;
    }
}
