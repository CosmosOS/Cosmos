// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// What <see cref="Driver.Probe"/> returns. <see cref="Bound"/> keeps the
/// binding and everything acquired through it; <see cref="Declined"/> and
/// <see cref="Failed"/> make the kit release the binding's ledger in reverse
/// order and record the reason with the offer.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct ProbeResult
{
    private ProbeResult(ProbeOutcome outcome, string? reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    /// <summary>How the probe ended.</summary>
    public ProbeOutcome Outcome { get; }

    /// <summary>Why the driver declined or failed; null for a bound result.</summary>
    public string? Reason { get; }

    /// <summary>The driver took the device.</summary>
    public static ProbeResult Bound => new(ProbeOutcome.Bound, null);

    /// <summary>The driver does not want the device.</summary>
    /// <param name="reason">A short reason for the log and the diagnostics view.</param>
    public static ProbeResult Declined(string reason) => new(ProbeOutcome.Declined, reason);

    /// <summary>The driver wanted the device but could not bring it up.</summary>
    /// <param name="reason">A short reason for the log and the diagnostics view.</param>
    public static ProbeResult Failed(string reason) => new(ProbeOutcome.Failed, reason);
}
