using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// Programmable Interval Timer
    /// with 1,193181818... MHz
    /// </summary>
    public class PIT : Device
    {
        /// <summary>
        /// Pit IO Ports.
        /// </summary>
        protected Core.IOGroup.PIT IO = Core.Global.BaseIOGroups.PIT;

        /// <summary>
        /// Global PIT Frequency.
        /// </summary>
        public const uint PITFrequency = 1193182;

        /// <summary>
        /// Channel0 Frequency. 1ms.
        /// </summary>
        public const uint Channel0Frequency = 1000;

        /// <summary>
        /// Divisor used to set channel0 frquency.
        /// </summary>
        public ushort Channel0Divisor = (ushort)(PITFrequency / Channel0Frequency);

        /// <summary>
        /// Is PIT Sleeping.
        /// </summary>
        bool Sleeping = false;

        /// <summary>
        /// Countdown in Ms.
        /// </summary>
        uint CountDown = 0;

        /// <summary>
        /// PIT ctor, Set IRQ2 Handler
        /// </summary>
        public PIT()
        {
            INTs.SetIrqHandler(2, HandleIRQ);
        }

        /// <summary>
        /// Enable PIT Interrupts
        /// </summary>
        public void Enable()
        {
            IO.Command.Byte = 0x34; //Mode 4, lobyte/hibyte, channel 0
            IO.Data0.Byte = (byte)(Channel0Divisor & 0xFF);
            IO.Data0.Byte = (byte)(Channel0Divisor >> 8);
        }

        /// <summary>
        /// Disable PIT Interrupts
        /// </summary>
        public void Disable()
        {
            IO.Command.Byte = 0b11000; //Mode 0, lobyte/hibyte, channel 0
            ushort div = 0xFFFF;
            IO.Data0.Byte = (byte)(div & 0xFF);
            IO.Data0.Byte = (byte)(div >> 8);
        }

        /// <summary>
        /// PIT Wait
        /// </summary>
        /// <param name="TimeoutMS">Timeout Value in Milliseconds.</param>
        public void Wait(uint TimeoutMS)
        {
            Enable();

            CountDown = TimeoutMS;

            if (CountDown > 0)
            {
                Sleeping = true;

                while (Sleeping && CountDown > 0)
                {
                    CPU.Halt();
                }
            }

            Disable();
        }

        /// <summary>
        /// Handle PIT IRQ
        /// </summary>
        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            if (Sleeping)
            {
                if (CountDown > 0)
                {
                    CountDown--; //remove 1ms
                }
                else
                {
                    Sleeping = false;
                }
            }
        }
    }
}
