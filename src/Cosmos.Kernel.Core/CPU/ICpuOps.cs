namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Essential CPU operations for multi-architecture support
/// </summary>
internal interface ICpuOps
{
    /// <summary>
    /// Halt CPU
    /// </summary>
    void Halt();

    /// <summary>
    /// Disable interrupts (x64: CLI, ARM64: MSR DAIF)
    /// </summary>
    void DisableInterrupts();

    /// <summary>
    /// Enable interrupts (x64: STI, ARM64: MSR DAIF)
    /// </summary>
    void EnableInterrupts();
}
