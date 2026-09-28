// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Matches the <c>spec</c> device by key (specificity 1). Same priority as
/// <see cref="AnyKeyDriver"/>, so only the specificity decides the order, and
/// this driver is offered first.
/// </summary>
[Driver]
public sealed class ExactKeyDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "spec";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(ExactKeyDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
