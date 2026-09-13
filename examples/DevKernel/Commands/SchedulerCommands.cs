using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timer;
using DevKernel.Diagnostics;
using DevKernel.Shell;
using SysThread = System.Threading.Thread;

namespace DevKernel.Commands;

/// <summary>
/// Scheduler introspection, plus the managed-thread smoke tests.
/// </summary>
internal static class SchedulerCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Scheduler";

    /// <summary>Delay (ms) after starting the test thread so its output can appear.</summary>
    private const uint ThreadTestWaitMs = 2000;

    /// <summary>Delay (ms) before the interrupt-context timer fires.</summary>
    private const uint TimerDelayMs = 100;

    /// <summary>Delay (ms) before the thread-context alarm fires.</summary>
    private const uint AlarmDelayMs = 200;

    /// <summary>Delay (ms) waited for both to fire, longer than either delay.</summary>
    private const uint TimersWaitMs = 500;

    /// <summary>Fire count for the interrupt-context timer; written from interrupt context.</summary>
    private static volatile int s_timerFireCount;

    /// <summary>Fire count for the thread-context alarm; written from the alarm thread.</summary>
    private static volatile int s_alarmFireCount;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "schedinfo",
                Usage = "schedinfo",
                Description = "Show scheduler status and threads",
                Execute = static (context, args) => ShowSchedulerInfo(),
            },
            new ShellCommand
            {
                Name = "thread",
                Usage = "thread",
                Description = "Test System.Threading.Thread",
                Execute = static (context, args) => TestThread(),
            },
            new ShellCommand
            {
                Name = "kill",
                Usage = "kill <thread_id>",
                Description = "Kill a thread by ID",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetUInt(0, out uint threadId))
                    {
                        args.PrintUsage();
                        return;
                    }

                    KillThread(threadId);
                },
            },
            new ShellCommand
            {
                Name = "cpustat",
                Usage = "cpustat",
                Description = "Live CPU% + thread monitor with stress wave",
                Execute = static (context, args) => CpuStat.Run(),
            },
            new ShellCommand
            {
                Name = "timers",
                Usage = "timers",
                Description = "Fire an interrupt-context timer and a thread-context alarm",
                Execute = static (context, args) => TestTimers(),
            });
    }

    private static void ShowSchedulerInfo()
    {
        Terminal.Header("Scheduler Information:");

        if (!SchedulerInfo.IsInitialized)
        {
            Terminal.InfoLine("Status", "Not initialized");
            return;
        }

        Terminal.StatusLine(
            "Status",
            SchedulerInfo.IsRunning ? "ENABLED" : "DISABLED",
            SchedulerInfo.IsRunning ? ConsoleColor.Green : ConsoleColor.Red);

        Terminal.InfoLine("Scheduler", SchedulerInfo.SchedulerName!);
        Terminal.InfoLine("CPU Count", SchedulerInfo.CpuCount.ToString());
        Terminal.InfoLine(
            "Tick period",
            SchedulerInfo.TickPeriodNs == 0
                ? "no tick yet"
                : (SchedulerInfo.TickPeriodNs / Units.NsPerMs).ToString() + " ms");
        Console.WriteLine();

        for (uint cpuId = 0; cpuId < SchedulerInfo.CpuCount; cpuId++)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  CPU " + cpuId + ":");
            Console.ResetColor();

            if (SchedulerInfo.TryGetCurrentThread(cpuId, out KernelThreadInfo currentThread))
            {
                PrintThreadInfo(currentThread);
            }

            int runQueueCount = SchedulerInfo.GetRunQueueCount(cpuId);
            for (int i = 0; i < runQueueCount; i++)
            {
                if (SchedulerInfo.TryGetRunQueueThread(cpuId, i, out KernelThreadInfo thread))
                {
                    PrintThreadInfo(thread);
                }
            }
        }

        Console.WriteLine();
    }

    private static void PrintThreadInfo(KernelThreadInfo info)
    {
        Console.Write("    ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Thread " + info.Id);

        Console.Write(" ");
        switch (info.State)
        {
            case KernelThreadState.Running:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Running");
                break;
            case KernelThreadState.Ready:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Ready");
                break;
            case KernelThreadState.Blocked:
            case KernelThreadState.Sleeping:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(info.State == KernelThreadState.Blocked ? "Blocked" : "Sleeping");
                break;
            case KernelThreadState.Dead:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Dead");
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Unknown");
                break;
        }

        if (info.HasPriority)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" Pri=" + info.Priority);
        }

        ulong runtimeMs = info.TotalRuntimeNs / Units.NsPerMs;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" Run=" + runtimeMs + "ms");

        Console.ResetColor();
        Console.WriteLine();
    }

    private static void TestTimers()
    {
        Terminal.Header("Timers:");

        s_timerFireCount = 0;
        s_alarmFireCount = 0;

        SoftwareTimer? timer = TimerManager.Schedule(static () => s_timerFireCount++, TimeSpan.FromMilliseconds(TimerDelayMs));
        if (timer is null)
        {
            Terminal.Error("No timer device registered");
            return;
        }

        ulong alarmId = AlarmManager.Schedule(static () => s_alarmFireCount++, TimeSpan.FromMilliseconds(AlarmDelayMs));
        if (alarmId == 0)
        {
            Terminal.Error("Scheduler is not running, alarm not scheduled");
            TimerManager.Cancel(timer);
            return;
        }

        Terminal.InfoLine("Timer", TimerDelayMs + "ms, interrupt context, active=" + timer.IsActive);
        Terminal.InfoLine("Alarm", AlarmDelayMs + "ms, thread context, id=" + alarmId);

        TimerManager.Wait(TimersWaitMs);

        Terminal.InfoLine("Timer fired", s_timerFireCount + "x, active=" + timer.IsActive);
        Terminal.InfoLine("Alarm fired", s_alarmFireCount + "x");

        // Both were one-shot, so cancelling now reports that they already fired.
        Terminal.InfoLine("Cancel timer", TimerManager.Cancel(timer) ? "was still pending" : "already fired");
        Terminal.InfoLine("Cancel alarm", AlarmManager.Cancel(alarmId) ? "was still pending" : "already fired");
        Console.WriteLine();
    }

    private static void TestThread()
    {
        Log.WriteString("[Thread] Testing System.Threading.Thread API\n");
        Terminal.Info("Creating and starting a thread...");

        SysThread thread = new(static () =>
        {
            Log.WriteString("[Thread] Hello from thread delegate!\n");
            Console.WriteLine("Hello from thread!");
        });

        thread.Start();
        Terminal.Success("Thread started!");
        Console.WriteLine();

        TimerManager.Wait(ThreadTestWaitMs);
    }

    private static void KillThread(uint threadId)
    {
        if (!SchedulerInfo.IsInitialized)
        {
            Terminal.Error("Scheduler not initialized");
            return;
        }

        switch (SchedulerInfo.RequestKill(threadId))
        {
            case ThreadKillResult.Killed:
                Terminal.Success("Thread " + threadId + " killed");
                Console.WriteLine();
                break;
            case ThreadKillResult.MarkedForExit:
                Terminal.Warning("Thread " + threadId + " is running; marked for exit at its next reschedule");
                break;
            case ThreadKillResult.RefusedBlocked:
                Terminal.Error("Thread " + threadId + " is neither running nor queued; it may be blocked, sleeping, never started, or already marked for exit");
                break;
            case ThreadKillResult.RefusedIdle:
                Terminal.Error("Cannot kill idle thread " + threadId);
                break;
            default:
                Terminal.Error("Thread " + threadId + " not found");
                break;
        }
    }
}
