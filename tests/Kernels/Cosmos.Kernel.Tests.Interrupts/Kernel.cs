using System;
using System.Diagnostics;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
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
// stays covered by the Storage suite's NVMe assertions. The MSI-X teardown
// cells program a scratch table through the real binder, and only borrow a
// live function whose MSI-X nobody has enabled (x64: the default e1000e NIC,
// whose driver uses INTx), restoring its registers afterwards.
public class Kernel : Sys.Kernel
{
    /// <summary>Total tests per cell: 12 cross-arch + 6 arch-specific.</summary>
    private const int ExpectedTestCount = 18;

#if ARCH_X64
    /// <summary>RFLAGS.IF (bit 9): set while maskable interrupts are delivered.</summary>
    private const ulong InterruptEnableFlag = 1UL << 9;
#else
    /// <summary>DAIF.I (bit 7): set while IRQs are masked.</summary>
    private const ulong IrqMaskFlag = 1UL << 7;
#endif

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

    /// <summary>A function with an MSI-X capability nobody enabled and memory decode on, or null when the cell has none.</summary>
    private static PciDevice? s_idleMsiXFunction;

    /// <summary>Whether interrupts were enabled when <see cref="OnBoot"/> began.</summary>
    private static bool s_interruptsEnabledAtOnBoot;

    protected override void OnBoot()
    {
        // Read before the base class brings up the console, so this is the
        // state Global.StartKernel handed the kernel.
        s_interruptsEnabledAtOnBoot = AreInterruptsEnabled();
        base.OnBoot();
    }

