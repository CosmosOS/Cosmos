// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Net;

/// <summary>
/// The built-in virtio-net driver: brings a virtio network device on the PCI
/// bus up and publishes its link to the kernel's network stack, which sends
/// through it and receives the frames the driver delivers. One instance per
/// bound function, and the link of the first one to bind becomes the primary
/// adapter unless the platform already registered a network device.
/// </summary>
/// <remarks>
/// <para>
/// PCI only. The same devices appear on the QEMU virt machine's virtio-mmio
/// window, where there is no PCI function to bind and no BAR to map, and
/// those are still driven by <c>Cosmos.Kernel.HAL</c>'s own virtio stack; the
/// kit has no seam for that bus. A kernel on ARM64 therefore reaches a
/// virtio NIC through this driver when the board puts it on PCI, and through
/// HAL when it puts it on the MMIO window.
/// </para>
/// <para>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreatePciNetworkRegistrations"/>, in a kernel
/// built with network support. Its name, <c>virtio-net</c>, is the owner the
/// function records, and a name the kit refuses to a kernel's own
/// registration.
/// </para>
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class VirtioNetDriver : PciDriver
{
    private const string DriverName = "virtio-net";

    /// <summary>
    /// Device ID of a modern virtio network device: virtio numbers them from
    /// 0x1040 by device type, and the network device is type 1.
    /// </summary>
    private const ushort ModernNetworkDeviceId = 0x1041;

    /// <summary>
    /// Device ID of a transitional virtio network device, the one QEMU
    /// presents unless asked for a modern-only device. It offers the modern
    /// interface beside the legacy one, and this driver uses the modern one.
    /// </summary>
    private const ushort TransitionalNetworkDeviceId = 0x1000;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private VirtioNetDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>virtio-net</c> and one match per network device ID, the modern and
    /// the transitional one, so no other virtio device type is offered to it.
    /// Any context: it only allocates the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per device it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new VirtioNetDriver(),
            PciMatch.Device(VirtioPciTransport.VirtioVendorId, ModernNetworkDeviceId),
            PciMatch.Device(VirtioPciTransport.VirtioVendorId, TransitionalNetworkDeviceId));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        // A device with no vendor capabilities offers the legacy interface
        // alone, which this driver does not speak: declined rather than
        // failed, so it stays free for a driver that does.
        if (!VirtioPciTransport.TryCreate(context, VirtioNetController.QueueCount, out VirtioPciTransport? transport))
        {
            return ProbeResult.Declined;
        }

        VirtioNetController controller = new(context, transport);
        if (!controller.TryStart())
        {
            // So the device stops waiting to be driven, and a later reset
            // finds it in a state it documents rather than half configured.
            transport.Fail();
            return ProbeResult.Failed;
        }

        return ProbeResult.Bound;
    }
}
