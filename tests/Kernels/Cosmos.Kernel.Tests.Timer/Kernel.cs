using System;
using System.Diagnostics;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using BclTimer = System.Threading.Timer;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;
#if ARCH_X64
using Cosmos.Kernel.Core.X64.Cpu;
using Cosmos.Kernel.HAL.X64.Devices.Clock;
using Cosmos.Kernel.HAL.X64.Devices.Timer;
#else
using Cosmos.Kernel.HAL.ARM64.Devices.Clock;
#endif

namespace Cosmos.Kernel.Tests.Timer;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Log.WriteString("[Timer Tests] Starting test suite\n");

#if ARCH_X64
        // x64: Stopwatch (2) + PIT (3) + TimerManager (7) + LAPIC (3) + DateTime (4) + AlarmManager (3) + BCL Timer (4) = 26
        TR.Start("Timer Tests", expectedTests: 26);

        // PIT Tests (using Stopwatch for verification)
        TR.Run("PIT_Initialized", TestPITInitialized);
        TR.Run("PIT_Wait_100ms", TestPITWait100ms);
        TR.Run("PIT_Wait_Proportional", TestPITWaitProportional);

        // LAPIC Timer Tests
        TR.Run("LAPIC_Initialized", TestLAPICInitialized);
        TR.Run("LAPIC_Wait_100ms", TestLAPICWait100ms);
        TR.Run("LAPIC_Wait_Proportional", TestLAPICWaitProportional);

#else
        // ARM64: No PIT or LAPIC, just basic timer manager tests
        // Stopwatch (2) + TimerManager (7) + DateTime (4) + AlarmManager (3) + BCL Timer (4) = 20
        TR.Start("Timer Tests", expectedTests: 20);
