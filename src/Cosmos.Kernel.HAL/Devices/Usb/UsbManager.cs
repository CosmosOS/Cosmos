// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Usb.Xhci;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// USB stack entry point. Brings up every supported host controller found
/// on PCI, enumerates the devices present at boot (through hubs as well)
/// and binds class drivers to their interfaces.
///
/// <para>The stack is split in three layers so each can grow on its own:
/// host controllers (<see cref="UsbHostController"/>, today
/// <see cref="XhciController"/>), the shared enumeration here plus the
/// <see cref="UsbDevice"/> model, and class drivers
/// (<see cref="UsbDriver"/>: <see cref="UsbHubDriver"/>,
/// <see cref="UsbKeyboardDriver"/>).</para>
///
/// <para>Devices are enumerated once, at boot: hot-plug needs a thread to
/// run enumeration outside the interrupt that reports the port change, and
/// no such worker exists yet.</para>
/// </summary>
internal static class UsbManager
{
    private const uint MicrosecondsPerMillisecond = 1000;

    private static List<UsbHostController>? s_controllers;
    private static List<UsbDriver>? s_drivers;
    private static List<UsbDevice>? s_devices;

    public static bool IsInitialized => s_controllers is not null;

    /// <summary>Host controllers that came up (empty before <see cref="Initialize"/>).</summary>
    public static IReadOnlyList<UsbHostController> Controllers =>
        (IReadOnlyList<UsbHostController>?)s_controllers ?? Array.Empty<UsbHostController>();

    /// <summary>Every device enumerated and configured, hubs included.</summary>
    public static IReadOnlyList<UsbDevice> Devices =>
        (IReadOnlyList<UsbDevice>?)s_devices ?? Array.Empty<UsbDevice>();

    /// <summary>
    /// Registers the built-in class drivers, starts every xHCI controller and
    /// enumerates the devices behind their root ports. Idempotent.
    /// </summary>
    public static void Initialize()
    {
        if (s_controllers is not null)
        {
            return;
        }

        s_drivers = [new UsbHubDriver()];
        if (CosmosFeatures.KeyboardEnabled)
        {
            s_drivers.Add(new UsbKeyboardDriver());
        }

        s_devices = [];

        List<UsbHostController> controllers = [];
        List<PciDevice> pciDevices = PciManager.GetAllDevicesClass(ClassId.SerialBusController, SubclassId.UsbController);
        foreach (PciDevice pci in pciDevices)
        {
            if (pci.ProgIf != (byte)ProgramIf.UsbXhci)
            {
                Serial.WriteString("[USB] Skipping non-xHCI USB controller (prog-if 0x");
                Serial.WriteHex((uint)pci.ProgIf);
                Serial.WriteString(")\n");
                continue;
            }

            // One controller failing (firmware that never releases it, a
            // reset that never completes) must not cost the others.
            try
            {
                XhciController controller = new(pci, controllers.Count);
                controller.Initialize();
                pci.Claimed = true;
                controllers.Add(controller);
            }
            catch (Exception ex)
            {
                Serial.WriteString("[USB] xHCI controller init failed: ");
                Serial.WriteString(ex.Message);
                Serial.WriteString("\n");
            }
        }

        if (controllers.Count == 0)
        {
            Serial.WriteString("[USB] No usable USB host controller\n");
        }

        foreach (UsbHostController controller in controllers)
        {
            try
            {
                controller.ProbeRootPorts();
            }
            catch (Exception ex)
            {
                Serial.WriteString("[USB] ");
                Serial.WriteString(controller.Name);
                Serial.WriteString(" port probe failed: ");
                Serial.WriteString(ex.Message);
                Serial.WriteString("\n");
            }
        }

        s_controllers = controllers;
    }

