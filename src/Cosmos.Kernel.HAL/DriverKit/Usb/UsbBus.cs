// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// One USB bus: a bound host controller, its 1-based ordinal, the device
/// tree below it and the enumeration core's entry points for root ports.
/// The host driver creates it from its probe and calls
/// <see cref="Attach"/> and <see cref="Detach"/> for root ports from its
/// probe and its hot-plug thread; the hub driver reaches the same core for
/// hub ports through <see cref="UsbAccess.AttachChild"/> and
/// <see cref="UsbAccess.DetachChild"/>. The tree is guarded by one
/// IRQ-safe lock around list changes only, never across a transfer or a
/// kit call: the host thread and a hub thread change it concurrently. The
/// execution context of each member is in its summary.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class UsbBus
{
    private static int s_nextOrdinal;

    private SchedSpinLock _lock;
    private readonly List<UsbDeviceState> _roots = [];
    private int _deviceCount;

    /// <summary>
    /// Creates the bus of a host controller and takes the next ordinal: the
    /// first controller bound is 1, a second xHCI function 2; a controller
    /// unbound and rebound takes a new number. Thread context, the host
    /// driver's probe.
    /// </summary>
    /// <param name="binding">The host driver's binding, the owner of the root ports' interface nodes.</param>
    /// <param name="host">The host controller contract.</param>
    public UsbBus(DeviceBinding binding, UsbHostController host)
    {
        ArgumentNullException.ThrowIfNull(binding);
        ArgumentNullException.ThrowIfNull(host);
        Binding = binding;
        Host = host;
        Ordinal = Interlocked.Increment(ref s_nextOrdinal);
    }

    /// <summary>The controller's 1-based ordinal, the first part of every port path below it.</summary>
    public int Ordinal { get; }

    /// <summary>The host driver's binding: the owner of the nodes published for root ports.</summary>
    public DeviceBinding Binding { get; }

    /// <summary>The host controller.</summary>
    public UsbHostController Host { get; }

    /// <summary>Devices attached, hubs included. Any context.</summary>
    public int DeviceCount => Volatile.Read(ref _deviceCount);

    /// <summary>
    /// Attaches the device on root port <paramref name="port"/>: addresses
    /// it through the host, reads its descriptors, selects its first
    /// configuration and publishes one node per interface beneath the
    /// host's node. Thread context, the host's probe or hot-plug thread.
    /// </summary>
    /// <param name="port">The 1-based root port number.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>False when a device is already recorded on that port (the caller's bug, logged <c>usb {path}: port already attached</c>), when enumeration failed, or when the host's binding began detaching before the nodes were published; logged either way.</returns>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool Attach(byte port, UsbSpeed speed)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(Attach));
        return UsbEnumeration.Attach(this, Binding, null, port, speed);
    }

    /// <summary>
    /// Detaches the device on root port <paramref name="port"/>: marks it
    /// and its subtree disconnected, retracts every interface node and
    /// releases the devices through the host, deepest first. Thread
    /// context, the host's hot-plug thread; nothing on that port returns at
    /// once. When the host's binding is itself being torn down the detach
    /// stops once its teardown has the nodes, and <see cref="ReleaseAll"/>
    /// from the host's <see cref="Driver.OnDetach"/> releases the device.
    /// </summary>
    /// <param name="port">The 1-based root port number.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void Detach(byte port)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(Detach));
        UsbDeviceState? state = Find(null, port);
        if (state is null)
        {
            return;
        }

        UsbEnumeration.Detach(this, Binding, state);
    }

    /// <summary>
    /// Marks every remaining device disconnected and releases each through
    /// the host, deepest first. Thread context, the host's
    /// <see cref="Driver.OnDetach"/>: the interface nodes were torn down by
    /// the kit's child step before it ran, so this frees the slots they
    /// left.
    /// </summary>
    /// <param name="hostPresent">Whether the controller is still there to be told.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void ReleaseAll(bool hostPresent)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(ReleaseAll));
        UsbDeviceState[] roots;
        using (_lock.AcquireIrqSafe())
        {
            roots = _roots.ToArray();
        }

        for (int i = 0; i < roots.Length; i++)
        {
            UsbEnumeration.Release(this, Binding, roots[i], hostPresent);
        }
    }

    /// <summary>The device on <paramref name="port"/> of <paramref name="parent"/> (null: a root port), or null. Takes the bus lock.</summary>
    internal UsbDeviceState? Find(UsbDeviceState? parent, byte port)
    {
        using (_lock.AcquireIrqSafe())
        {
            List<UsbDeviceState> siblings = parent is null ? _roots : parent.Children;
            for (int i = 0; i < siblings.Count; i++)
            {
                if (siblings[i].Port == port)
                {
                    return siblings[i];
                }
            }
        }

        return null;
    }

    /// <summary>Puts a state under its parent, or among the roots, and counts it. Takes the bus lock.</summary>
    internal void Link(UsbDeviceState state)
    {
        using (_lock.AcquireIrqSafe())
        {
            List<UsbDeviceState> siblings = state.Parent is null ? _roots : state.Parent.Children;
            siblings.Add(state);
            _deviceCount++;
        }
    }

    /// <summary>Takes a state and its subtree out of the tree and off the count. Takes the bus lock.</summary>
    internal void Unlink(UsbDeviceState state)
    {
        using (_lock.AcquireIrqSafe())
        {
            List<UsbDeviceState> siblings = state.Parent is null ? _roots : state.Parent.Children;
            for (int i = 0; i < siblings.Count; i++)
            {
                if (ReferenceEquals(siblings[i], state))
                {
                    siblings.RemoveAt(i);
                    _deviceCount -= CountSubtree(state);
                    return;
                }
            }
        }
    }

    /// <summary>A snapshot of the devices attached below <paramref name="state"/>. Takes the bus lock.</summary>
    internal UsbDeviceState[] ChildrenOf(UsbDeviceState state)
    {
        using (_lock.AcquireIrqSafe())
        {
            return state.Children.ToArray();
        }
    }

    /// <summary>The state and every device below it. Under the bus lock.</summary>
    private static int CountSubtree(UsbDeviceState state)
    {
        int count = 1;
        for (int i = 0; i < state.Children.Count; i++)
        {
            count += CountSubtree(state.Children[i]);
        }

        return count;
    }
}
