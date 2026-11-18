// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Threading;
using SysThread = System.Threading.Thread;

namespace DevKernel.Diagnostics.Smp;

// Full-screen monitor of the scheduler on a machine with several CPUs: one lane per CPU
// with its load, the threads it holds and a timeline of who ran there, while a scenario
// plays what the scheduler does with them (placement, affinity, the pull of an idle CPU,
// migration, the thread pool, a collection that stops every CPU).
//
// What is shown is read through the ring's diagnostics (SchedulerDiagnostics, and
// MemoryDiagnostics for the collections) and KernelCpus; the scenario acts through
// ThreadAffinity, plain managed threads and the thread pool.
//
// SPACE skips to the next step, ESC exits.
internal static class SmpStat
{
    // A frame is drawn this often. At the scheduler's default tick of 10 ms that is five
    // ticks, so a thread gains a whole number of ticks from one frame to the next.
    public const int FramePeriodMs = 50;

    private const int ShutdownBudgetMs = 3000;

    // Slept at least, so the other threads of the monitor's CPU run even when drawing
    // takes the whole period.
    private const int MinSleepMs = 5;

    // Keys read at one frame, so a held key cannot keep the frame from being drawn.
    private const int MaxKeysPerFrame = 8;

    public static void Run()
    {
        if (!SchedulerDiagnostics.IsSupported)
        {
            Console.WriteLine("smpstat: scheduler disabled (set CosmosEnableScheduler=true).");
            return;
        }

        if (SmpScenario.IsRunning)
        {
            Console.WriteLine("smpstat: the last run is still stopping its threads, try again in a moment.");
            return;
        }

        Canvas canvas = Canvas.GetFullScreen();
        PCScreenFont font = PCScreenFont.DefaultFont;
        int cpuCount = KernelCpus.Count;

        // Out of the workers' way first: the scenario's first step tells whether the
        // monitor moved.
        CpuSet ownSet = ThreadAffinity.Current;
        bool pinned = PinToHousekeeping();

        try
        {
            SmpScenario.Start(pinned);

            // The view fits the banner's texts to the screen: Start has just written them.
            SmpView view = new(canvas, font, cpuCount);
            SmpSampler sampler = new(cpuCount, view.Columns, SmpWorker.ReadCurrentThreadId());

            long tickMs = (long)(SchedulerDiagnostics.TickPeriodNs / SchedulerDiagnostics.NanosecondsPerMillisecond);

            while (HandleKeys())
            {
                long start = Stopwatch.GetTimestamp();

                sampler.Sample();
                view.Draw(sampler);
                canvas.Display();

                // A sleep ends on the first tick past its time, so asking for a tick less
                // than what is left of the period lands on the tick the next frame is due.
                long sleepMs = FramePeriodMs - tickMs - SmpClock.MsSince(start);
                SysThread.Sleep(sleepMs > MinSleepMs ? (int)sleepMs : MinSleepMs);
            }
        }
        finally
        {
            // Whichever way the monitor ends, its threads are asked to leave and the shell
            // gets its CPU set and its screen back.
            SmpScenario.Stop(ShutdownBudgetMs);

            if (pinned)
            {
                ThreadAffinity.Current = ownSet;
            }

            Console.Clear();
        }
    }

    // Moves the monitor to the housekeeping CPU, where the kernel's own threads are, so
    // that drawing the screen takes nothing from the CPUs the scenario loads.
    private static bool PinToHousekeeping()
    {
        if (KernelCpus.Count < 2)
        {
            return false;
        }

        try
        {
            ThreadAffinity.Current = KernelCpus.Housekeeping;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // Acts on the keys typed since the last frame: SPACE skips the scenario's step. False
    // once ESC is among them.
    private static bool HandleKeys()
    {
        for (int i = 0; i < MaxKeysPerFrame && Console.KeyAvailable; i++)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
            {
                return false;
            }

            if (key == ConsoleKey.Spacebar)
            {
                SmpScenario.Skip();
            }
        }

        return true;
    }
}
