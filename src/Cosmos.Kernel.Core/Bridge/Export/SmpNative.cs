// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// The managed entry of an application processor, what the native trampoline
/// (<c>CPU/Smp.s</c>) calls once the CPU has its per-CPU register, SIMD and, on ARM64, its
/// vector table and SP_EL1.
/// </summary>
internal static unsafe class SmpNative
{
    /// <summary>
    /// Runs on the application processor, on the stack the bootloader gave it, with interrupts
    /// masked. Never returns: the CPU ends in <see cref="Smp.ApMain"/>'s halt loop.
    /// </summary>
    /// <param name="mpInfo">The CPU's <c>limine_mp_info</c>, unused: the block the trampoline loaded carries the index.</param>
    /// <param name="bootStackTop">The top of this CPU's boot stack, as the trampoline aligned it.</param>
    [UnmanagedCallersOnly(EntryPoint = "__managed__ap_main")]
    public static void ApMain(void* mpInfo, nuint bootStackTop)
    {
        Smp.ApMain(bootStackTop);
    }
}
