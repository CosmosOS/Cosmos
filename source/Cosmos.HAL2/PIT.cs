using System;
using System.Collections.Generic;
using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// Handles the Programmable Interval Timer (PIT).
    /// </summary>
    public class PIT : Device
    {
        /// <summary>
        /// Represents a virtual timer that can be handled using the
        /// Programmable Interrupt Timer (PIT).
        /// </summary>
        public class PITTimer : IDisposable
        {
            internal ulong NSRemaining;
            internal int ID = -1;

            /// <summary>
            /// The delay between each timer cycle.
            /// </summary>
            public ulong NanosecondsTimeout;

            /// <summary>
            /// Whether this timer will fire once, or will fire indefinetly until unregistered.
            /// </summary>
            public bool Recurring;

            /// <summary>
            /// The ID of the timer.
            /// </summary>
            public int TimerID => ID;

            /// <summary>
            /// The method to invoke for each cycle of the timer.
            /// </summary>
            public OnTrigger HandleTrigger;

            /// <summary>
            /// Represents the trigger handler for a <see cref="PITTimer"/>.
            /// </summary>
            /// <param name="irqContext">The state of the CPU when the PIT interrupt has occured.</param>
            public delegate void OnTrigger(INTs.IRQContext irqContext);

            /// <summary>
            /// Initializes a new <see cref="PITTimer"/>, with the specified
            /// callback method and properties.
            /// </summary>
            /// <param name="callback">The method to invoke for each timer cycle.</param>
            /// <param name="nanosecondsTimeout">The delay between timer cycles.</param>
            /// <param name="recurring">Whether this timer will fire once, or will fire indefinetly until unregistered.</param>
            public PITTimer(OnTrigger callback, ulong nanosecondsTimeout, bool recurring)
            {
                HandleTrigger = callback;
                NanosecondsTimeout = nanosecondsTimeout;
                NSRemaining = NanosecondsTimeout;
                Recurring = recurring;
            }

            /// <inheritdoc cref="PITTimer(OnTrigger, UInt64, Boolean)"/>
            public PITTimer(Action callback, ulong nanosecondsTimeout, bool recurring)
                : this(_ => callback(), nanosecondsTimeout, recurring)
            { }

            /// <summary>
            /// Initializes a new recurring <see cref="PITTimer"/>, with the specified
            /// callback method and amount of nanoseconds left until the next timer cycle.
            /// </summary>
            /// <param name="callback">The method to invoke for each timer cycle.</param>
            /// <param name="nanosecondsTimeout">The delay between timer cycles.</param>
            /// <param name="nanosecondsLeft">The amount of time left before the first timer cycle is fired.</param>
            public PITTimer(OnTrigger callback, ulong nanosecondsTimeout, ulong nanosecondsLeft)
            {
                HandleTrigger = callback;
                NanosecondsTimeout = nanosecondsTimeout;
                NSRemaining = nanosecondsLeft;
                Recurring = true;
            }

            /// <inheritdoc cref="PITTimer(OnTrigger, UInt64, UInt64)"/>
            public PITTimer(Action callback, ulong nanosecondsTimeout, ulong nanosecondsLeft)
                : this(_ => callback(), nanosecondsTimeout, nanosecondsLeft)
            { }

            ~PITTimer()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (ID != -1)
                {
                    Global.PIT.UnregisterTimer(ID);
                }
            }

            #region (deprecated)

            [Obsolete($"Use the {nameof(Recurring)} property instead.")]
            public bool Recuring => Recurring;

            #endregion
        }

        public const uint PITFrequency = 1193180;
        public const uint PITDelayNS = 838;

        public bool T0RateGen = false;

        private readonly List<PITTimer> activeHandlers = new();
        private ushort _T0Countdown = 65535;
        private ushort _T2Countdown = 65535;
        private int timerCounter;
        private bool waitSignaled;

        /// <summary>
        /// Channel 0 data port.
        /// </summary>
        public const int Data0 = 0x40;
        /// <summary>
        /// Channel 1 data port.
        /// </summary>
        public const int Data1 = 0x41;
        /// <summary>
        /// Channel 2 data port.
        /// </summary>
        public const int Data2 = 0x42;
        /// <summary>
        /// Command register port.
        /// </summary>
        public const int Command = 0x43;

        public PIT()
        {
            INTs.SetIrqHandler(0x00, HandleIRQ);
            T0Countdown = 65535;
        }

        public ushort T0Countdown
        {
            get => _T0Countdown;
            set {
                _T0Countdown = value;

                IOPort.Write8(Command, (byte)(T0RateGen ? 0x34 : 0x30));
                IOPort.Write8(Data0, (byte)(value & 0xFF));
                IOPort.Write8(Data0, (byte)(value >> 8));
            }
        }

        public uint T0Frequency
        {
            get => PITFrequency / (uint)_T0Countdown;
            set {
                if (value < 19 || value > 1193180) {
                    throw new ArgumentException("Frequency must be between 19 and 1193180!");
                }

                T0Countdown = (ushort)(PITFrequency / value);
            }
        }

        public uint T0DelayNS
        {
            get => PITDelayNS * _T0Countdown;
            set {
                if (value > 54918330) {
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

                IOPort.Write8(Command, 0xB6);
                IOPort.Write8(Data0, (byte)(value & 0xFF));
                IOPort.Write8(Data0, (byte)(value >> 8));
            }
        }

        public uint T2Frequency
        {
            get => PITFrequency / (uint)_T2Countdown;
            set
            {
                if (value < 19 || value > 1193180)
                {
                    throw new ArgumentException("Frequency must be between 19 and 1193180!");
                }

                T2Countdown = (ushort)(PITFrequency / value);
            }
        }

        public uint T2DelayNS
        {
            get => PITDelayNS * _T2Countdown;
            set
            {
                if (value > 54918330) {
                    throw new ArgumentException("Delay must be no greater than 54918330");
                }

                T2Countdown = (ushort)(value / PITDelayNS);
            }
        }

        [Obsolete("This method has been deprecated and is equivalent to a no-op.")]
        public void EnableSound()
        {
            //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0x03);
        }

        [Obsolete("This method has been deprecated and is equivalent to a no-op.")]
        public void DisableSound()
        {
            //IO.Port61.Byte = (byte)(IO.Port61.Byte | 0xFC);
        }

        public void PlaySound(int aFreq)
        {
            EnableSound();
            T2Frequency = (uint)aFreq;
        }

        [Obsolete("This method has been deprecated and is equivalent to a no-op.")]
        public void MuteSound()
        {
            DisableSound();
        }

        private void SignalWait(INTs.IRQContext irqContext)
        {
            waitSignaled = true;
        }

        /// <summary>
        /// Halts the CPU for the specified amount of milliseconds.
        /// </summary>
        /// <param name="timeoutMs">The amount of milliseconds to halt the CPU for.</param>
        public void Wait(uint timeoutMs)
        {
            waitSignaled = false;

            RegisterTimer(new PITTimer(SignalWait, timeoutMs * 1000000UL, false));

            while (!waitSignaled)
            {
                CPU.Halt();
            }
        }

        /// <summary>
        /// Halts the CPU for the specified amount of nanoseconds.
        /// </summary>
        /// <param name="timeoutNs">The amount of nanoseconds to halt the CPU for.</param>
        public void WaitNS(ulong timeoutNs)
        {
            waitSignaled = false;

            RegisterTimer(new PITTimer(SignalWait, timeoutNs, false));

            while (!waitSignaled)
            {
                CPU.Halt();
            }
        }

        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            ulong T0Delay = T0DelayNS;

            if (activeHandlers.Count > 0)
            {
                T0Countdown = 65535;
            }

            PITTimer handler;
            for (int i = activeHandlers.Count - 1; i >= 0; i--)
            {
                handler = activeHandlers[i];

                if (handler.NSRemaining <= T0Delay)
                {
                    if (handler.Recurring)
                    {
                        handler.NSRemaining = handler.NanosecondsTimeout;
                    }
                    else
                    {
                        handler.ID = -1;
                        activeHandlers.RemoveAt(i);
                    }

                    handler.HandleTrigger(aContext);
                } else {
                    handler.NSRemaining -= T0Delay;
				}
            }

        }

        /// <summary>
        /// Registers a timer to this <see cref="PIT"/> object.
        /// </summary>
        /// <param name="timer">The target timer.</param>
        /// <returns>The newly assigned ID to the timer.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the given timer has already been registered.</exception>
        public int RegisterTimer(PITTimer timer)
        {
            if (timer.ID != -1)
            {
                throw new InvalidOperationException("The provided timer has already been registered.");
            }

            timer.ID = timerCounter++;
            activeHandlers.Add(timer);
            T0Countdown = 65535;
            return timer.ID;
        }

        /// <summary>
        /// Unregisters a timer that has been previously registered to this
        /// <see cref="PIT"/> object.
        /// </summary>
        /// <param name="timerId">The ID of the timer to unregister.</param>
        public void UnregisterTimer(int timerId)
        {
            for (int i = 0; i < activeHandlers.Count; i++)
            {
                if (activeHandlers[i].ID == timerId)
                {
                    activeHandlers[i].ID = -1;
                    activeHandlers.RemoveAt(i);
                    return;
                }
            }
        }

        #region (deprecated)

        [Obsolete($"Use the {nameof(T0DelayNS)} property instead.")]
        public uint T0DelyNS => T0DelayNS;

        [Obsolete($"Use the {nameof(T2DelayNS)} property instead.")]
        public uint T2DelyNS => T2DelayNS;

        #endregion
    }
}
