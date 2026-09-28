// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>How an offer of a device to a driver ended.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum ProbeOutcome
{
    /// <summary>The driver took the device; its binding stays live.</summary>
    Bound,

    /// <summary>The driver looked and did not want the device; the next candidate is offered.</summary>
    Declined,

    /// <summary>The driver wanted the device but could not bring it up, or its probe threw; the next candidate is offered.</summary>
    Failed,
}
