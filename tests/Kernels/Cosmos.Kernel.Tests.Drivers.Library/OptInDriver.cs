// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Not registered by default; the project takes it in with a
/// <c>CosmosDriverInclude</c> item, so the manifest must carry it.
/// </summary>
[Driver(Default = false)]
public sealed class OptInDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "optin";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(OptInDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
