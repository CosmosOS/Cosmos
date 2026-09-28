// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A device a bus found, in the kit's tree: its identity, the resources and
/// interrupt sources it exposes, the bus's own access object, and where it
/// is in its life. Every node ever published stays in
/// <see cref="DriverEngine.Nodes"/> with its state, so the diagnostics view
/// can say what happened to it. Lists a reader may see while the worker
/// appends are copy-on-write arrays, so a snapshot is always consistent.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DeviceNode
{
    private readonly DeviceResource[] _resources;
    private readonly InterruptSource[] _interrupts;
    private readonly object? _access;
    private DeviceOffer[] _offers = [];
    private DeviceNode[] _children = [];
    private volatile int _faultCount;
    private string? _lastFault;

    internal DeviceNode(DeviceIdentity identity, DeviceResource[] resources, InterruptSource[] interrupts, object? access, DeviceNode? parent)
    {
        Identity = identity;
        _resources = resources;
        _interrupts = interrupts;
        _access = access;
        Parent = parent;
        Path = $"{identity.BusName}:{identity.Address}";
        Description = identity.Describe();
    }

    /// <summary>The node's name in the log and the diagnostics view: bus name, colon, bus address.</summary>
    public string Path { get; }

    /// <summary>What the bus knows about the device.</summary>
    public DeviceIdentity Identity { get; }

    /// <summary>The identity in words, built once so a diagnostics read allocates nothing.</summary>
    public string Description { get; }

    /// <summary>The windows and port ranges a driver may map, by index.</summary>
    public IReadOnlyList<DeviceResource> Resources => _resources;

    /// <summary>The interrupts a driver may connect, by index.</summary>
    public IReadOnlyList<InterruptSource> Interrupts => _interrupts;

    /// <summary>The node this one hangs off, or null for a root.</summary>
    public DeviceNode? Parent { get; }

    /// <summary>Where the node is in its life. Changed by the worker only.</summary>
    public NodeState State { get; internal set; }

    /// <summary>The live binding while <see cref="State"/> is bound; the last binding after retraction; null otherwise.</summary>
    public DeviceBinding? Binding { get; internal set; }

    /// <summary>Every offer made for the node, in order, with its outcome.</summary>
    public IReadOnlyList<DeviceOffer> Offers => _offers;

    /// <summary>Nodes published beneath this one by its driver, retracted ones included.</summary>
    public IReadOnlyList<DeviceNode> Children => _children;

    /// <summary>How many interrupt handler exceptions the node's driver has had.</summary>
    public int FaultCount => _faultCount;

    /// <summary>The message of the most recent handler exception, or null.</summary>
    public string? LastFault => _lastFault;

    /// <summary>
    /// Resources a retraction could not take back because a driver thread
    /// did not stop: they stay allocated rather than be handed to someone
    /// else while that thread may still touch them.
    /// </summary>
    public int LeakedResourceCount { get; internal set; }

    /// <summary>The bus's access object, whatever its type.</summary>
    internal object? AccessObject => _access;

    /// <summary>
    /// The bus's own allocation for the node (a synthetic node's RAM page),
    /// released by the kit after the node's teardown unless resources leaked.
    /// </summary>
    internal IKitResource? BusResource { get; set; }

    /// <summary>The bus's access object as a <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The access type the bus documents.</typeparam>
    /// <exception cref="InvalidOperationException">The node has no access object of that type.</exception>
    public T Access<T>() where T : class
    {
        if (_access is T access)
        {
            return access;
        }

        throw new InvalidOperationException("The node's access object is not of the requested type.");
    }

    /// <summary>The bus's access object as a <typeparamref name="T"/>, when it is one.</summary>
    /// <typeparam name="T">The access type the bus documents.</typeparam>
    /// <param name="access">The access object, when the node has one of that type.</param>
    public bool TryGetAccess<T>([NotNullWhen(true)] out T? access) where T : class
    {
        access = _access as T;
        return access is not null;
    }

    /// <summary>Records an offer. Worker only.</summary>
    internal void AddOffer(DeviceOffer offer)
    {
        DeviceOffer[] offers = new DeviceOffer[_offers.Length + 1];
        Array.Copy(_offers, offers, _offers.Length);
        offers[_offers.Length] = offer;
        _offers = offers;
    }

    /// <summary>Records a child. Thread context, under the parent binding's lock.</summary>
    internal void AddChild(DeviceNode child)
    {
        DeviceNode[] children = new DeviceNode[_children.Length + 1];
        Array.Copy(_children, children, _children.Length);
        children[_children.Length] = child;
        _children = children;
    }

    /// <summary>Records a handler exception. Interrupt context: keeps the message reference, formats nothing.</summary>
    internal void RecordFault(string message)
    {
        _lastFault = message;
        _faultCount++;
    }

    /// <summary>True when <paramref name="source"/> is one of this node's interrupt sources.</summary>
    internal bool Owns(InterruptSource source)
    {
        for (int i = 0; i < _interrupts.Length; i++)
        {
            if (ReferenceEquals(_interrupts[i], source))
            {
                return true;
            }
        }

        return false;
    }
}
