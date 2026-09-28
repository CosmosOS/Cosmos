// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Acquires a register window, a DMA buffer, an event and a work item, then
/// declines. The kit must release all four before the next offer, record how
/// many it released with the offer, and leave the total of held resources
/// where it was.
/// </summary>
[Driver]
public sealed class DecliningDriver : RecordingDriver
{
    /// <summary>The synthetic key this driver matches.</summary>
    public const string Key = "decline";

    /// <summary>The reason the driver declines with.</summary>
    public const string Reason = "declined on purpose";

    /// <summary>How many kit resources the probe acquires before declining.</summary>
    public const int AcquiredResourceCount = 4;

    /// <summary>Bytes of the register window the node must carry.</summary>
    public const int WindowBytes = 16;

    private const int DmaBytes = 64;
    private const int DmaAlignment = 16;

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];
    private volatile int _workItemRuns;

    /// <inheritdoc/>
    public override string Name => nameof(DecliningDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>How many times the work item ran; stays zero, since the kit cancels it on the decline.</summary>
    public int WorkItemRuns => _workItemRuns;

    /// <inheritdoc/>
    protected override ProbeResult ProbeCore(DeviceBinding binding)
    {
        binding.MapRegisters(0);
        binding.AllocateDma(DmaBytes, DmaAlignment);
        binding.CreateEvent();

        // Queued behind this very probe: the unwind must take it out of the
        // queue again, or it would run for a driver that declined.
        WorkItem item = binding.CreateWorkItem(OnWorkItem);
        item.Schedule();
        return ProbeResult.Declined(Reason);
    }

    private void OnWorkItem() => _workItemRuns++;
}
