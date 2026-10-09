// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Binds the child <see cref="BusDriver"/> publishes. Its detach reason is
/// what proves a parent's retraction reaches its children with
/// <see cref="DetachCause.ParentRetracted"/>.
/// </summary>
[Driver]
public sealed class ChildDriver : RecordingDriver
{
    private readonly DeviceMatch[] _matches = [new ChildMatch(BusDriver.ChildName)];

    /// <inheritdoc/>
    public override string Name => nameof(ChildDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}
