using System;
using System.Collections.Generic;
using Cosmos.Core;

namespace Cosmos.HAL;

/// <summary>
///     Programmable Interval Timer
///     with 1,193181818... MHz
/// </summary>
public class PIT : Device
{
    public const uint PITFrequency = 1193180;
    public const uint PITDelayNS = 838;
    private readonly List<PITTimer> ActiveHandlers = new();
    private ushort _T0Countdown = 65535;
    private ushort _T2Countdown = 65535;

    protected Core.IOGroup.PIT IO = Core.Global.BaseIOGroups.PIT;
    public bool T0RateGen = false;
    private int TimerCounter;
    private bool WaitSignaled;

    public PIT()
    {
        INTs.SetIrqHandler(0x00, HandleIRQ);
        T0Countdown = 65535;
    }

    public ushort T0Countdown
    {
        get => _T0Countdown;
        set
        {
            _T0Countdown = value;

            IO.Command.Byte = (byte)(T0RateGen ? 0x34 : 0x30);
            IO.Data0.Byte = (byte)(value & 0xFF);
            IO.Data0.Byte = (byte)(value >> 8);
        }
    }

    public uint T0Frequency
    {
        get => PITFrequency / _T0Countdown;
        set
        {
            if (value < 19 || value > 1193180)
            {
                throw new ArgumentException("Frequency must be between 19 and 1193180!");
            }

            T0Countdown = (ushort)(PITFrequency / value);
        }
    }

    public uint T0DelyNS
    {
        get => PITDelayNS * _T0Countdown;
        set
        {
            if (value > 54918330)
            {
                throw new ArgumentException("Delay must be no greater that 54918330");
            }

            T0Countdown = (ushort)(value / PITDelayNS);
        }
    }

    public ushort T2Countdown
    {
        get => _T2Countdown;
        set
        {
            _T2Countdown = value;

            IO.Command.Byte = 0xB6;
            IO.Data0.Byte = (byte)(value & 0xFF);
            IO.Data0.Byte = (byte)(value >> 8);
        }
    }

    public uint T2Frequency
    {
        get => PITFrequency / _T2Countdown;
        set
        {
            if (value < 19 || value > 1193180)
            {
                throw new ArgumentException("Frequency must be between 19 and 1193180!");
            }

            T2Countdown = (ushort)(PITFrequency / value);
        }
    }

    public uint T2DelyNS
    {
        get => PITDelayNS * _T2Countdown;
        set
        {
            if (value > 54918330)
            {
                throw new ArgumentException("Delay must be no greater than 54918330");
            }

            T2Countdown = (ushort)(value / PITDelayNS);
        }
    }

    //TODO: Why is sound in PIT? Is it a function of the PIT?
    //Channel 3 is for the pc speaker ^
    public void EnableSound()
    {
        //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0x03);
    }

    public void DisableSound()
    {
        //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0xFC);
    }

    public void PlaySound(int aFreq)
    {
        EnableSound();
        T2Frequency = (uint)aFreq;
    }

    public void MuteSound() => DisableSound();

    private void SignalWait() => WaitSignaled = true;

    public void Wait(uint TimeoutMS)
    {
        WaitSignaled = false;

        RegisterTimer(new PITTimer(SignalWait, (int)(TimeoutMS * 1000000), false));

        while (!WaitSignaled)
        {
            CPU.Halt();
        }
    }

    public void WaitNS(int TimeoutNS)
    {
        WaitSignaled = false;

        RegisterTimer(new PITTimer(SignalWait, TimeoutNS, false));

        while (!WaitSignaled)
        {
            CPU.Halt();
        }
    }

    private void HandleIRQ(ref INTs.IRQContext aContext)
    {
        var T0Delay = (int)T0DelyNS;
        PITTimer hndlr = null;

        if (ActiveHandlers.Count > 0)
        {
            T0Countdown = 65535;
        }

        for (var i = ActiveHandlers.Count - 1; i >= 0; i--)
        {
            hndlr = ActiveHandlers[i];

            hndlr.NSRemaining -= T0Delay;

            if (hndlr.NSRemaining < 1)
            {
                if (hndlr.Recuring)
                {
                    hndlr.NSRemaining = hndlr.NanosecondsTimeout;
                }
                else
                {
                    hndlr.ID = -1;
                    ActiveHandlers.RemoveAt(i);
                }

                hndlr.HandleTrigger();
            }
        }
    }

    public int RegisterTimer(PITTimer timer)
    {
        if (timer.ID != -1)
        {
            throw new InvalidOperationException("Timer has already been registered!");
        }

        timer.ID = TimerCounter++;
        ActiveHandlers.Add(timer);
        T0Countdown = 65535;
        return timer.ID;
    }

    public void UnregisterTimer(int timerid)
    {
        for (var i = 0; i < ActiveHandlers.Count; i++)
        {
            if (ActiveHandlers[i].ID == timerid)
            {
                ActiveHandlers[i].ID = -1;
                ActiveHandlers.RemoveAt(i);
                return;
            }
        }
    }

    public class PITTimer : IDisposable
    {
        public delegate void dOnTrigger();

        public dOnTrigger HandleTrigger;
        internal int ID = -1;
        public int NanosecondsTimeout;
        internal int NSRemaining;
        public bool Recuring;

        public PITTimer(dOnTrigger HandleOnTrigger, int NanosecondsTimeout, bool Recuring)
        {
            HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            NSRemaining = this.NanosecondsTimeout;
            this.Recuring = Recuring;
        }

        public PITTimer(dOnTrigger HandleOnTrigger, int NanosecondsTimeout, int NanosecondsLeft)
        {
            HandleTrigger = HandleOnTrigger;
            this.NanosecondsTimeout = NanosecondsTimeout;
            NSRemaining = NanosecondsLeft;
            Recuring = true;
        }

        public int TimerID => ID;

        public void Dispose()
        {
            if (ID != -1)
            {
                Global.PIT.UnregisterTimer(ID);
            }
        }

        ~PITTimer()
        {
            Dispose();
        }
    }
}
