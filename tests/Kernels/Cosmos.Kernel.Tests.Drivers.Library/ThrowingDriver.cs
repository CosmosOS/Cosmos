// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Throws from its probe. The kit must record the offer as failed with the
/// exception's message and go on to the next candidate.
/// </summary>
[Driver]
public sealed class ThrowingDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "throw";

    /// <summary>The message of the exception the probe throws.</summary>
    public const string Message = "probe threw on purpose";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(ThrowingDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => throw new InvalidOperationException(Message);
}
