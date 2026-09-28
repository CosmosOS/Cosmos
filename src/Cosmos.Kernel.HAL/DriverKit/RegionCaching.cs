// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// How the CPU caches a bulk device region: the attribute the architecture applies
/// to its accesses to it.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum RegionCaching
{
    /// <summary>
    /// Uncached device memory, for command FIFOs and register-like regions:
    /// every access reaches the device, in program order.
    /// </summary>
    Device,

    /// <summary>
    /// Write-combining, for framebuffers: stores are gathered into bursts.
    /// Offered where the architecture has it; otherwise mapped as
    /// <see cref="Device"/> memory.
    /// </summary>
    WriteCombining,

    /// <summary>
    /// Normal cacheable memory, for coherent RAM the device and the CPU share.
    /// </summary>
    Normal,
}
