// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The USB interfaces the HAL's internal <c>UsbManager</c> enumerated, as
/// the ground truth the cells hold the driver kit's public device list and
/// its contexts against, and for what neither shows: an interface's owner
/// before the driver pass, which class driver the USB stack records for it,
/// the binding it keeps, and whether the hot-plug thread runs. The USB
/// stack's lists are not locked, so these are read at points where the
/// hot-plug thread is not changing them: before or during the pass, or
/// once a hot-plug wait on
/// <see cref="Cosmos.Kernel.System.Drivers.DriverManager.Devices"/>, which
/// is safe to read meanwhile, has seen the change. Thread context.
/// </summary>
/// <remarks>
/// Reached through <see cref="UnsafeAccessorAttribute"/> and
/// <see cref="UnsafeAccessorTypeAttribute"/>, as <see cref="PciFunctions"/>
/// describes: an accessor whose target moved throws, failing its cell. The
/// USB stack's lists come back as generic lists of internal types, which
/// are read through their non-generic <see cref="IList"/>.
/// </remarks>
internal static class UsbInterfaces
{
    private const string UsbManagerType = "Cosmos.Kernel.HAL.Devices.Usb.UsbManager, Cosmos.Kernel.HAL";
    private const string UsbDeviceType = "Cosmos.Kernel.HAL.Devices.Usb.UsbDevice, Cosmos.Kernel.HAL";
    private const string UsbInterfaceType = "Cosmos.Kernel.HAL.Devices.Usb.UsbInterface, Cosmos.Kernel.HAL";
    private const string UsbClassDriverType = "Cosmos.Kernel.HAL.Devices.Usb.UsbClassDriver, Cosmos.Kernel.HAL";
    private const string KitUsbDriverType = "Cosmos.Kernel.HAL.Drivers.Engine.KitUsbDriver, Cosmos.Kernel.HAL";
    private const string DeviceListType = $"System.Collections.Generic.IReadOnlyList`1[[{UsbDeviceType}]]";
    private const string InterfaceListType = $"System.Collections.Generic.List`1[[{UsbInterfaceType}]]";

    /// <summary>True once the USB hot-plug thread started.</summary>
    /// <exception cref="MissingMethodException">The USB manager no longer has the property.</exception>
    public static bool IsHotPlugRunning => GetIsHotPlugRunning(null);

    /// <summary>The kit's class driver, the one the USB stack records for every interface a registered driver binds.</summary>
    /// <exception cref="MissingMethodException">The kit's class driver no longer has the property.</exception>
    internal static object KitDriver => GetKitDriver(null);

    /// <summary>
    /// Every interface of every configured device, in the USB stack's
    /// order, each with the path the kit documents for it, worked out here
    /// from the device's position rather than read from the kit.
    /// </summary>
    /// <returns>The interfaces; empty when no USB device is configured.</returns>
    public static UsbInterfaceState[] Enumerate()
    {
        IList devices = (IList)GetDevices(null);
        List<UsbInterfaceState> found = [];
        for (int i = 0; i < devices.Count; i++)
        {
            object? device = devices[i];
            if (device is null)
            {
                continue;
            }

            string devicePath = $"usb/{BusOf(device)}-{PortsOf(device)}:{GetConfigurationValue(device)}";
            IList interfaces = (IList)GetInterfaces(device);
            for (int j = 0; j < interfaces.Count; j++)
            {
                if (interfaces[j] is { } usbInterface)
                {
                    found.Add(new UsbInterfaceState(device, usbInterface, $"{devicePath}.{GetNumber(usbInterface)}", interfaces.Count));
                }
            }
        }

        return [.. found];
    }

