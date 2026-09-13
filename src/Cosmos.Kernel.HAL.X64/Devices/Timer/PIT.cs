// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.HAL2/PIT.cs

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Timer;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.X64.Devices.Timer;

/// <summary>
/// Handles the Programmable Interval Timer (PIT). Software timers registered
/// via <see cref="TimerDevice.RegisterTimer"/> are driven by the channel 0 IRQ.
/// </summary>
internal class PIT : TimerDevice
{
    /// <summary>
    /// Singleton instance of the PIT.
    /// </summary>
    public static PIT? Instance { get; private set; }

    public const uint PITFrequency = 1193180;
    public const uint PITDelayNS = 838;

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
        T0Countdown = 65535;

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
    /// Gets the timer frequency in Hz.
    /// </summary>
    public override uint Frequency => PITFrequency / _t0Countdown;

    /// <summary>
    /// Lowest frequency the 16-bit divisor can express: <see cref="PITFrequency"/>
    /// divided by 65535, rounded up. The highest is <see cref="PITFrequency"/>
    /// itself, at a divisor of 1.
    /// </summary>
    public const uint MinFrequency = 19;

    public uint T0Frequency
    {
        get => PITFrequency / _t0Countdown;
        set
        {
            if (value < MinFrequency || value > PITFrequency)
            {
                Serial.Write("[PIT] ERROR: Frequency must be between 19 and 1193180!\n");
                return;
            }

            T0Countdown = (ushort)(PITFrequency / value);
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

    public uint T0DelayNS
    {
        get => PITDelayNS * _t0Countdown;
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
    public override void RegisterTimer(SoftwareTimer timer)
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
