// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>Where a published device came from.</summary>
internal enum DeviceProvenance
{
    /// <summary>A driver bound through the kit published it.</summary>
    Driver,

    /// <summary>The firmware handed it over (a boot framebuffer); no binding stands behind it.</summary>
    Firmware,
}
