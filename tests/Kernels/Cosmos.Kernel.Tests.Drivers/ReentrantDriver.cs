// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for the edu function that tries to register another driver
/// from its factory and from its Probe, records what each attempt threw,
/// and declines. Ranked above the driver that binds edu, so it also shows a
/// declined candidate passing the function on.
/// </summary>
internal sealed class ReentrantDriver : PciDriver
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

    /// <summary>The registration's factory: tries to register, then creates the driver.</summary>
    public static PciDriver Create()
    {
        FactoryRan = true;
        FactoryRegisterMessage = TryRegister();
        return new ReentrantDriver();
    }

    /// <inheritdoc />
    protected internal override ProbeResult Probe(PciDeviceContext context)
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
            DriverCore.Register(new PciDriverRegistration(NestedName, static () => new RecordingDriver(NestedName, ProbeResult.Declined),
                PciMatch.Device(EduDriver.VendorId, EduDriver.DeviceId)));
            return null;
        }
        catch (InvalidOperationException exception)
        {
            return exception.Message;
        }
    }
}
