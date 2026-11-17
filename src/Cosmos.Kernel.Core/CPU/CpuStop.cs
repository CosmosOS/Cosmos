// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Runtime.ExceptionHandling;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// The stop rendezvous: one CPU holds every other online CPU, with interrupts masked, for the
/// duration of a section only one CPU may run, and every held CPU's current thread has a saved
/// context the collector can scan. The stop has an owner, claimed by compare-exchange in
/// <see cref="Enter"/> and released in <see cref="Exit"/>, and a depth, so the owner may enter
/// it again (a collection the policy swap's allocations trigger) without stopping anyone twice.
/// A held CPU answers in one of two ways: the stop interrupt's handler
/// (<see cref="HandleStopInterrupt"/>) when the CPU had interrupts enabled, which records the
/// interrupted frame as its thread's context; or the safepoint (<see cref="Safepoint"/>) from
/// inside a masked wait that can outlast a collection, which captures the registers it needs
/// on its own stack. A CPU that does not answer within <see cref="StopTimeoutSeconds"/> is a
/// panic that names it: a CPU that masks interrupts and waits on something that is neither a
/// kernel spinlock nor another CPU's interrupt stub cannot be stopped, and the kernel says so
/// rather than hang.
/// </summary>
/// <remarks>
/// <para>The policy swap uses it (SMP phase 3a, CosmosOS/Cosmos#3340): with the world stopped
/// and interrupts masked on the stopping CPU, nothing else touches a run structure, so the
/// swap needs no spinlock, and may allocate, which the lock-order rule forbids under one. The
/// collector takes the same rendezvous (SMP phase 4, #3342): the stop is its outer lock and
/// the heap lock its inner one, so a CPU waiting for the stop never holds the heap lock and the
/// swap's allocations never wait for a CPU the swap holds.</para>
/// <para>Each stop carries a sequence number. A held CPU acknowledges with it, waits for it to
/// be released, and leaves: two consecutive stops, the second claimed by another CPU in the
/// instant between the first's release and a held CPU's exit, are then never confused, which a
/// held CPU spinning on the owner word alone would do (it would read 0, leave, and run under
/// the second stop, or park again under it with an acknowledgement nobody reads).</para>
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

    // The owner: 0 for none, else the owning CPU's index plus one. Claimed by compare-exchange
    // in Enter, cleared with a release store in Exit at depth zero.
    private static int s_owner;

    // How many times the owner entered: only the owner's CPU writes it, under the ownership.
    private static int s_depth;

    // The sequence number of the stop in progress or last completed: bumped by the owner before
    // the interrupts go out. What a held CPU acknowledges with, in its block's Stopped slot.
    private static ulong s_sequence;

    // The sequence number of the last stop released: a held CPU leaves once this reaches the
    // sequence it acknowledged.
    private static ulong s_released;

    // The acknowledgements each path made, for the suites: a stop answered from the handler,
    // and one answered from a safepoint.
    private static ulong s_handlerAcknowledgements;
    private static ulong s_safepointAcknowledgements;

    /// <summary>How many stops were answered from the stop interrupt's handler, over the kernel's life.</summary>
    internal static ulong HandlerAcknowledgements => Volatile.Read(ref s_handlerAcknowledgements);

    /// <summary>How many stops were answered from a safepoint inside a masked wait, over the kernel's life.</summary>
    internal static ulong SafepointAcknowledgements => Volatile.Read(ref s_safepointAcknowledgements);

    /// <summary>
    /// Claims the stop for this CPU and, on its first entry, stops every other online CPU,
    /// returning once each one has acknowledged. The caller masks interrupts before calling and
    /// keeps them masked until <see cref="Exit"/>, so no interrupt on this CPU can wait on a
    /// held CPU. A nested entry by the owner stops nobody again: the held CPUs' contexts are
    /// already saved. Called while this CPU holds one of the locks a collection takes, it is an
    /// allocation under that lock, and panics naming the caller rather than wait for a CPU that
    /// is waiting for it.
    /// </summary>
    /// <param name="cpuCount">The CPUs the scheduler manages, the caller's included.</param>
    /// <param name="caller">The member that entered, for the panic. Filled in by the compiler.</param>
    internal static void Enter(uint cpuCount, [CallerMemberName] string caller = "")
    {
        ref PerCpuBlock block = ref PerCpuBlock.Current;
        if (block.CollectorLockDepth != 0)
        {
            Serial.WriteString("[SMP] CPU ");
            Serial.WriteNumber(block.CpuIndex);
            Serial.WriteString(" entered the stop holding a collector lock\n");
            Panic.Halt(
                "A collection or a policy swap started while this CPU holds a lock the collector takes: "
                + "an allocation under the heap lock, the handle store's lock or the page allocator's. "
                + "Nothing allocates under a spinlock.",
                caller);
        }

        int self = (int)block.CpuIndex + 1;
        if (Volatile.Read(ref s_owner) != self)
        {
            // Bounded by the owner's Exit. Another CPU's stop may land on this one meanwhile:
            // the safepoint answers it from here.
            while (Interlocked.CompareExchange(ref s_owner, self, 0) != 0)
            {
                Safepoint();
            }
        }

        if (++s_depth > 1)
        {
            return;
        }

        StopOthers(cpuCount, (uint)block.CpuIndex);
    }

    /// <summary>
    /// Leaves the stop: at depth zero, lets the held CPUs go and releases the ownership, with
    /// the release stores that make everything the owner wrote while the world was stopped
    /// visible to a CPU once it leaves its handler or its safepoint.
    /// </summary>
    internal static void Exit()
    {
        if (--s_depth > 0)
        {
            return;
        }

        Volatile.Write(ref s_released, s_sequence);
        Volatile.Write(ref s_owner, 0);
    }

    /// <summary>
    /// Sends the stop interrupt to every other online CPU and waits for each acknowledgement.
    /// </summary>
    private static void StopOthers(uint cpuCount, uint self)
    {
        if (cpuCount <= 1)
        {
            return;
        }

        ulong sequence = s_sequence + 1;
        Volatile.Write(ref s_sequence, sequence);

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

        // Bounded: a CPU that does not answer is named, not waited for forever. Every CPU that
        // did not answer is named before the panic, so one log shows the whole set.
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * StopTimeoutSeconds;
        uint missing = 0;
        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            if (!stopSent[(int)cpu])
            {
                continue;
            }

            ref PerCpuBlock block = ref PerCpuBlock.At(cpu);
            while (Volatile.Read(ref block.Stopped) != sequence && Stopwatch.GetTimestamp() < deadline)
            {
            }

            if (Volatile.Read(ref block.Stopped) != sequence)
            {
                Serial.WriteString("[SMP] CPU ");
                Serial.WriteNumber(cpu);
                Serial.WriteString(" did not stop\n");
                missing++;
            }
        }

        if (missing != 0)
        {
            Panic.Halt("A CPU did not answer the stop interrupt: it masks interrupts and waits on something that is neither a kernel spinlock nor another CPU's interrupt stub.");
        }
    }

    /// <summary>
    /// The stop interrupt's handler, on the held CPU: records the interrupted frame as the
    /// current thread's saved context, acknowledges, and spins, interrupts masked by the
    /// interrupt itself, until the owner releases the stop. The return is the ordinary interrupt
    /// exit with nothing staged. The interrupt landed with interrupts enabled, so this CPU holds
    /// no IRQ-safe lock and has no switch staged: the thread on the stack is the current thread,
    /// and the frame is its <see cref="ThreadContext"/>. A delivery that lands after the release
    /// finds no owner and leaves at once.
    /// </summary>
    /// <param name="contextBase">The base of the <see cref="ThreadContext"/> the vector stub saved for this interrupt.</param>
    internal static void HandleStopInterrupt(nuint contextBase)
    {
        if (Volatile.Read(ref s_owner) == 0)
        {
            return;
        }

        // A null thread is the kernel before the scheduler exists, which is before any other
        // CPU does; the frame is then nobody's and the scan has no other thread to read.
        SchedulerThread? thread = SchedulerManager.CurrentCpuState?.CurrentThread;
        thread?.StackPointer = contextBase;

        Interlocked.Increment(ref s_handlerAcknowledgements);
        Park();
    }

    /// <summary>
    /// Answers a stop from inside a masked wait that can outlast a collection: the spin of a
    /// kernel spinlock, the claim of the stop itself, the interrupt exit's wait on the on-CPU
    /// marker. One volatile read when nothing is stopping. Returns at once when no stop is in
    /// progress, when this CPU owns it, when interrupts are enabled (the interrupt lands), or
    /// when this CPU holds a lock the collector takes (<see cref="PerCpuBlock.CollectorLockDepth"/>:
    /// it releases first, and answers on its next wait or by the interrupt). Otherwise it gives
    /// the current thread a saved context and parks until the release. With a switch staged on
    /// this CPU, both threads' contexts are current already (the outgoing thread's from the
    /// switch-out, the incoming one's from its own) and nothing is captured; else the
    /// callee-saved registers and the stack pointer are captured into a register display on
    /// this stack, which the collector scans conservatively from, up to the stack's end. The
    /// interrupts stay as they are throughout.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void Safepoint()
    {
        int owner = Volatile.Read(ref s_owner);
        if (owner == 0)
        {
            return;
        }

        ref PerCpuBlock block = ref PerCpuBlock.Current;
        if (owner == (int)block.CpuIndex + 1 || block.CollectorLockDepth != 0 || InternalCpu.InterruptsEnabled)
        {
            return;
        }

        SchedulerThread? thread = SchedulerManager.CurrentCpuState?.CurrentThread;
        if (thread is not null && ContextSwitchNative.GetContextSwitchSp() == 0)
        {
            REGDISPLAY rd = default;
            ContextSwitchNative.CaptureRegDisplay(&rd);
            thread.StackPointer = (nuint)(&rd);
            thread.SavedContextIsCapture = true;

            // Parked inside the capture's scope, so the display stays live on this stack for
            // as long as the collector may read it; the arm below acknowledges the same way
            // with nothing to capture.
            Interlocked.Increment(ref s_safepointAcknowledgements);
            Park();
            return;
        }

        Interlocked.Increment(ref s_safepointAcknowledgements);
        Park();
    }

    /// <summary>
    /// Acknowledges the stop in progress with its sequence and waits for that sequence to be
    /// released. Acquire on the owner, release on the acknowledgement: the owner's writes before
    /// the stop are visible here, and this CPU's writes before the acknowledgement are visible
    /// to the owner once it reads the sequence in the block. Bounded by the owner's
    /// <see cref="Exit"/>.
    /// </summary>
    private static void Park()
    {
        ref PerCpuBlock block = ref PerCpuBlock.Current;
        ulong sequence = Volatile.Read(ref s_sequence);
        Volatile.Write(ref block.Stopped, sequence);

        while (Volatile.Read(ref s_released) < sequence)
        {
        }

        Volatile.Write(ref block.Stopped, 0);
    }
}