    /// <summary>How many enumerated interfaces have the given class, subclass and protocol.</summary>
    /// <param name="interfaceClass">bInterfaceClass.</param>
    /// <param name="subclass">bInterfaceSubClass.</param>
    /// <param name="protocol">bInterfaceProtocol.</param>
    /// <returns>The count.</returns>
    public static int Count(byte interfaceClass, byte subclass, byte protocol)
    {
        int count = 0;
        foreach (UsbInterfaceState usbInterface in Enumerate())
        {
            if (usbInterface.Is(interfaceClass, subclass, protocol))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The first enumerated interface with the given class, subclass and protocol, or null.</summary>
    /// <param name="interfaceClass">bInterfaceClass.</param>
    /// <param name="subclass">bInterfaceSubClass.</param>
    /// <param name="protocol">bInterfaceProtocol.</param>
    /// <returns>The interface.</returns>
    public static UsbInterfaceState? Find(byte interfaceClass, byte subclass, byte protocol)
    {
        foreach (UsbInterfaceState usbInterface in Enumerate())
        {
            if (usbInterface.Is(interfaceClass, subclass, protocol))
            {
                return usbInterface;
            }
        }

        return null;
    }

    /// <summary>
    /// The bus of <paramref name="device"/>: the number the USB core gave
    /// its host controller when the driver pass delivered it, from 1.
    /// </summary>
    private static int BusOf(object device) => GetBusNumber(GetBus(device));

    /// <summary>The root port, then each hub port down to <paramref name="device"/>, joined with dots.</summary>
    private static string PortsOf(object device)
    {
        string ports = $"{GetPortNumber(device)}";
        for (object? hub = GetParent(device); hub is not null; hub = GetParent(hub))
        {
            ports = $"{GetPortNumber(hub)}.{ports}";
        }

        return ports;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_IsHotPlugRunning")]
    private static extern bool GetIsHotPlugRunning([UnsafeAccessorType(UsbManagerType)] object? manager);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Devices")]
    [return: UnsafeAccessorType(DeviceListType)]
    private static extern object GetDevices([UnsafeAccessorType(UsbManagerType)] object? manager);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_Instance")]
    [return: UnsafeAccessorType(KitUsbDriverType)]
    private static extern object GetKitDriver([UnsafeAccessorType(KitUsbDriverType)] object? driver);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Interfaces")]
    [return: UnsafeAccessorType(InterfaceListType)]
    private static extern object GetInterfaces([UnsafeAccessorType(UsbDeviceType)] object device);

    /// <summary>The bus the device was enumerated on, which its path's bus number comes from.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Bus")]
    private static extern UsbBus GetBus([UnsafeAccessorType(UsbDeviceType)] object device);

    /// <summary>The bus number the USB core gave the controller, from 1.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Number")]
    private static extern int GetBusNumber(UsbBus bus);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Parent")]
    [return: UnsafeAccessorType(UsbDeviceType)]
    private static extern object? GetParent([UnsafeAccessorType(UsbDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PortNumber")]
    private static extern byte GetPortNumber([UnsafeAccessorType(UsbDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ConfigurationValue")]
    private static extern byte GetConfigurationValue([UnsafeAccessorType(UsbDeviceType)] object device);

    /// <summary>The device's idVendor.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_VendorId")]
    internal static extern ushort GetVendorId([UnsafeAccessorType(UsbDeviceType)] object device);

    /// <summary>The device's idProduct.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ProductId")]
    internal static extern ushort GetProductId([UnsafeAccessorType(UsbDeviceType)] object device);

    /// <summary>The interface's bInterfaceNumber.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Number")]
    internal static extern byte GetNumber([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>The interface's bInterfaceClass.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Class")]
    internal static extern byte GetClass([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>The interface's bInterfaceSubClass.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Subclass")]
    internal static extern byte GetSubclass([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>The interface's bInterfaceProtocol.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Protocol")]
    internal static extern byte GetProtocol([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>The class driver the USB stack records for the interface, or null.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Driver")]
    [return: UnsafeAccessorType(UsbClassDriverType)]
    internal static extern object? GetClassDriver([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>A class driver's name.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Name")]
    internal static extern string GetClassDriverName([UnsafeAccessorType(UsbClassDriverType)] object driver);

    /// <summary>The binding the USB stack keeps for a registered driver that bound the interface, or null.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DriverContext")]
    internal static extern UsbDeviceContext? GetDriverContext([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>The name of the interface's owner, a registration's or a class driver's, or null.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DriverName")]
    internal static extern string? GetDriverName([UnsafeAccessorType(UsbInterfaceType)] object usbInterface);

    /// <summary>A context over the interface, built with the constructor the kit uses; see <see cref="UsbInterfaceState.CreateContext"/>.</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    internal static extern UsbDeviceContext NewContext(string driverName, string path,
        [UnsafeAccessorType(UsbDeviceType)] object device, [UnsafeAccessorType(UsbInterfaceType)] object usbInterface);
}
