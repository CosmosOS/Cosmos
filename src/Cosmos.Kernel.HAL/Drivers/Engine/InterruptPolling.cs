// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// Polled interrupts: a driver whose function has no MSI-X, or whose
/// platform cannot route it, has its handler called from the platform
/// timer's interrupt instead, on every tick. That is about every 55 ms on
/// x64, where the PIT runs one-shot at its longest count and is reloaded
/// on each tick, and every 10 ms on ARM64, whose generic timer ticks at the
/// scheduler's quantum. Only a timer that actually ticks can poll, so the
/// driver pass checks that once, before the first probe.
/// </summary>
/// <remarks>
/// On ARM64 the generic timer is started by the scheduler, so it never
/// ticks in a kernel built without one. On x64 the PIT ticks with or
/// without the scheduler, but only through the I/O APIC, which ACPI's MADT
/// describes: with ACPI off it never ticks either.
/// </remarks>
internal static class InterruptPolling
{
    /// <summary>
    /// How long the check waits for a tick, in milliseconds of Stopwatch
    /// time: two x64 PIT periods. Registering the check's timer restarts
    /// the PIT's count, which can push the next tick back by a full period.
    /// </summary>
    private const long TickWaitMilliseconds = 120;

    private const long MillisecondsPerSecond = 1000;

    /// <summary>The platform timer, once it was seen ticking; null before the check and when it did not tick.</summary>
    private static ITimerDevice? s_timer;

    /// <summary>Ticks the check saw. Written from the timer interrupt.</summary>
    private static int s_checkTicks;

    /// <summary>True once <see cref="CheckTimerTicks"/> saw the platform timer tick.</summary>
    internal static bool IsTimerTicking => s_timer is not null;

    /// <summary>
    /// Registers a timer on the platform timer device and waits up to
    /// <see cref="TickWaitMilliseconds"/> of Stopwatch time for it to fire.
    /// The Stopwatch reads a counter that runs whether or not any interrupt
    /// comes (the TSC on x64, the generic timer's count on ARM64), so the
    /// wait ends either way. Runs once, from the driver pass, with interrupts on.
    /// </summary>
    internal static void CheckTimerTicks()
    {
        if (!CosmosFeatures.TimerEnabled)
        {
            Serial.WriteString("[Drivers] Timer compiled out: no polled interrupts\n");
            return;
        }

        // The platform keeps one timer device and hands the same one back on
        // every call: the one TimerManager drives, not a new one.
        IPlatformInitializer? platform = PlatformHAL.Initializer;
        ITimerDevice? timer = platform?.CreateTimer();
        if (timer is null)
        {
            Serial.WriteString("[Drivers] No platform timer: no polled interrupts\n");
            return;
        }

        // A zero period fires on every tick, whatever the timer's rate.
        SoftwareTimer check = new(CountCheckTick, 0, recurring: true);
        Register(timer, check);

        long waitTicks = Stopwatch.Frequency / MillisecondsPerSecond * TickWaitMilliseconds;
        long startedAt = Stopwatch.GetTimestamp();
        while (Volatile.Read(ref s_checkTicks) == 0 && Stopwatch.GetTimestamp() - startedAt < waitTicks)
        {
        }

        timer.UnregisterTimer(check);
        if (Volatile.Read(ref s_checkTicks) == 0)
        {
            Serial.WriteString("[Drivers] The platform timer is not ticking: no polled interrupts\n");
            return;
        }

        s_timer = timer;
        Serial.WriteString("[Drivers] The platform timer ticks: interrupts can be polled\n");
    }

    /// <summary>
    /// Calls <paramref name="callback"/> from the platform timer's interrupt
    /// on every tick, until <see cref="Stop"/>. Thread context.
    /// </summary>
    /// <param name="callback">What to call on each tick, in interrupt context.</param>
    /// <param name="pollTimer">The registered timer, to hand to <see cref="Stop"/>, when the call returns true.</param>
    /// <returns>False when the check found no ticking timer.</returns>
    internal static bool TryStart(Action callback, [NotNullWhen(true)] out SoftwareTimer? pollTimer)
    {
        if (s_timer is not { } timer)
        {
            pollTimer = null;
            return false;
        }

        pollTimer = new SoftwareTimer(callback, 0, recurring: true);
        Register(timer, pollTimer);
        return true;
    }

    /// <summary>
    /// Stops calling the callback of <paramref name="pollTimer"/>. A tick
    /// already running it finishes. Thread context.
    /// </summary>
    internal static void Stop(SoftwareTimer pollTimer) => s_timer?.UnregisterTimer(pollTimer);

    /// <summary>
    /// Registers <paramref name="softwareTimer"/> with interrupts masked
    /// throughout. A timer device may reprogram its hardware as it adds one
    /// (the x64 PIT reloads its counter with three port writes), and a tick
    /// handled between two of those writes reloads it from the interrupt
    /// mid-sequence, which leaves the counter stopped.
    /// </summary>
    private static void Register(ITimerDevice timer, SoftwareTimer softwareTimer)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            timer.RegisterTimer(softwareTimer);
        }
    }

    /// <summary>The check's timer callback. Interrupt context.</summary>
    private static void CountCheckTick() => s_checkTicks++;
}