    protected override void BeforeRun()
    {
        Serial.WriteString("[Interrupts] BeforeRun() reached!\n");

        // 12 cross-arch + 6 arch-specific = 18 tests per cell.
        TR.Start("Interrupt System Tests", expectedTests: ExpectedTestCount);

        s_idleMsiXFunction = FindIdleMsiXFunction();
        bool routing = MsiRouting.IsAvailable;
        bool liveFunction = routing && s_idleMsiXFunction is not null;
        const string NoRouting = "no MSI routing in this cell (GICv2 has no ITS)";
        const string NoIdleFunction = "needs MSI routing and an MSI-X function no driver has enabled (arm64: virtio-net owns the only one)";

        // ==================== Cross-arch ====================
        TR.Run("InterruptManager_Enabled", TestInterruptManagerEnabled);
        TR.Run("Boot_OnBootRunsWithInterruptsEnabled", TestOnBootRunsWithInterruptsEnabled);
        TR.Run("TimerSource_Registered", TestTimerSourceRegistered);
        TR.Run("VectorAllocator_ReturnsDistinctDynamicVectors", TestVectorAllocatorDistinct);
        TR.Run("VectorAllocator_ReusesFreedSlots", TestVectorAllocatorReusesFreedSlots);
        TR.Run("MsiX_TableAccessors_BoundsChecked", TestMsiXTableAccessorsBoundsChecked);
        TR.RunIf(routing, "MsiX_SetEntryMasked_LeavesEntryMasked", TestMsiXSetEntryMaskedLeavesEntryMasked, NoRouting);
        TR.RunIf(routing, "MsiRouting_UnbindEntry_ReturnsSlot", TestMsiRoutingUnbindEntryReturnsSlot, NoRouting);
        TR.RunIf(routing, "MsiRouting_PrepareDevice_RemapDoesNotLeak", TestMsiRoutingRemapDoesNotLeak, NoRouting);
        TR.RunIf(liveFunction, "MsiX_Disable_ClearsEnableAndMasksEntries", TestMsiXDisableClearsEnableAndMasks, NoIdleFunction);
        TR.RunIf(liveFunction, "MsiX_EnableDisableEnable_SameFunction", TestMsiXEnableDisableEnable, NoIdleFunction);
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

    // Global.StartKernel enables interrupts before it calls the kernel's
    // Start, so OnBoot already runs with them on. x64 has them on from the
    // IDT load in HAL bring-up, and ARM64 from storage bring-up when the
    // kernel has storage, so only an ARM64 kernel without storage reaches
    // StartKernel with them still masked.
    private static void TestOnBootRunsWithInterruptsEnabled()
    {
        Assert.True(s_interruptsEnabledAtOnBoot, "interrupts should already be enabled when OnBoot runs");
    }

    // Reads the interrupt-enable state without changing it: the save call
    // masks, and the restore puts back exactly what it read.
    private static bool AreInterruptsEnabled()
    {
        ulong flags = CpuNative.SaveIrqAndDisable();
        CpuNative.RestoreIrq(flags);
#if ARCH_X64
        return (flags & InterruptEnableFlag) != 0;
#else
        return (flags & IrqMaskFlag) == 0;
#endif
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

    // SetEntryMasked must bind and write the message like SetEntry but leave
    // the mask bit set, so a half-built driver never sees the vector. The
    // scratch table starts zeroed (every entry unmasked), so an entry that
    // stays unmasked reads back 0. SetEntry is probed on the other entry to
    // pin that existing callers still get an unmasked entry.
    private static unsafe void TestMsiXSetEntryMaskedLeavesEntryMasked()
    {
        void* table = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        Assert.True(table != null, "probe table allocation must succeed");
        if (table == null)
        {
            return;
        }

        object? device = MsiRouting.PrepareDevice(SyntheticBus, SyntheticSlot, SyntheticFunction, MsiXProbeEntryCount);
        MsiXContext ctx = new MsiXContext((ulong)table, MsiXProbeEntryCount, device);
        byte* entry = (byte*)table + MsiXProbeEntryOffset;

        MsiX.SetEntryMasked(ctx, MsiXProbeEntryIndex, NoopHandler);
        uint maskedControl = *(uint*)(entry + MsiXVectorControlOffset);
        uint maskedAddress = *(uint*)(entry + MsiXMessageAddressOffset);

        MsiX.SetEntry(ctx, 0, NoopHandler);
        uint unmaskedControl = *(uint*)((byte*)table + MsiXVectorControlOffset);

        MsiRouting.ReleaseDevice(device);
        PageAllocator.Free(table);

        Assert.True(maskedControl == MsiXVectorControlMaskBit, "SetEntryMasked must leave the entry's mask bit set");
        Assert.True(maskedAddress != 0, "SetEntryMasked must still write the message address");
        Assert.True(unmaskedControl == 0, "SetEntry must keep unmasking the entry it programs");
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

    // The second driver's MsiX.Enable case: an owner prepares and binds the
    // function, never releases it (no driver disables MSI-X today), and a
    // second owner prepares it again, binds and releases. Each take-over
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

    // On a live function: Enable turns MSI-X on, a masked program stays
    // masked in the device's own table, and Disable clears Enable, leaves
    // Function Mask set and masks every entry. Enable masks every entry
    // itself, so they are unmasked before Disable for the last check to
    // depend on it; Function Mask goes on first, through config space, so
    // the live function cannot signal an unmasked entry meanwhile.
    private static void TestMsiXDisableClearsEnableAndMasks()
    {
        if (s_idleMsiXFunction is not PciDevice device)
        {
            Assert.Fail("the gate found an idle MSI-X function, so it must still be recorded");
            return;
        }

        byte cap = device.FindCapability(MsiX.CapId);
        byte msgCtrlRegister = (byte)(cap + MsiXMessageControlOffset);
        ushort savedCommand = device.ReadRegister16((byte)Config.Command);
        ushort savedMsgCtrl = device.ReadRegister16(msgCtrlRegister);

        MsiXContext? enabled = MsiX.Enable(device);
        Assert.True(enabled is not null, "Enable must succeed on a function with MSI-X and a routing backend");
        if (enabled is not MsiXContext ctx)
        {
            return;
        }

        ushort enabledMsgCtrl = device.ReadRegister16(msgCtrlRegister);
        MsiX.SetEntryMasked(ctx, 0, NoopHandler);
        uint programmedControl = Native.MMIO.Read32(ctx.TableVirt + MsiXVectorControlOffset);

        device.WriteRegister16(msgCtrlRegister, (ushort)(device.ReadRegister16(msgCtrlRegister) | MsiXFunctionMaskBit));
        for (int i = 0; i < ctx.EntryCount; i++)
        {
            MsiX.UnmaskEntry(ctx, i);
        }

        int maskedBeforeDisable = CountMaskedEntries(ctx);

        MsiX.Disable(ctx);
        ushort disabledMsgCtrl = device.ReadRegister16(msgCtrlRegister);
        int maskedAfterDisable = CountMaskedEntries(ctx);

        RestoreFunction(device, msgCtrlRegister, savedMsgCtrl, savedCommand);

        Assert.True((enabledMsgCtrl & MsiXEnableBit) != 0, "Enable must set MSI-X Enable");
        Assert.True((programmedControl & MsiXVectorControlMaskBit) != 0, "SetEntryMasked must leave the live entry masked");
        Assert.True(maskedBeforeDisable == 0, "every entry must read back unmasked before Disable, or the mask check below proves nothing");
        Assert.True((disabledMsgCtrl & MsiXEnableBit) == 0, "Disable must clear MSI-X Enable");
        Assert.True((disabledMsgCtrl & MsiXFunctionMaskBit) != 0, "Disable must leave Function Mask set");
        Assert.True(maskedAfterDisable == ctx.EntryCount, "Disable must mask every table entry");
    }

    private static int CountMaskedEntries(MsiXContext ctx)
    {
        int masked = 0;
        for (int i = 0; i < ctx.EntryCount; i++)
        {
            ulong control = ctx.TableVirt + (ulong)(i * MsiXEntryStride) + MsiXVectorControlOffset;
            if ((Native.MMIO.Read32(control) & MsiXVectorControlMaskBit) != 0)
            {
                masked++;
            }
        }

        return masked;
    }

    // A second owner must be able to enable the same function after the
    // first disabled it: fresh routing context, MSI-X on again with Function
    // Mask clear, and an entry that programs. Disable must have released the
    // first routing context before the second Enable, which would otherwise
    // retire it itself; entry 0's message is cleared in between, so the
    // address read back is the second program's and not the first one's.
    private static void TestMsiXEnableDisableEnable()
    {
        if (s_idleMsiXFunction is not PciDevice device)
        {
            Assert.Fail("the gate found an idle MSI-X function, so it must still be recorded");
            return;
        }

        byte cap = device.FindCapability(MsiX.CapId);
        byte msgCtrlRegister = (byte)(cap + MsiXMessageControlOffset);
        ushort savedCommand = device.ReadRegister16((byte)Config.Command);
        ushort savedMsgCtrl = device.ReadRegister16(msgCtrlRegister);

        MsiXContext? first = MsiX.Enable(device);
        Assert.True(first is not null, "the first Enable must succeed");
        if (first is not MsiXContext firstCtx)
        {
            return;
        }

        MsiX.SetEntryMasked(firstCtx, 0, NoopHandler);
        MsiX.Disable(firstCtx);
        bool firstContextReleased = !TryBindEntry(firstCtx.DeviceCtx, 0);
        if (!firstContextReleased)
        {
            MsiRouting.ReleaseDevice(firstCtx.DeviceCtx);
        }

        // MSI-X is off and every entry masked: clearing the message cannot
        // make the function signal anything.
        ulong firstEntry = firstCtx.TableVirt;
        Native.MMIO.Write32(firstEntry + MsiXMessageAddressOffset, 0);
        Native.MMIO.Write32(firstEntry + MsiXMessageUpperAddressOffset, 0);
        Native.MMIO.Write32(firstEntry + MsiXMessageDataOffset, 0);

        MsiXContext? second = MsiX.Enable(device);
        Assert.True(second is not null, "Enable after Disable must succeed on the same function");
        if (second is not MsiXContext secondCtx)
        {
            RestoreFunction(device, msgCtrlRegister, savedMsgCtrl, savedCommand);
            return;
        }

        ushort reenabledMsgCtrl = device.ReadRegister16(msgCtrlRegister);
        MsiX.SetEntryMasked(secondCtx, 0, NoopHandler);
        ulong entry = secondCtx.TableVirt;
        uint address = Native.MMIO.Read32(entry + MsiXMessageAddressOffset);
        uint control = Native.MMIO.Read32(entry + MsiXVectorControlOffset);
        MsiX.Disable(secondCtx);

        RestoreFunction(device, msgCtrlRegister, savedMsgCtrl, savedCommand);

        Assert.True(firstContextReleased, "Disable must release the routing context, so binding through it fails");
        Assert.True((reenabledMsgCtrl & MsiXEnableBit) != 0, "the second Enable must set MSI-X Enable again");
        Assert.True((reenabledMsgCtrl & MsiXFunctionMaskBit) == 0, "the second Enable must clear the Function Mask Disable left set");
        Assert.True(address != 0, "an entry of the re-enabled function must program");
        Assert.True((control & MsiXVectorControlMaskBit) != 0, "the re-programmed entry must stay masked");
    }

    // Puts back what the borrowed function's driver had: Message Control
    // (MSI-X off) and the Command register, whose INTx Disable bit Enable
    // set and Disable leaves for the owner to restore.
    private static void RestoreFunction(PciDevice device, byte msgCtrlRegister, ushort savedMsgCtrl, ushort savedCommand)
    {
        device.WriteRegister16(msgCtrlRegister, savedMsgCtrl);
        device.WriteRegister16((byte)Config.Command, savedCommand);
    }

    // The first function with an MSI-X capability that is not enabled and
    // whose memory decode is on, so its table is reachable. Only such a
    // function can be borrowed without taking interrupts from a driver.
    private static PciDevice? FindIdleMsiXFunction()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return null;
        }

        for (int i = 0; i < PciManager.Count && i < devices.Length; i++)
        {
            PciDevice device = devices[i];
            byte cap = device.FindCapability(MsiX.CapId);
            if (cap == 0)
            {
                continue;
            }

            ushort msgCtrl = device.ReadRegister16((byte)(cap + MsiXMessageControlOffset));
            ushort command = device.ReadRegister16((byte)Config.Command);
            if ((msgCtrl & MsiXEnableBit) == 0 && (command & (ushort)PciCommand.Memory) != 0)
            {
                return device;
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
