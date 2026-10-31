// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.RISCV64.Bridge;
using Cosmos.Kernel.Core.RISCV64.Cpu;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Timers;

namespace Cosmos.Kernel.HAL.RISCV64;

/// <summary>
/// The RISC-V supervisor timer: the time CSR counts at the timebase
/// frequency and the supervisor timer interrupt fires once it passes the
/// comparator. S-mode programs the comparator through the SBI TIME
/// extension, which writes stimecmp itself on a CPU with Sstc; one ecall per
/// tick at the scheduler's 100 Hz is not worth detecting Sstc for. Writing a
/// new comparator also clears the pending interrupt.
/// </summary>
internal class SbiTimer : TickSource
{
    /// <summary>
    /// Singleton instance of the timer.
    /// </summary>
    public static SbiTimer? Instance { get; private set; }

    /// <summary>
    /// Default timer period: 10ms (100 Hz) for scheduling.
    /// </summary>
    public const ulong DefaultPeriodNs = 10_000_000;

    /// <summary>Time CSR frequency in Hz (the device tree's timebase-frequency).</summary>
    private ulong _timerFrequency;

    /// <summary>Configured timer period in nanoseconds.</summary>
    private ulong _periodNs;

    /// <summary>Time CSR ticks per period.</summary>
    private ulong _ticksPerPeriod;

    /// <summary>Timer tick counter for debugging.</summary>
    private static uint s_timerTickCount;

    /// <summary>The time CSR frequency in Hz.</summary>
    public ulong TimerFrequency => _timerFrequency;

    /// <inheritdoc/>
    public override uint Frequency => (uint)(1_000_000_000UL / _periodNs);

    /// <summary>Gets the timer period in nanoseconds.</summary>
    public ulong PeriodNs => _periodNs;

    /// <summary>
    /// Initialize the timer: reads the timebase and sets the default period.
    /// The timer interrupt stays masked until <see cref="Start"/>.
    /// </summary>
    public override void Initialize()
    {
        Serial.Write("[SbiTimer] Initializing RISC-V supervisor timer...\n");

        Instance = this;

        _timerFrequency = Timebase.Frequency;
        Serial.Write("[SbiTimer] Timebase frequency: ");
        Serial.WriteNumber(_timerFrequency);
        Serial.Write(" Hz\n");

        SetPeriod(DefaultPeriodNs);

        Serial.Write("[SbiTimer] Initialized\n");
    }

    /// <summary>
    /// Sets the timer period in nanoseconds.
    /// </summary>
    /// <param name="periodNs">Period in nanoseconds.</param>
    public void SetPeriod(ulong periodNs)
    {
        _periodNs = periodNs;
        _ticksPerPeriod = (_timerFrequency * periodNs) / 1_000_000_000UL;

        Serial.Write("[SbiTimer] Period: ");
        Serial.WriteNumber(periodNs / 1_000_000);
        Serial.Write(" ms, ticks per period: ");
        Serial.WriteNumber(_ticksPerPeriod);
        Serial.Write("\n");
    }

    /// <inheritdoc/>
    public override bool SetFrequency(uint frequency)
    {
        // Above the timebase the period rounds down to fewer than one tick:
        // the handler would re-arm the comparator at the current time and
        // the interrupt would fire again on every sret.
        if (frequency == 0 || frequency > _timerFrequency)
        {
            return false;
        }

        SetPeriod(1_000_000_000UL / frequency);
        return true;
    }

    /// <summary>
    /// Starts the timer: arms the comparator one period ahead and unmasks
    /// the supervisor timer interrupt.
    /// </summary>
    public void Start()
    {
        Serial.Write("[SbiTimer] Starting timer...\n");
        Sbi.SetTimer(CsrNative.ReadTime() + _ticksPerPeriod);
        CsrNative.SetSie(RISCV64InterruptController.InterruptBitTimer);
        Serial.Write("[SbiTimer] Timer started\n");
    }

    /// <summary>
    /// Stops the timer: masks the interrupt and moves the comparator out of reach.
    /// </summary>
    public void Stop()
    {
        CsrNative.ClearSie(RISCV64InterruptController.InterruptBitTimer);
        Sbi.SetTimer(ulong.MaxValue);
        Serial.Write("[SbiTimer] Timer stopped\n");
    }

    /// <summary>
    /// Registers the handler of the supervisor timer interrupt.
    /// </summary>
    public void RegisterIRQHandler()
    {
        Serial.Write("[SbiTimer] Registering timer handler at vector ");
        Serial.WriteNumber(RISCV64InterruptController.TimerVector);
        Serial.Write("\n");
        InterruptManager.SetHandler(RISCV64InterruptController.TimerVector, HandleIRQ);
    }

    /// <summary>
    /// Handles the timer interrupt.
    /// </summary>
    private static void HandleIRQ(ref IRQContext ctx)
    {
        if (Instance is null)
        {
            return;
        }

        s_timerTickCount++;

        // Re-arm from now rather than from the last deadline: a tick the
        // kernel was too slow to take is dropped instead of firing in a burst.
        Sbi.SetTimer(CsrNative.ReadTime() + Instance._ticksPerPeriod);

        // Advance software timers and invoke the OnTick handler
        Instance.HandleTick(Instance._periodNs);

        // The saved context's SP, which the scheduler switches from.
        nuint currentSp = RISCV64InterruptController.SavedContextStackPointer(ref ctx);

        if (s_timerTickCount <= 5 || s_timerTickCount % 100 == 0)
        {
            Serial.Write("[SbiTimer] Tick ");
            Serial.WriteNumber(s_timerTickCount);
            Serial.Write(" SP=0x");
            Serial.WriteHex((ulong)currentSp);
            Serial.Write("\n");
        }

        // Single hart for now: CPU 0
        SchedulerManager.OnTimerInterrupt(0, currentSp, Instance._periodNs);
    }

    /// <inheritdoc/>
    public override void Wait(uint timeoutMs)
    {
        ulong targetTicks = CsrNative.ReadTime() + ((_timerFrequency * timeoutMs) / 1000);

        while (CsrNative.ReadTime() < targetTicks)
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>
    /// Gets the current time CSR value.
    /// </summary>
    public ulong GetCurrentCounter() => CsrNative.ReadTime();
}
