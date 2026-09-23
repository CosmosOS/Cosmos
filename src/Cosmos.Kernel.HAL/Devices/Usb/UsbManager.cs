// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Devices.Usb.Xhci;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// USB stack entry point. Brings up every supported host controller found
/// on PCI, enumerates the devices present at boot (through hubs as well),
/// binds class drivers to their interfaces, and follows the devices plugged
/// in and pulled out afterwards.
///
/// <para>The stack is split in three layers so each can grow on its own:
/// host controllers (<see cref="UsbHostController"/>, today
/// <see cref="XhciController"/>), the shared enumeration here plus the
/// <see cref="UsbDevice"/> model, and class drivers
/// (<see cref="UsbDriver"/>: <see cref="UsbHubDriver"/>,
/// <see cref="UsbKeyboardDriver"/>, <see cref="UsbMassStorageDriver"/>).</para>
///
/// <para>Hot-plug runs on a thread of its own, which
/// <see cref="StartHotPlug"/> starts once the scheduler runs. A port change
/// is reported in interrupt context (a root port's status change event, a
/// hub's status change endpoint), where nothing can be enumerated, so the
/// report only wakes the thread (<see cref="NotifyPortChange"/>), and the
/// thread asks every controller and every hub which of their ports
/// changed. Enumeration, driver binding and disconnects after boot all run
/// on it, one at a time: <see cref="Devices"/> is only changed by the boot
/// path, then by this thread.</para>
/// </summary>
internal static class UsbManager
{
    private const uint MicrosecondsPerMillisecond = 1000;

    /// <summary>How often the hot-plug thread polls a controller whose port changes raise no interrupt.</summary>
    private const uint PollIntervalMs = 250;

    /// <summary>Longest wait for the scheduler's first tick: a few of its 10 ms quanta.</summary>
    private const uint SchedulerTickWaitMs = 50;

    private static List<UsbHostController>? s_controllers;
    private static List<UsbDriver>? s_drivers;
    private static List<UsbDevice>? s_devices;

    /// <summary>Signaled from interrupt context by every port change report; the hot-plug thread waits on it.</summary>
    private static InterruptEvent? s_portChange;

    private static SysThread? s_hotPlugThread;

    public static bool IsInitialized => s_controllers is not null;

    /// <summary>
    /// True once <see cref="StartHotPlug"/> started the thread that follows
    /// devices plugged in or pulled out; before that, and when it could not
    /// start, the devices are the ones found at boot.
    /// </summary>
    public static bool IsHotPlugRunning => s_hotPlugThread is not null;

    /// <summary>Host controllers that came up (empty before <see cref="Initialize"/>).</summary>
    public static IReadOnlyList<UsbHostController> Controllers =>
        (IReadOnlyList<UsbHostController>?)s_controllers ?? Array.Empty<UsbHostController>();

    /// <summary>
    /// Every device enumerated and configured, hubs included. Changed by the
    /// boot path, then by the hot-plug thread only.
    /// </summary>
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

        if (CosmosFeatures.StorageEnabled)
        {
            s_drivers.Add(new UsbMassStorageDriver());
        }

        s_devices = [];
        s_portChange = new InterruptEvent();

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
    /// Starts following the devices plugged in and pulled out of the
    /// controllers <see cref="Initialize"/> brought up. Needs the scheduler
    /// running and its timer ticking, so interrupts enabled: without them,
    /// and in a kernel built without the scheduler, the devices are the ones
    /// found at boot. Idempotent.
    /// </summary>
    public static void StartHotPlug()
    {
        if (s_hotPlugThread is not null || s_controllers is not { Count: > 0 } || !SchedulerManager.IsRunning)
        {
            return;
        }

        // Thread.Start returns once the new thread ran, and only a scheduler
        // tick can run it: with a timer that never started (x64 with ACPI
        // off has nothing to calibrate the LAPIC timer against), Start would
        // wait forever.
        if (!WaitForSchedulerTick())
        {
            Serial.WriteString("[USB] Scheduler timer not ticking, hot-plug disabled\n");
            return;
        }

        s_hotPlugThread = new SysThread(RunHotPlug);
        s_hotPlugThread.Start();
    }

    /// <summary>
    /// Wakes the hot-plug thread to look at the ports again. Safe from
    /// interrupt context: host controllers call it for a root port change,
    /// hubs for a report of their status change endpoint.
    /// </summary>
    public static void NotifyPortChange() => s_portChange?.Signal();

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

    /// <summary>
    /// Disconnects the device on <paramref name="port"/> of
    /// <paramref name="parentHub"/> (a root port of <paramref name="host"/>
    /// when null) and, when it is a hub, every device behind it: the class
    /// drivers let go of them, then the host controller frees them. Nothing
    /// happens when no device was enumerated on that port. Hot-plug thread
    /// only.
    /// </summary>
    internal static void DisconnectPort(UsbHostController host, UsbDevice? parentHub, byte port)
    {
        UsbDevice? device = FindDevice(host, parentHub, port);
        if (device is null)
        {
            return;
        }

        // The whole branch is gone: its transfers fail from here on, so no
        // driver below waits on a device that will never answer.
        MarkDisconnected(device);
        Disconnect(device);
    }

