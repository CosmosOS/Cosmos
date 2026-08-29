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
    /// Initializes the scheduler subsystem with idle threads for each CPU.
    /// </summary>
    private static void InitializeScheduler(uint cpuCount)
    {
        Serial.WriteString("[SCHED] Detected ");
        Serial.WriteNumber(cpuCount);
        Serial.WriteString(" CPU(s)\n");

        // Initialize scheduler manager
        SchedulerManager.Initialize(cpuCount);

        // Set up stride scheduler
        var scheduler = new StrideScheduler();
        SchedulerManager.SetScheduler(scheduler);

        Serial.WriteString("[SCHED] Using ");
        Serial.WriteString(scheduler.Name);
        Serial.WriteString(" scheduler\n");

        // Create idle thread for each CPU
        // The idle thread represents the main kernel - no separate stack needed
        // When the shell is preempted, its context is saved to this thread
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            var idleThread = new Cosmos.Kernel.Core.Scheduler.SchedulerThread
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = cpu,
                State = Cosmos.Kernel.Core.Scheduler.SchedulerThreadState.Running,  // Already running (it's the current code!)
                Flags = SchedulerThreadFlags.Pinned | SchedulerThreadFlags.IdleThread
            };

            // DON'T initialize a separate stack - the idle thread IS the current execution
            // When preempted, the IRQ stub saves context to the current stack
            // and we store that RSP in StackPointer

            // Register with scheduler (but don't add to run queue)
            SchedulerManager.CreateThread(cpu, idleThread);

            // Set as CPU's idle and current thread
            SchedulerManager.SetupIdleThread(cpu, idleThread);

            Serial.WriteString("[SCHED] Idle thread ");
            Serial.WriteNumber(idleThread.Id);
            Serial.WriteString(" (main kernel) for CPU ");
            Serial.WriteNumber(cpu);
            Serial.WriteString("\n");
        }

        // Enable scheduler (timer will start invoking it)
        SchedulerManager.IsRunning = true;
        Serial.WriteString("[SCHED] Scheduler enabled\n");

    }
}
