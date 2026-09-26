// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// One interface of a configured USB device, as <see cref="UsbInterfaces"/>
/// read it from the USB stack. The IDs, class and path are fixed when it is
/// read; the members about its driver read the stack's interface each time,
/// so a state kept across an unplug shows what the unplug left on it.
/// Thread context.
/// </summary>
internal sealed class UsbInterfaceState
{
    private readonly object _device;
    private readonly object _interface;

    /// <summary>The path the kit documents for the interface, as in <c>usb/1-2.1:1.0</c>, worked out from the device's position.</summary>
    public string Path { get; }

    /// <summary>bInterfaceNumber.</summary>
    public byte Number { get; }

    /// <summary>bInterfaceClass.</summary>
    public byte Class { get; }

    /// <summary>bInterfaceSubClass.</summary>
    public byte Subclass { get; }

    /// <summary>bInterfaceProtocol.</summary>
    public byte Protocol { get; }

    /// <summary>The device's idVendor.</summary>
    public ushort VendorId { get; }

    /// <summary>The device's idProduct.</summary>
    public ushort ProductId { get; }

    /// <summary>How many interfaces the device's configuration has.</summary>
    public int DeviceInterfaceCount { get; }

    /// <summary>The name the USB stack gives the interface's owner: a registration's, a built-in class driver's, or null.</summary>
    public string? DriverName => UsbInterfaces.GetDriverName(_interface);

    /// <summary>The binding the USB stack keeps for a registered driver that bound the interface, or null.</summary>
    public UsbDeviceContext? DriverContext => UsbInterfaces.GetDriverContext(_interface);

    /// <summary>True while the USB stack records a class driver for the interface.</summary>
    public bool HasClassDriver => UsbInterfaces.GetClassDriver(_interface) is not null;

    /// <summary>True while the class driver the USB stack records for the interface is the kit's.</summary>
    public bool IsHeldByKit => ReferenceEquals(UsbInterfaces.GetClassDriver(_interface), UsbInterfaces.KitDriver);

    /// <summary>The name of the class driver the USB stack records for the interface, or null for none.</summary>
    public string? ClassDriverName => UsbInterfaces.GetClassDriver(_interface) is { } driver ? UsbInterfaces.GetClassDriverName(driver) : null;

    /// <summary>Reads the fixed facts of <paramref name="usbInterface"/> of <paramref name="device"/>.</summary>
    /// <param name="device">The USB stack's device.</param>
    /// <param name="usbInterface">One of its interfaces.</param>
    /// <param name="path">The interface's path.</param>
    /// <param name="deviceInterfaceCount">How many interfaces the device has.</param>
    public UsbInterfaceState(object device, object usbInterface, string path, int deviceInterfaceCount)
    {
        _device = device;
        _interface = usbInterface;
        Path = path;
        DeviceInterfaceCount = deviceInterfaceCount;
        Number = UsbInterfaces.GetNumber(usbInterface);
        Class = UsbInterfaces.GetClass(usbInterface);
        Subclass = UsbInterfaces.GetSubclass(usbInterface);
        Protocol = UsbInterfaces.GetProtocol(usbInterface);
        VendorId = UsbInterfaces.GetVendorId(device);
        ProductId = UsbInterfaces.GetProductId(device);
    }

    /// <summary>True when the interface has the given class, subclass and protocol.</summary>
    /// <param name="interfaceClass">bInterfaceClass.</param>
    /// <param name="subclass">bInterfaceSubClass.</param>
    /// <param name="protocol">bInterfaceProtocol.</param>
    /// <returns>Whether all three match.</returns>
    public bool Is(byte interfaceClass, byte subclass, byte protocol) =>
        Class == interfaceClass && Subclass == subclass && Protocol == protocol;

    /// <summary>
    /// A context over the interface that the kit never offered anything and
    /// that no driver holds, built with the constructor the kit uses, whose
    /// control requests reach the device through the USB stack as a bound
    /// driver's do. For a cell that must talk to a device whose last binding
    /// attempt is over, so its own context refuses.
    /// </summary>
    /// <param name="driverName">The name its log lines carry.</param>
    /// <returns>The context.</returns>
    /// <exception cref="MissingMethodException">The context's constructor changed.</exception>
    public UsbDeviceContext CreateContext(string driverName) => UsbInterfaces.NewContext(driverName, Path, _device, _interface);
}
