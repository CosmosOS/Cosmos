// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// How one offer of a device node to a driver ended, as reported by
/// <see cref="DriverInfo.TryGetOffer"/>.
/// </summary>
public enum DeviceOfferOutcome : byte
{
    /// <summary>The driver took the device; its binding stays live.</summary>
    Bound,

    /// <summary>The driver looked and did not want the device; the next candidate was offered.</summary>
    Declined,

    /// <summary>The driver wanted the device but could not bring it up, or its probe threw; the next candidate was offered.</summary>
    Failed,
}
