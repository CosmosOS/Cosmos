// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.System.Drivers;

namespace SampleDrivers;

/// <summary>
/// A driver for the edu function that tries to register another driver
/// from its factory and from its Probe, records what each attempt threw,
/// and declines. Ranked above the driver that binds edu, so it also shows a
/// declined candidate passing the function on.
/// </summary>
public sealed class ReentrantDriver : PciDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "edu-reentrant";

    /// <summary>Name of the registration it tries to add from inside the pass.</summary>
    private const string NestedName = "edu-nested";

    /// <summary>True once the factory ran.</summary>
    public static bool FactoryRan { get; private set; }

    /// <summary>
    /// The message of the InvalidOperationException Register threw from
    /// inside the factory, or null when it threw none.
    /// </summary>
    public static string? FactoryRegisterMessage { get; private set; }

    /// <summary>
    /// The message of the InvalidOperationException Register threw from
    /// inside Probe, or null when it threw none.
    /// </summary>
    public static string? ProbeRegisterMessage { get; private set; }

    /// <summary>
    /// The registration the kernel passes to DriverManager.Register: edu by
    /// device ID, with <see cref="Create"/> as its factory.
    /// </summary>
    /// <returns>A registration named <see cref="Name"/>.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(Name, Create, PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId));

    /// <summary>The registration's factory: tries to register, then creates the driver. Called by the kit during the pass.</summary>
    /// <returns>A new driver.</returns>
    public static PciDriver Create()
    {
        FactoryRan = true;
        FactoryRegisterMessage = TryRegister();
        return new ReentrantDriver();
    }

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        ProbeRegisterMessage = TryRegister();
        return ProbeResult.Declined;
    }

    /// <summary>
    /// Registers a well-formed driver under a free name, so Register can
    /// refuse only because a driver callback is running or because the pass
    /// closed registration; the message says which.
    /// </summary>
    /// <returns>The message of the InvalidOperationException Register threw, or null when it threw none.</returns>
    private static string? TryRegister()
    {
        try
        {
            DriverManager.Register(RecordingDriver.CreateRegistration(NestedName, ProbeResult.Declined,
                PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId)));
            return null;
        }
        catch (InvalidOperationException exception)
        {
            return exception.Message;
        }
    }
}
