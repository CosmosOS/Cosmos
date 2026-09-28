// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Drivers;

/// <summary>
/// Two drivers that match the <c>tie</c> device with the same priority and
/// the same specificity, so only their manifest position separates them.
/// The manifest orders a kernel's drivers by source file and then by
/// position in the file, so the two are declared here, in one file, first
/// before second: that is the order the tie test asserts, and it does not
/// depend on how the compiler orders files.
/// </summary>
internal abstract class TieDriver : RecordingDriver
{
    /// <summary>The synthetic key both tie drivers match.</summary>
    public const string Key = "tie";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding) => ProbeResult.Bound;
}

/// <summary>The tie driver declared first: the one the kit offers first.</summary>
[Driver]
internal sealed class TieFirstDriver : TieDriver
{
    /// <inheritdoc/>
    public override string Name => nameof(TieFirstDriver);
}

/// <summary>The tie driver declared second: never probed while the first one binds.</summary>
[Driver]
internal sealed class TieSecondDriver : TieDriver
{
    /// <inheritdoc/>
    public override string Name => nameof(TieSecondDriver);
}
