// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// The kit's USB core. Takes the host controllers PCI drivers publish,
/// enumerates the devices behind them (through hubs as well), binds class
/// drivers to their interfaces, and follows the devices plugged in and
/// pulled out afterwards.
///
/// <para>The stack is split in three layers so each can grow on its own:
/// host controllers (<see cref="UsbHostController"/>, written against the
/// driver kit and published from a PCI driver's Probe, such as the built-in
/// xHCI driver of Cosmos.Kernel.HAL.Drivers), the shared enumeration here
/// plus the <see cref="UsbDevice"/> model, and class drivers
/// (<see cref="UsbClassDriver"/>: <see cref="UsbHubDriver"/>, and last
/// <see cref="KitUsbDriver"/>, which stands for the drivers the kit binds,
/// the built-in mass storage and boot keyboard drivers of
/// Cosmos.Kernel.HAL.Drivers among them, and the kernel's own).</para>
///
/// <para>A controller arrives during the driver pass, when the kit delivers
/// what its driver published: <see cref="AddController"/> gives it a bus
/// and enumerates what sits on its root ports, on the boot thread. Hot-plug
/// runs on a thread of its own, which <see cref="StartHotPlug"/> starts
/// once the pass is over. A port change is reported in interrupt context (a
/// root port's status change event, a hub's status change endpoint), where
/// nothing can be enumerated, so the report only wakes the thread
/// (<see cref="NotifyPortChange"/>), and the thread asks every controller
/// and every hub which of their ports changed. Enumeration, driver binding
/// and disconnects after the pass all run on it, one at a time:
/// <see cref="Devices"/> is only changed by the pass, then by this
/// thread.</para>
/// </summary>
internal static class UsbManager
{
    private const uint MicrosecondsPerMillisecond = 1000;

    /// <summary>How often the hot-plug thread polls a controller whose port changes raise no interrupt.</summary>
    private const uint PollIntervalMs = 250;

    // Null until the first controller arrives: a kernel whose pass binds
    // none never builds the class drivers.
    private static List<UsbBus>? s_buses;
    private static List<UsbHostController>? s_controllers;
    private static List<UsbClassDriver>? s_drivers;
    private static List<UsbDevice>? s_devices;

    /// <summary>Signaled from interrupt context by every port change report; the hot-plug thread waits on it.</summary>
    private static InterruptEvent? s_portChange;

    /// <summary>
    /// True once <see cref="StartHotPlug"/> started the thread that follows
    /// devices plugged in or pulled out; before that, and when it could not
    /// start, the devices are the ones found during the driver pass.
    /// </summary>
    public static bool IsHotPlugRunning { get; private set; }

    /// <summary>Host controllers delivered so far, in delivery order, which is also their bus numbers' (empty before the first).</summary>
    public static IReadOnlyList<UsbHostController> Controllers =>
        (IReadOnlyList<UsbHostController>?)s_controllers ?? Array.Empty<UsbHostController>();

    /// <summary>
    /// Every device enumerated and configured, hubs included. Changed by the
    /// driver pass, then by the hot-plug thread only.
    /// </summary>
    public static IReadOnlyList<UsbDevice> Devices =>
        (IReadOnlyList<UsbDevice>?)s_devices ?? Array.Empty<UsbDevice>();

