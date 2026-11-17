using Cosmos.Kernel;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.Core.Scheduler.Stride;
using Cosmos.Kernel.HAL.Boot;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// This class is responsible for initializing the library and its dependencies. It is called by the runtime before any managed code is executed.
/// </summary>
internal class LibraryInitializer
{
    /// <summary>
    /// Miscellaneous initialization of core kernel services that depend on HAL, such as interrupts, exception handlers, and scheduler. This method is called by the runtime before any managed code is executed.
    /// </summary>
    public static void InitializeLibrary()
    {
        // Get the platform initializer (registered by HAL.X64 or HAL.ARM64 module initializer)
        IPlatformInitializer? initializer = PlatformHAL.Initializer;

        if (initializer is null)
        {
            // Qualified: this assembly has a Panic of its own, for CPU
            // exceptions, and it has no Halt.
            Cosmos.Kernel.Core.Panic.Halt("No platform initializer registered.");
        }

        // Initialize exception handlers (must be after InterruptManager)
        if (InterruptManager.IsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Initializing exception handlers...\n");
            ExceptionHandler.Initialize();
        }

        // Initialize Scheduler
        if (SchedulerManager.IsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Initializing scheduler...\n");
            InitializeScheduler(initializer.GetCpuCount());
        }

        // Start scheduler timer for preemptive scheduling (after all init is complete)
        if (SchedulerManager.IsEnabled)
        {
            Serial.WriteString("[KERNEL]   - Starting scheduler timer...\n");
            // Arm the timer at the reference quantum rather than a literal:
            // Stride advances the running thread's pass by one stride per
            // DefaultQuantumNs, charged a tick at a time, so the two have to
            // agree.
            initializer.StartSchedulerTimer(
                (uint)(SchedulerManager.DefaultQuantumNs / SchedulerManager.NanosecondsPerMillisecond));

            // The application processors, last: each needs its idle thread
            // (set up above), the shared interrupt tables and the timer's
            // calibration, and comes online with interrupts enabled, halted
            // on its own tick. Before the drivers so a driver that binds a
            // line to a CPU (phase 3) finds every CPU up.
            if (InterruptManager.IsEnabled)
            {
                Serial.WriteString("[KERNEL]   - Starting application processors...\n");
                Smp.StartApplicationProcessors(initializer.StartSchedulerTimerOnCurrentCpu);
            }
        }
    }

    /// <summary>
    /// Initializes the scheduler subsystem: one idle thread per CPU, then the policy.
    /// </summary>
    private static void InitializeScheduler(uint cpuCount)
    {
        Serial.WriteString("[SCHED] Detected ");
        Serial.WriteNumber(cpuCount);
        Serial.WriteString(" CPU(s)\n");

        SchedulerManager.Initialize(cpuCount);

        // The idle thread of each CPU is the code running right now, so it
        // gets no stack of its own: when the kernel is preempted, the IRQ stub
        // saves the context to the current stack and keeps that RSP in
        // StackPointer. It becomes the CPU's current thread before any policy
        // is installed. Thread statics live on the current thread, and a
        // policy hook may read a static whose class constructor has not run
        // yet; the class constructor runner's lock identifies its holder by a
        // thread static, so it needs a current thread to exist. On the boot
        // CPU the idle thread runs the rest of the library initializers, then
        // starts Main on a thread of its own and ends in a halt loop
        // (KernelMain.Run). On an application processor it is the halt loop
        // the managed entry ends in, on the boot stack the loader gave it;
        // the CPU's block points at the thread's ExInfo chain head before the
        // CPU is released (Smp.StartApplicationProcessors).
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            SchedulerThread idleThread = new()
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = cpu,
                State = SchedulerThreadState.Running,
                Flags = SchedulerThreadFlags.IdleThread,
                // An idle thread is its CPU's: the set that never lets it move.
                Affinity = CpuSet.Single(cpu),
                IdealCpu = cpu
            };

            SchedulerManager.SetupIdleThread(cpu, idleThread);

            Serial.WriteString("[SCHED] Idle thread ");
            Serial.WriteNumber(idleThread.Id);
            Serial.WriteString(" for CPU ");
            Serial.WriteNumber(cpu);
            Serial.WriteString("\n");
        }

        // No thread but the idle ones exists yet, and a policy is never
        // handed those: it starts with nothing to run, and each CPU runs its
        // idle thread until the first thread is readied on it.
        StrideScheduler scheduler = new();
        SchedulerManager.SetScheduler(scheduler);

        Serial.WriteString("[SCHED] Using ");
        Serial.WriteString(scheduler.Name);
        Serial.WriteString(" scheduler\n");

        // Enable scheduler (timer will start invoking it)
        SchedulerManager.IsRunning = true;
        Serial.WriteString("[SCHED] Scheduler enabled\n");
    }
}
