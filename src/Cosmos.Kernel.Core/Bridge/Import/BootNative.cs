// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native imports for boot-time facts recorded by the C bootstrap (kmain.c),
/// and the entry point the compiler generates for the kernel.
/// </summary>
internal static unsafe partial class BootNative
{
    /// <summary>
    /// Top of the bootloader-provided stack, captured at kmain entry before any
    /// managed code runs. Every managed frame of the boot code, and then of
    /// the boot CPU's idle thread, lies below it.
    /// </summary>
    [LibraryImport("*", EntryPoint = "__cosmos_get_boot_stack_top")]
    [SuppressGCTransition]
    public static partial nuint GetBootStackTop();

    /// <summary>
    /// The kernel's entry point as ILC generates it: records the command line
    /// for <c>Environment.GetCommandLineArgs</c>, then calls the kernel's
    /// <c>Main</c>. The library initializers ran before, from
    /// <c>__managed__Startup</c>. Called once, by <c>KernelMain</c>, from the
    /// thread <c>Main</c> runs on.
    /// </summary>
    /// <param name="argc">The number of arguments on the kernel command line.</param>
    /// <param name="argv">The arguments, as C strings.</param>
    /// <returns><c>Main</c>'s exit code, for a <c>Main</c> that returns.</returns>
    [LibraryImport("*", EntryPoint = "__managed__Main")]
    public static partial int ManagedMain(int argc, byte** argv);
}
