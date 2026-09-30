// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Binds the <c>blocking</c> device and connects a handler that sleeps through
/// the binding, which is forbidden in interrupt context. The kit must turn
/// that into a recorded fault, leave the source masked and keep running.
/// </summary>
[Driver]
public sealed class BlockingHandlerDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "blocking";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(BlockingHandlerDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        BlockingHandlerState state = new(binding);
        binding.DriverState = state;
        if (!binding.TryRequestInterrupt(binding.Node.Interrupts[0], _ => state.OnInterrupt(), out InterruptHandle? handle))
        {
            return ProbeResult.Failed("interrupt 0 could not be connected");
        }

        state.Handle = handle;
        return ProbeResult.Bound;
    }
}
