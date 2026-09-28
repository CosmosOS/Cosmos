// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// Handed to <see cref="Driver.OnDetach"/>: why the device is going away and
/// whether the hardware is still there to be quiesced.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct DetachReason
{
    internal DetachReason(DetachCause cause, bool hardwarePresent)
    {
        Cause = cause;
        HardwarePresent = hardwarePresent;
    }

    /// <summary>Why the device is being detached.</summary>
    public DetachCause Cause { get; }

    /// <summary>
    /// True when the device is still present and register writes reach it,
    /// so the driver may quiesce it; false when it is gone (hot-unplug) and
    /// touching its registers would fault or reach another device.
    /// </summary>
    public bool HardwarePresent { get; }
}
