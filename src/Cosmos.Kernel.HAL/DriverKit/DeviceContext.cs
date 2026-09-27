// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A driver's handle on one device it was offered, whatever bus the device
/// sits on. The kit builds one for each binding attempt and hands it to the
/// driver's Probe. Everything acquired through it belongs to it: when the
/// attempt is declined or fails, the kit releases all of it, and when a
/// bound USB device leaves its bus, the kit withdraws what the driver
/// published and stops its work items and events, so a driver never writes
/// teardown code of its own. Only the kit creates one: a driver receives
/// the <see cref="Pci.PciDeviceContext"/> or
/// <see cref="Usb.UsbDeviceContext"/> of its attempt as Probe's argument.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public abstract class DeviceContext
{
    /// <summary>
    /// Longest busy-wait handed to the platform in one call, in microseconds.
    /// A longer <see cref="Delay"/> loops, so the platform's own tick
    /// arithmetic never sees a count large enough to overflow.
    /// </summary>
    private const long MaximumDelayChunkMicroseconds = 1_000_000;

    /// <summary>
    /// Longest a USB unplug waits for a work item of the binding that is
    /// running when the device leaves, in milliseconds of Stopwatch time,
    /// before it calls the driver's Remove anyway.
    /// </summary>
    private const long RunningWorkItemWaitMilliseconds = 1000;

    /// <summary>How long the unplug sleeps between two looks at the running work item, in milliseconds.</summary>
    private const uint RunningWorkItemPollMilliseconds = 10;

    private const long MillisecondsPerSecond = 1000;

    private const string KeyboardDisabledMessage = "Keyboard support is disabled. Set CosmosEnableKeyboard=true in the kernel's csproj to publish a keyboard.";
    private const string MouseDisabledMessage = "Mouse support is disabled. Set CosmosEnableMouse=true in the kernel's csproj to publish a mouse.";
    private const string NetworkDisabledMessage = "Network support is disabled. Set CosmosEnableNetwork=true in the kernel's csproj to publish a network link.";
    private const string StorageDisabledMessage = "Storage support is disabled. Set CosmosEnableStorage=true in the kernel's csproj to publish a disk.";

    // What Probe created, which Bound arms, and teardown or a USB unplug
    // drops. Null until the first one: most attempts create neither.
    private List<DeviceEvent>? _events;
    private List<DeviceWorkItem>? _workItems;

    // What Probe published, held until Bound delivers it to the managers or
    // teardown drops it; once delivered, kept until a USB unplug withdraws
    // it from the managers. Null until the first one.
    private List<PublishedKeyboard>? _keyboards;
    private List<PublishedMouse>? _mice;
    private List<PublishedNetworkDevice>? _networkLinks;

    // The disks, which a bound driver's work item may add to as well, on
    // the driver-work thread while the USB hot-plug thread withdraws them:
    // guarded by _publicationLock from Bound on.
    private List<PublishedBlockDevice>? _blockDevices;

    /// <summary>
    /// Makes a work item's disk and a USB unplug's withdrawal one step each.
    /// Not readonly: SpinLock is a mutable struct.
    /// </summary>
    private SchedSpinLock _publicationLock;

    /// <summary>
    /// Cleared on the USB hot-plug thread when the device leaves its bus,
    /// while a work item on the driver-work thread may be reading it.
    /// </summary>
    private volatile bool _present = true;

    /// <summary>
    /// Where the device sits, such as <c>pci/0000:00:04.0</c> for a PCI
    /// function (segment, bus, device and function, in hexadecimal) or
    /// <c>usb/1-2.1:1.0</c> for a USB interface (host controller, the root
    /// port and each hub port below it, configuration value and interface
    /// number, in decimal). Any context: reading it allocates nothing.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// True while the device is on its bus: false once a USB device was
    /// unplugged, from before the kit calls the driver's Remove. A PCI
    /// function never leaves it in this version, so a PCI context always
    /// reports true. Any context, interrupt handlers included.
    /// </summary>
    public bool IsPresent => _present;

    /// <summary>Name of the registration this context was built for, which prefixes its log lines.</summary>
    internal string DriverName { get; }

    /// <summary>Where the binding attempt stands, which decides the members a driver may call.</summary>
    internal DeviceContextState State { get; private protected set; }

    private protected DeviceContext(string driverName, string path)
    {
        DriverName = driverName;
        Path = path;
    }

    /// <summary>
    /// Writes <paramref name="message"/> to the serial log as one line,
    /// prefixed with <c>[Drivers]</c>, the driver's registered name and
    /// <see cref="Path"/>. Thread context only: it builds a string.
    /// </summary>
    /// <param name="message">What to log, without a trailing newline.</param>
    public void WriteLog(string message) => Serial.WriteString($"[Drivers] {DriverName} {Path}: {message}\n");

    /// <summary>
    /// Busy-waits for at least <paramref name="duration"/>, rounded up to
    /// the microsecond, without needing the scheduler or interrupts. Thread
    /// context only. Meant for the short settle times a device's
    /// documentation asks for; a probe runs on the boot thread, which is the
    /// idle thread, so it cannot sleep instead.
    /// </summary>
    /// <param name="duration">How long to wait; zero returns at once.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">
    /// No platform is registered to busy-wait on, which only happens before
    /// the HAL is brought up, never in a callback the kit makes.
    /// </exception>
    public void Delay(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);

        IPlatformInitializer platform = PlatformHAL.Initializer
            ?? throw new InvalidOperationException("No platform is registered to busy-wait on.");

        // Rounded up: a driver waiting out a settle time must never wait less
        // than the device asked for.
        long remaining = duration.Ticks / TimeSpan.TicksPerMicrosecond;
        if (duration.Ticks % TimeSpan.TicksPerMicrosecond != 0)
        {
            remaining++;
        }

        while (remaining > 0)
        {
            long chunk = Math.Min(remaining, MaximumDelayChunkMicroseconds);
            platform.DelayMicroseconds((uint)chunk);
            remaining -= chunk;
        }
    }

    /// <summary>
    /// Creates an event the driver's interrupt handler signals and a
    /// thread waits on. Probe only.
    /// </summary>
    /// <returns>The event, owned by this binding: once the attempt is declined or fails, its Wait returns false.</returns>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public DeviceEvent CreateEvent()
    {
        ThrowIfNotProbing(nameof(CreateEvent));

        DeviceEvent created = new(this);
        (_events ??= []).Add(created);
        return created;
    }

    /// <summary>
    /// Creates a work item that runs <paramref name="callback"/> on the
    /// kit's <c>driver-work</c> thread each time it is scheduled, typically
    /// by the driver's interrupt handler. The first call starts that thread,
    /// waiting briefly for the scheduler to run it. Probe only.
    /// </summary>
    /// <param name="callback">What to run, in thread context, once the binding is Bound.</param>
    /// <param name="workItem">The work item when the call returns true.</param>
    /// <returns>
    /// False when there is no thread to run it: the scheduler is compiled
    /// out, or it did not start the driver-work thread.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public bool TryCreateWorkItem(Action callback, [NotNullWhen(true)] out DeviceWorkItem? workItem)
    {
        ThrowIfNotProbing(nameof(TryCreateWorkItem));
        ArgumentNullException.ThrowIfNull(callback);

        if (!DriverWorkQueue.TryEnsureStarted())
        {
            workItem = null;
            WriteLog("no work item: the driver-work thread is not running");
            return false;
        }

        workItem = new DeviceWorkItem(this, callback);
        (_workItems ??= []).Add(workItem);
        return true;
    }

    /// <summary>
    /// Publishes a keyboard the driver reports through, which types into the
    /// kernel's key queue like a built-in keyboard. The kit hands it to the
    /// keyboard manager right after Probe returns Bound, before the
    /// interrupts are armed; if the attempt is declined or fails, it is
    /// dropped and its reports go nowhere, and once a USB device leaves its
    /// bus, the kit takes it back out of the keyboard manager. Probe only.
    /// </summary>
    /// <returns>The reporter the driver calls, typically from its interrupt handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Called outside the driver's Probe, or the kernel is built without
    /// keyboard support.
    /// </exception>
    public KeyboardReporter PublishKeyboard() => PublishKeyboardCore(null);

    /// <summary>
    /// Publishes a keyboard whose lock lamps the driver can set, as
    /// <see cref="PublishKeyboard()"/> does otherwise. The kit tracks the
    /// lamps from the lock keys the driver reports and hands them to
    /// <paramref name="updateLeds"/> whenever the kernel's keyboard manager
    /// toggles one. Probe only.
    /// </summary>
    /// <param name="updateLeds">Called with the lamps to show, in the context the driver reported the lock key from.</param>
    /// <returns>The reporter the driver calls, typically from its interrupt handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updateLeds"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Called outside the driver's Probe, or the kernel is built without
    /// keyboard support.
    /// </exception>
    public KeyboardReporter PublishKeyboard(KeyboardLedHandler updateLeds)
    {
        ArgumentNullException.ThrowIfNull(updateLeds);
        return PublishKeyboardCore(updateLeds);
    }

    /// <summary>The body both <see cref="PublishKeyboard()"/> overloads share.</summary>
    /// <param name="updateLeds">The lamp path, or null for a keyboard that shows none.</param>
    private KeyboardReporter PublishKeyboardCore(KeyboardLedHandler? updateLeds)
    {
        ThrowIfNotProbing(nameof(PublishKeyboard));

        // The switch alone first, as in PublishMouse.
        if (!CosmosFeatures.KeyboardEnabled)
        {
            throw new InvalidOperationException(KeyboardDisabledMessage);
        }

        // Installed by System's initializer whenever the switch is on.
        if (DriverCore.KeyboardSink is null)
        {
            throw new InvalidOperationException(KeyboardDisabledMessage);
        }

        PublishedKeyboard keyboard = new(updateLeds);
        (_keyboards ??= []).Add(keyboard);
        return new KeyboardReporter(keyboard);
    }

    /// <summary>
    /// Publishes a mouse the driver reports through, which moves the
    /// kernel's pointer like a built-in mouse. The kit hands it to the mouse
    /// manager right after Probe returns Bound, before the interrupts are
    /// armed; if the attempt is declined or fails, it is dropped and its
    /// reports go nowhere, and once a USB device leaves its bus, the kit
    /// takes it back out of the mouse manager. Probe only.
    /// </summary>
    /// <returns>The reporter the driver calls, typically from its interrupt handler.</returns>
    /// <exception cref="InvalidOperationException">
    /// Called outside the driver's Probe, or the kernel is built without
    /// mouse support.
    /// </exception>
    public MouseReporter PublishMouse()
    {
        ThrowIfNotProbing(nameof(PublishMouse));

        // The switch alone first, so ILC folds it and a kernel without mouse
        // support trims the adapter.
        if (!CosmosFeatures.MouseEnabled)
        {
            throw new InvalidOperationException(MouseDisabledMessage);
        }

        // Installed by System's initializer whenever the switch is on.
        if (DriverCore.MouseSink is null)
        {
            throw new InvalidOperationException(MouseDisabledMessage);
        }

        PublishedMouse mouse = new();
        (_mice ??= []).Add(mouse);
        return new MouseReporter(mouse);
    }

    /// <summary>
    /// Publishes a network interface: the kernel's network stack sends
    /// through <paramref name="transmit"/>, and the driver hands it received
    /// frames through the returned link. The kit registers it with the
    /// network manager right after Probe returns Bound, before the
    /// interrupts are armed, and after every device a built-in driver
    /// registered, so the primary device does not change. If the attempt is
    /// declined or fails, the link is dropped; once a USB device leaves its
    /// bus, the kit takes it back out of the network manager, and the stack
    /// forgets its addresses. Probe only.
    /// </summary>
    /// <param name="address">The device's MAC address, which the stack sends from.</param>
    /// <param name="transmit">Called for each frame the stack sends, with interrupts masked, one call at a time.</param>
    /// <returns>The link the driver delivers received frames and link changes through.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> or <paramref name="transmit"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Called outside the driver's Probe, or the kernel is built without
    /// network support.
    /// </exception>
    public NetworkLink PublishNetworkLink(MACAddress address, NetworkTransmitHandler transmit)
    {
        ThrowIfNotProbing(nameof(PublishNetworkLink));
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(transmit);

        // The switch alone first, as in PublishMouse.
        if (!CosmosFeatures.NetworkEnabled)
        {
            throw new InvalidOperationException(NetworkDisabledMessage);
        }

        if (DriverCore.NetworkSink is null)
        {
            throw new InvalidOperationException(NetworkDisabledMessage);
        }

        PublishedNetworkDevice device = new(this, address, transmit);
        (_networkLinks ??= []).Add(device);
        return new NetworkLink(device);
    }

    /// <summary>
    /// Publishes a disk the driver implements. The kernel's storage manager
    /// registers it like a built-in disk: it reads its partition table
    /// through it at once, on the thread the kit delivers it on, and ranks
    /// it among the other disks by the primary-disk rule. From Probe, the
    /// kit delivers it right after Probe returns Bound, before the
    /// interrupts are armed, on the thread that ran the probe, so a disk
    /// published there must complete its I/O without the driver's
    /// interrupts, by polling its device, at least until the handler first
    /// runs, since the partition scan comes before it can. From one of the
    /// binding's own work items, once Bound, the kit delivers it before the
    /// call returns, on the driver-work thread, with the driver's interrupts
    /// armed: where a disk that waits for its completion interrupt is
    /// published. The partition scan then holds that thread, which runs one
    /// work item at a time, so the disk's completions must reach its I/O
    /// through the interrupt handler (<see cref="DeviceEvent.Signal"/>), never
    /// through another work item, which could not run until the scan
    /// returned. If the attempt is declined or fails, a disk published in
    /// Probe is dropped; once a USB device leaves its bus, the kit takes its
    /// disks back out of the storage manager, which detaches the filesystems
    /// mounted from them without a flush, and every I/O through them throws
    /// from then on.
    /// </summary>
    /// <param name="device">
    /// The disk, which the storage manager calls from any thread. Its
    /// <see cref="IBlockDevice.Name"/> must be unique among the kernel's
    /// disks, such as <c>sata0</c>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="device"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Called outside the driver's Probe and outside the binding's own work
    /// items, or the kernel is built without storage support.
    /// </exception>
    public void PublishBlockDevice(IBlockDevice device)
    {
        bool probing = State == DeviceContextState.Probing;
        if (!probing && !(State == DeviceContextState.Bound && DriverWorkQueue.IsCurrentItemOf(this)))
        {
            throw new InvalidOperationException($"{nameof(PublishBlockDevice)} can only be called from the driver's Probe or from one of the binding's work items.");
        }

        ArgumentNullException.ThrowIfNull(device);

        // The switch alone first, as in PublishMouse.
        if (!CosmosFeatures.StorageEnabled)
        {
            throw new InvalidOperationException(StorageDisabledMessage);
        }

        if (DriverCore.BlockDeviceSink is null)
        {
            throw new InvalidOperationException(StorageDisabledMessage);
        }

        PublishedBlockDevice disk = new(this, device);
        if (probing)
        {
            (_blockDevices ??= []).Add(disk);
            return;
        }

        // A USB unplug clears IsPresent before it takes the list to withdraw
        // it, so under the lock a disk either lands in the list the unplug
        // withdraws, or finds the device gone and is dropped here.
        bool recorded;
        using (_publicationLock.AcquireIrqSafe())
        {
            recorded = IsPresent;
            if (recorded)
            {
                (_blockDevices ??= []).Add(disk);
            }
        }

        if (!recorded)
        {
            disk.Drop();
            WriteLog($"dropped disk {disk.Name}: the device left the bus");
            return;
        }

        DeliverBlockDevice(disk);
    }

    /// <summary>Marks the start of the driver's Probe: resources can be acquired until it returns.</summary>
    internal void BeginProbe() => State = DeviceContextState.Probing;

    /// <summary>Records that the device left its bus, which only a USB device does in this version.</summary>
    private protected void MarkNotPresent() => _present = false;

    /// <summary>
    /// Records that Probe returned Bound and the engine kept the binding,
    /// then delivers what Probe published to the managers and arms the
    /// binding: the interrupt handler starts running, and the work items
    /// scheduled during Probe are queued. Delivery comes first, so the
    /// handler and the work items find their mouse and link already wired
    /// to the kernel, and a USB host controller is handed to the USB core,
    /// which enumerates its root ports, last before the arming.
    /// </summary>
    internal void MarkBound()
    {
        State = DeviceContextState.Bound;
        DeliverPublications();
        DeliverUsbHostController();
        ArmInterrupts();

        if (_workItems is { } workItems)
        {
            for (int i = 0; i < workItems.Count; i++)
            {
                workItems[i].Arm();
            }
        }
    }

    /// <summary>
    /// Lets the interrupts the driver requested during Probe through to its
    /// handler. Called once, when the binding becomes Bound.
    /// </summary>
    private protected abstract void ArmInterrupts();

    /// <summary>
    /// Hands the USB host controller Probe published to the USB core, on the
    /// thread that ran the probe, once the other publications are delivered
    /// and before the interrupts are armed. Only a PCI binding can publish
    /// one.
    /// </summary>
    private protected virtual void DeliverUsbHostController()
    {
        // Empty on purpose: a USB binding has no way to publish one.
    }

    /// <summary>
    /// Hands what Probe published to the managers, on the thread that ran
    /// the probe: the keyboards, then the mice, then the network links, then
    /// the disks, whose
    /// partition tables the storage manager reads here, before the driver's
    /// interrupts are armed. A manager that throws is logged with the
    /// driver's name and the device's path; the binding stands, only that
    /// publication is lost.
    /// </summary>
    private void DeliverPublications()
    {
        if (_keyboards is { } keyboards)
        {
            for (int i = 0; i < keyboards.Count; i++)
            {
                try
                {
                    DriverCore.KeyboardSink?.Invoke(keyboards[i]);
                    WriteLog("published a keyboard");
                }
                catch (Exception exception)
                {
                    WriteLog($"the keyboard manager refused its keyboard: {exception.Message}");
                }
            }
        }

        if (_mice is { } mice)
        {
            for (int i = 0; i < mice.Count; i++)
            {
                try
                {
                    DriverCore.MouseSink?.Invoke(mice[i]);
                    WriteLog("published a mouse");
                }
                catch (Exception exception)
                {
                    WriteLog($"the mouse manager refused its mouse: {exception.Message}");
                }
            }
        }

        if (_networkLinks is { } links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                PublishedNetworkDevice link = links[i];
                try
                {
                    // Live before the manager holds it, so the stack's first
                    // send through it goes out.
                    link.Initialize();
                    DriverCore.NetworkSink?.Invoke(link);
                    WriteLog($"published network link {link.MacAddress}");
                }
                catch (Exception exception)
                {
                    WriteLog($"the network manager refused its link: {exception.Message}");
                }
            }
        }

        if (_blockDevices is { } disks)
        {
            for (int i = 0; i < disks.Count; i++)
            {
                DeliverBlockDevice(disks[i]);
            }
        }
    }

    /// <summary>
    /// Hands one disk to the storage manager, which scans its partition
    /// table through it before it returns, on the calling thread: the
    /// probing thread for a disk Probe published, the driver-work thread for
    /// one a work item did. A manager that throws is logged; the binding
    /// stands.
    /// </summary>
    private void DeliverBlockDevice(PublishedBlockDevice disk)
    {
        // Live first, so the manager's partition scan reaches the driver. A
        // disk dropped or withdrawn before this point is not delivered.
        if (!disk.TryGoLive())
        {
            return;
        }

        try
        {
            bool registered = DriverCore.BlockDeviceSink?.Invoke(disk) ?? false;
            WriteLog(registered
                ? $"published disk {disk.Name}"
                : $"the storage manager holds as many disks as it can and did not take disk {disk.Name}");
        }
        catch (Exception exception)
        {
            WriteLog($"the storage manager refused disk {disk.Name}: {exception.Message}");
        }

        // A USB unplug that withdrew a work item's disk while the manager was
        // still registering it found nothing to unregister yet, so the disk
        // is taken back out here. Unregistering a disk the manager no longer
        // holds does nothing.
        if (disk.IsWithdrawn)
        {
            try
            {
                DriverCore.BlockDeviceWithdrawSink?.Invoke(disk);
            }
            catch (Exception exception)
            {
                WriteLog($"the storage manager failed to withdraw disk {disk.Name}: {exception.Message}");
            }
        }
    }

    /// <summary>
    /// The second step of every teardown, after the interrupts are
    /// disarmed: drops what Probe published, so no manager ever sees it,
    /// forgets the work items Probe scheduled, so they never run, and
    /// cancels the events, so every Wait returns false.
    /// </summary>
    private protected void DropQueuedAndCancelEvents()
    {
        // A dropped keyboard or mouse was never enabled, so its reports
        // already go nowhere; forgetting them is all there is to do.
        _keyboards = null;
        _mice = null;

        if (_networkLinks is { } links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                links[i].Drop();
            }

            _networkLinks = null;
        }

        // Only Probe added to it: a work item never ran for this attempt.
        if (_blockDevices is { } disks)
        {
            for (int i = 0; i < disks.Count; i++)
            {
                disks[i].Drop();
            }

            _blockDevices = null;
        }

        DropWorkAndCancelEvents();
    }

    /// <summary>
    /// The second step of a USB unplug, once the reports are disarmed:
    /// takes what the driver published back out of the managers. Each
    /// publication is marked withdrawn before its manager lets go of it, so
    /// a report through the driver's <see cref="MouseReporter"/>, or a send
    /// through its <see cref="NetworkLink"/>'s device, goes nowhere from the
    /// first instant on, and an I/O through one of its disks throws,
    /// whatever the manager does. A manager that throws is
    /// logged with the driver's name and the device's path, and the next
    /// publication is still withdrawn.
    /// </summary>
    private protected void WithdrawPublications()
    {
        if (_keyboards is { } keyboards)
        {
            for (int i = 0; i < keyboards.Count; i++)
            {
                PublishedKeyboard keyboard = keyboards[i];
                keyboard.Withdraw();
                try
                {
                    DriverCore.KeyboardWithdrawSink?.Invoke(keyboard);
                    WriteLog("withdrew its keyboard");
                }
                catch (Exception exception)
                {
                    WriteLog($"the keyboard manager failed to withdraw its keyboard: {exception.Message}");
                }
            }

            _keyboards = null;
        }

        if (_mice is { } mice)
        {
            for (int i = 0; i < mice.Count; i++)
            {
                PublishedMouse mouse = mice[i];
                mouse.Withdraw();
                try
                {
                    DriverCore.MouseWithdrawSink?.Invoke(mouse);
                    WriteLog("withdrew its mouse");
                }
                catch (Exception exception)
                {
                    WriteLog($"the mouse manager failed to withdraw its mouse: {exception.Message}");
                }
            }

            _mice = null;
        }

        if (_networkLinks is { } links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                PublishedNetworkDevice link = links[i];
                link.Withdraw();
                try
                {
                    DriverCore.NetworkWithdrawSink?.Invoke(link);
                    WriteLog($"withdrew network link {link.MacAddress}");
                }
                catch (Exception exception)
                {
                    WriteLog($"the network manager failed to withdraw its link: {exception.Message}");
                }
            }

            _networkLinks = null;
        }

        // Taken under the lock, since a work item may be adding one: the
        // device is already marked gone, so none lands in the list after.
        List<PublishedBlockDevice>? disks;
        using (_publicationLock.AcquireIrqSafe())
        {
            disks = _blockDevices;
            _blockDevices = null;
        }

        if (disks is not null)
        {
            for (int i = 0; i < disks.Count; i++)
            {
                PublishedBlockDevice disk = disks[i];
                disk.Withdraw();
                try
                {
                    DriverCore.BlockDeviceWithdrawSink?.Invoke(disk);
                    WriteLog($"withdrew disk {disk.Name}");
                }
                catch (Exception exception)
                {
                    WriteLog($"the storage manager failed to withdraw disk {disk.Name}: {exception.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Forgets the binding's work items, so a queued one never runs and
    /// every later Schedule is refused, and cancels its events, so every
    /// Wait returns false. A callback already running on the driver-work
    /// thread is not stopped; a USB unplug waits for it with
    /// <see cref="WaitForRunningWorkItem"/>.
    /// </summary>
    private protected void DropWorkAndCancelEvents()
    {
        if (_workItems is { } workItems)
        {
            for (int i = 0; i < workItems.Count; i++)
            {
                workItems[i].Drop();
            }
        }

        if (_events is { } events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                events[i].Cancel();
            }
        }
    }

    /// <summary>
    /// Waits, on the USB hot-plug thread, while the driver-work thread runs
    /// a work item of this binding, for up to a second of Stopwatch time: a
    /// callback that started before the device left may still be using what
    /// the driver's Remove is about to let go of. Its items were dropped
    /// before, so none starts after it. It sleeps between looks, so the
    /// driver-work thread gets the CPU it needs to finish. An item still
    /// running after a second is logged, and Remove runs anyway: a callback
    /// stuck on a device that is gone must not stall every later hot-plug.
    /// </summary>
    private protected void WaitForRunningWorkItem()
    {
        if (_workItems is null)
        {
            return;
        }

        long limit = Stopwatch.Frequency / MillisecondsPerSecond * RunningWorkItemWaitMilliseconds;
        long startedAt = Stopwatch.GetTimestamp();
        while (DriverWorkQueue.IsRunningItemOf(this))
        {
            if (Stopwatch.GetTimestamp() - startedAt >= limit)
            {
                WriteLog($"a work item was still running {RunningWorkItemWaitMilliseconds} ms after the device left; Remove runs anyway");
                return;
            }

            SchedulerManager.Sleep(RunningWorkItemPollMilliseconds);
        }
    }

    /// <summary>
    /// Throws unless the driver's Probe is running. Resources are acquired
    /// there only, so the kit knows everything an attempt holds by the time
    /// it decides whether the attempt binds.
    /// </summary>
    /// <param name="member">The member the driver called, for the message.</param>
    private protected void ThrowIfNotProbing(string member)
    {
        if (State != DeviceContextState.Probing)
        {
            throw new InvalidOperationException($"{member} can only be called from the driver's Probe.");
        }
    }

    /// <summary>
    /// Throws unless the driver's Probe is running or the binding is Bound:
    /// for what a driver may also take once it holds the device for good,
    /// since no teardown can race it then.
    /// </summary>
    /// <param name="member">The member the driver called, for the message.</param>
    private protected void ThrowIfNotProbingOrBound(string member)
    {
        if (State is not (DeviceContextState.Probing or DeviceContextState.Bound))
        {
            throw new InvalidOperationException($"{member} can only be called from the driver's Probe or once the binding is Bound.");
        }
    }

    /// <summary>
    /// Throws once the attempt this context belonged to was declined or
    /// failed: its device went back to the kit, and may be another driver's
    /// by now.
    /// </summary>
    private protected void ThrowIfTornDown()
    {
        if (State == DeviceContextState.TornDown)
        {
            throw new InvalidOperationException("The binding attempt this context belonged to was declined or failed; the device is no longer the driver's.");
        }
    }
}
