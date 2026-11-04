using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.RISCV64.Bridge;

/// <summary>
/// RISC-V trap entry setup import (writes stvec to install the trap entry
/// defined in Cosmos.Kernel.Native.RISCV64, CPU/Interrupts.s).
/// </summary>
internal static partial class TrapNative
{
    [LibraryImport("*", EntryPoint = "_native_riscv64_init_trap_vector")]
    [SuppressGCTransition]
    public static partial void InitTrapVector();
}