    /// <summary>
    /// Takes a host controller a PCI driver published: gives it the next bus
    /// number and enumerates the devices on its root ports, binding the hub
    /// driver as it goes; the kit's own USB drivers are offered what it left
    /// once the pass reaches its USB step. Called by the kit when it delivers
    /// the publication, on the boot thread during the driver pass, before the
    /// controller's driver has its interrupts armed. The first controller
    /// also brings up the class drivers.
    /// </summary>
    /// <param name="controller">The published controller.</param>
    internal static void AddController(UsbHostController controller)
    {
        if (s_buses is null || s_controllers is null || s_drivers is null)
        {
            // HAL's hub driver first, then the drivers the kit binds, the
            // built-in mass storage and boot keyboard drivers among them,
            // which are offered only what the hub driver left, on hot-plug
            // as at boot.
            s_drivers = [new UsbHubDriver(), KitUsbDriver.Instance];

            s_devices = [];
            s_portChange = new InterruptEvent();
            s_controllers = [];
            s_buses = [];
        }

        // Listed before its ports are probed, so the devices behind it find
        // their bus number. The pass is the only thread that adds one, and
        // the hot-plug thread starts after it.
        UsbBus bus = new(controller, s_buses.Count + 1);
        s_buses.Add(bus);
        s_controllers.Add(controller);

        try
        {
            bus.ProbeRootPorts();
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

    /// <summary>
    /// Starts following the devices plugged in and pulled out of the
    /// controllers the driver pass delivered. Needs the scheduler running
    /// and switching threads, so interrupts enabled: without them, and in a
    /// kernel built without the scheduler, the devices are the ones found
    /// during the pass. Idempotent once it succeeded.
    /// </summary>
    public static void StartHotPlug()
    {
        if (IsHotPlugRunning || s_buses is not { Count: > 0 } || !SchedulerManager.IsRunning)
        {
            return;
        }

        // The thread only runs once the scheduler switches to it. A timer
        // that never started (x64 with ACPI off has nothing to calibrate the
        // LAPIC timer against) or never schedules would leave CoreLib's
        // Thread.Start spinning forever; KernelThread gives up after a few
        // quanta and makes sure the thread never runs later.
        if (!KernelThread.TryStart(RunHotPlug))
        {
            Serial.WriteString("[USB] Scheduler timer not ticking, hot-plug disabled\n");
            return;
        }

        IsHotPlugRunning = true;
    }

    /// <summary>
    /// Wakes the hot-plug thread to look at the ports again. Safe from
    /// interrupt context: host controllers call it for a root port change,
    /// through <see cref="UsbBus.NotifyPortChange"/>, hubs for a report of
    /// their status change endpoint.
    /// </summary>
    public static void NotifyPortChange() => s_portChange?.Signal();

    /// <summary>
    /// Addresses the device just reset on <paramref name="port"/>, reads its
    /// descriptors, selects its first configuration and offers each of its
    /// interfaces to the class drivers. Called for root ports through the
    /// controller's <see cref="UsbBus"/> and for hub ports by
    /// <see cref="UsbHubDriver"/>.
    /// </summary>
    /// <returns>The configured device, or null when enumeration failed.</returns>
    internal static UsbDevice? EnumerateDevice(UsbBus bus, UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        UsbHostController controller = bus.Controller;
        UsbHostDevice? host = controller.AddressDevice(parentHub?.Host, port, speed);
        if (host is null)
        {
            return null;
        }

        UsbDevice device = new(bus, host, parentHub, port, speed);
        if (!device.ReadDescriptors())
        {
            WriteDevicePrefix(device);
            Serial.WriteString("could not read descriptors\n");
            controller.ReleaseDevice(host);
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
            controller.ReleaseDevice(host);
            return null;
        }

        s_devices?.Add(device);
        BindDrivers(device);

        // Once the driver pass published the device list, the enumerating
        // thread keeps it current; the boot-time enumeration runs before.
        DriverCore.RefreshDevices();
        return device;
    }

    /// <summary>
    /// Disconnects the device on <paramref name="port"/> of
    /// <paramref name="parentHub"/> (a root port of <paramref name="bus"/>
    /// when null) and, when it is a hub, every device behind it: the class
    /// drivers let go of them, then the host controller frees them. Nothing
    /// happens when no device was enumerated on that port. Hot-plug thread
    /// only.
    /// </summary>
    internal static void DisconnectPort(UsbBus bus, UsbDevice? parentHub, byte port)
    {
        UsbDevice? device = FindDevice(bus, parentHub, port);
        if (device is null)
        {
            return;
        }

        // The whole branch is gone: its transfers fail from here on, so no
        // driver below waits on a device that will never answer.
        MarkDisconnected(device);
        Disconnect(device);
        DriverCore.RefreshDevices();
    }

    internal static void DelayMilliseconds(uint milliseconds) =>
        PlatformHAL.Initializer?.DelayMicroseconds(milliseconds * MicrosecondsPerMillisecond);

    /// <summary>
    /// Hot-plug thread: lets every controller and hub handle their changed
    /// ports, then sleeps until another change is reported. The first pass
    /// runs at once, for the changes left over from the boot probe, which
    /// raised no report of their own.
    /// </summary>
    private static void RunHotPlug()
    {
        if (s_buses is not { } buses)
        {
            return;
        }

        Serial.WriteString("[USB] Hot-plug thread started\n");
        while (true)
        {
            foreach (UsbBus bus in buses)
            {
                try
                {
                    bus.HandlePortChanges();
                }
                catch (Exception ex)
                {
                    Serial.WriteString("[USB] ");
                    Serial.WriteString(bus.Controller.Name);
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

            WaitForPortChange(buses);
        }
    }

    /// <summary>
    /// Blocks until a port change is reported. A controller that reports
    /// nothing by interrupt is polled instead, which also delivers what its
    /// hubs report.
    /// </summary>
    private static void WaitForPortChange(List<UsbBus> buses)
    {
        bool polled = false;
        foreach (UsbBus bus in buses)
        {
            polled |= bus.Controller.IsPolled;
        }

        if (!polled)
        {
            s_portChange?.Wait();
            return;
        }

        SchedulerManager.Sleep(PollIntervalMs);
        foreach (UsbBus bus in buses)
        {
            if (bus.Controller.IsPolled)
            {
                bus.Controller.Poll();
            }
        }
    }

    private static UsbDevice? FindDevice(UsbBus bus, UsbDevice? parentHub, byte port)
    {
        if (s_devices is null)
        {
            return null;
        }

        foreach (UsbDevice device in s_devices)
        {
            if (device.Bus == bus && device.Parent == parentHub && device.PortNumber == port)
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
            UsbClassDriver? driver = usbInterface.Driver;
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

        device.HostController.ReleaseDevice(device.Host);
    }

    private static void BindDrivers(UsbDevice device)
    {
        if (s_drivers is null)
        {
            return;
        }

        foreach (UsbInterface usbInterface in device.Interfaces)
        {
            foreach (UsbClassDriver driver in s_drivers)
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
            // The registration's name for an interface a kernel's driver
            // took, not the driver kit's.
            Serial.WriteString(usbInterface.DriverName ?? "no driver");
            Serial.WriteString("\n");
        }
    }

    /// <summary>
    /// Offers one interface to one driver. A driver that throws loses the
    /// interface instead of aborting the enumeration of every device after it
    /// (the hub driver enumerates whole subtrees from inside its bind).
    /// </summary>
    private static bool TryBind(UsbClassDriver driver, UsbDevice device, UsbInterface usbInterface)
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
