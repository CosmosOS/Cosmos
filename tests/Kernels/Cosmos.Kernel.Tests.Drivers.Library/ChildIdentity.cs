// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The identity of a device <see cref="BusDriver"/> finds on its own bus: a
/// name on the <c>synthetic-child</c> bus. Not a synthetic identity, so no
/// synthetic match sees it, and a child no <see cref="ChildMatch"/> names
/// has no candidate at all.
/// </summary>
public sealed class ChildIdentity : DeviceIdentity
{
    /// <summary>The bus name every child identity reports.</summary>
    public const string Bus = "synthetic-child";

    internal ChildIdentity(string name)
    {
        Name = name;
    }

    /// <summary>The child's name on the bus.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override string BusName => Bus;

    /// <inheritdoc/>
    public override string Address => Name;

    /// <inheritdoc/>
    public override string Describe() => string.Concat("child ", Name);
}
