// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.ARM64.Cpu;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Devices.Timer;

namespace Cosmos.Kernel.HAL.ARM64.Devices.Timer;

/// <summary>
/// ARM64 Generic Timer (Architected Timer) implementation.
/// Uses the physical timer (CNTP_*) for scheduling interrupts.
/// Native imports live in Cosmos.Kernel.Core.ARM64/Bridge/Import/GenericTimerNative.cs.
/// </summary>
internal class GenericTimer : TimerDevice
{
    /// <summary>
    /// Singleton instance of the Generic Timer.
    /// </summary>
    public static GenericTimer? Instance { get; private set; }

    /// <summary>
    /// Timer frequency in Hz (read from CNTFRQ_EL0).
    /// Typically 62.5 MHz on QEMU virt.
    /// </summary>
    private ulong _timerFrequency;

    /// <summary>
    /// Configured timer period in nanoseconds.
    /// </summary>
    private ulong _periodNs;

    /// <summary>
    /// Timer ticks per period.
    /// </summary>
    private ulong _ticksPerPeriod;

    /// <summary>
    /// Physical timer interrupt number on GIC: INTID 30, the non-secure
    /// physical timer PPI (alias of <see cref="GIC.TIMER_NONSEC_PHYS"/>).
    /// </summary>
    public const uint PhysicalTimerIrq = GIC.TIMER_NONSEC_PHYS;

    /// <summary>
    /// Default timer period: 10ms (100 Hz) for scheduling.
    /// </summary>
    public const ulong DefaultPeriodNs = 10_000_000;

    public GenericTimer()
    {
    }

    /// <summary>
    /// The hardware counter frequency in Hz (CNTFRQ_EL0).
    /// </summary>
    public ulong TimerFrequency => _timerFrequency;

    /// <inheritdoc/>
    public override uint Frequency => (uint)(1_000_000_000UL / _periodNs);

    /// <summary>
    /// Initialize the Generic Timer.
    /// </summary>
    public override void Initialize()
    {
        Serial.Write("[GenericTimer] Initializing ARM64 Generic Timer...\n");

        Instance = this;

        // Read timer frequency from CNTFRQ_EL0
        _timerFrequency = GenericTimerNative.GetFrequency();
        Serial.Write("[GenericTimer] Timer frequency: ");
        Serial.WriteNumber(_timerFrequency);
        Serial.Write(" Hz\n");

        // Set default period (10ms)
        SetPeriod(DefaultPeriodNs);

        Serial.Write("[GenericTimer] Initialized\n");
    }

    /// <summary>
    /// Sets the timer period in nanoseconds.
    /// </summary>
    /// <param name="periodNs">Period in nanoseconds.</param>
    public void SetPeriod(ulong periodNs)
    {
        _periodNs = periodNs;

        // Calculate ticks per period
        // ticks = (frequency * period_ns) / 1_000_000_000
        _ticksPerPeriod = (_timerFrequency * periodNs) / 1_000_000_000UL;

        Serial.Write("[GenericTimer] Period: ");
        Serial.WriteNumber(periodNs / 1_000_000);
        Serial.Write(" ms, ticks per period: ");
        Serial.WriteNumber(_ticksPerPeriod);
        Serial.Write("\n");
    }

    /// <inheritdoc/>
    public override bool SetFrequency(uint frequency)
    {
        // Above the counter frequency the period rounds down to fewer than one
        // tick, and SetPeriod would store a zero comparator: the tick handler
        // re-arms with SetTval(0), so the timer fires again on every eret and
        // the CPU never leaves the handler. Past 1 GHz the period itself
        // truncates to zero and the Frequency getter divides by it.
        if (frequency == 0 || frequency > _timerFrequency)
        {
            return false;
        }

        ulong periodNs = 1_000_000_000UL / frequency;
        SetPeriod(periodNs);
        return true;
    }

