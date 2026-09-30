// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The suite's block driver: binds the <c>block</c> device and publishes a
/// <see cref="BlockState"/>, a small blank disk held in memory, so the test
/// can watch the ring's storage manager take the disk in, scan it, make it
/// the primary device and drop it when the node is retracted. Everything it
/// holds lives on the state in <see cref="DeviceBinding.DriverState"/>.
/// </summary>
[Driver]
public sealed class BlockDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "block";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(BlockDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        BlockState state = new(binding);
        binding.DriverState = state;
        binding.PublishBlockDevice(state);
        return ProbeResult.Bound;
    }
}
