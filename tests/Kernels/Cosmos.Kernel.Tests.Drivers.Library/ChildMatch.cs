// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>A match over <see cref="ChildIdentity"/>: one child name, specificity 1.</summary>
public sealed class ChildMatch : DeviceMatch
{
    private readonly string _name;

    internal ChildMatch(string name)
    {
        _name = name;
    }

    /// <inheritdoc/>
    public override int Specificity => 1;

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) =>
        identity is ChildIdentity child && string.Equals(_name, child.Name);
}