#endif

        // Stopwatch/TSC Tests - must run first to verify timing source
        TR.Run("Stopwatch_Incrementing", TestStopwatchIncrementing);
        TR.Run("Stopwatch_Frequency", TestStopwatchFrequency);

        // TimerManager Tests
        TR.Run("TimerManager_Initialized", TestTimerManagerInitialized);
        TR.Run("TimerManager_Wait_500ms", TestTimerManagerWait500ms);
        TR.Run("TimerManager_Schedule_OneShot", TestScheduleOneShot);
        TR.Run("TimerManager_Schedule_Recurring", TestScheduleRecurring);
        TR.Run("TimerManager_Schedule_Cancel", TestScheduleCancel);
        TR.Run("Deferred_RejectNonPositivePeriod", TestRejectNonPositivePeriod);
        TR.Run("TimerManager_Callback_CancelsOtherTimers", TestCallbackCancelsOtherTimers);

        // Alarm Tests
        TR.Run("Alarm_Schedule_Fires", TestAlarmFires);
        TR.Run("Alarm_ScheduleRecurring", TestAlarmRecurring);
        TR.Run("Alarm_Cancel", TestAlarmRemove);

        // DateTime/RTC Tests
        TR.Run("RTC_Initialized", TestRTCInitialized);
        TR.Run("DateTime_Now_Valid", TestDateTimeNowValid);
        TR.Run("DateTime_Now_Incrementing", TestDateTimeNowIncrementing);
        TR.Run("DateTime_UtcNow", TestDateTimeUtcNow);

        // System.Threading.Timer / Task.Delay Tests (BCL TimerQueue on the
        // portable timer thread) — kept last so a TimerQueue failure cannot
        // mask the other results
        TR.Run("BclTimer_OneShot_Fires", TestBclTimerOneShot);
        TR.Run("BclTimer_Periodic_Fires", TestBclTimerPeriodic);
        TR.Run("BclTimer_Dispose_Stops", TestBclTimerDisposeStops);
        TR.Run("Task_Delay_Completes", TestTaskDelayCompletes);

        Log.WriteString("[Timer Tests] All tests completed\n");
        TR.Finish();
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== DateTime/RTC Tests ====================
    private static void TestRTCInitialized()
    {
        Assert.True(RTC.Instance != null, "RTC: Instance should be initialized");
        Assert.True(RTC.Instance!.IsAvailable, "RTC: Should be initialized");

        Log.WriteString("[Timer Tests] RTC boot time ticks: ");
        Log.WriteNumber((ulong)RTC.Instance.BootTimeTicks);
        Log.WriteString("\n");
    }

    private static void TestDateTimeNowValid()
    {
        DateTime now = DateTime.Now;

        Log.WriteString("[Timer Tests] DateTime.Now: ");
        Log.WriteNumber((ulong)now.Year);
        Log.WriteString("-");
        Log.WriteNumber((ulong)now.Month);
        Log.WriteString("-");
        Log.WriteNumber((ulong)now.Day);
        Log.WriteString(" ");
        Log.WriteNumber((ulong)now.Hour);
        Log.WriteString(":");
        Log.WriteNumber((ulong)now.Minute);
        Log.WriteString(":");
        Log.WriteNumber((ulong)now.Second);
        Log.WriteString("\n");

        // Year should be >= 2020 (reasonable minimum for RTC)
        Assert.True(now.Year >= 2020, "DateTime: Year should be >= 2020");
        // Month should be 1-12
        Assert.True(now.Month >= 1 && now.Month <= 12, "DateTime: Month should be 1-12");
        // Day should be 1-31
        Assert.True(now.Day >= 1 && now.Day <= 31, "DateTime: Day should be 1-31");
    }

    private static void TestDateTimeNowIncrementing()
    {
        DateTime dt1 = DateTime.Now;

        Thread.Sleep(100);

        DateTime dt2 = DateTime.Now;

        Log.WriteString("[Timer Tests] DateTime dt1 ticks: ");
        Log.WriteNumber((ulong)dt1.Ticks);
        Log.WriteString(", dt2 ticks: ");
        Log.WriteNumber((ulong)dt2.Ticks);
        Log.WriteString("\n");

        Assert.True(dt2 > dt1, "DateTime: Now should increment over time");

        // The difference should be roughly 100ms (1,000,000 ticks = 100ms)
        long tickDiff = dt2.Ticks - dt1.Ticks;
        // Allow 50ms to 200ms range (500,000 to 2,000,000 ticks)
        bool inRange = tickDiff >= 500_000 && tickDiff <= 2_000_000;
        Assert.True(inRange, "DateTime: 100ms wait should show ~100ms elapsed");
    }

    private static void TestDateTimeUtcNow()
    {
        DateTime utcNow = DateTime.UtcNow;

        Log.WriteString("[Timer Tests] DateTime.UtcNow: ");
        Log.WriteNumber((ulong)utcNow.Year);
        Log.WriteString("-");
        Log.WriteNumber((ulong)utcNow.Month);
        Log.WriteString("-");
        Log.WriteNumber((ulong)utcNow.Day);
        Log.WriteString("\n");

        // Should have Utc kind
        Assert.True(utcNow.Kind == DateTimeKind.Utc, "DateTime: UtcNow should have Utc kind");
        // Year should be valid
        Assert.True(utcNow.Year >= 2020, "DateTime: UtcNow year should be >= 2020");
    }

    // ==================== Stopwatch Tests ====================
    private static void TestStopwatchIncrementing()
    {
        // Read timestamp twice and verify it's incrementing
        long ts1 = Stopwatch.GetTimestamp();

        // Small busy loop to ensure time passes
        for (int i = 0; i < 10000; i++) { }

        long ts2 = Stopwatch.GetTimestamp();

        Log.WriteString("[Timer Tests] Stopwatch ts1: ");
        Log.WriteNumber((ulong)ts1);
        Log.WriteString(", ts2: ");
        Log.WriteNumber((ulong)ts2);
        Log.WriteString("\n");

        Assert.True(ts2 > ts1, "Stopwatch: GetTimestamp() should return incrementing values");
    }
    private static void TestStopwatchFrequency()
    {
        long freq = Stopwatch.Frequency;

        Log.WriteString("[Timer Tests] Stopwatch.Frequency: ");
        Log.WriteNumber((ulong)freq);
        Log.WriteString(" Hz\n");

        // TSC frequency should be at least 100 MHz on x64; ARM64 generic timer is typically 62.5 MHz
#if ARCH_X64
        Assert.True(freq >= 100_000_000, "Stopwatch: Frequency should be >= 100 MHz");
#else
        Assert.True(freq >= 1_000_000, "Stopwatch: Frequency should be >= 1 MHz");
#endif
        Assert.True(Stopwatch.IsHighResolution, "Stopwatch: Should be high resolution");
    }

    // ==================== TimerManager Tests ====================

    private static void TestTimerManagerInitialized()
    {
        // IsInitialized is exactly "a timer device is registered": the ring
        // publishes the fact, so the suite does not read the device itself.
        Assert.True(TimerManager.IsInitialized, "TimerManager: a timer device should be registered");
    }

    private static void TestTimerManagerWait500ms()
    {
        long tsStart = Stopwatch.GetTimestamp();
        TimerManager.Wait(500);
        long tsEnd = Stopwatch.GetTimestamp();

        long elapsed = tsEnd - tsStart;
        long frequency = Stopwatch.Frequency;
        long elapsedMs = (elapsed * 1000) / frequency;

        Log.WriteString("[Timer Tests] TimerManager Wait(500ms) - elapsed ms: ");
        Log.WriteNumber((ulong)elapsedMs);
        Log.WriteString("\n");

        // Check if within tolerance (250-1000ms for 500ms wait)
        bool inRange = elapsedMs >= 250 && elapsedMs <= 1000;
        Assert.True(inRange, "TimerManager: Wait(500ms) should complete in roughly 500ms");
    }

    private static volatile int s_oneShotFireCount;
    private static volatile int s_recurringFireCount;
    private static volatile int s_cancelledFireCount;

    private static void TestScheduleOneShot()
    {
        s_oneShotFireCount = 0;
        SoftwareTimer? timer = TimerManager.Schedule(static () => s_oneShotFireCount++, TimeSpan.FromMilliseconds(50));

        Assert.True(timer != null, "Schedule: should return a timer");
        Assert.True(timer!.IsActive, "Schedule: timer should be active before firing");

        TimerManager.Wait(300);

        Log.WriteString("[Timer Tests] One-shot fire count: ");
        Log.WriteNumber((ulong)s_oneShotFireCount);
        Log.WriteString("\n");

        Assert.True(s_oneShotFireCount == 1, "Schedule: one-shot timer should fire exactly once");
        Assert.True(!timer.IsActive, "Schedule: one-shot timer should be inactive after firing");
        Assert.True(!TimerManager.Cancel(timer), "Schedule: fired timer should no longer be cancellable");
    }

    private static void TestScheduleRecurring()
    {
        s_recurringFireCount = 0;
        SoftwareTimer? timer = TimerManager.ScheduleRecurring(static () => s_recurringFireCount++, TimeSpan.FromMilliseconds(50));

        Assert.True(timer != null, "ScheduleRecurring: should return a timer");

        TimerManager.Wait(500);

        int count = s_recurringFireCount;
        Log.WriteString("[Timer Tests] Recurring fire count after 500ms: ");
        Log.WriteNumber((ulong)count);
        Log.WriteString("\n");

        Assert.True(TimerManager.Cancel(timer), "ScheduleRecurring: pending timer should be cancellable");

        Assert.True(count >= 3, "ScheduleRecurring: timer should fire repeatedly (>= 3 in 500ms)");
        Assert.True(!timer!.IsActive, "ScheduleRecurring: cancelled timer should be inactive");
    }

    private static void TestScheduleCancel()
    {
        s_cancelledFireCount = 0;
        SoftwareTimer? timer = TimerManager.Schedule(static () => s_cancelledFireCount++, TimeSpan.FromMilliseconds(200));

        Assert.True(TimerManager.Cancel(timer), "Cancel: pending timer should be cancellable");
        Assert.True(!TimerManager.Cancel(timer), "Cancel: Cancel should return false the second time");

        TimerManager.Wait(400);

        Assert.True(s_cancelledFireCount == 0, "Cancel: cancelled timer should not fire");
        Assert.True(timer != null && !timer.IsActive, "Cancel: cancelled timer should be inactive");
    }

    private static void TestRejectNonPositivePeriod()
    {
        // A zero or negative period reloads to 0 and fires on every tick, so
        // both managers refuse it and both accept a zero one-shot delay, which
        // simply fires on the next tick.
        Assert.True(
            TimerManager.ScheduleRecurring(static () => { }, TimeSpan.Zero) == null,
            "ScheduleRecurring: a zero period should be refused");
        Assert.True(
            TimerManager.ScheduleRecurring(static () => { }, TimeSpan.FromMilliseconds(-50)) == null,
            "ScheduleRecurring: a negative period should be refused");
        Assert.True(
            AlarmManager.ScheduleRecurring(static () => { }, TimeSpan.Zero) == 0,
            "Alarm ScheduleRecurring: a zero period should be refused");
        Assert.True(
            AlarmManager.ScheduleRecurring(static () => { }, TimeSpan.FromMilliseconds(-50)) == 0,
            "Alarm ScheduleRecurring: a negative period should be refused");

        SoftwareTimer? timer = TimerManager.Schedule(static () => { }, TimeSpan.Zero);
        Assert.True(timer != null, "Schedule: a zero delay should still be scheduled");
        TimerManager.Cancel(timer);

        // Sub-millisecond periods used to round to zero and be refused; the
        // conversion is exact now, so the alarm is accepted and simply fires no
        // faster than the scheduler tick.
        ulong id = AlarmManager.ScheduleRecurring(static () => { }, TimeSpan.FromTicks(5000));
        Assert.True(id != 0, "Alarm ScheduleRecurring: a sub-millisecond period should be accepted");
        Assert.True(AlarmManager.Cancel(id), "Alarm ScheduleRecurring: the sub-millisecond alarm should be cancellable");
    }

    private static volatile int s_reentrantFireCount;
    private static SoftwareTimer? s_reentrantFirst;
    private static SoftwareTimer? s_reentrantSecond;

    private static void TestCallbackCancelsOtherTimers()
    {
        // Cancelling from inside a timer callback mutates the registry the
        // device is walking. The two victims are registered first, so they sit
        // below the canceller in the registry and removing them shifts every
        // index the walk has not reached yet.
        s_reentrantFireCount = 0;
        s_reentrantFirst = TimerManager.Schedule(static () => s_reentrantFireCount += 100, TimeSpan.FromSeconds(10));
        s_reentrantSecond = TimerManager.Schedule(static () => s_reentrantFireCount += 100, TimeSpan.FromSeconds(10));

        SoftwareTimer? canceller = TimerManager.Schedule(
            static () =>
            {
                TimerManager.Cancel(s_reentrantFirst);
                TimerManager.Cancel(s_reentrantSecond);
                s_reentrantFireCount++;
            },
            TimeSpan.FromMilliseconds(50));

        Assert.True(canceller != null, "Reentrancy: the cancelling timer should be scheduled");

        TimerManager.Wait(300);

        Assert.True(s_reentrantFireCount == 1, "Reentrancy: only the cancelling timer should have fired");
        Assert.True(!TimerManager.Cancel(s_reentrantFirst), "Reentrancy: the first victim should be gone");
        Assert.True(!TimerManager.Cancel(s_reentrantSecond), "Reentrancy: the second victim should be gone");
        Assert.True(!canceller!.IsActive, "Reentrancy: the one-shot canceller should be inactive");
    }

    // ==================== Alarm Tests ====================

    private static volatile int s_alarmFireCount;
    private static volatile int s_alarmRecurringCount;
    private static volatile int s_alarmRemovedCount;

    private static void TestAlarmFires()
    {
        s_alarmFireCount = 0;
        ulong id = AlarmManager.Schedule(static () => s_alarmFireCount++, TimeSpan.FromMilliseconds(50));

        Assert.True(id != 0, "Alarm: Schedule should return a valid ID");

        TimerManager.Wait(500);

        Log.WriteString("[Timer Tests] Alarm fire count: ");
        Log.WriteNumber((ulong)s_alarmFireCount);
        Log.WriteString("\n");

        Assert.True(s_alarmFireCount == 1, "Alarm: one-shot alarm should fire exactly once");
        Assert.True(!AlarmManager.Cancel(id), "Alarm: fired alarm should no longer be pending");
    }

    private static void TestAlarmRecurring()
    {
        s_alarmRecurringCount = 0;
        ulong id = AlarmManager.ScheduleRecurring(static () => s_alarmRecurringCount++, TimeSpan.FromMilliseconds(50));

        Assert.True(id != 0, "Alarm: ScheduleRecurring should return a valid ID");

        TimerManager.Wait(500);

        int count = s_alarmRecurringCount;
        Log.WriteString("[Timer Tests] Recurring alarm count after 500ms: ");
        Log.WriteNumber((ulong)count);
        Log.WriteString("\n");

        Assert.True(AlarmManager.Cancel(id), "Alarm: recurring alarm should be cancellable");
        Assert.True(count >= 3, "Alarm: recurring alarm should fire repeatedly (>= 3 in 500ms)");
    }

    private static void TestAlarmRemove()
    {
        s_alarmRemovedCount = 0;
        ulong id = AlarmManager.Schedule(static () => s_alarmRemovedCount++, TimeSpan.FromMilliseconds(200));

        Assert.True(AlarmManager.Cancel(id), "Alarm: pending alarm should be cancellable");

        TimerManager.Wait(400);

        Assert.True(s_alarmRemovedCount == 0, "Alarm: cancelled alarm should not fire");
        Assert.True(!AlarmManager.Cancel(id), "Alarm: Cancel should return false for unknown ID");
    }

    // ==================== System.Threading.Timer Tests ====================

    private static volatile int s_bclOneShotCount;
    private static volatile int s_bclPeriodicCount;
    private static volatile int s_bclDisposedCount;

    private static void TestBclTimerOneShot()
    {
        s_bclOneShotCount = 0;
        using (BclTimer timer = new(static _ => s_bclOneShotCount++, null, 50, Timeout.Infinite))
        {
            TimerManager.Wait(500);
        }

        Log.WriteString("[Timer Tests] BCL one-shot fire count: ");
        Log.WriteNumber((ulong)s_bclOneShotCount);
        Log.WriteString("\n");

        Assert.True(s_bclOneShotCount == 1, "System.Threading.Timer: one-shot should fire exactly once");
    }

    private static void TestBclTimerPeriodic()
    {
        s_bclPeriodicCount = 0;
        int count;
        using (BclTimer timer = new(static _ => s_bclPeriodicCount++, null, 50, 50))
        {
            TimerManager.Wait(500);
            count = s_bclPeriodicCount;
        }

        Log.WriteString("[Timer Tests] BCL periodic fire count after 500ms: ");
        Log.WriteNumber((ulong)count);
        Log.WriteString("\n");

        Assert.True(count >= 3, "System.Threading.Timer: periodic should fire repeatedly (>= 3 in 500ms)");
    }

    private static void TestBclTimerDisposeStops()
    {
        s_bclDisposedCount = 0;
        BclTimer timer = new(static _ => s_bclDisposedCount++, null, 200, Timeout.Infinite);
        timer.Dispose();

        TimerManager.Wait(400);

        Assert.True(s_bclDisposedCount == 0, "System.Threading.Timer: disposed timer should not fire");
    }

    private static void TestTaskDelayCompletes()
    {
        long tsStart = Stopwatch.GetTimestamp();
        Task delay = Task.Delay(100);

        // Bounded poll so a broken TimerQueue fails the assertion instead of
        // hanging the suite
        long elapsedMs = 0;
        while (!delay.IsCompleted && elapsedMs < 2000)
        {
            TimerManager.Wait(10);
            elapsedMs = (Stopwatch.GetTimestamp() - tsStart) * 1000 / Stopwatch.Frequency;
        }

        Log.WriteString("[Timer Tests] Task.Delay(100) completed=");
        Log.WriteNumber((ulong)(delay.IsCompleted ? 1 : 0));
        Log.WriteString(" after ms: ");
        Log.WriteNumber((ulong)elapsedMs);
        Log.WriteString("\n");

        Assert.True(delay.IsCompleted, "Task.Delay(100) should complete");
        Assert.True(elapsedMs >= 50, "Task.Delay(100) should take at least ~100ms");
    }

