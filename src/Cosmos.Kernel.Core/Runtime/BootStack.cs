using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Bounds of the bootloader-provided stack that the boot code and then the idle thread of the
/// running CPU run on; the kernel's <c>Main</c> runs on a thread with a stack of its own, unless
/// the kernel has no running scheduler, where it stays on this one. The top is what <c>kmain</c>
/// captured at its entry, carried by the CPU's per-CPU block since its installation (an
/// application processor records its own, SMP phase 2); the size is the Limine stack-size
/// request when honored, else the 64 KiB protocol default, the same for every CPU since Limine
/// allocates each application processor's stack with the size it gave the boot CPU.
/// </summary>
internal static unsafe class BootStack
{
    /// <summary>Stack size Limine guarantees when no stack-size request is honored.</summary>
    private const nuint LimineDefaultStackSize = 64 * 1024;

    /// <summary>Highest address of the running CPU's boot stack (exclusive).</summary>
    public static nuint Top => PerCpuBlock.Current.BootStackTop;

    /// <summary>Boot stack size in bytes.</summary>
    public static nuint Size => Limine.StackSize.Response != null
        ? (nuint)Limine.StackSize.StackSize
        : LimineDefaultStackSize;
}
