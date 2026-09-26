// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver matching QEMU's HID devices by vendor and product ID, the most
/// specific match there is, so it is offered each of their interfaces
/// first. It tries to register another driver from its factory and from its
/// Probe, asks the context for endpoints it must refuse, and declines,
/// having opened nothing: each interface then goes on to the next
/// candidate.
/// </summary>
internal sealed class UsbHidDeviceDriver : UsbDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "usb-qemu-hid";

    /// <summary>Name of the registration it tries to add from inside the kit.</summary>
    private const string NestedName = "usb-nested";

    /// <summary>Number of Probes that ran, one per interface it was offered.</summary>
    public static int Probes { get; private set; }

    /// <summary>
    /// The message of the InvalidOperationException Register threw from
    /// inside the factory, or null when it threw none or the factory never ran.
    /// </summary>
    public static string? FactoryRegisterMessage { get; private set; }

    /// <summary>
    /// The message of the InvalidOperationException Register threw from
    /// inside Probe, or null when it threw none or Probe never ran.
    /// </summary>
    public static string? ProbeRegisterMessage { get; private set; }

    // Recorded the wrong way round, so they start false: a static
    // initializer would make a class constructor run inside the pass.

    /// <summary>
    /// True once a Probe got true from TryOpenBulk for its interface's
    /// interrupt IN endpoint, which is not a bulk endpoint.
    /// </summary>
    public static bool BulkOpenOfInterruptEndpointAccepted { get; private set; }

    /// <summary>
    /// True once a Probe got true from OpenInterruptIn for an endpoint its
    /// interface does not have: a default endpoint description, address 0.
    /// </summary>
    public static bool InterruptOpenOfMissingEndpointAccepted { get; private set; }

    /// <summary>True when some Probe found no interrupt IN endpoint to try TryOpenBulk on.</summary>
    public static bool InterruptEndpointMissing { get; private set; }

    /// <summary>The registration's factory: tries to register, then creates the driver.</summary>
    public static UsbDriver Create()
    {
        FactoryRegisterMessage = TryRegister();
        return new UsbHidDeviceDriver();
    }

    /// <inheritdoc />
    protected internal override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(Name, context.Path);
        Probes++;
        ProbeRegisterMessage = TryRegister();

        if (context.Interface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
        {
            if (context.TryOpenBulk(endpoint, out _))
            {
                BulkOpenOfInterruptEndpointAccepted = true;
            }
        }
        else
        {
            InterruptEndpointMissing = true;
        }

        if (context.OpenInterruptIn(default, static _ => { }))
        {
            InterruptOpenOfMissingEndpointAccepted = true;
        }

        return ProbeResult.Declined;
    }

    /// <summary>
    /// Registers a well-formed USB driver under a free name, so Register can
    /// refuse only because a driver callback is running or because the pass
    /// closed registration; the message says which.
    /// </summary>
    /// <returns>The message of the InvalidOperationException Register threw, or null when it threw none.</returns>
    private static string? TryRegister()
    {
        try
        {
            DriverCore.Register(new UsbDriverRegistration(NestedName, static () => new UsbRecordingDriver(NestedName, ProbeResult.Declined),
                UsbMatch.Device(UsbDescriptors.QemuHidVendorId, UsbDescriptors.QemuHidProductId)));
            return null;
        }
        catch (InvalidOperationException exception)
        {
            return exception.Message;
        }
    }
}
