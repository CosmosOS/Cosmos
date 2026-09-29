// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A driver's handle on one device, created for each offer and kept while
/// the driver is bound. Everything a driver acquires goes through it and is
/// written to its ledger: windows, regions, DMA memory, interrupts, events,
/// work items, periodic work, threads, published devices, child nodes. When
/// the driver declines, fails, or the device goes away, the kit releases the
/// ledger in a fixed order and the driver frees nothing itself. A
/// <see cref="DeviceLock"/> is the one thing on the ledger that is nothing
/// to release: it is recorded for the diagnostics only.
/// <para>
/// Every member here is thread context: probe, work items, driver threads.
/// Called from an interrupt handler, a member stops with an exception naming
/// itself (in a synthetic dispatch) or a panic (in a real interrupt). Once
/// <see cref="IsDetaching"/> is true no new resource can be acquired.
/// </para>
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed unsafe partial class DeviceBinding
{
    /// <summary>How long teardown waits for each driver thread to exit.</summary>
    internal const uint JoinTimeoutMilliseconds = 500;

    private const ulong FourGiB = 1UL << 32;

    /// <summary>
    /// Guards <see cref="_detaching"/> and every list below. IRQ-safe so a
    /// preemption never parks a holder while another thread spins on it.
    /// Nothing under it maps or calls out; the lists may grow.
    /// </summary>
    private SchedSpinLock _lock;
    private volatile bool _detaching;

    private readonly List<IKitResource> _memory = new();
    private readonly List<InterruptHandle> _handles = new();
    private readonly List<WorkItem> _workItems = new();
    private readonly List<WorkItem> _kitItems = new();
    private readonly List<PeriodicWork> _periodic = new();
    private readonly List<DeviceEvent> _events = new();
    private readonly List<DriverThread> _threads = new();
    private readonly List<PublishedDevice> _devices = new();
    private readonly List<DeviceNode> _children = new();
    private readonly List<DeviceLock> _locks = new();

    internal DeviceBinding(DeviceNode node, Driver driver)
    {
        Node = node;
        Driver = driver;
        DetachEvent = new DeviceEvent();
    }

    /// <summary>The device.</summary>
    public DeviceNode Node { get; }

    /// <summary>The driver offered the device.</summary>
    public Driver Driver { get; }

    /// <summary>
    /// The driver's own per-device state, set during <see cref="Driver.Probe"/>
    /// and read back in work items, threads and <see cref="Driver.OnDetach"/>.
    /// The driver instance is shared across devices; this slot is not.
    /// </summary>
    public object? DriverState { get; set; }

    /// <summary>
    /// True from the first moment of teardown. Driver threads loop on it;
    /// acquiring members refuse once it is set.
    /// </summary>
    public bool IsDetaching => _detaching;

    /// <summary>
    /// Cancelled when teardown begins, like every event of the binding, so a
    /// thread waiting on it (or on any of them) wakes, finds
    /// <see cref="IsDetaching"/> true, and returns.
    /// </summary>
    public DeviceEvent DetachEvent { get; }

    /// <summary>Kit resources the binding holds: windows, regions, DMA buffers, interrupts, work items, periodic work, events and threads.</summary>
    internal int HeldResourceCount =>
        _memory.Count + _handles.Count + _workItems.Count + _periodic.Count + _events.Count + _threads.Count;

    /// <summary>Devices the binding has published and not yet withdrawn.</summary>
    internal int PublishedDeviceCount => _devices.Count;

    /// <summary>Locks the driver created through <see cref="CreateLock"/>; not resources, not released.</summary>
    internal int LockCount => _locks.Count;

    /// <summary>True when the calling code runs on one of this binding's driver threads.</summary>
    internal bool IsCurrentThreadOwned
    {
        get
        {
            using (_lock.AcquireIrqSafe())
            {
                for (int i = 0; i < _threads.Count; i++)
                {
                    if (_threads[i].IsCurrent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Maps a resource as registers. A memory window is mapped as device
    /// memory; a RAM window is reached through the kernel's own mapping; a
    /// port range becomes a window over <c>in</c> and <c>out</c>.
    /// </summary>
    /// <param name="resourceIndex">Index into <see cref="DeviceNode.Resources"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">No such resource.</exception>
    /// <exception cref="PlatformNotSupportedException">A port range on an architecture without port I/O.</exception>
    /// <exception cref="InvalidOperationException">The slot is unassigned (<see cref="DeviceResource.None"/>), the window overlaps the kernel heap, cannot be mapped, the binding is being torn down, or the caller is an interrupt handler.</exception>
    public RegisterWindow MapRegisters(int resourceIndex)
    {
        ThrowIfNotThreadContext(nameof(MapRegisters));
        DeviceResource resource = ResourceAt(resourceIndex);
        RegisterWindow window;
        switch (resource.Kind)
        {
            case DeviceResourceKind.PortRange:
                ThrowIfNoPortIO();
                window = new RegisterWindow(resource.Base, resource.Length, isPortRange: true);
                break;
            case DeviceResourceKind.RamWindow:
                window = new RegisterWindow(resource.Base, resource.Length, isPortRange: false);
                break;
            default:
                window = new RegisterWindow(MapDeviceMemory(resource), resource.Length, isPortRange: false);
                break;
        }

        Record(_memory, window, nameof(MapRegisters));
        return window;
    }

    /// <summary>
    /// Maps a resource as bulk memory: a framebuffer, a queue, a descriptor
    /// area. A memory window is mapped as device memory whatever
    /// <paramref name="caching"/> asks for, until a platform offers
    /// write-combining; a RAM window keeps the kernel's normal mapping.
    /// </summary>
    /// <param name="resourceIndex">Index into <see cref="DeviceNode.Resources"/>; not a port range.</param>
    /// <param name="caching">The caching the driver wants; recorded on the region.</param>
    /// <exception cref="ArgumentOutOfRangeException">No such resource.</exception>
    /// <exception cref="ArgumentException">The resource is a port range.</exception>
    /// <exception cref="InvalidOperationException">The slot is unassigned (<see cref="DeviceResource.None"/>), the window overlaps the kernel heap, cannot be mapped, the binding is being torn down, or the caller is an interrupt handler.</exception>
    public DeviceRegion MapRegion(int resourceIndex, RegionCaching caching)
    {
        ThrowIfNotThreadContext(nameof(MapRegion));
        DeviceResource resource = ResourceAt(resourceIndex);
        if (resource.Kind == DeviceResourceKind.PortRange)
        {
            throw new ArgumentException("A port range cannot be mapped as memory.", nameof(resourceIndex));
        }

        ulong address = resource.Kind == DeviceResourceKind.RamWindow ? resource.Base : MapDeviceMemory(resource);
        DeviceRegion region = new(address, resource.Length, caching);
        Record(_memory, region, nameof(MapRegion));
        return region;
    }

    /// <summary>
    /// Allocates zeroed, physically contiguous DMA memory the device can
    /// address anywhere in physical memory.
    /// </summary>
    /// <param name="length">Bytes wanted, at least one.</param>
    /// <param name="alignment">Alignment of the first byte, a power of two; less than a page is raised to the page.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not positive or <paramref name="alignment"/> is not a power of two.</exception>
    /// <exception cref="InvalidOperationException">No pages left, the binding is being torn down, or the caller is an interrupt handler.</exception>
    public DmaBuffer AllocateDma(int length, int alignment)
    {
        ThrowIfNotThreadContext(nameof(AllocateDma));
        DmaBuffer buffer = AllocateDmaCore(length, alignment) ?? throw new InvalidOperationException("No pages left for DMA memory.");
        Record(_memory, buffer, nameof(AllocateDma));
        return buffer;
    }

    /// <summary>
    /// <see cref="AllocateDma"/> for a device with addressing limits: fails
    /// instead of throwing when the memory the allocator has does not meet
    /// <paramref name="constraints"/>, so the driver can decline.
    /// </summary>
    /// <param name="length">Bytes wanted, at least one.</param>
    /// <param name="alignment">Alignment of the first byte, a power of two.</param>
    /// <param name="constraints">What the device can address.</param>
    /// <param name="buffer">The buffer, when allocated.</param>
    /// <returns>False when no memory meeting the constraints is available.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is not positive or <paramref name="alignment"/> is not a power of two.</exception>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public bool TryAllocateDma(int length, int alignment, DmaConstraints constraints, [NotNullWhen(true)] out DmaBuffer? buffer)
    {
        ThrowIfNotThreadContext(nameof(TryAllocateDma));
        buffer = AllocateDmaCore(length, alignment);
        if (buffer is null)
        {
            return false;
        }

        if (constraints.Below4GiB && buffer.PhysicalAddress + (ulong)length > FourGiB)
        {
            // No low pool exists yet: the allocator hands out what it has.
            buffer.Release();
            buffer = null;
            return false;
        }

        Record(_memory, buffer, nameof(TryAllocateDma));
        return true;
    }

    /// <summary>
    /// Connects <paramref name="handler"/> to one of the node's interrupt
    /// sources. The handler is live from the moment this returns true, so the
    /// driver arms the device only afterwards.
    /// </summary>
    /// <param name="source">One of <see cref="DeviceNode.Interrupts"/>.</param>
    /// <param name="handler">The handler; interrupt context.</param>
    /// <param name="handle">The connection, for masking and unmasking.</param>
    /// <returns>False when the platform cannot deliver the source; nothing is recorded then.</returns>
    /// <exception cref="ArgumentException"><paramref name="source"/> belongs to another node.</exception>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public bool TryRequestInterrupt(InterruptSource source, InterruptHandler handler, [NotNullWhen(true)] out InterruptHandle? handle)
    {
        ThrowIfNotThreadContext(nameof(TryRequestInterrupt));
        if (!Node.Owns(source))
        {
            throw new ArgumentException("The interrupt source belongs to another node.", nameof(source));
        }

        InterruptHandle created = new(source);
        InterruptContext context = new(created);
        WorkItem faultLog = new(() => DriverLog.HandlerThrew(Node, Driver, Node.LastFault), this);
        InterruptTrampoline trampoline = new(handler, context, created, Node, faultLog);
        created.Trampoline = trampoline;

        // Recorded before connecting, so a teardown that starts in between
        // disconnects a connection it has not seen complete.
        using (_lock.AcquireIrqSafe())
        {
            ThrowIfDetachingLocked(nameof(TryRequestInterrupt));
            _handles.Add(created);
            _kitItems.Add(faultLog);
        }

        if (!source.TryConnect(trampoline))
        {
            using (_lock.AcquireIrqSafe())
            {
                Remove(_handles, created);
                Remove(_kitItems, faultLog);
            }

            handle = null;
            return false;
        }

        handle = created;
        return true;
    }

    /// <summary>Creates an event a handler signals and a thread waits on through <see cref="Wait"/>.</summary>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public DeviceEvent CreateEvent()
    {
        ThrowIfNotThreadContext(nameof(CreateEvent));
        DeviceEvent evt = new();
        Record(_events, evt, nameof(CreateEvent));
        return evt;
    }

    /// <summary>
    /// Creates a lock for a device entered from more than one context at
    /// once; see <see cref="DeviceLock"/>. Recorded for the diagnostics
    /// only: it holds no resource, is not counted among them and needs no
    /// release.
    /// </summary>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public DeviceLock CreateLock()
    {
        ThrowIfNotThreadContext(nameof(CreateLock));
        DeviceLock created = new();
        Record(_locks, created, nameof(CreateLock));
        return created;
    }

    /// <summary>Creates a work item that runs <paramref name="callback"/> on the kit worker when scheduled.</summary>
    /// <param name="callback">The work; thread context.</param>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public WorkItem CreateWorkItem(Action callback)
    {
        ThrowIfNotThreadContext(nameof(CreateWorkItem));
        WorkItem item = new(callback, this);
        Record(_workItems, item, nameof(CreateWorkItem));
        return item;
    }

    /// <summary>
    /// Schedules <paramref name="item"/> every <paramref name="intervalMilliseconds"/>
    /// from the platform timer, until teardown. The interval is rounded to the
    /// timer's tick, which is tens of milliseconds on some platforms.
    /// </summary>
    /// <param name="intervalMilliseconds">The period.</param>
    /// <param name="item">A work item of this binding.</param>
    /// <returns>False when the kernel has no platform timer (<c>CosmosEnableTimer</c> off) or no kit worker (<c>CosmosEnableScheduler</c> off).</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public bool TrySchedulePeriodic(uint intervalMilliseconds, WorkItem item)
    {
        ThrowIfNotThreadContext(nameof(TrySchedulePeriodic));
        if (!DriverEngine.HasWorker)
        {
            return false;
        }

        if (!Core.CosmosFeatures.TimerEnabled)
        {
            return false;
        }

        ITimerDevice? timer = PlatformHAL.Initializer?.CreateTimer();
        if (timer is null)
        {
            return false;
        }

        PeriodicWork periodic = new(timer, item, intervalMilliseconds);
        Record(_periodic, periodic, nameof(TrySchedulePeriodic));
        periodic.Start();
        return true;
    }

    /// <summary>
    /// Starts a thread that runs <paramref name="entry"/>. The body loops on
    /// the binding's events and returns once <see cref="IsDetaching"/> is
    /// true; teardown waits <see cref="JoinTimeoutMilliseconds"/> for it.
    /// </summary>
    /// <param name="name">The thread's name, for the log.</param>
    /// <param name="entry">The thread's body.</param>
    /// <param name="thread">The started thread.</param>
    /// <returns>False when the scheduler is not running (<c>CosmosEnableScheduler</c> off) or the thread did not start.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public bool TryStartThread(string name, Action entry, [NotNullWhen(true)] out DriverThread? thread)
    {
        ThrowIfNotThreadContext(nameof(TryStartThread));

        // Recorded before it starts, so a teardown that begins in between
        // joins it, and the thread is known as this binding's from its first
        // instruction (it retracting its own node is refused on that basis).
        DriverThread created = new(name, entry);
        Record(_threads, created, nameof(TryStartThread));

        if (!KernelThread.TryStart(created.Run, out _))
        {
            using (_lock.AcquireIrqSafe())
            {
                Remove(_threads, created);
            }

            thread = null;
            return false;
        }

        thread = created;
        return true;
    }

    /// <summary>Busy-waits for <paramref name="microseconds"/>, for a register that needs a moment.</summary>
    /// <param name="microseconds">How long.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void Delay(uint microseconds)
    {
        ThrowIfNotThreadContext(nameof(Delay));
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is null)
        {
            KitTime.Delay(microseconds);
            return;
        }

        initializer.DelayMicroseconds(microseconds);
    }

    /// <summary>
    /// Gives up the CPU for <paramref name="milliseconds"/>: a scheduler
    /// sleep on a driver thread or the worker, a busy wait without a scheduler.
    /// </summary>
    /// <param name="milliseconds">How long.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void Sleep(uint milliseconds)
    {
        ThrowIfNotThreadContext(nameof(Sleep));
        KitTime.Sleep(milliseconds);
    }

    /// <summary>Waits for <paramref name="evt"/> to be signaled, up to <paramref name="timeoutMilliseconds"/>.</summary>
    /// <param name="evt">An event of this binding.</param>
    /// <param name="timeoutMilliseconds">Longest time to wait.</param>
    /// <returns>True when a signal was consumed; false on timeout, or at once when the binding is being torn down.</returns>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool Wait(DeviceEvent evt, uint timeoutMilliseconds)
    {
        ThrowIfNotThreadContext(nameof(Wait));
        return evt.Wait(timeoutMilliseconds);
    }

    /// <summary>Writes one line to the kit log: <c>[Drivers] path driver: message</c>.</summary>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void Log(string message)
    {
        ThrowIfNotThreadContext(nameof(Log));
        DriverLog.DriverMessage(Node, Driver, message);
    }

    /// <summary>The resource at <paramref name="resourceIndex"/>, once it is known to exist and to be assigned.</summary>
    private DeviceResource ResourceAt(int resourceIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(resourceIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(resourceIndex, Node.Resources.Count);
        DeviceResource resource = Node.Resources[resourceIndex];
        if (resource.IsNone)
        {
            throw new InvalidOperationException($"resource {resourceIndex} is not assigned");
        }

        return resource;
    }

    private static void ThrowIfNoPortIO()
    {
        if (PlatformHAL.Architecture != Build.API.Enum.PlatformArchitecture.X64)
        {
            throw new PlatformNotSupportedException("This architecture has no port I/O space.");
        }
    }

    /// <summary>
    /// The virtual address of a memory window: the HHDM alias, once the
    /// platform has mapped every block of it. Refuses a window that overlaps
    /// the kernel heap, which on ARM64 would turn heap pages into device
    /// memory.
    /// </summary>
    private static ulong MapDeviceMemory(DeviceResource resource)
    {
        ulong heapPhysical = PageAllocator.VirtualToPhysical((ulong)PageAllocator.RamStart);
        if (resource.PhysicalBase < heapPhysical + PageAllocator.RamSize && resource.PhysicalBase + resource.Length > heapPhysical)
        {
            throw new InvalidOperationException("The window overlaps the kernel heap.");
        }

        if (!DeviceMemory.EnsureWindowMapped(resource.PhysicalBase, resource.Length))
        {
            throw new InvalidOperationException("The window cannot be mapped.");
        }

        return resource.PhysicalBase + DeviceMemory.HhdmOffset();
    }

    private static DmaBuffer? AllocateDmaCore(int length, int alignment)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);
        if ((alignment & (alignment - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "The alignment is a power of two.");
        }

        ulong pageSize = PageAllocator.PageSize;
        ulong wanted = (ulong)length;
        if ((ulong)alignment > pageSize)
        {
            // Pages are page-aligned; a larger alignment needs slack to
            // slide the start up to it.
            wanted += (ulong)alignment - pageSize;
        }

        ulong pageCount = (wanted + pageSize - 1) / pageSize;
        void* pages = PageAllocator.AllocPages(PageType.Unmanaged, pageCount, zero: true);
        if (pages == null)
        {
            return null;
        }

        ulong mask = (ulong)alignment - 1;
        ulong address = ((ulong)pages + mask) & ~mask;
        ulong physical = PageAllocator.VirtualToPhysical(address);
        return new DmaBuffer(address, physical, length, (ulong)pages);
    }

    private void Record<T>(List<T> list, T resource, string member) where T : class
    {
        using (_lock.AcquireIrqSafe())
        {
            if (!_detaching)
            {
                list.Add(resource);
                return;
            }
        }

        // Acquired after teardown began: give it back, then refuse.
        if (resource is IKitResource kitResource)
        {
            kitResource.Release();
        }

        ThrowDetaching(member);
    }

    private static void Remove<T>(List<T> list, T resource) where T : class
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], resource))
            {
                list.RemoveAt(i);
                return;
            }
        }
    }

    private static void ThrowIfNotThreadContext(string member) => InterruptContextGuard.ThrowIfInHandler(member);

    private void ThrowIfDetachingLocked(string member)
    {
        if (_detaching)
        {
            ThrowDetaching(member);
        }
    }

    [DoesNotReturn]
    private static void ThrowDetaching(string member) =>
        throw new InvalidOperationException($"{member} cannot be called once the binding is being torn down.");
}
