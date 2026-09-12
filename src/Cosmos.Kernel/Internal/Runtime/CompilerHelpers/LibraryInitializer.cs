using Cosmos.Kernel;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.Core.Scheduler.Stride;
using Cosmos.Kernel.HAL;

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
        var initializer = PlatformHAL.Initializer;

        if (initializer == null)
        {
            Serial.WriteString("[KERNEL] ERROR: No platform initializer registered!\n");
            while (true) { }
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
            // Stride's fallback preemption test compares one tick's elapsed
            // time against DefaultQuantumNs, so the two have to agree.
            initializer.StartSchedulerTimer(
                (uint)(SchedulerManager.DefaultQuantumNs / SchedulerManager.NanosecondsPerMillisecond));
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
        // thread static, so it needs a current thread to exist.
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            SchedulerThread idleThread = new()
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = cpu,
                State = SchedulerThreadState.Running,
                Flags = SchedulerThreadFlags.Pinned | SchedulerThreadFlags.IdleThread
            };

            SchedulerManager.SetupIdleThread(cpu, idleThread);

            Serial.WriteString("[SCHED] Idle thread ");
            Serial.WriteNumber(idleThread.Id);
            Serial.WriteString(" (main kernel) for CPU ");
            Serial.WriteNumber(cpu);
            Serial.WriteString("\n");
        }

        // SetScheduler hands every registered thread to the incoming policy,
        // so the idle threads reach OnThreadCreate already Running, the same
        // way a thread alive across a later policy swap does.
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
