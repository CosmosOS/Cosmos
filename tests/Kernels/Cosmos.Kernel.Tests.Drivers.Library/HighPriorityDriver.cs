// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Matches the <c>prio</c> device with priority 10, so the kit offers it the
/// node ahead of <see cref="LowPriorityDriver"/>, which matches the same key
/// with the same specificity at priority 0.
/// </summary>
[Driver]
public sealed class HighPriorityDriver : RecordingDriver
{
    /// <summary>The synthetic key both priority drivers match.</summary>
    public const string Key = "prio";

    /// <summary>The priority the driver claims; anything above the default wins the tie on specificity.</summary>
    public const int ClaimedPriority = 10;

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(HighPriorityDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    public override int Priority => ClaimedPriority;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
