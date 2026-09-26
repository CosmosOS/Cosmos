// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A driver's handle on one device it was offered, whatever bus the device
/// sits on. The kit builds one for each binding attempt and hands it to the
/// driver's Probe. Everything acquired through it belongs to it: when the
/// attempt is declined or fails, the kit releases all of it, so a driver
/// never writes teardown code of its own.
/// </summary>
internal abstract class DeviceContext
{
    /// <summary>
    /// Longest busy-wait handed to the platform in one call, in microseconds.
    /// A longer <see cref="Delay"/> loops, so the platform's own tick
    /// arithmetic never sees a count large enough to overflow.
    /// </summary>
    private const long MaximumDelayChunkMicroseconds = 1_000_000;

    private const string MouseDisabledMessage = "Mouse support is disabled. Set CosmosEnableMouse=true in the kernel's csproj to publish a mouse.";
    private const string NetworkDisabledMessage = "Network support is disabled. Set CosmosEnableNetwork=true in the kernel's csproj to publish a network link.";

    // What Probe created, which Bound arms and teardown drops. Null until
    // the first one: most attempts create neither.
    private List<DeviceEvent>? _events;
    private List<DeviceWorkItem>? _workItems;

    // What Probe published, held until Bound delivers it to the managers or
    // teardown drops it. Null until the first one.
    private List<PublishedMouse>? _mice;
    private List<PublishedNetworkDevice>? _networkLinks;

    /// <summary>
    /// Where the device sits, such as <c>pci/0000:00:04.0</c> for a PCI
    /// function (segment, bus, device and function, in hexadecimal).
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// True while the device is on its bus. A PCI function never leaves it in
    /// this version, so a PCI context always reports true.
    /// </summary>
    public bool IsPresent { get; } = true;

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
    /// Publishes a mouse the driver reports through, which moves the
    /// kernel's pointer like a built-in mouse. The kit hands it to the mouse
    /// manager right after Probe returns Bound, before the interrupts are
    /// armed; if the attempt is declined or fails, it is dropped and its
    /// reports go nowhere. Probe only.
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
    /// declined or fails, the link is dropped. Probe only.
    /// </summary>
    /// <param name="address">The device's MAC address, which the stack sends from.</param>
    /// <param name="transmit">Called for each frame the stack sends, with interrupts masked, one call at a time.</param>
    /// <returns>The link the driver delivers received frames and link changes through.</returns>
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

    /// <summary>Marks the start of the driver's Probe: resources can be acquired until it returns.</summary>
    internal void BeginProbe() => State = DeviceContextState.Probing;

    /// <summary>
    /// Records that Probe returned Bound and the engine kept the binding,
    /// then delivers what Probe published to the managers and arms the
    /// binding: the interrupt handler starts running, and the work items
    /// scheduled during Probe are queued. Delivery comes first, so the
    /// handler and the work items find their mouse and link already wired
    /// to the kernel.
    /// </summary>
    internal void MarkBound()
    {
        State = DeviceContextState.Bound;
        DeliverPublications();
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
    /// Hands what Probe published to the managers, on the thread that ran
    /// the probe. A manager that throws is logged with the driver's name and
    /// the device's path; the binding stands, only that publication is lost.
    /// </summary>
    private void DeliverPublications()
    {
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
    }

    /// <summary>
    /// The second step of every teardown, after the interrupts are
    /// disarmed: drops what Probe published, so no manager ever sees it,
    /// forgets the work items Probe scheduled, so they never run, and
    /// cancels the events, so every Wait returns false.
    /// </summary>
    private protected void DropQueuedAndCancelEvents()
    {
        // A dropped mouse was never enabled, so its reports already go
        // nowhere; forgetting it is all there is to do.
        _mice = null;

        if (_networkLinks is { } links)
        {
            for (int i = 0; i < links.Count; i++)
            {
                links[i].Drop();
            }

            _networkLinks = null;
        }

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
