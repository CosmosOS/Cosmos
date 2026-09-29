// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The suite's display driver: binds the <c>display</c> device and publishes
/// a <see cref="DisplayState"/>, a display in one fixed mode with no
/// framebuffer, so the test can watch the ring's display manager take the
/// display in, prefer it over the firmware framebuffer, and drop it when
/// the node is retracted. Everything it holds lives on the state in
/// <see cref="DeviceBinding.DriverState"/>.
/// </summary>
[Driver]
public sealed class DisplayDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "display";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(DisplayDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        DisplayState state = new(binding);
        binding.DriverState = state;
        state.Sink = binding.PublishDisplay(state);
        return ProbeResult.Bound;
    }
}
