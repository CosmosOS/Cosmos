// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Two drivers that match the <c>tie</c> device with the same priority and
/// the same specificity, so only their manifest position separates them.
/// The drivers of this library reach the kernel's manifest as
/// referenced-assembly drivers, which the generator orders by assembly name
/// and then by full type name, both ordinal; the two names are chosen so
/// that <see cref="TieFirstDriver"/> sorts before <see cref="TieSecondDriver"/>
/// (<c>F</c> before <c>S</c>), and that is the order the manifest order
/// test and the tie test assert. Declaration order plays no part for a
/// referenced driver, so the two sharing this file is for reading only.
/// </summary>
public abstract class TieDriver : RecordingDriver
{
    /// <summary>The synthetic key both tie drivers match.</summary>
    public const string Key = "tie";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}

/// <summary>The tie driver that sorts first by type name: the one the kit offers first.</summary>
[Driver]
public sealed class TieFirstDriver : TieDriver
{
    /// <inheritdoc/>
    public override string Name => nameof(TieFirstDriver);
}

/// <summary>The tie driver that sorts second by type name: never probed while the first one binds.</summary>
[Driver]
public sealed class TieSecondDriver : TieDriver
{
    /// <inheritdoc/>
    public override string Name => nameof(TieSecondDriver);
}
