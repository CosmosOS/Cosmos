// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// One offer of a node to a driver and how it ended, kept on the node for the
/// diagnostics view: which drivers were tried, in what order, and why each
/// one passed.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct DeviceOffer
{
    internal DeviceOffer(string driverName, int priority, int specificity, ProbeOutcome outcome, string? reason, int releasedResourceCount)
    {
        DriverName = driverName;
        Priority = priority;
        Specificity = specificity;
        Outcome = outcome;
        Reason = reason;
        ReleasedResourceCount = releasedResourceCount;
    }

    /// <summary>Name of the driver offered the node.</summary>
    public string DriverName { get; }

    /// <summary>The driver's priority at the time of the offer.</summary>
    public int Priority { get; }

    /// <summary>Specificity of the driver's best matching entry.</summary>
    public int Specificity { get; }

    /// <summary>How the offer ended.</summary>
    public ProbeOutcome Outcome { get; }

    /// <summary>The driver's reason for declining or failing, or the message of the exception its probe threw; null when bound.</summary>
    public string? Reason { get; }

    /// <summary>
    /// How many kit resources the probe had acquired when it declined or
    /// failed, all released by the kit before the next offer; zero when bound.
    /// </summary>
    public int ReleasedResourceCount { get; }
}
