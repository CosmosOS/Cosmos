// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// A bus driver: binds the <c>bus</c> device and publishes two children
/// beneath it from its probe, one <see cref="ChildDriver"/> takes and one
/// nobody matches. Retracting the bus node must retract both with it.
/// </summary>
[Driver]
public sealed class BusDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "bus";

    /// <summary>Name of the child <see cref="ChildDriver"/> matches.</summary>
    public const string ChildName = "child";

    /// <summary>Name of the child no driver matches.</summary>
    public const string OrphanName = "orphan";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(BusDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        DeviceNode child = binding.PublishChild(new ChildIdentity(ChildName), [], [], null);
        DeviceNode orphan = binding.PublishChild(new ChildIdentity(OrphanName), [], [], null);
        binding.DriverState = new BusState(child, orphan);
        return ProbeResult.Bound;
    }
}