#if ARCH_X64

    // ==================== PIT Tests ====================

    private static void TestPITInitialized()
    {
        Assert.True(PIT.Instance != null, "PIT: Instance should be initialized");
    }

    private static void TestPITWait100ms()
    {
        long tsStart = Stopwatch.GetTimestamp();
        PIT.Instance!.Wait(100);
        long tsEnd = Stopwatch.GetTimestamp();

        long elapsed = tsEnd - tsStart;
        long frequency = Stopwatch.Frequency;

        // Calculate elapsed milliseconds: (elapsed * 1000) / frequency
        long elapsedMs = (elapsed * 1000) / frequency;

        Log.WriteString("[Timer Tests] PIT Wait(100ms) - elapsed ticks: ");
        Log.WriteNumber((ulong)elapsed);
        Log.WriteString(", elapsed ms: ");
        Log.WriteNumber((ulong)elapsedMs);
        Log.WriteString("\n");

        // Check if within tolerance (50-200ms for 100ms wait)
        bool inRange = elapsedMs >= 50 && elapsedMs <= 200;
        Assert.True(inRange, "PIT: Wait(100ms) should complete in roughly 100ms");
    }

    private static void TestPITWaitProportional()
    {
        // Test that 200ms wait takes roughly 2x the ticks of 100ms wait
        long tsStart1 = Stopwatch.GetTimestamp();
        PIT.Instance!.Wait(100);
        long tsEnd1 = Stopwatch.GetTimestamp();
        long elapsed100ms = tsEnd1 - tsStart1;

        long tsStart2 = Stopwatch.GetTimestamp();
        PIT.Instance!.Wait(200);
        long tsEnd2 = Stopwatch.GetTimestamp();
        long elapsed200ms = tsEnd2 - tsStart2;

        // 200ms should be roughly 2x of 100ms (allow 50% tolerance for ratio)
        // ratio * 100 should be between 150 and 250
        long ratio100 = (elapsed200ms * 100) / elapsed100ms;

        Log.WriteString("[Timer Tests] PIT 100ms ticks: ");
        Log.WriteNumber((ulong)elapsed100ms);
        Log.WriteString(", 200ms ticks: ");
        Log.WriteNumber((ulong)elapsed200ms);
        Log.WriteString(", ratio*100: ");
        Log.WriteNumber((ulong)ratio100);
        Log.WriteString("\n");

        bool proportional = ratio100 >= 150 && ratio100 <= 250;
        Assert.True(proportional, "PIT: 200ms should take ~2x ticks of 100ms");
    }

    // ==================== LAPIC Timer Tests ====================

    private static void TestLAPICInitialized()
    {
        Assert.True(LocalApic.IsInitialized, "LAPIC: Should be initialized");
        Assert.True(LocalApic.IsTimerCalibrated, "LAPIC: Timer should be calibrated");

        Log.WriteString("[Timer Tests] LAPIC ticks/ms: ");
        Log.WriteNumber(LocalApic.TicksPerMs);
        Log.WriteString("\n");
    }

    private static void TestLAPICWait100ms()
    {
        long tsStart = Stopwatch.GetTimestamp();
        LocalApic.Wait(100);
        long tsEnd = Stopwatch.GetTimestamp();

        long elapsed = tsEnd - tsStart;
        long frequency = Stopwatch.Frequency;
        long elapsedMs = (elapsed * 1000) / frequency;

        Log.WriteString("[Timer Tests] LAPIC Wait(100ms) - elapsed ms: ");
        Log.WriteNumber((ulong)elapsedMs);
        Log.WriteString("\n");

        // Check if within tolerance (50-200ms for 100ms wait)
        bool inRange = elapsedMs >= 50 && elapsedMs <= 200;
        Assert.True(inRange, "LAPIC: Wait(100ms) should complete in roughly 100ms");
    }

    private static void TestLAPICWaitProportional()
    {
        // Test that 200ms wait takes roughly 2x the ticks of 100ms wait
        long tsStart1 = Stopwatch.GetTimestamp();
        LocalApic.Wait(100);
        long tsEnd1 = Stopwatch.GetTimestamp();
        long elapsed100ms = tsEnd1 - tsStart1;

        long tsStart2 = Stopwatch.GetTimestamp();
        LocalApic.Wait(200);
        long tsEnd2 = Stopwatch.GetTimestamp();
        long elapsed200ms = tsEnd2 - tsStart2;

        // ratio * 100 should be between 150 and 250
        long ratio100 = (elapsed200ms * 100) / elapsed100ms;

        Log.WriteString("[Timer Tests] LAPIC 100ms ticks: ");
        Log.WriteNumber((ulong)elapsed100ms);
        Log.WriteString(", 200ms ticks: ");
        Log.WriteNumber((ulong)elapsed200ms);
        Log.WriteString(", ratio*100: ");
        Log.WriteNumber((ulong)ratio100);
        Log.WriteString("\n");

        bool proportional = ratio100 >= 150 && ratio100 <= 250;
        Assert.True(proportional, "LAPIC: 200ms should take ~2x ticks of 100ms");
    }

#endif
}
