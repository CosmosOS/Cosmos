using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Power;

/// <summary>
/// Platform-specific system power transitions (reboot / shutdown).
/// Implementations route through firmware mechanisms (8042 / ACPI on x64,
/// PSCI on ARM64) rather than per-CPU instructions.
/// </summary>
internal interface IPowerOps
{
    /// <summary>
    /// Restart the machine. Does not return on success.
    /// </summary>
    [DoesNotReturn]
    void Reboot();

    /// <summary>
    /// Power off the machine. Does not return on success.
    /// </summary>
    [DoesNotReturn]
    void Shutdown();
}
