// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// What a driver's Probe reports about the device it was offered.
/// </summary>
internal enum ProbeResult
{
    /// <summary>
    /// The driver took the device. The kit keeps the context, and everything
    /// acquired through it, for as long as the device is present.
    /// </summary>
    Bound,

    /// <summary>
    /// The device is not one this driver handles after all, such as a
    /// revision it does not support. The kit releases whatever the probe
    /// acquired and offers the device to the next candidate.
    /// </summary>
    Declined,

    /// <summary>
    /// The driver handles the device but could not bring it up. The kit
    /// releases whatever the probe acquired, logs the failure, and offers the
    /// device to the next candidate. An exception thrown from the factory or
    /// from Probe counts as this result.
    /// </summary>
    Failed
}
