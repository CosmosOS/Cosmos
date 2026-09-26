// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// Where a device context stands in its binding attempt, which decides the
/// members a driver may call on it.
/// </summary>
internal enum DeviceContextState
{
    /// <summary>Built by the engine; its driver's Probe has not started.</summary>
    Created,

    /// <summary>The driver's Probe is running: the only time resources can be acquired.</summary>
    Probing,

    /// <summary>Probe returned Bound and the engine recorded the binding.</summary>
    Bound,

    /// <summary>The attempt was declined or failed, and the engine released what it acquired.</summary>
    TornDown,

    /// <summary>
    /// The bound USB device left its bus: the engine withdrew what the
    /// driver published, ran its Remove, and the binding is over. Its
    /// transfers answer Disconnected.
    /// </summary>
    Removed
}