    /// <summary>
    /// Starts the timer and arms it for the first interrupt.
    /// </summary>
    public void Start()
    {
        Serial.Write("[GenericTimer] Starting timer...\n");

        // Set TVAL to trigger after one period
        if (_ticksPerPeriod > uint.MaxValue)
        {
            Serial.Write("[GenericTimer] WARNING: ticks per period exceeds 32-bit, clamping\n");
            GenericTimerNative.SetTval(uint.MaxValue);
        }
        else
        {
            GenericTimerNative.SetTval((uint)_ticksPerPeriod);
        }

        // Enable the timer (ENABLE=1, IMASK=0)
        GenericTimerNative.Enable();

        Serial.Write("[GenericTimer] Timer started, CTL=0x");
        Serial.WriteHex(GenericTimerNative.GetCtl());
        Serial.Write("\n");
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void Stop()
    {
        GenericTimerNative.Disable();
        Serial.Write("[GenericTimer] Timer stopped\n");
    }

    /// <summary>
    /// Registers the IRQ handler for the timer.
    /// Should be called after GIC is initialized.
    /// </summary>
    public void RegisterIRQHandler()
    {
        Serial.Write("[GenericTimer] Registering timer IRQ handler for INTID ");
        Serial.WriteNumber(PhysicalTimerIrq);
        Serial.Write("\n");

        // Register handler for GIC interrupt
        // The vector will be PhysicalTimerIrq (30) mapped through GIC
        InterruptManager.SetHandler((byte)PhysicalTimerIrq, HandleIRQ);

        Serial.Write("[GenericTimer] Timer IRQ handler registered\n");
    }

    /// <summary>
    /// Timer tick counter for debugging.
    /// </summary>
    private static uint s_timerTickCount;

    /// <summary>
    /// Handles the timer interrupt.
    /// </summary>
    private static unsafe void HandleIRQ(ref IRQContext ctx)
    {
        if (Instance == null)
        {
            return;
        }

        s_timerTickCount++;

        // Re-arm the timer for the next period
        if (Instance._ticksPerPeriod > uint.MaxValue)
        {
            GenericTimerNative.SetTval(uint.MaxValue);
        }
        else
        {
            GenericTimerNative.SetTval((uint)Instance._ticksPerPeriod);
        }

        // Advance software timers and invoke the OnTick handler
        Instance.HandleTick(Instance._periodNs);

        // Get current CPU ID (for now, always 0 on single CPU ARM64)
        uint cpuId = 0;

        // Calculate SP pointing to saved context for context switching
        // On ARM64, ctx sits one NEON save area above the start of the saved context
        nuint contextPtr = (nuint)Unsafe.AsPointer(ref ctx);
        nuint currentSp = contextPtr - ARM64InterruptController.NeonSaveAreaBytes;  // SP points to start of NEON save area

        // Log first few ticks and then periodically
        if (s_timerTickCount <= 5 || s_timerTickCount % 100 == 0)
        {
            Serial.Write("[GenericTimer] Tick ");
            Serial.WriteNumber(s_timerTickCount);
            Serial.Write(" SP=0x");
            Serial.WriteHex((ulong)currentSp);
            Serial.Write("\n");
        }

        // Call scheduler with elapsed time
        SchedulerManager.OnTimerInterrupt(cpuId, currentSp, Instance._periodNs);

        // EOI is sent by the interrupt handler after we return
    }

    /// <inheritdoc/>
    public override void Wait(uint timeoutMs)
    {
        ulong targetTicks = GenericTimerNative.GetCounter() + ((_timerFrequency * timeoutMs) / 1000);

        while (GenericTimerNative.GetCounter() < targetTicks)
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>
    /// Gets the current counter value.
    /// </summary>
    public ulong GetCurrentCounter() => GenericTimerNative.GetCounter();

    /// <summary>
    /// Gets the timer period in nanoseconds.
    /// </summary>
    public ulong PeriodNs => _periodNs;
}
