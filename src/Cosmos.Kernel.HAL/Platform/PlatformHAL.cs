// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Power;

namespace Cosmos.Kernel.HAL.Platform;

/// <summary>
/// Platform HAL manager - provides access to platform-specific hardware.
/// </summary>
internal static class PlatformHAL
{
    /// <summary>
    /// Legacy POST diagnostic I/O port (0x80); a read takes ~1 µs on PC chipsets,
    /// used for calibration-free delays by the x64 platform initializer.
    /// </summary>
    public const ushort LegacyPostPort = 0x80;

    private static IPortIO? s_portIO;
    private static string? s_platformName;

    public static IPortIO PortIO => s_portIO!;
    public static ICpuOps? CpuOps { get; private set; }
    public static IPowerOps? PowerOps { get; private set; }
    public static PlatformArchitecture Architecture { get; private set; }
    public static string PlatformName => s_platformName ?? "Unknown";

    /// <summary>
    /// Gets the registered platform initializer, if any.
    /// </summary>
    public static IPlatformInitializer? Initializer { get; private set; }

    /// <summary>
    /// Registers a platform initializer for the library initializers to read at boot.
    /// Called by the HAL.X64 or HAL.ARM64 eager static constructor.
    /// </summary>
    /// <param name="initializer">Platform-specific initializer to register.</param>
    public static void SetInitializer(IPlatformInitializer initializer)
    {
        Initializer = initializer;
    }

    /// <summary>
    /// Initializes the platform HAL using the provided initializer.
    /// </summary>
    /// <param name="initializer">Platform-specific initializer (X64 or ARM64).</param>
    public static void Initialize(IPlatformInitializer initializer)
    {
        Initializer = initializer;
        s_platformName = initializer.PlatformName;
        Architecture = initializer.Architecture;
        s_portIO = initializer.CreatePortIO();
        CpuOps = initializer.CreateCpuOps();
        PowerOps = initializer.CreatePowerOps();
    }
}
