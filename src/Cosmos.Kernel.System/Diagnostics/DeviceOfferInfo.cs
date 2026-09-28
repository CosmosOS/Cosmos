// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// One offer of a device node to a driver and how it ended, produced by
/// <see cref="DriverInfo.TryGetOffer"/>. A node's offers are recorded in
/// the order they were made, so walking them from index 0 replays the
/// arbitration: which drivers were tried, in what order, and why each one
/// passed. Offers are written once, when the node is offered, and never
/// change afterwards.
/// </summary>
public readonly struct DeviceOfferInfo
{
    internal DeviceOfferInfo(string driverName, int priority, int specificity, DeviceOfferOutcome outcome, string? reason, int releasedResourceCount)
    {
        DriverName = driverName;
        Priority = priority;
        Specificity = specificity;
        Outcome = outcome;
        Reason = reason;
        ReleasedResourceCount = releasedResourceCount;
    }

    /// <summary>Name of the driver the node was offered to.</summary>
    public string DriverName { get; }

    /// <summary>The driver's priority at the time of the offer.</summary>
    public int Priority { get; }

    /// <summary>
    /// Specificity of the driver's best matching entry: how many identity
    /// fields it constrained. Among drivers of equal priority, the more
    /// specific match is offered first.
    /// </summary>
    public int Specificity { get; }

    /// <summary>How the offer ended.</summary>
    public DeviceOfferOutcome Outcome { get; }

    /// <summary>
    /// The driver's reason for declining or failing, or the message of the
    /// exception its probe threw; <see langword="null"/> when it bound.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// How many kit resources the probe had acquired when it declined or
    /// failed, all of them released before the next offer; zero when it
    /// bound.
    /// </summary>
    public int ReleasedResourceCount { get; }
}
