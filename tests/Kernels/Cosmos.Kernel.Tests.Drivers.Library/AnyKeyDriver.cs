// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The catch-all: matches every synthetic device at specificity 0 and binds
/// whatever it is offered, acquiring nothing. A keyed driver of the same
/// priority is always offered ahead of it, which the specificity test
/// asserts, and a node no keyed driver takes ends up here.
/// </summary>
[Driver]
public sealed class AnyKeyDriver : RecordingDriver
{
    private readonly DeviceMatch[] _matches = [SyntheticMatch.Any()];

    /// <inheritdoc/>
    public override string Name => nameof(AnyKeyDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
