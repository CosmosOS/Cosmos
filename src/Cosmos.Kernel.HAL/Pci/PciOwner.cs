// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci;

/// <summary>
/// Every name a built-in writes to <see cref="PciDevice.Owner"/>, kept in one
/// place so logs, tests and the planned build-time opt-out of built-in drivers
/// spell them the same way. A name is the short lowercase name of the driver
/// that owns the function; <c>e1000e</c>, <c>virtio-net</c>, <c>ahci</c> and
/// <c>nvme</c> are also the names that opt-out will accept.
/// </summary>
/// <remarks>
/// <para>
/// virtio has one name per device type rather than a single <c>virtio</c>:
/// the opt-out addresses <c>virtio-net</c> alone, and a driver that replaces
/// it must still find the input and GPU functions owned by their built-ins.
/// An input function is <c>virtio-input</c> whether a keyboard or a mouse
/// driver ends up on it, because the scan claims the function before it
/// probes which of the two it is.
/// </para>
/// <para>
/// USB class drivers (<c>usb-keyboard</c>, <c>usb-mass-storage</c>) own USB
/// interfaces, never a PCI function: the xHCI host controller that carries
/// them is owned by <see cref="Xhci"/>.
/// </para>
/// </remarks>
internal static class PciOwner
{
    /// <summary>
    /// The display function that scans out the Limine boot framebuffer,
    /// reserved at enumeration so a driver bound later never resizes or
    /// reprograms the BAR the console draws into. It is a reservation, not a
    /// driver: <see cref="PciDevice.Claimed"/> stays false and a built-in
    /// display driver may still take the function over.
    /// </summary>
    public const string Gop = "gop";

    /// <summary>Intel 82574 (E1000E) network driver, x64 only.</summary>
    public const string E1000E = "e1000e";

    /// <summary>virtio network device over PCI.</summary>
    public const string VirtioNet = "virtio-net";

    /// <summary>virtio input device over PCI, keyboard or mouse.</summary>
    public const string VirtioInput = "virtio-input";

    /// <summary>virtio GPU over PCI.</summary>
    public const string VirtioGpu = "virtio-gpu";

    /// <summary>xHCI USB host controller.</summary>
    public const string Xhci = "xhci";

    /// <summary>AHCI SATA host bus adapter.</summary>
    public const string Ahci = "ahci";

    /// <summary>NVMe controller.</summary>
    public const string Nvme = "nvme";

    /// <summary>VMware SVGA II adapter, taken when the full-screen canvas first drives it.</summary>
    public const string VmwareSvga = "vmware-svga";
}
