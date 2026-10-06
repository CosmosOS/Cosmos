using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Pci;
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
// stays covered by the Storage suite's NVMe assertions. The MSI-X table cells
// go through the driver kit's PciMessageTable: the bounds cell over a scratch
// table that never gets past its index guard, the connect and disconnect
// cells over the table of a live function whose MSI-X nobody has enabled
// (x64: the default e1000e NIC, whose driver takes its line), restoring its
// registers afterwards.
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests per cell: 10 cross-arch + 6 arch-specific.</summary>
    private const int ExpectedTestCount = 16;

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

    /// <summary>Entry count of the scratch table the bounds cell probes (also its first out-of-range index), and the fewest entries a borrowed function needs for the live cells to connect two.</summary>
    private const int MsiXProbeEntryCount = 2;
    /// <summary>Second entry the live table cells connect beside entry 0, so one disconnect leaves an entry bound.</summary>
    private const int MsiXProbeEntryIndex = 1;
    /// <summary>Vector Control register offset within an MSI-X table entry (PCI 3.0 §6.8.2).</summary>
    private const int MsiXVectorControlOffset = 12;
    /// <summary>Mask bit (bit 0) of the MSI-X Vector Control register.</summary>
    private const uint MsiXVectorControlMaskBit = 1;
    /// <summary>Out-of-range entry index the bounds cell probes Unmask and Disconnect with.</summary>
    private const int MsiXOutOfRangeIndex = 5;
    /// <summary>Byte stride between MSI-X table entries (PCI 3.0 §6.8.2).</summary>
    private const int MsiXEntryStride = 16;
    /// <summary>Message Address (low dword) offset within an MSI-X table entry (PCI 3.0 §6.8.2).</summary>
    private const int MsiXMessageAddressOffset = 0;
    /// <summary>Message Upper Address offset within an MSI-X table entry (PCI 3.0 §6.8.2).</summary>
    private const int MsiXMessageUpperAddressOffset = 4;
    /// <summary>Message Data offset within an MSI-X table entry (PCI 3.0 §6.8.2).</summary>
    private const int MsiXMessageDataOffset = 8;
    /// <summary>Offset of Message Control within the MSI-X capability (PCI 3.0 §6.8.2.3).</summary>
    private const byte MsiXMessageControlOffset = 2;
    /// <summary>Message Control bit 15: MSI-X Enable.</summary>
    private const ushort MsiXEnableBit = 1 << 15;
    /// <summary>Message Control bit 14: Function Mask.</summary>
    private const ushort MsiXFunctionMaskBit = 1 << 14;
    /// <summary>Largest MSI-X table a function can expose (Table Size is 11 bits, PCI 3.0 §6.8.2.3); more entries than either arch has vectors or LPIs, so binding them all drains the allocator.</summary>
    private const int MsiXMaxTableSize = 2048;
    /// <summary>The MSI-X capability id (PCI 3.0 6.8.2).</summary>
    private const byte MsiXCapabilityId = 0x11;
    /// <summary>Offset of the Table Offset/Table BIR register within the MSI-X capability (PCI 3.0 6.8.2.4).</summary>
    private const byte MsiXTableOffsetBirOffset = 4;
    /// <summary>Bits 2:0 of the Table Offset/Table BIR register: the base address register holding the table.</summary>
    private const uint MsiXTableBirMask = 0x7;
    /// <summary>Bits 31:3 of the Table Offset/Table BIR register: the table's offset into that register's window.</summary>
    private const uint MsiXTableOffsetMask = 0xFFFFFFF8;
    /// <summary>Command register offset in configuration space (PCI 3.0 6.2.2).</summary>
    private const ushort PciCommandOffset = 0x04;
    /// <summary>Command bit 1: memory space decode, without which a function's MSI-X table is not decoded.</summary>
    private const ushort PciCommandMemorySpace = 0x0002;
    /// <summary>Command bit 10: INTx disabled, which the kit's first message connect sets.</summary>
    private const ushort PciCommandInterruptDisable = 0x0400;
    /// <summary>Capability offset of the bounds cell's scratch table: none, since none of its calls gets past the index guard.</summary>
    private const byte ScratchTableCapability = 0;

    // A function no QEMU machine the suite runs populates (00:1f.7). The
    // binder only turns it into a routing key (ARM64: ITS DeviceID 0xFF, well
    // inside the flat device table), so the cells below exercise the real
    // PrepareDevice/BindEntry/UnbindEntry/ReleaseDevice path, ITS commands
    // included, without touching a device.
    private const uint SyntheticBus = 0;
    private const uint SyntheticSlot = 31;
    private const uint SyntheticFunction = 7;

    /// <summary>Owner hand-overs per remap cell: each one would orphan an ITT page without the unmap, so the leak dwarfs the slack below.</summary>
    private const int RemapRounds = 64;
    /// <summary>Entries per remapped context: a one-page ITT on ARM64.</summary>
    private const int RemapEntryCount = 64;
    /// <summary>
    /// Pages the remap cell tolerates losing: half a page per round. The
    /// contexts it allocates grow the GC heap by about a quarter page per
    /// round (15 pages over 64 rounds on arm64, 6 on x64), and an orphaned
    /// ITT costs a full page per round on top of that.
    /// </summary>
    private const ulong RemapLeakSlackPages = RemapRounds / 2;

    /// <summary>The access of a PCI function with no assigned base address register (a host bridge), which the bounds cell builds its scratch table over; null when the cell has none.</summary>
    private static PciAccess? s_barlessPciFunction;

    /// <summary>The access of a PCI function with an MSI-X capability nobody enabled, memory decoding on and at least two table entries, or null when the cell has none.</summary>
    private static PciAccess? s_idleMsiXFunction;

    protected override void BeforeRun()
    {
        Serial.WriteString("[Interrupts] BeforeRun() reached!\n");

        // 10 cross-arch + 6 arch-specific = 16 tests per cell.
        TR.Start("Interrupt System Tests", expectedTests: ExpectedTestCount);

        s_barlessPciFunction = FindBarlessPciFunction();
        s_idleMsiXFunction = FindIdleMsiXFunction();
        bool routing = MsiRouting.IsAvailable;
        bool barlessFunction = s_barlessPciFunction is not null;
        bool liveFunction = routing && s_idleMsiXFunction is not null;
        const string NoRouting = "no MSI routing in this cell (GICv2 has no ITS)";
        const string NoBarlessFunction = "no PCI function without an assigned base address register in this cell";
        const string NoIdleFunction = "needs MSI routing and an MSI-X function no driver has enabled (arm64: virtio-net owns the only one)";

        // ==================== Cross-arch ====================
        TR.Run("InterruptManager_Enabled", TestInterruptManagerEnabled);
        TR.Run("TimerSource_Registered", TestTimerSourceRegistered);
        TR.Run("VectorAllocator_ReturnsDistinctDynamicVectors", TestVectorAllocatorDistinct);
        TR.Run("VectorAllocator_ReusesFreedSlots", TestVectorAllocatorReusesFreedSlots);
        TR.RunIf(barlessFunction, "MsiXTable_OutOfRangeEntry_Refused", TestMsiXTableOutOfRangeEntryRefused, NoBarlessFunction);
        TR.RunIf(routing, "MsiRouting_UnbindEntry_ReturnsSlot", TestMsiRoutingUnbindEntryReturnsSlot, NoRouting);
        TR.RunIf(routing, "MsiRouting_PrepareDevice_RemapDoesNotLeak", TestMsiRoutingRemapDoesNotLeak, NoRouting);
        TR.RunIf(liveFunction, "MsiXTable_LastDisconnect_DisablesAndMasks", TestMsiXTableLastDisconnectDisablesAndMasks, NoIdleFunction);
        TR.RunIf(liveFunction, "MsiXTable_ConnectAfterDisconnect_SameFunction", TestMsiXTableConnectAfterDisconnect, NoIdleFunction);
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

    // The kit's MSI-X table refuses an entry index outside the table rather
    // than write past it: TryConnect answers false, and Mask, Unmask and
    // Disconnect return without a write. Probed hardware-free on a scratch
    // table over the access of a function with no assigned base address
    // register: every call below stops at the index guard, before
    // configuration space or the table is reached, and a TryConnect that got
    // past a broken guard would still find no window to map. A missing range
    // check shows as the IndexOutOfRangeException the per-entry bookkeeping
    // throws, caught here as a failed assertion.
    private static void TestMsiXTableOutOfRangeEntryRefused()
    {
        if (s_barlessPciFunction is not PciAccess pci)
        {
            Assert.Fail("the gate found a function without an assigned base address register, so it must still be recorded");
            return;
        }

        PciMessageTable table = new(pci, ScratchTableCapability, MsiXProbeEntryCount);

        bool connectHigh = TableConnectRefused(table, MsiXProbeEntryCount);
        bool connectNegative = TableConnectRefused(table, -1);
        bool maskHigh = !Throws(() => table.Mask(MsiXProbeEntryCount));
        bool maskNegative = !Throws(() => table.Mask(-1));
        bool unmaskHigh = !Throws(() => table.Unmask(MsiXOutOfRangeIndex));
        bool disconnectHigh = !Throws(() => table.Disconnect(MsiXOutOfRangeIndex));

        Assert.True(connectHigh, "TryConnect must refuse index == EntryCount");
        Assert.True(connectNegative, "TryConnect must refuse a negative index");
        Assert.True(maskHigh, "Mask must ignore index == EntryCount");
        Assert.True(maskNegative, "Mask must ignore a negative index");
        Assert.True(unmaskHigh, "Unmask must ignore an out-of-range index");
        Assert.True(disconnectHigh, "Disconnect must ignore an out-of-range index");
    }

    // Single try/catch per helper (arm64 EH inlining quirk, see
    // TryAllocateVector). True when the table refused the entry without
    // throwing.
    private static bool TableConnectRefused(PciMessageTable table, int index)
    {
        try
        {
            return !table.TryConnect(index, NoopHandler);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Single try/catch per helper (arm64 EH inlining quirk, see
    // TryAllocateVector).
    private static bool Throws(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    // Binds entries of one oversized synthetic function until the vector /
    // LPI allocator runs dry, unbinds one, and proves the next bind succeeds
    // on the slot that came back. Then ReleaseDevice must return every slot
    // the function still held: a second function drains exactly as many.
    private static void TestMsiRoutingUnbindEntryReturnsSlot()
    {
        object? device = MsiRouting.PrepareDevice(SyntheticBus, SyntheticSlot, SyntheticFunction, MsiXMaxTableSize);
        int bound = BindUntilExhausted(device);
        bool exhausted = bound > 0 && bound < MsiXMaxTableSize;

        MsiRouting.UnbindEntry(device, bound / 2);
        bool reboundAfterUnbind = exhausted && TryBindEntry(device, bound);
        MsiRouting.ReleaseDevice(device);

        int boundAfterRelease = CountFreeSlots();

        Serial.WriteString("[Interrupts] MSI slots bound before exhaustion: ");
        Serial.WriteNumber((ulong)bound);
        Serial.WriteString(", after release: ");
        Serial.WriteNumber((ulong)boundAfterRelease);
        Serial.WriteString("\n");

        Assert.True(exhausted, "binding 2048 entries should run the vector / LPI allocator dry");
        Assert.True(reboundAfterUnbind, "UnbindEntry must return the entry's vector / LPI to the allocator");
        Assert.True(boundAfterRelease == bound, "ReleaseDevice must return every vector / LPI the function still held");
    }

    // The take-over case: an owner prepares and binds the function and
    // never releases its context (a table whose last disconnect never ran),
    // and a second owner prepares it again, binds and releases. Each take-over
    // must free what the first owner held: on ARM64 the DeviceID mapping and
    // its ITT, or every round orphans a page; on both arches the slot it
    // bound, or every round loses one (64 of x64's 175 vectors). The
    // abandoned contexts are released afterwards: the take-over already
    // retired them, so those releases must free nothing, where a second
    // teardown would free each ITT twice and push the free count up by a
    // page per round. One warm-up round keeps first-use heap growth out of
    // the page measurement.
    private static void TestMsiRoutingRemapDoesNotLeak()
    {
        MsiRouting.ReleaseDevice(RemapRound(out _));

        object?[] abandoned = new object?[RemapRounds];
        int slotsBefore = CountFreeSlots();
        ulong freeBefore = PageAllocator.FreePageCount;

        int failedRounds = 0;
        for (int i = 0; i < RemapRounds; i++)
        {
            abandoned[i] = RemapRound(out bool bothBound);
            if (!bothBound)
            {
                failedRounds++;
            }
        }

        ulong freeAfterTakeOvers = PageAllocator.FreePageCount;
        // Counted before the stale releases, which would hand back any slot
        // a take-over failed to free and hide the leak.
        int slotsAfterTakeOvers = CountFreeSlots();
        for (int i = 0; i < abandoned.Length; i++)
        {
            MsiRouting.ReleaseDevice(abandoned[i]);
        }

        ulong freeAfterStaleReleases = PageAllocator.FreePageCount;
        Serial.WriteString("[Interrupts] free pages before remap rounds: ");
        Serial.WriteNumber(freeBefore);
        Serial.WriteString(", after take-overs: ");
        Serial.WriteNumber(freeAfterTakeOvers);
        Serial.WriteString(", after stale releases: ");
        Serial.WriteNumber(freeAfterStaleReleases);
        Serial.WriteString("; free MSI slots before: ");
        Serial.WriteNumber((ulong)slotsBefore);
        Serial.WriteString(", after take-overs: ");
        Serial.WriteNumber((ulong)slotsAfterTakeOvers);
        Serial.WriteString("\n");

        Assert.True(failedRounds == 0, "every owner must be able to bind after preparing the function");
        Assert.True(freeAfterTakeOvers + RemapLeakSlackPages >= freeBefore, "re-preparing a mapped function must free the earlier ITT, not leak it");
        Assert.True(slotsAfterTakeOvers == slotsBefore, "re-preparing a function must free the vector / LPI its earlier owner bound");
        Assert.True(freeAfterStaleReleases <= freeAfterTakeOvers + RemapLeakSlackPages, "releasing a context a later PrepareDevice took over must free nothing twice");
    }

    // Returns the first owner's context, abandoned without a release.
    private static object? RemapRound(out bool bothBound)
    {
        object? first = MsiRouting.PrepareDevice(SyntheticBus, SyntheticSlot, SyntheticFunction, RemapEntryCount);
        bool firstBound = TryBindEntry(first, 0);
        object? second = MsiRouting.PrepareDevice(SyntheticBus, SyntheticSlot, SyntheticFunction, RemapEntryCount);
        bool secondBound = TryBindEntry(second, 0);
        MsiRouting.ReleaseDevice(second);

        bothBound = firstBound && secondBound;
        return first;
    }

    // Slots free right now: binds a fresh context of the synthetic function
    // until the vector / LPI allocator is dry, then releases it again.
    private static int CountFreeSlots()
    {
        object? probe = MsiRouting.PrepareDevice(SyntheticBus, SyntheticSlot, SyntheticFunction, MsiXMaxTableSize);
        int free = BindUntilExhausted(probe);
        MsiRouting.ReleaseDevice(probe);
        return free;
    }

    private static int BindUntilExhausted(object? device)
    {
        int bound = 0;
        while (bound < MsiXMaxTableSize && TryBindEntry(device, bound))
        {
            bound++;
        }

        return bound;
    }

    // Single try/catch per helper (arm64 EH inlining quirk, see
    // TryAllocateVector). False once the vector / LPI allocator is dry.
    private static bool TryBindEntry(object? device, int index)
    {
        try
        {
            MsiRouting.BindEntry(device, index, NoopHandler, 0, out _, out _);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // On a live function, through the kit's table: the first connect enables
    // MSI-X with Function Mask clear and INTx disabled and masks every entry,
    // then each connect programs its own entry and leaves it unmasked; a
    // second connect of a bound entry is refused; Mask and Unmask flip the
    // entry's vector control bit; a disconnect that leaves another entry
    // bound masks its entry and keeps MSI-X on, and the last one clears
    // Enable, leaves Function Mask set and every entry masked, and gives
    // back every vector / LPI the connects bound. While both entries are
    // unmasked the function may signal one: NoopHandler takes the message
    // and the dispatcher acknowledges it.
    private static void TestMsiXTableLastDisconnectDisablesAndMasks()
    {
        if (s_idleMsiXFunction is not PciAccess pci || pci.MessageTable is not PciMessageTable table)
        {
            Assert.Fail("the gate found an idle MSI-X function, so it must still be recorded");
            return;
        }

        byte capability = pci.FindCapability(MsiXCapabilityId);
        ushort messageControlRegister = (ushort)(capability + MsiXMessageControlOffset);
        ushort savedCommand = pci.ReadConfig16(PciCommandOffset);
        ushort savedMessageControl = pci.ReadConfig16(messageControlRegister);
        int slotsBefore = CountFreeSlots();

        bool connectedFirst = table.TryConnect(0, NoopHandler);
        bool connectedSecond = table.TryConnect(MsiXProbeEntryIndex, NoopHandler);
        if (!connectedFirst || !connectedSecond)
        {
            table.Disconnect(0);
            table.Disconnect(MsiXProbeEntryIndex);
            RestoreFunction(pci, messageControlRegister, savedMessageControl, savedCommand);
            Assert.Fail("both connects must succeed on an idle function with MSI-X and a routing backend");
            return;
        }

        bool reconnectRefused = !table.TryConnect(0, NoopHandler);
        ushort enabledMessageControl = pci.ReadConfig16(messageControlRegister);
        ushort enabledCommand = pci.ReadConfig16(PciCommandOffset);
        ulong entry = MsiXTableAddress(pci, capability);
        uint programmedAddress = Native.MMIO.Read32(entry + MsiXMessageAddressOffset);
        int maskedBound = CountMaskedEntries(entry, 0, MsiXProbeEntryCount);
        int maskedUnbound = CountMaskedEntries(entry, MsiXProbeEntryCount, table.EntryCount);

        table.Mask(0);
        uint maskedControl = Native.MMIO.Read32(entry + MsiXVectorControlOffset);
        table.Unmask(0);
        uint unmaskedControl = Native.MMIO.Read32(entry + MsiXVectorControlOffset);

        table.Disconnect(0);
        ushort partialMessageControl = pci.ReadConfig16(messageControlRegister);
        uint disconnectedControl = Native.MMIO.Read32(entry + MsiXVectorControlOffset);

        table.Disconnect(MsiXProbeEntryIndex);
        ushort disabledMessageControl = pci.ReadConfig16(messageControlRegister);
        int maskedAfterDisable = CountMaskedEntries(entry, 0, table.EntryCount);

        RestoreFunction(pci, messageControlRegister, savedMessageControl, savedCommand);
        int slotsAfter = CountFreeSlots();

        Assert.True(reconnectRefused, "a connect of an entry already bound must be refused");
        Assert.True((enabledMessageControl & MsiXEnableBit) != 0, "the first connect must set MSI-X Enable");
        Assert.True((enabledMessageControl & MsiXFunctionMaskBit) == 0, "the first connect must clear Function Mask");
        Assert.True((enabledCommand & PciCommandInterruptDisable) != 0, "the first connect must disable the function's INTx line");
        Assert.True(programmedAddress != 0, "a connect must write the message address");
        Assert.True(maskedBound == 0, "both connected entries must read unmasked, or the mask checks below prove nothing");
        Assert.True(maskedUnbound == table.EntryCount - MsiXProbeEntryCount, "the first connect must mask every entry no connect programmed");
        Assert.True((maskedControl & MsiXVectorControlMaskBit) != 0, "Mask must set the entry's mask bit");
        Assert.True((unmaskedControl & MsiXVectorControlMaskBit) == 0, "Unmask must clear the entry's mask bit");
        Assert.True((partialMessageControl & MsiXEnableBit) != 0, "a disconnect that leaves an entry bound must keep MSI-X enabled");
        Assert.True((disconnectedControl & MsiXVectorControlMaskBit) != 0, "a disconnect must mask its entry");
        Assert.True((disabledMessageControl & MsiXEnableBit) == 0, "the last disconnect must clear MSI-X Enable");
        Assert.True((disabledMessageControl & MsiXFunctionMaskBit) != 0, "the last disconnect must leave Function Mask set");
        Assert.True(maskedAfterDisable == table.EntryCount, "after the last disconnect every table entry must read masked");
        Assert.True(slotsAfter == slotsBefore, "the disconnects must give back every vector / LPI the connects bound");
    }

    // Entries first to end - 1 of the table at tableAddress whose vector
    // control mask bit reads set.
    private static int CountMaskedEntries(ulong tableAddress, int first, int end)
    {
        int masked = 0;
        for (int i = first; i < end; i++)
        {
            ulong control = tableAddress + (ulong)(i * MsiXEntryStride) + MsiXVectorControlOffset;
            if ((Native.MMIO.Read32(control) & MsiXVectorControlMaskBit) != 0)
            {
                masked++;
            }
        }

        return masked;
    }

    // A second owner can connect the same function after the last
    // disconnect disabled it: MSI-X on again with the Function Mask that
    // disconnect left set cleared, and the entry programmed afresh. The slot
    // count, which the two rounds must leave as they found it, shows that
    // each disconnect gave back the vector / LPI its connect bound; the
    // release of the routing context the first connect prepared is private
    // to the table and is not observed here. Entry 0's message is cleared in
    // between, so the address read back is the second connect's and not the
    // first one's.
    private static void TestMsiXTableConnectAfterDisconnect()
    {
        if (s_idleMsiXFunction is not PciAccess pci || pci.MessageTable is not PciMessageTable table)
        {
            Assert.Fail("the gate found an idle MSI-X function, so it must still be recorded");
            return;
        }

        byte capability = pci.FindCapability(MsiXCapabilityId);
        ushort messageControlRegister = (ushort)(capability + MsiXMessageControlOffset);
        ushort savedCommand = pci.ReadConfig16(PciCommandOffset);
        ushort savedMessageControl = pci.ReadConfig16(messageControlRegister);
        int slotsBefore = CountFreeSlots();

        if (!table.TryConnect(0, NoopHandler))
        {
            RestoreFunction(pci, messageControlRegister, savedMessageControl, savedCommand);
            Assert.Fail("the first connect must succeed on an idle function with MSI-X and a routing backend");
            return;
        }

        ulong entry = MsiXTableAddress(pci, capability);
        table.Disconnect(0);
        ushort disabledMessageControl = pci.ReadConfig16(messageControlRegister);

        // MSI-X is off and every entry masked: clearing the message cannot
        // make the function signal anything.
        Native.MMIO.Write32(entry + MsiXMessageAddressOffset, 0);
        Native.MMIO.Write32(entry + MsiXMessageUpperAddressOffset, 0);
        Native.MMIO.Write32(entry + MsiXMessageDataOffset, 0);

        bool reconnected = table.TryConnect(0, NoopHandler);
        ushort reconnectedMessageControl = pci.ReadConfig16(messageControlRegister);
        uint address = Native.MMIO.Read32(entry + MsiXMessageAddressOffset);
        uint control = Native.MMIO.Read32(entry + MsiXVectorControlOffset);
        table.Disconnect(0);

        RestoreFunction(pci, messageControlRegister, savedMessageControl, savedCommand);
        int slotsAfter = CountFreeSlots();

        Assert.True((disabledMessageControl & MsiXFunctionMaskBit) != 0, "the last disconnect must leave Function Mask set, or the clear checked below proves nothing");
        Assert.True(reconnected, "a connect after the last disconnect must succeed on the same function");
        Assert.True((reconnectedMessageControl & MsiXEnableBit) != 0, "the second connect must set MSI-X Enable again");
        Assert.True((reconnectedMessageControl & MsiXFunctionMaskBit) == 0, "the second connect must clear the Function Mask the disconnect left set");
        Assert.True(address != 0, "the entry of the reconnected function must program");
        Assert.True((control & MsiXVectorControlMaskBit) == 0, "the reprogrammed entry must be unmasked");
        Assert.True(slotsAfter == slotsBefore, "two connect and disconnect rounds must give back every vector / LPI they bound");
    }

    // The table's virtual address through the HHDM alias, decoded as the
    // kit's table decodes it: Table Offset/Table BIR names the base address
    // register and the offset into its window. Read only after a connect
    // succeeded, which mapped the window.
    private static ulong MsiXTableAddress(PciAccess pci, byte capability)
    {
        uint tableOffsetBir = pci.ReadConfig32((ushort)(capability + MsiXTableOffsetBirOffset));
        PciBar bar = pci.Bars[(int)(tableOffsetBir & MsiXTableBirMask)];
        return bar.Base + (tableOffsetBir & MsiXTableOffsetMask) + DeviceMemory.HhdmOffset();
    }

    // Puts back what the borrowed function's driver had: Message Control
    // (MSI-X off) and Command's INTx disable bit, which the first connect
    // sets and the last disconnect leaves for the owner to restore. Only
    // that bit of Command is written, under the mechanism's lock, so the
    // decode and bus master bits the driver owns are never replaced with a
    // stale copy.
    private static void RestoreFunction(PciAccess pci, ushort messageControlRegister, ushort savedMessageControl, ushort savedCommand)
    {
        pci.WriteConfig16(messageControlRegister, savedMessageControl);
        pci.SetInterruptDisable((savedCommand & PciCommandInterruptDisable) != 0);
    }

    // The first PCI function the kit published with no assigned base
    // address register (q35's and virt's host bridge at 00:00.0), for the
    // scratch table the bounds cell builds over its access: a TryConnect
    // that got past a broken index guard has no window to map there.
    private static PciAccess? FindBarlessPciFunction()
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].TryGetAccess(out PciAccess? pci) && !HasAssignedBar(pci))
            {
                return pci;
            }
        }

        return null;
    }

    // True when the kit sized any of the function's base address registers
    // as decoding something.
    private static bool HasAssignedBar(PciAccess pci)
    {
        ReadOnlySpan<PciBar> bars = pci.Bars;
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i].IsAssigned)
            {
                return true;
            }
        }

        return false;
    }

    // The first PCI function the kit published whose MSI-X capability is
    // not enabled, whose memory decoding is on, so its table is reachable,
    // and whose table holds the two entries the live cells connect. The kit
    // enables the capability at a driver's first message connect, so a
    // clear Enable bit means no driver holds a message on the function:
    // only such a function can be borrowed without taking interrupts from a
    // driver.
    private static PciAccess? FindIdleMsiXFunction()
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (!nodes[i].TryGetAccess(out PciAccess? pci)
                || pci.MessageTable is not PciMessageTable table
                || table.EntryCount < MsiXProbeEntryCount)
            {
                continue;
            }

            byte capability = pci.FindCapability(MsiXCapabilityId);
            ushort messageControl = pci.ReadConfig16((ushort)(capability + MsiXMessageControlOffset));
            ushort command = pci.ReadConfig16(PciCommandOffset);
            if ((messageControl & MsiXEnableBit) == 0 && (command & PciCommandMemorySpace) != 0)
            {
                return pci;
            }
        }

        return null;
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
            "the LAPIC timer vector must be outside the dynamic window: FreeVector+AllocateVector must never touch it");
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
