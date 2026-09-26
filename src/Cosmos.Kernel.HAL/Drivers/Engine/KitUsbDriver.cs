// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// The USB drivers a kernel registered, as the USB stack sees them: one
/// class driver, last in <see cref="UsbManager"/>'s list, behind the hub,
/// keyboard and mass storage built-ins, so those keep the first pick of
/// every interface, at boot and when a device is plugged in later. The
/// interfaces present at boot are left to the driver pass, which runs once
/// the kernel constructor registered its drivers; after it, a device the
/// hot-plug thread enumerates gets the same ranked offering through
/// <see cref="TryBind"/>. The binding of the registration that took an
/// interface is kept with it, in <see cref="UsbInterface.DriverContext"/>,
/// which is also what names its owner in the logs and the device list,
/// until <see cref="Disconnect"/> ends it.
/// </summary>
internal sealed class KitUsbDriver : UsbClassDriver
{
    /// <summary>
    /// Created on first use rather than by a static initializer, which would
    /// be a class constructor running during USB bring-up.
    /// </summary>
    private static KitUsbDriver? s_instance;

    /// <summary>
    /// What <see cref="UsbManager"/> logs when the kit as a whole fails; an
    /// interface it bound is named after the registration that bound it.
    /// </summary>
    public override string Name => "driver kit";

    private KitUsbDriver()
    {
    }

    /// <summary>
    /// The one instance: the entry in <see cref="UsbManager"/>'s list, and
    /// the driver the pass records on each interface it binds, so the USB
    /// stack hands every kit binding back to it on disconnect.
    /// </summary>
    internal static KitUsbDriver Instance => s_instance ??= new KitUsbDriver();

    /// <summary>
    /// Offers <paramref name="usbInterface"/>, which no built-in took, to the
    /// registered USB drivers that match it, best match first. Always false
    /// before the driver pass ran, which offers the interfaces present at
    /// boot itself, and when no USB driver is registered.
    /// </summary>
    /// <returns>True when a registered driver bound the interface.</returns>
    public override bool TryBind(UsbDevice device, UsbInterface usbInterface) =>
        DriverCore.OfferHotPluggedInterface(device, usbInterface);

    /// <summary>
    /// Ends the binding of an interface a registered driver bound, once its
    /// device left the bus, on the hot-plug thread and before the host
    /// controller frees the device: the unplug teardown of
    /// <see cref="DriverCore.RemoveUsbBinding"/>, which withdraws what the
    /// driver published and calls its Remove. The interface keeps no
    /// binding afterwards, so the device list the hot-plug thread rebuilds
    /// next names no owner for it.
    /// </summary>
    public override void Disconnect(UsbDevice device, UsbInterface usbInterface)
    {
        if (usbInterface.DriverContext is not { } context)
        {
            return;
        }

        try
        {
            DriverCore.RemoveUsbBinding(context);
        }
        finally
        {
            usbInterface.DriverContext = null;
        }
    }
}
