// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// The managed entry of the boot CPU, what the C bootstrap (<c>kmain.c</c>)
/// calls once the managed world is up, where it used to call the kernel's
/// <c>Main</c> itself.
/// </summary>
internal static unsafe class MainNative
{
    /// <summary>
    /// Runs on the boot CPU, on the boot stack, with interrupts masked.
    /// Never returns: the boot CPU ends in <see cref="KernelMain.Run"/>'s
    /// halt loop, with <c>Main</c> on a thread of its own, or runs
    /// <c>Main</c> itself on a kernel without the scheduler.
    /// </summary>
    /// <param name="argc">The number of arguments on the kernel command line.</param>
    /// <param name="argv">The arguments, as C strings.</param>
    [UnmanagedCallersOnly(EntryPoint = "__managed__boot_main")]
    public static void BootMain(int argc, byte** argv)
    {
        KernelMain.Run(argc, argv);
    }
}
