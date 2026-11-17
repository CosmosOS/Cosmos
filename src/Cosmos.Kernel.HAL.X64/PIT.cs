// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.HAL2/PIT.cs

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Timers;

namespace Cosmos.Kernel.HAL.X64;

/// <summary>
/// Handles the Programmable Interval Timer (PIT). Software timers registered
/// via <see cref="TickSource.RegisterTimer"/> are driven by the channel 0 IRQ,
/// which ticks at <see cref="DefaultFrequency"/> from <see cref="Initialize"/>
/// on.
/// </summary>
internal class PIT : TickSource
{
    /// <summary>
    /// Singleton instance of the PIT.
    /// </summary>
    public static PIT? Instance { get; private set; }

    public const uint PITFrequency = 1193180;
    public const uint PITDelayNS = 838;

    /// <summary>
    /// Tick rate <see cref="Initialize"/> programs: 100 Hz, a 10 ms tick, the
    /// period of the ARM64 generic timer and of the scheduler tick. Without it
    /// channel 0 runs at its widest divisor, a 55 ms tick, and every wait is
    /// rounded up to a multiple of 55 ms.
    /// </summary>
    public const uint DefaultFrequency = 100;

    /// <summary>Nanoseconds in one second.</summary>
    private const ulong NanosecondsPerSecond = 1_000_000_000;

    public bool T0RateGen = false;

    private ushort _t0Countdown = 65535;
    private ushort _t2Countdown = 65535;

    /// <summary>
    /// Channel 0 data port.
    /// </summary>
    public const ushort Data0 = 0x40;
    /// <summary>
    /// Channel 1 data port.
    /// </summary>
    public const ushort Data1 = 0x41;
    /// <summary>
    /// Channel 2 data port.
    /// </summary>
    public const ushort Data2 = 0x42;
    /// <summary>
    /// Command register port.
    /// </summary>
    public const ushort Command = 0x43;

    public PIT()
    {
    }

    /// <summary>
    /// Initialize the PIT device.
    /// </summary>
    public override void Initialize()
    {
        Serial.Write("[PIT] Initializing...\n");

        Instance = this;
        T0Frequency = DefaultFrequency;

        Serial.Write("[PIT] Initialized\n");
    }

    /// <summary>
    /// Registers the IRQ handler for the PIT.
    /// Should be called after interrupt system is ready.
    /// </summary>
    public void RegisterIRQHandler()
    {
        Serial.Write("[PIT] Registering IRQ 0 handler...\n");
        InterruptManager.SetIrqHandler(0, HandleIRQ);
        Serial.Write("[PIT] IRQ 0 handler registered\n");
    }

    public ushort T0Countdown
    {
        get => _t0Countdown;
        set
        {
            _t0Countdown = value;

            Native.IO.Write8(Command, (byte)(T0RateGen ? 0x34 : 0x30));
            Native.IO.Write8(Data0, (byte)(value & 0xFF));
            Native.IO.Write8(Data0, (byte)(value >> 8));
        }
    }

    /// <summary>
    /// Gets the timer frequency in Hz, rounded to the nearest.
    /// </summary>
    public override uint Frequency => T0Frequency;

    /// <summary>
    /// Lowest frequency the 16-bit divisor can express: <see cref="PITFrequency"/>
    /// divided by 65535, rounded up. The highest is <see cref="PITFrequency"/>
    /// itself, at a divisor of 1.
    /// </summary>
    public const uint MinFrequency = 19;

    /// <summary>
    /// Channel 0's rate in Hz. Read, it is rounded to the nearest; written,
    /// the divisor is rounded up, so the tick is never shorter than the period
    /// asked for and a wait of whole periods takes that many ticks: the
    /// truncated divisor of 100 Hz, 11931, gives a 9.9993 ms tick, and every
    /// Wait(10) would then last two.
    /// </summary>
    public uint T0Frequency
    {
        get => (PITFrequency + (_t0Countdown / 2u)) / _t0Countdown;
        set
        {
            if (value < MinFrequency || value > PITFrequency)
            {
                Serial.Write("[PIT] ERROR: Frequency must be between 19 and 1193180!\n");
                return;
            }

            T0Countdown = (ushort)((PITFrequency + value - 1) / value);
        }
    }

    /// <inheritdoc/>
    public override bool SetFrequency(uint frequency)
    {
        if (frequency < MinFrequency || frequency > PITFrequency)
        {
            return false;
        }

        T0Frequency = frequency;
        return true;
    }

    /// <summary>
    /// Channel 0's period in nanoseconds, the elapsed time each tick reports.
    /// Read from <see cref="PITFrequency"/> rather than <see cref="PITDelayNS"/>,
    /// which truncates the 838.1 ns input period: at the 100 Hz divisor that
    /// truncation alone puts the tick under 10 ms.
    /// </summary>
    public uint T0DelayNS
    {
        get => (uint)(_t0Countdown * NanosecondsPerSecond / PITFrequency);
        set
        {
            if (value > 54918330)
            {
                Serial.Write("[PIT] ERROR: Delay must be no greater than 54918330!\n");
                return;
            }

            T0Countdown = (ushort)(value / PITDelayNS);
        }
    }

    public ushort T2Countdown
    {
        get => _t2Countdown;
        set
        {
            _t2Countdown = value;

            Native.IO.Write8(Command, 0xB6);
            Native.IO.Write8(Data2, (byte)(value & 0xFF));
            Native.IO.Write8(Data2, (byte)(value >> 8));
        }
    }

    public uint T2Frequency
    {
        get => PITFrequency / _t2Countdown;
        set
        {
            if (value < 19 || value > 1193180)
            {
                Serial.Write("[PIT] ERROR: Frequency must be between 19 and 1193180!\n");
                return;
            }

            T2Countdown = (ushort)(PITFrequency / value);
        }
    }

    public uint T2DelayNS
    {
        get => PITDelayNS * _t2Countdown;
        set
        {
            if (value > 54918330)
            {
                Serial.Write("[PIT] ERROR: Delay must be no greater than 54918330!\n");
                return;
            }

            T2Countdown = (ushort)(value / PITDelayNS);
        }
    }

    /// <summary>
    /// Registers a software timer and reprograms channel 0 to ensure the tick is running.
    /// </summary>
    /// <param name="timer">Timer to register.</param>
    public override void RegisterTimer(TimerEntry timer)
    {
        base.RegisterTimer(timer);
        T0Countdown = _t0Countdown;
    }

    private static void HandleIRQ(ref IRQContext aContext)
    {
        if (Instance == null)
        {
            return;
        }

        ulong t0Delay = Instance.T0DelayNS;

        // In one-shot mode, must reload after each interrupt
        if (!Instance.T0RateGen)
        {
            Instance.T0Countdown = Instance._t0Countdown;
        }

        Instance.HandleTick(t0Delay);
    }
}
