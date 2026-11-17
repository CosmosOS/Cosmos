// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Runs the kernel's <c>Main</c> on a thread of its own (SMP phase 5b,
/// CosmosOS/Cosmos#3344) and leaves the boot context to what it is from then
/// on: the boot CPU's idle thread, a halt loop like the one every application
/// processor ends in (<see cref="Smp.ApMain"/>).
///
/// <para><c>Main</c> used to run on the boot context, so CPU 0's idle thread
/// took mutexes, waited and slept, and the blocking primitives each carried a
/// path for a caller the scheduler could not park. The main thread is a
/// thread like any other: it parks, it is queued, and it is placed on the
/// CPUs of <see cref="SchedulerManager.DefaultAffinity"/>, the compute CPUs
/// unless a kernel says otherwise, so on a machine with several CPUs the main
/// flow leaves CPU 0 to the interrupts and the kernel's own threads.</para>
///
/// <para>A kernel without the scheduler has one flow, and <c>Main</c> runs
/// on the boot context as it always did. So does a kernel whose main thread
/// cannot start, for want of a stack or because the scheduler never gives it
/// the CPU (a timer that never ticks, or ticks without switching), which
/// <see cref="KernelThread"/> reports: the scheduler is stopped, and the
/// kernel goes on as one without it.</para>
/// </summary>
internal static unsafe class KernelMain
{
    /// <summary>
    /// The main thread's stack, in bytes: the size the kernel asks the
    /// bootloader for the boot stack (1 MiB), which is what <c>Main</c> had
    /// while it ran there, four times a thread's default.
    /// </summary>
    internal const nuint StackSize = (nuint)LimineStackSizeRequest.DefaultRequestedSize;

    // The command line the native boot code split, kept for the main thread:
    // its body takes no argument.
    private static int s_argc;
    private static byte** s_argv;

    /// <summary>
    /// What the boot CPU runs once the managed world is up, on the boot
    /// stack, with interrupts masked: starts the main thread and becomes the
    /// boot CPU's idle loop. Never returns.
    /// </summary>
    /// <param name="argc">The number of arguments on the kernel command line.</param>
    /// <param name="argv">The arguments, as C strings.</param>
    [DoesNotReturn]
    internal static void Run(int argc, byte** argv)
    {
        s_argc = argc;
        s_argv = argv;

        if (CosmosFeatures.SchedulerEnabled && SchedulerManager.IsRunning)
        {
            // A thread reaches a CPU through an interrupt exit: on one CPU
            // this one's tick, on several the reschedule interrupt the wake
            // sends to the compute CPU the thread was placed on, and the
            // kernel's own threads, which the main thread starts next, need
            // CPU 0's. So the boot CPU takes interrupts from here on, as
            // every application processor does from its entry.
            InternalCpu.EnableInterrupts();

            if (KernelThread.TryStart(MainBody, SchedulerManager.DefaultAffinity, StackSize, out _))
            {
                Idle();
            }

            // The thread never began, so nothing runs but this context.
            // Stopped, the scheduler leaves the kernel one flow: the ticks
            // return early, no thread is created, and the primitives take
            // the path they have for a kernel without a scheduler. Masked
            // again, since that kernel's Main enables interrupts itself.
            InternalCpu.DisableInterrupts();
            SchedulerManager.IsRunning = false;
            Serial.WriteString("[KERNEL] The main thread did not start: no memory for its stack, or a scheduler that never switched to it.\n");
            Serial.WriteString("[KERNEL] Scheduler stopped, Main runs on the boot context.\n");
        }

        BootNative.ManagedMain(argc, argv);

        Serial.WriteString("[KERNEL] ERROR: Main() returned unexpectedly!\n");
        Idle();
    }

    /// <summary>
    /// The main thread's body: ties the thread to its managed
    /// <see cref="System.Threading.Thread"/> and names it the main thread,
    /// then runs <c>Main</c>. A <c>Main</c> that returns ends the thread, and
    /// the kernel idles.
    /// </summary>
    private static void MainBody()
    {
        SchedulerThread? self = SchedulerManager.CurrentThread;
        if (self is not null)
        {
            SchedulerManager.RegisterMainThread(self, SysThread.CurrentThread);
        }

        BootNative.ManagedMain(s_argc, s_argv);

        Serial.WriteString("[KERNEL] Main() returned: the main thread exits\n");
    }

    /// <summary>
    /// The idle loop of the boot CPU, on the boot stack: the halt every
    /// interrupt ends, for ever. A halt loop that never returns, which is
    /// why the loop has no bound.
    /// </summary>
    [DoesNotReturn]
    private static void Idle()
    {
        while (true)
        {
            InternalCpu.Halt();
        }
    }
}
