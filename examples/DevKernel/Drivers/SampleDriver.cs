// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace DevKernel.Drivers;

/// <summary>
/// The smallest driver a kernel can carry, written over the public driver
/// kit seam exactly as a kernel author's would be. DevKernel holds no
/// <c>InternalsVisibleTo</c> grant, so this class compiling is the proof
/// that the seam is enough to write one. It matches the synthetic device
/// published under <see cref="Key"/>, maps nothing, writes one kit log line
/// when it binds and one when the device goes away. The generated manifest
/// registers it because it carries <see cref="DriverAttribute"/>; the
/// <c>drivers</c> shell command lists it, and <c>sampledev publish</c>
/// publishes the device it binds to.
/// </summary>
[Driver]
public sealed class SampleDriver : Driver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "devkernel-sample";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    /// <inheritdoc/>
    public override string Name => nameof(SampleDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <inheritdoc/>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        binding.Log("bound");
        return ProbeResult.Bound;
    }

    /// <inheritdoc/>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        string cause = reason.Cause == DetachCause.ParentRetracted ? "parent retracted" : "retracted";
        string hardware = reason.HardwarePresent ? "hardware still present" : "hardware gone";
        binding.Log("detached: " + cause + ", " + hardware);
    }
}