    /// <summary>
    /// Addresses the device just reset on <paramref name="port"/>, reads its
    /// descriptors, selects its first configuration and offers each of its
    /// interfaces to the class drivers. Called for root ports by the host
    /// controller and for hub ports by <see cref="UsbHubDriver"/>.
    /// </summary>
    /// <returns>The configured device, or null when enumeration failed.</returns>
    internal static UsbDevice? EnumerateDevice(UsbHostController host, UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        UsbDevice? device = host.AddressDevice(parentHub, port, speed);
        if (device is null)
        {
            return null;
        }

        if (!device.ReadDescriptors())
        {
            WriteDevicePrefix(device);
            Serial.WriteString("could not read descriptors\n");
            host.ReleaseDevice(device);
            return null;
        }

        WriteDevicePrefix(device);
        Serial.WriteHex((uint)device.VendorId);
        Serial.WriteString(":");
        Serial.WriteHex((uint)device.ProductId);
        Serial.WriteString(" ");
        Serial.WriteString(SpeedName(device.Speed));
        Serial.WriteString(", ");
        Serial.WriteNumber((uint)device.Interfaces.Count);
        Serial.WriteString(" interface(s)\n");

        if (device.SetConfiguration() != UsbTransferStatus.Success)
        {
            WriteDevicePrefix(device);
            Serial.WriteString("SET_CONFIGURATION failed\n");
            host.ReleaseDevice(device);
            return null;
        }

        s_devices?.Add(device);
        BindDrivers(device);
        return device;
    }

    internal static void DelayMilliseconds(uint milliseconds) =>
        PlatformHAL.Initializer?.DelayMicroseconds(milliseconds * MicrosecondsPerMillisecond);

    private static void BindDrivers(UsbDevice device)
    {
        if (s_drivers is null)
        {
            return;
        }

        foreach (UsbInterface usbInterface in device.Interfaces)
        {
            foreach (UsbDriver driver in s_drivers)
            {
                if (TryBind(driver, device, usbInterface))
                {
                    usbInterface.Driver = driver;
                    break;
                }
            }

            WriteDevicePrefix(device);
            Serial.WriteString("interface ");
            Serial.WriteNumber((uint)usbInterface.Number);
            Serial.WriteString(" (class 0x");
            Serial.WriteHex((uint)usbInterface.Class);
            Serial.WriteString("): ");
            Serial.WriteString(usbInterface.Driver?.Name ?? "no driver");
            Serial.WriteString("\n");
        }
    }

    /// <summary>
    /// Offers one interface to one driver. A driver that throws loses the
    /// interface instead of aborting the enumeration of every device after it
    /// (the hub driver enumerates whole subtrees from inside its bind).
    /// </summary>
    private static bool TryBind(UsbDriver driver, UsbDevice device, UsbInterface usbInterface)
    {
        try
        {
            return driver.TryBind(device, usbInterface);
        }
        catch (Exception ex)
        {
            WriteDevicePrefix(device);
            Serial.WriteString(driver.Name);
            Serial.WriteString(" driver failed: ");
            Serial.WriteString(ex.Message);
            Serial.WriteString("\n");
            return false;
        }
    }

    private static void WriteDevicePrefix(UsbDevice device)
    {
        Serial.WriteString("[USB] ");
        Serial.WriteString(device.HostController.Name);
        Serial.WriteString(" root port ");
        Serial.WriteNumber((uint)device.RootPortNumber);
        if (device.Parent is not null)
        {
            Serial.WriteString(" hub depth ");
            Serial.WriteNumber((uint)device.HubDepth);
            Serial.WriteString(" port ");
            Serial.WriteNumber((uint)device.PortNumber);
        }

        Serial.WriteString(": ");
    }

    private static string SpeedName(UsbSpeed speed) => speed switch
    {
        UsbSpeed.Low => "low-speed",
        UsbSpeed.Full => "full-speed",
        UsbSpeed.High => "high-speed",
        UsbSpeed.Super => "SuperSpeed",
        UsbSpeed.SuperPlus => "SuperSpeedPlus",
        _ => "unknown speed"
    };
}
