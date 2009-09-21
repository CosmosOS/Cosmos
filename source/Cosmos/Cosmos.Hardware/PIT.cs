using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware
{
    /*
	public class PIT: Hardware
    {
		public const int TicksPerSecond = 1000;
		public static void Wait(uint aMSecs) {
            for(int i = 0; i < (aMSecs/54);i++)
		    {
                InternalWait(54);
		    }
		    byte xRestWait = (byte)(aMSecs % 54);
            InternalWait(xRestWait);
		}

        private static void InternalWait(byte aMSecs) {
            mTicked = false;
            int xDivisor = aMSecs * 1193;
            IOWriteByte(0x43, 0x30);
            IOWriteByte(0x40, (byte)(xDivisor & 0xFF));
            IOWriteByte(0x40, (byte)(xDivisor >> 8));
            while(!mTicked){CPU.Halt();}
        }

	    //private static void SetInterval(ushort hz) {
        //    ushort xDivisor = (ushort)(1193180 / hz);       /* Calculate our divisor * /
        //    SetDivisor(xDivisor);
        //}

        //public static void Initialize(EventHandler aTick) {
        //    mTick = aTick;
        //    //SetInterval(1); // interval 1 is slowest, mostly useful for debugging..
        //    //SetInterval(1193); // interval 1193 is aprox 1 millisecond
        //    SetInterval(1);
        //}

		//private static EventHandler mTick;
	    private static bool mTicked;

		public static void HandleInterrupt() {
			//mTick(null, null);
		    mTicked = true;
		}

        public static void PlaySound(int aFrequency)
        {
            // http://www.ifi.uio.no/~inf3150/grupper/1/pcspeaker.html
            int xFrequency = (1193180 / aFrequency);
            IOWriteByte(0x61, IOReadByte((0x61) | 3)); //Enable speaker
            IOWriteByte(0x43, 0xB6); //Init sound
            IOWriteByte(0x42, (byte)((byte)0xff & xFrequency)); //LSB
            IOWriteByte(0x42, (byte)(xFrequency >> 8)); //MSB
        }

        public static void MuteSound()
        {
            //Note; Disabling speaker doesn't work??
            IOWriteByte(0x61, (byte)(IOReadByte(0x61) & 0xFC)); //Disable speaker
            
        }
	}
    */

    public class PIT : Hardware
    {
        public class PITTimer : IDisposable
        {
            internal int NSRemaining;
            public int NanosecondsTimeout;
            public bool Recuring;
            internal int ID = -1;

            public int TimerID
            {
                get
                {
                    return ID;
                }
            }

            public delegate void dOnTrigger();
            public dOnTrigger HandleTrigger;

            public PITTimer(dOnTrigger HandleOnTrigger, int NanosecondsTimeout, bool Recuring)
            {
                this.HandleTrigger = HandleOnTrigger;
                this.NanosecondsTimeout = NanosecondsTimeout;
                this.NSRemaining = this.NanosecondsTimeout;
                this.Recuring = Recuring;
            }
            public PITTimer(dOnTrigger HandleOnTrigger, int NanosecondsTimeout, int NanosecondsLeft)
            {
                this.HandleTrigger = HandleOnTrigger;
                this.NanosecondsTimeout = NanosecondsTimeout;
                this.NSRemaining = NanosecondsLeft;
                this.Recuring = true;
            }
            ~PITTimer()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (ID != -1)
                    PIT.UnregisterTimer(ID);
            }
        }
        private static List<PITTimer> ActiveHandlers = new List<PITTimer>();
        private static ushort _T0Countdown = 65535;
        private static ushort _T2Countdown = 65535;
        private static int TimerCounter = 0;
        private static bool WaitSignaled = false;
        public const uint PITFrequency = 1193180;
        public const uint PITDelayNS = 838;
        public static bool T0RateGen = false;

        public static ushort T0Countdown
        {
            get
            {
                return _T0Countdown;
            }
            set
            {
                _T0Countdown = value;

                IOWriteByte(0x43, (byte)(T0RateGen ? 0x34 : 0x30));
                IOWriteByte(0x40, (byte)(value & 0xFF));
                IOWriteByte(0x40, (byte)(value >> 8));
            }
        }
        public static uint T0Frequency
        {
            get
            {
                return (PITFrequency / ((uint)_T0Countdown));
            }
            set
            {
                if (value < 19 || value > 1193180)
                    throw new ArgumentException("Frequency must be between 19 and 1193180!");

                T0Countdown = (ushort)(PITFrequency / value);
            }
        }
        public static uint T0DelyNS
        {
            get
            {
                return (PITDelayNS * _T0Countdown);
            }
            set
            {
                if (value > 54918330)
                    throw new ArgumentException("Delay must be no greater that 54918330");

                T0Countdown = (ushort)(value / PITDelayNS);
            }
        }

        public static ushort T2Countdown
        {
            get
            {
                return _T2Countdown;
            }
            set
            {
                _T2Countdown = value;

                IOWriteByte(0x43, 0xB6);
                IOWriteByte(0x42, (byte)(value & 0xFF));
                IOWriteByte(0x42, (byte)(value >> 8));
            }
        }
        public static uint T2Frequency
        {
            get
            {
                return (PITFrequency / ((uint)_T2Countdown));
            }
            set
            {
                if (value < 19 || value > 1193180)
                    throw new ArgumentException("Frequency must be between 19 and 1193180!");

                T2Countdown = (ushort)(PITFrequency / value);
            }
        }
        public static uint T2DelyNS
        {
            get
            {
                return (PITDelayNS * _T2Countdown);
            }
            set
            {
                if (value > 54918330)
                    throw new ArgumentException("Delay must be no greater than 54918330");

                T2Countdown = (ushort)(value / PITDelayNS);
            }
        }

        public static void EnableSound()
        {
            IOWriteByte(0x61, (byte)(IOReadByte(0x61) | 0x03));
        }
        public static void DisableSound()
        {
            IOWriteByte(0x61, (byte)(IOReadByte(0x61) & 0xFC));
        }
        public static void PlaySound(int aFreq)
        {
            EnableSound();
            T2Frequency = (uint)aFreq;
        }
        public static void MuteSound()
        {
            DisableSound();
        }

        private static void SignalWait()
        {
            WaitSignaled = true;
        }

        public static void Wait(uint TimeoutMS)
        {
            WaitSignaled = false;

            RegisterTimer(new PITTimer(SignalWait, (int)(TimeoutMS * 1000000), false));

            while (!WaitSignaled)
            {
                CPU.Halt();
            }
        }
        public static void WaitNS(int TimeoutNS)
        {
            WaitSignaled = false;

            RegisterTimer(new PITTimer(SignalWait, TimeoutNS, false));

            while (!WaitSignaled)
            {
                CPU.Halt();
            }
        }

        public static void HandleInterrupt()
        {
            int T0Delay = (int)T0DelyNS;
            PITTimer hndlr = null;
            for (int i = ActiveHandlers.Count - 1;i >= 0;i--)
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

        public static int RegisterTimer(PITTimer timer)
        {
            if (timer.ID != -1)
                throw new InvalidOperationException("Timer has allready been registered!");

            timer.ID = (TimerCounter++);
            ActiveHandlers.Add(timer);

            return timer.ID;
        }
        public static void UnregisterTimer(int timerid)
        {
            for (int i = 0;i < ActiveHandlers.Count;i++)
            {
                if (ActiveHandlers[i].ID == timerid)
                {
                    ActiveHandlers[i].ID = -1;
                    ActiveHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        public static void Init()
        {
          T0RateGen = true;
          T0Countdown = _T0Countdown;
        }
    }
}