    internal static void DelayMilliseconds(uint milliseconds) =>
        PlatformHAL.Initializer?.DelayMicroseconds(milliseconds * MicrosecondsPerMillisecond);

    /// <summary>True once the scheduler timer has fired, waiting a few quanta for it.</summary>
    private static bool WaitForSchedulerTick()
    {
        for (uint waitedMs = 0; SchedulerManager.TickPeriodNs == 0; waitedMs++)
        {
            if (waitedMs >= SchedulerTickWaitMs)
            {
                return false;
            }

            DelayMilliseconds(1);
        }

        return true;
    }

    /// <summary>
    /// Hot-plug thread: lets every controller and hub handle their changed
    /// ports, then sleeps until another change is reported. The first pass
    /// runs at once, for the changes left over from the boot probe, which
    /// raised no report of their own.
    /// </summary>
    private static void RunHotPlug()
    {
        if (s_controllers is not { } controllers)
        {
            return;
        }

        Serial.WriteString("[USB] Hot-plug thread started\n");
        while (true)
        {
            foreach (UsbHostController controller in controllers)
            {
                try
                {
                    controller.HandlePortChanges();
                }
                catch (Exception ex)
                {
                    Serial.WriteString("[USB] ");
                    Serial.WriteString(controller.Name);
                    Serial.WriteString(" port change failed: ");
                    Serial.WriteString(ex.Message);
                    Serial.WriteString("\n");
                }
            }

            try
            {
                UsbHubDriver.HandlePortChanges();
            }
            catch (Exception ex)
            {
                Serial.WriteString("[USB] Hub port change failed: ");
                Serial.WriteString(ex.Message);
                Serial.WriteString("\n");
            }

            WaitForPortChange(controllers);
        }
    }

    /// <summary>
    /// Blocks until a port change is reported. A controller that reports
    /// nothing by interrupt is polled instead, which also delivers what its
    /// hubs report.
    /// </summary>
    private static void WaitForPortChange(List<UsbHostController> controllers)
    {
        bool polled = false;
        foreach (UsbHostController controller in controllers)
        {
            polled |= controller.IsPolled;
        }

        if (!polled)
        {
            s_portChange?.Wait();
            return;
        }

        SchedulerManager.Sleep(PollIntervalMs);
        foreach (UsbHostController controller in controllers)
        {
            if (controller.IsPolled)
            {
                controller.Poll();
            }
        }
    }

    private static UsbDevice? FindDevice(UsbHostController host, UsbDevice? parentHub, byte port)
    {
        if (s_devices is null)
        {
            return null;
        }

        foreach (UsbDevice device in s_devices)
        {
            if (device.HostController == host && device.Parent == parentHub && device.PortNumber == port)
            {
                return device;
            }
        }

        return null;
    }

    private static List<UsbDevice> FindChildren(UsbDevice hub)
    {
        List<UsbDevice> children = [];
        if (s_devices is not null)
        {
            foreach (UsbDevice device in s_devices)
            {
                if (device.Parent == hub)
                {
                    children.Add(device);
                }
            }
        }

        return children;
    }

    private static void MarkDisconnected(UsbDevice device)
    {
        device.MarkDisconnected();
        foreach (UsbDevice child in FindChildren(device))
        {
            MarkDisconnected(child);
        }
    }

    /// <summary>
    /// Takes one device off the bus: the devices behind it first when it is
    /// a hub, then the drivers of its interfaces, then its host controller
    /// state.
    /// </summary>
    private static void Disconnect(UsbDevice device)
    {
        foreach (UsbDevice child in FindChildren(device))
        {
            Disconnect(child);
        }

        WriteDevicePrefix(device);
        Serial.WriteString("disconnected\n");

        foreach (UsbInterface usbInterface in device.Interfaces)
        {
            UsbDriver? driver = usbInterface.Driver;
            if (driver is null)
            {
                continue;
            }

            // One driver failing to let go must not keep the device's slot
            // (and the others' interfaces) allocated.
            try
            {
                driver.Disconnect(device, usbInterface);
            }
            catch (Exception ex)
            {
                WriteDevicePrefix(device);
                Serial.WriteString(driver.Name);
                Serial.WriteString(" driver failed to disconnect: ");
                Serial.WriteString(ex.Message);
                Serial.WriteString("\n");
            }

            usbInterface.Driver = null;
        }

        if (s_devices is not null)
        {
            for (int i = 0; i < s_devices.Count; i++)
            {
                if (s_devices[i] == device)
                {
                    s_devices.RemoveAt(i);
                    break;
                }
            }
        }

        device.HostController.ReleaseDevice(device);
    }

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
