// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Matches the <c>prio</c> device at the default priority. Never probed while
/// <see cref="HighPriorityDriver"/> binds the node first, which is what the
/// priority test asserts.
/// </summary>
[Driver]
public sealed class LowPriorityDriver : RecordingDriver
{
    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(HighPriorityDriver.Key)];

    /// <inheritdoc/>
    public override string Name => nameof(LowPriorityDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
