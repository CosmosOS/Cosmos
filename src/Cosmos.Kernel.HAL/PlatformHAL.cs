// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.HAL.Interfaces;

namespace Cosmos.Kernel.HAL;

/// <summary>
/// Platform HAL manager - provides access to platform-specific hardware.
/// </summary>
internal static class PlatformHAL
{
    /// <summary>
    /// Legacy POST diagnostic I/O port (0x80); a read takes ~1 µs on PC chipsets,
    /// used for calibration-free delays (shared by the AHCI wait loop and the x64
    /// platform initializer).
    /// </summary>
    public const ushort LegacyPostPort = 0x80;

    private static IPortIO? s_portIO;
    private static ICpuOps? s_cpuOps;
    private static IPowerOps? s_powerOps;
    private static PlatformArchitecture s_architecture;
    private static string? s_platformName;
    private static IPlatformInitializer? s_initializer;

    public static IPortIO PortIO => s_portIO!;
    public static ICpuOps? CpuOps => s_cpuOps;
    public static IPowerOps? PowerOps => s_powerOps;
    public static PlatformArchitecture Architecture => s_architecture;
    public static string PlatformName => s_platformName ?? "Unknown";

    /// <summary>
    /// Gets the registered platform initializer, if any.
    /// </summary>
    public static IPlatformInitializer? Initializer => s_initializer;

    /// <summary>
    /// Registers a platform initializer for later use by Kernel.Initialize().
    /// Called by HAL.X64 or HAL.ARM64 module initializers.
    /// </summary>
    /// <param name="initializer">Platform-specific initializer to register.</param>
    public static void SetInitializer(IPlatformInitializer initializer)
    {
        s_initializer = initializer;
    }

    /// <summary>
    /// Initializes the platform HAL using the provided initializer.
    /// </summary>
    /// <param name="initializer">Platform-specific initializer (X64 or ARM64).</param>
    public static void Initialize(IPlatformInitializer initializer)
    {
        s_initializer = initializer;
        s_platformName = initializer.PlatformName;
        s_architecture = initializer.Architecture;
        s_portIO = initializer.CreatePortIO();
        s_cpuOps = initializer.CreateCpuOps();
        s_powerOps = initializer.CreatePowerOps();
    }
}
