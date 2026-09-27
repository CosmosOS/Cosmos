// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Input;

/// <summary>
/// The built-in virtio-input driver: brings a virtio input device on the PCI
/// bus up and publishes it to the kernel as the keyboard or the mouse it
/// turns out to be, which the kernel then reads keys and pointer movement
/// from like a built-in one. One instance per bound function, and a machine
/// with both a virtio keyboard and a virtio mouse binds two.
/// </summary>
/// <remarks>
/// <para>
/// PCI only. The same devices appear on the QEMU virt machine's virtio-mmio
/// window, where there is no PCI function to bind and no BAR to map, and
/// those are still driven by <c>Cosmos.Kernel.HAL</c>'s own virtio stack;
/// the kit has no seam for that bus. A kernel on ARM64 therefore reaches a
/// virtio keyboard or mouse through this driver when the board puts it on
/// PCI, and through HAL when it puts it on the MMIO window.
/// </para>
/// <para>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreatePciInputRegistrations"/>, in a kernel
/// built with keyboard or mouse support. Only with both does it cover every
/// device it binds: with one of the two switched off, a device of the other
/// kind still binds here and its publication then fails, which the kit logs
/// and which leaves the function without a driver, as it would with the
/// driver not registered at all. Its name, <c>virtio-input</c>, is the owner
/// the function records, and a name the kit refuses to a kernel's own
/// registration.
/// </para>
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class VirtioInputDriver : PciDriver
{
    private const string DriverName = "virtio-input";

    /// <summary>
    /// Device ID of a modern virtio input device: virtio numbers them from
    /// 0x1040 by device type, and the input device is type 18. There is no
    /// transitional ID to match beside it, as there is for the network
    /// device: the input device was defined after the legacy interface, and
    /// only the first nine types ever had one.
    /// </summary>
    private const ushort ModernInputDeviceId = 0x1052;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private VirtioInputDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>virtio-input</c> and the one virtio input device ID, so no other
    /// virtio device type is offered to it. Any context: it only allocates
    /// the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per device it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new VirtioInputDriver(),
            PciMatch.Device(VirtioPciTransport.VirtioVendorId, ModernInputDeviceId));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        // A device with no vendor capabilities offers the legacy interface
        // alone, which this driver does not speak: declined rather than
        // failed, so it stays free for a driver that does.
        if (!VirtioPciTransport.TryCreate(context, VirtioInputController.QueueCount, out VirtioPciTransport? transport))
        {
            return ProbeResult.Declined;
        }

        VirtioInputController controller = new(context, transport);
        ProbeResult result = controller.Start();
        if (result != ProbeResult.Bound)
        {
            // So the device stops waiting to be driven, and a later reset
            // finds it in a state it documents rather than half configured.
            // A driver that takes it next resets it first, which clears this.
            transport.Fail();
        }

        return result;
    }
}
