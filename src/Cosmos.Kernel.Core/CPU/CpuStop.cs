// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Threading;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// The stop rendezvous: one CPU holds every other online CPU in an interrupt handler, with
/// interrupts masked, for the duration of a section only one CPU may run. The caller sets the
/// stop flag, sends the stop interrupt to every other online CPU
/// (<see cref="IInterruptController.SendStop"/>), and waits for each one to report itself
/// stopped in its per-CPU block; the handler on the stopped CPU acknowledges and spins there
/// until the flag clears, so returning from it is the ordinary interrupt exit with nothing
/// staged. A CPU that does not answer within <see cref="StopTimeoutSeconds"/> is a panic that
/// names it: a CPU that masks interrupts and waits on something other than a kernel spinlock
/// cannot be stopped, and the kernel says so rather than hang.
/// </summary>
/// <remarks>
/// The policy swap uses it (SMP phase 3a, CosmosOS/Cosmos#3340): with the world stopped and
/// interrupts masked on the stopping CPU, nothing else touches a run structure, so the swap
/// needs no spinlock, and may allocate, which the lock-order rule forbids under one. The
/// collector takes the same rendezvous in phase 4 (#3342), where the handler also records the
/// stopped thread's context for the scan.
/// </remarks>
internal static class CpuStop
{
    /// <summary>
    /// How long the stopping CPU waits for the others to acknowledge, in <see cref="Stopwatch"/>
    /// seconds. A CPU answers an interrupt in microseconds; one that takes seconds is one that
    /// cannot take it at all. Generous because the counter runs on host time under emulation,
    /// where a virtual CPU the host descheduled takes no interrupt for as long as it is off
    /// the host's CPU, and the panic is final.
    /// </summary>
    private const long StopTimeoutSeconds = 10;

    // 1 while a stop is in progress: the stopped CPUs spin on it, and the handler of a stop
    // interrupt that lands after the release (a late delivery) sees 0 and leaves at once.
    private static int s_stopRequested;

    /// <summary>
    /// Whether a stop is in progress: what <see cref="HandleStopInterrupt"/> spins on.
    /// </summary>
    internal static bool IsStopRequested => Volatile.Read(ref s_stopRequested) != 0;

    /// <summary>
    /// Stops every other online CPU and returns once each one sits in its stop handler. The
    /// caller masks interrupts before calling, so no interrupt on this CPU can take a spinlock a
    /// stopped CPU holds, and restores them after <see cref="ReleaseAll"/>. Does nothing on a
    /// one-CPU machine.
    /// </summary>
    /// <param name="cpuCount">The CPUs the scheduler manages, the caller's included.</param>
    internal static void StopOthers(uint cpuCount)
    {
        if (cpuCount <= 1)
        {
            return;
        }

        uint self = PerCpuBlock.CurrentIndex;
        Volatile.Write(ref s_stopRequested, 1);

        // The set the interrupt went to, so the wait below is on exactly those CPUs: a CPU that
        // came online between the two loops was never sent the interrupt, and waiting on it
        // would end in the panic. Today the window is empty, the application processors come
        // online at boot before the first swap; a CPU brought up later would need the stop
        // checked on its way online.
        Span<bool> stopSent = stackalloc bool[(int)cpuCount];
        uint sent = 0;
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            if (cpu == self || Volatile.Read(ref PerCpuBlock.At(cpu).Online) == 0)
            {
                continue;
            }

            InterruptManager.SendStop(PerCpuBlock.At(cpu).HardwareId);
            stopSent[(int)cpu] = true;
            sent++;
        }

        if (sent == 0)
        {
            return;
        }

        // Bounded: a CPU that does not answer is named, not waited for forever.
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * StopTimeoutSeconds;
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            if (!stopSent[(int)cpu])
            {
                continue;
            }

            ref PerCpuBlock block = ref PerCpuBlock.At(cpu);
            while (Volatile.Read(ref block.Stopped) == 0 && Stopwatch.GetTimestamp() < deadline)
            {
            }

            if (Volatile.Read(ref block.Stopped) == 0)
            {
                Serial.WriteString("[SMP] CPU ");
                Serial.WriteNumber(cpu);
                Serial.WriteString(" did not stop\n");
                Panic.Halt("A CPU did not answer the stop interrupt: it masks interrupts and waits on something that is not a kernel spinlock.");
            }
        }
    }

    /// <summary>
    /// Lets the stopped CPUs go: clears the flag with a release store, so everything the caller
    /// wrote while the world was stopped is visible to a CPU once it leaves its handler.
    /// </summary>
    internal static void ReleaseAll()
    {
        Volatile.Write(ref s_stopRequested, 0);
    }

    /// <summary>
    /// The stop interrupt's handler, on the stopped CPU: records the CPU as stopped and spins,
    /// interrupts masked by the interrupt itself, until the stopping CPU clears the flag. The
    /// return is the ordinary interrupt exit. A delivery that lands after the release finds the
    /// flag clear and leaves at once.
    /// </summary>
    internal static void HandleStopInterrupt()
    {
        ref PerCpuBlock block = ref PerCpuBlock.Current;

        // Acquire on the flag, release on the acknowledgement: the stopping CPU's writes before
        // the stop are visible here, and this CPU's writes before the acknowledgement are visible
        // to the stopping CPU once it reads Stopped = 1.
        if (!IsStopRequested)
        {
            return;
        }

        Volatile.Write(ref block.Stopped, 1);

        while (IsStopRequested)
        {
        }

        Volatile.Write(ref block.Stopped, 0);
    }
}
