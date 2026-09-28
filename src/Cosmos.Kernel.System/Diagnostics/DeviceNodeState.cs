// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Where a device node is in its life, as reported by
/// <see cref="DriverInfo"/>. A node enters the tree pending, is offered to
/// the matching drivers once, ends up bound or unbound, and stays listed
/// as retracted after its bus took it away.
/// </summary>
public enum DeviceNodeState : byte
{
    /// <summary>Published, not yet offered to any driver.</summary>
    Pending,

    /// <summary>A driver took it and its binding is live.</summary>
    Bound,

    /// <summary>Every candidate declined or failed, or there was none; the node stays visible.</summary>
    Unbound,

    /// <summary>The bus took it away; its binding, if any, was torn down.</summary>
    Retracted,
}
