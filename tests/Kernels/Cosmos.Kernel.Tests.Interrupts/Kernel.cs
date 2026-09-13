using System;
using System.Diagnostics;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;
using SysThread = System.Threading.Thread;
#if ARCH_X64
using Cosmos.Kernel.Core.X64.Cpu;
#else
using Cosmos.Kernel.Core.ARM64.Cpu;
#endif

namespace Cosmos.Kernel.Tests.Interrupts;

// Exercises the interrupt subsystem itself (no device drivers): the controller
// is up, the dynamic MSI vector allocator behaves, and an interrupt actually
// fires and is dispatched end-to-end. On arm64 the suite opts into the gicv2
// and gicv3 QEMU profiles (tests/profiles.json) so BOTH interrupt-controller
// paths are covered deterministically rather than depending on the machine
// default. End-to-end MSI *delivery* (a device raising an MSI through the ITS)
// stays covered by the Storage suite's NVMe assertions.
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests per cell: 6 cross-arch + 6 arch-specific.</summary>
    private const int ExpectedTestCount = 12;

    /// <summary>First vector of the dynamic MSI/MSI-X allocation window [0x40, 0xFE].</summary>
    private const byte DynamicVectorFirst = 0x40;
    /// <summary>Last vector of the dynamic MSI/MSI-X allocation window [0x40, 0xFE].</summary>
    private const byte DynamicVectorLast = 0xFE;
    /// <summary>Capacity of the scratch array used to drain the dynamic vector window; covers every possible byte vector.</summary>
    private const int VectorDrainCapacity = 256;

    /// <summary>Scheduler sleep duration exercised by the timer-IRQ wake test (ms).</summary>
    private const int SleepDurationMs = 200;
    /// <summary>Lower bound the measured sleep must reach to prove the timer IRQ actually elapsed it (ms).</summary>
    private const int MinMeasuredSleepMs = 100;

    /// <summary>Entry count of the fake MSI-X table used for bounds probing (also the first out-of-range index).</summary>
    private const int MsiXProbeEntryCount = 2;
    /// <summary>In-range entry index the MSI-X mask/unmask accessors are exercised on.</summary>
    private const int MsiXProbeEntryIndex = 1;
    /// <summary>Byte offset of probe entry 1 in the MSI-X table (index × 16-byte entry stride, PCI 3.0 §6.8.2).</summary>
    private const int MsiXProbeEntryOffset = 16;
    /// <summary>Vector Control register offset within an MSI-X table entry (PCI 3.0 §6.8.2).</summary>
    private const int MsiXVectorControlOffset = 12;
    /// <summary>Mask bit (bit 0) of the MSI-X Vector Control register.</summary>
    private const uint MsiXVectorControlMaskBit = 1;
    /// <summary>Out-of-range entry index used to probe UnmaskEntry rejection.</summary>
    private const int MsiXOutOfRangeIndex = 5;

    protected override void BeforeRun()
    {
        Serial.WriteString("[Interrupts] BeforeRun() reached!\n");

        // 6 cross-arch + 6 arch-specific = 12 tests per cell.
        TR.Start("Interrupt System Tests", expectedTests: ExpectedTestCount);

        // ==================== Cross-arch ====================
        TR.Run("InterruptManager_Enabled", TestInterruptManagerEnabled);
        TR.Run("TimerSource_Registered", TestTimerSourceRegistered);
        TR.Run("VectorAllocator_ReturnsDistinctDynamicVectors", TestVectorAllocatorDistinct);
        TR.Run("VectorAllocator_ReusesFreedSlots", TestVectorAllocatorReusesFreedSlots);
        TR.Run("MsiX_TableAccessors_BoundsChecked", TestMsiXTableAccessorsBoundsChecked);
        TR.Run("TimerInterrupt_WakesSleepingThread", TestTimerInterruptWakesSleepingThread);

#if ARCH_X64
        // ==================== x64 (LAPIC + MSI) ====================
        TR.Run("Lapic_Initialized", TestLapicInitialized);
        TR.Run("Lapic_TimerCalibrated", TestLapicTimerCalibrated);
        TR.Run("Msi_RoutingAvailable", TestMsiRoutingAvailableX64);
        TR.Run("IrqExit_ReschedulesSignaledWaiter", TestIrqExitReschedulesSignaledWaiter);
        TR.Run("Msi_AddressTargetsRunningLapic", TestMsiAddressTargetsRunningLapic);
        TR.Run("TimerVector_OutsideDynamicWindow", TestTimerVectorOutsideDynamicWindow);
#else
        // ==================== arm64 (GIC, per cell's gic-version) ====================
        TR.Run("Gic_Initialized", TestGicInitialized);

        if (TR.ProfileContains("gicv3"))
        {
            // GICv3 + ITS: the MSI delivery path must be fully up.
            TR.Run("Its_Lpi_BroughtUp", TestItsLpiUp);
            TR.Run("Msi_RoutingAvailable", TestMsiRoutingAvailableArm64);
        }
        else if (TR.ProfileContains("gicv2"))
        {
            // GICv2 has no ITS, so there is no LPI/MSI path: the kernel must
            // report it absent and fall back to polled rather than claim MSI.
            TR.Run("Its_Lpi_BroughtUp", TestItsAbsentOnGicv2);
            TR.Run("Msi_RoutingAvailable", TestMsiRoutingUnavailableOnGicv2);
        }
        else
        {
            // Bare cell: gic-version is whatever the machine defaults to, so the
            // ITS/MSI state is not determinate. The gicv2/gicv3 cells assert it.
            TR.Skip("Its_Lpi_BroughtUp", "gic-version not pinned by this cell");
            TR.Skip("Msi_RoutingAvailable", "gic-version not pinned by this cell");
        }

        // The IRQ-exit reschedule path is cross-arch, but the on-demand
        // ISR-context trigger it needs (LAPIC self-IPI) only exists on x64.
        TR.Skip("IrqExit_ReschedulesSignaledWaiter", "self-IPI harness is x64-only");
        TR.Skip("Msi_AddressTargetsRunningLapic", "LAPIC MSI address contract is x64-only");
        TR.Skip("TimerVector_OutsideDynamicWindow", "LAPIC timer vector is x64-only");
#endif

        TR.Finish();

        Serial.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Cross-arch ====================

    private static void TestInterruptManagerEnabled()
    {
        Assert.True(InterruptManager.IsEnabled, "InterruptManager should report enabled");
    }

    // A registered timer device is the periodic interrupt source that drives
    // the scheduler; without one nothing would generate IRQs to dispatch.
    private static void TestTimerSourceRegistered()
    {
        // IsInitialized is exactly "a timer device is registered": the ring
        // publishes the fact, so the suite does not read the device itself.
        Assert.True(TimerManager.IsInitialized, "A timer device should be registered");
    }

    // Exercises the dynamic-vector allocator MSI/MSI-X programmers depend on:
    // every allocation must fall in the [0x40, 0xFE] dynamic window and be
    // unique, so two devices never collide on the same vector.
    private static void TestVectorAllocatorDistinct()
    {
        byte v1 = InterruptManager.AllocateVector(NoopHandler);
        byte v2 = InterruptManager.AllocateVector(NoopHandler);
        byte v3 = InterruptManager.AllocateVector(NoopHandler);

        Assert.True(v1 >= DynamicVectorFirst && v1 <= DynamicVectorLast, "vector 1 should be in the dynamic range");
        Assert.True(v2 >= DynamicVectorFirst && v2 <= DynamicVectorLast, "vector 2 should be in the dynamic range");
        Assert.True(v3 >= DynamicVectorFirst && v3 <= DynamicVectorLast, "vector 3 should be in the dynamic range");
        Assert.True(v1 != v2 && v2 != v3 && v1 != v3, "allocated vectors must be distinct");
    }

    // End-to-end proof the interrupt path is live: a scheduler-blocking sleep
    // can only resume once the periodic timer IRQ fires, is dispatched through
    // the controller, and the handler wakes the blocked thread. The elapsed
    // time is read from the free-running Stopwatch (TSC / CNTPCT), which does
    // not depend on interrupts, so a sane duration isolates the IRQ path. If
    // interrupts were dead the sleep would never return and the suite would
    // time out.
    private static void TestTimerInterruptWakesSleepingThread()
    {
        long start = Stopwatch.GetTimestamp();
        SysThread.Sleep(SleepDurationMs);
        long end = Stopwatch.GetTimestamp();

        long elapsedMs = (end - start) * TR.MillisecondsPerSecond / Stopwatch.Frequency;
        Serial.WriteString("[Interrupts] Sleep(200ms) measured ms: ");
        Serial.WriteNumber((ulong)elapsedMs);
        Serial.WriteString("\n");

        // Lower bound only: returning early means the wake fired without
        // the timer IRQ actually elapsing the sleep — the regression this
        // cell pins. No upper bound on purpose: a loaded CI host can
        // deschedule the whole VM for hundreds of ms (arm64 TCG runners
        // especially), and the failure an upper bound would catch — a dead
        // IRQ path — never returns at all, which the engine's suite
        // timeout already converts into a failure.
        Assert.True(elapsedMs >= MinMeasuredSleepMs,
            "a 200ms scheduler sleep should resume via the timer IRQ, not return early");
    }

    // Fills the dynamic range to exhaustion, frees one slot, and proves the
    // allocator's wrap pass hands the freed slot back out — the behavior the
    // wrap-scan comment always promised but that FreeVector only now makes
    // possible. Frees everything it allocated so later cells see a clean
    // allocator.
    private static void TestVectorAllocatorReusesFreedSlots()
    {
        byte[] allocated = new byte[VectorDrainCapacity];
        int count = 0;
        while (count < allocated.Length)
        {
            byte v = TryAllocateVector();
            if (v == 0)
            {
                break;
            }
            allocated[count++] = v;
        }

        Assert.True(count > 0, "the dynamic range should not already be exhausted");

        byte freed = allocated[count / 2];
        InterruptManager.FreeVector(freed);
        byte reused = TryAllocateVector();

        // Free every vector this cell allocated before asserting, so a
        // failure doesn't leave the allocator exhausted for later cells.
        for (int i = 0; i < count; i++)
        {
            InterruptManager.FreeVector(allocated[i]);
        }
        InterruptManager.FreeVector(reused);

        Assert.True(reused == freed, "after exhaustion, the allocator must reuse the freed slot");
    }

    // Single try/catch in its own helper (arm64 EH dispatch mismatches the
    // clause when try/catch blocks share a frame with other locals). Returns
    // 0 (an out-of-range vector) on exhaustion.
    private static byte TryAllocateVector()
    {
        try
        {
            return InterruptManager.AllocateVector(NoopHandler);
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    private static void NoopHandler(ref IRQContext context)
    {
    }

    // MSI-X table accessors must bounds-check like SetEntry does: an
    // out-of-range Mask/UnmaskEntry index is a stray 32-bit MMIO write past
    // the device's table. Probed hardware-free against a fake context whose
    // "table" is a private heap page, so a missing guard writes into this
    // cell's own buffer instead of a live device — and is caught as a
    // missing exception, not as corruption.
    private static unsafe void TestMsiXTableAccessorsBoundsChecked()
    {
        void* table = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        Assert.True(table != null, "probe table allocation must succeed");

        MsiXContext ctx = new MsiXContext((ulong)table, MsiXProbeEntryCount, null);

        // In-range accessors keep programming VectorControl (offset 12).
        MsiX.MaskEntry(ctx, MsiXProbeEntryIndex);
        Assert.True(*(uint*)((byte*)table + MsiXProbeEntryOffset + MsiXVectorControlOffset) == MsiXVectorControlMaskBit, "in-range MaskEntry must set the mask bit");
        MsiX.UnmaskEntry(ctx, MsiXProbeEntryIndex);
        Assert.True(*(uint*)((byte*)table + MsiXProbeEntryOffset + MsiXVectorControlOffset) == 0, "in-range UnmaskEntry must clear the mask bit");

        bool maskHigh = MsiXMaskRejects(ctx, MsiXProbeEntryCount);
        bool maskNegative = MsiXMaskRejects(ctx, -1);
        bool unmaskHigh = MsiXUnmaskRejects(ctx, MsiXOutOfRangeIndex);
        PageAllocator.Free(table);

        Assert.True(maskHigh, "MaskEntry must reject index == EntryCount");
        Assert.True(maskNegative, "MaskEntry must reject negative indices");
        Assert.True(unmaskHigh, "UnmaskEntry must reject an out-of-range index");
    }

    // Single try/catch per helper (arm64 EH inlining quirk, see
    // TryAllocateVector).
    private static bool MsiXMaskRejects(MsiXContext ctx, int index)
    {
        try
        {
            MsiX.MaskEntry(ctx, index);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    private static bool MsiXUnmaskRejects(MsiXContext ctx, int index)
    {
        try
        {
            MsiX.UnmaskEntry(ctx, index);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

#if ARCH_X64

    private static void TestLapicInitialized()
    {
        Assert.True(LocalApic.IsInitialized, "LAPIC should be initialized");
    }

    private static void TestLapicTimerCalibrated()
    {
        Assert.True(LocalApic.IsTimerCalibrated, "LAPIC timer should be calibrated");
        Assert.True(LocalApic.TicksPerMs > 0, "LAPIC ticks-per-ms should be non-zero");
    }

    // On x64 the LAPIC is the MSI routing backend and its binder is registered
    // at boot, so MsiRouting must report available.
    private static void TestMsiRoutingAvailableX64()
    {
        Assert.True(MsiRouting.IsAvailable, "x64 MSI routing (LAPIC binder) should be available");
    }

    // ==================== IRQ-exit reschedule latency ====================
    // An ISR-side InterruptEvent.Signal only readies the waiter; if nothing
    // reschedules at device-IRQ exit, the woken thread sits in the run queue
    // until the next timer tick — up to a full 10ms quantum of added latency
    // per I/O. This measures signal-to-wake latency with a LAPIC self-IPI as
    // the "device" interrupt: the waiter parks on the event, the main thread
    // fires the IPI whose handler Signals from ISR context, and the waiter
    // timestamps its wake-up.
    private const int IrqLatencyRounds = 12;
    // Rescheduled at IRQ exit the wake costs microseconds; parked until the
    // next timer tick it costs up to a full 10_000us quantum, uniformly
    // spread over the tick phase. 2_000us splits the two regimes per round.
    private const long IrqLatencyFastUs = 2_000;
    // A loaded CI host can steal the vCPU for several milliseconds inside
    // any single signal→wake window, so no absolute worst-case bound is
    // stable there. Requiring 8 of 12 rounds under the threshold tolerates
    // a few host-induced spikes yet still refutes the tick-parked regime
    // decisively: with P(round < 2ms) = 0.2 per round, 8 fast rounds is a
    // ~6e-4 fluke.
    private const int IrqLatencyFastRoundsRequired = 8;
    /// <summary>Delay giving the waiter several full quanta to actually park in Blocked before the self-IPI fires (ms).</summary>
    private const uint WaiterParkDelayMs = 30;
    /// <summary>Drain delay letting the exited waiter's async thread-exit bookkeeping finish before the vector is freed (ms).</summary>
    private const uint WaiterExitDrainMs = 200;
    /// <summary>Microseconds per second — converts Stopwatch ticks to microseconds.</summary>
    private const int MicrosecondsPerSecond = 1_000_000;
    /// <summary>Mask isolating the fixed window of an x64 MSI doorbell address (Intel SDM message address, bits 31:20).</summary>
    private const ulong MsiAddressWindowMask = 0xFFF00000UL;
    /// <summary>LAPIC MSI doorbell base — the 0xFEE message address window (Intel SDM Vol. 3, 11.11.1).</summary>
    private const ulong LapicMsiAddressBase = 0xFEE00000UL;
    /// <summary>Bit position of the destination APIC ID field in the MSI message address (Intel SDM Vol. 3, 11.11.1).</summary>
    private const int MsiDestinationIdShift = 12;
    /// <summary>Width mask of the destination APIC ID field in the MSI message address (8 bits).</summary>
    private const int MsiDestinationIdMask = 0xFF;
    private static Cosmos.Kernel.Core.Scheduler.InterruptEvent? _wakeEvent;
    private static volatile bool _waiterReady;
    private static volatile bool _waiterDone;
    private static volatile bool _waiterExited;
    private static long _wakeTimestamp;

    private static void IpiSignalHandler(ref IRQContext context)
    {
        _wakeEvent?.Signal();
    }

    private static void TestIrqExitReschedulesSignaledWaiter()
    {
        _wakeEvent = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
        _waiterReady = false;
        _waiterDone = false;
        _waiterExited = false;
        byte vector = InterruptManager.AllocateVector(IpiSignalHandler);

        var waiter = new SysThread(IrqLatencyWaiter);
        waiter.Start();

        int fastRounds = 0;
        long worstTicks = 0;
        for (int round = 0; round < IrqLatencyRounds; round++)
        {
            while (!_waiterReady)
            {
                // waiter not yet at Wait()
            }
            _waiterReady = false;
            _waiterDone = false;

            // Give the waiter a few full quanta to actually park (Blocked):
            // a signal latched before it blocks would measure ~0 and mask
            // the very latency this cell pins down.
            TimerManager.Wait(WaiterParkDelayMs);

            long signalTicks = Stopwatch.GetTimestamp();
            LocalApic.SendSelfIpi(vector);

            while (!_waiterDone)
            {
                // the parked waiter records its wake timestamp
            }

            long latency = _wakeTimestamp - signalTicks;
            if (latency * MicrosecondsPerSecond / Stopwatch.Frequency < IrqLatencyFastUs)
            {
                fastRounds++;
            }

            if (latency > worstTicks)
            {
                worstTicks = latency;
            }
        }

        // Hermetic exit: don't leave the waiter's async thread-exit
        // bookkeeping (or a stale dynamic vector) to interleave with
        // whatever the next cell sets up.
        while (!_waiterExited)
        {
            // waiter finishing its last round
        }
        TimerManager.Wait(WaiterExitDrainMs);
        InterruptManager.FreeVector(vector);

        long worstUs = worstTicks * MicrosecondsPerSecond / Stopwatch.Frequency;
        Serial.WriteString("[Interrupts] fast rounds: ");
        Serial.WriteNumber((ulong)fastRounds);
        Serial.WriteString("/");
        Serial.WriteNumber((ulong)IrqLatencyRounds);
        Serial.WriteString(", worst signal-to-wake latency us: ");
        Serial.WriteNumber((ulong)worstUs);
        Serial.WriteString("\n");

        Assert.True(fastRounds >= IrqLatencyFastRoundsRequired,
            "an ISR-side Signal must wake the parked waiter at IRQ exit, not at the next timer tick");
    }

    // Pins the MSI address contract: the destination field must carry the
    // running CPU's ACTUAL APIC ID (LocalApic.GetId()), not its scheduler
    // index. QEMU's BSP APIC ID is 0, so this cell cannot fail there today —
    // it exists to catch the index-as-ID shortcut on any machine (or future
    // emulator config) whose BSP APIC ID is nonzero, where the mistake makes
    // device MSIs silently target a nonexistent LAPIC.
    private static void TestMsiAddressTargetsRunningLapic()
    {
        MsiRouting.BindEntry(null, 0, NoopHandler, 0, out ulong address, out uint data);

        Assert.True((address & MsiAddressWindowMask) == LapicMsiAddressBase, "MSI doorbell must sit in the LAPIC address window");
        byte destination = (byte)((address >> MsiDestinationIdShift) & MsiDestinationIdMask);
        Assert.True(destination == LocalApic.GetId(), "MSI destination must be the running CPU's actual APIC ID, not its index");
        Assert.True(data != 0, "MSI data must carry the allocated vector");
    }

    private static void IrqLatencyWaiter()
    {
        for (int round = 0; round < IrqLatencyRounds; round++)
        {
            _waiterReady = true;
            _wakeEvent!.Wait();
            _wakeTimestamp = Stopwatch.GetTimestamp();
            _waiterDone = true;
        }

        _waiterExited = true;
    }

    // The scheduler timer claims LocalApic.TIMER_VECTOR (0xEF) through
    // SetHandler, so it must live OUTSIDE the dynamic allocation window:
    // inside it, FreeVector would happily unregister the running timer
    // handler and the allocator would then hand the scheduler's vector to
    // the next MSI consumer. This cell attempts exactly that abuse: free
    // the timer vector, exhaust the allocator, and assert the timer's slot
    // was never handed out. Re-registers the timer handler unconditionally
    // on the way out so a failing run heals itself.
    private static void TestTimerVectorOutsideDynamicWindow()
    {
        InterruptManager.FreeVector(LocalApic.TIMER_VECTOR);

        byte[] allocated = new byte[VectorDrainCapacity];
        int count = 0;
        bool timerVectorHandedOut = false;
        while (count < allocated.Length)
        {
            byte v = TryAllocateVector();
            if (v == 0)
            {
                break;
            }

            if (v == LocalApic.TIMER_VECTOR)
            {
                timerVectorHandedOut = true;
            }

            allocated[count++] = v;
        }

        for (int i = 0; i < count; i++)
        {
            InterruptManager.FreeVector(allocated[i]);
        }

        // Heal before asserting: on a buggy kernel the FreeVector above
        // really did strip the scheduler timer's handler.
        LocalApic.RegisterTimerHandler();

        Assert.True(count > 0, "the dynamic range should not already be exhausted");
        Assert.True(!timerVectorHandedOut,
            "the LAPIC timer vector must be outside the dynamic window — FreeVector+AllocateVector must never touch it");
    }

#else

    private static void TestGicInitialized()
    {
        Assert.True(GIC.IsInitialized, "GIC should be initialized");
    }

    // gicv3 cell: the ITS and its LPI configuration/pending tables are the
    // ARM64 MSI delivery path and must both be up.
    private static void TestItsLpiUp()
    {
        Assert.True(GICv3Its.IsInitialized, "GICv3 ITS should be initialized on a gicv3 machine");
        Assert.True(GICv3Lpi.IsInitialized, "GICv3 LPI tables should be initialized on a gicv3 machine");
    }

    private static void TestMsiRoutingAvailableArm64()
    {
        Assert.True(MsiRouting.IsAvailable, "arm64 MSI routing (ITS binder) should be available on a gicv3 machine");
    }

    // gicv2 cell: no ITS exists, so the LPI/MSI path must report absent. This
    // is the path that forces drivers to the polled fallback.
    private static void TestItsAbsentOnGicv2()
    {
        Assert.False(GICv3Its.IsInitialized, "GICv2 exposes no ITS");
    }

    private static void TestMsiRoutingUnavailableOnGicv2()
    {
        Assert.False(MsiRouting.IsAvailable, "GICv2 has no ITS, so MSI routing must be unavailable");
    }

#endif
}
