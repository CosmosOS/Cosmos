// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// What a bus knows about a device before any driver looks at it: the bus
/// name, a bus-specific address, and a description for the log. Each bus
/// kind derives its own identity type (synthetic, platform and PCI today;
/// virtio, USB and PS/2 later) and the matching <see cref="DeviceMatch"/>
/// types read the fields they constrain.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class DeviceIdentity
{
    /// <summary>The bus the device sits on: "synthetic", "platform", "pci"; later "virtio", "usb", "ps2".</summary>
    public abstract string BusName { get; }

    /// <summary>The device's address on its bus, in the bus's own notation.</summary>
    public abstract string Address { get; }

    /// <summary>The identity in words, for the log and the diagnostics view: "vendor 8086 device 100e class 02".</summary>
    public abstract string Describe();
}
