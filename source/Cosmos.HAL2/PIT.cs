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
        protected Core.IOGroup.PIT IO = Core.Global.BaseIOGroups.PIT;

        public const uint PITFrequency = 1193182;
        public const uint Channel0Frequency = 1000;
        public ushort Channel0Divisor = (ushort)(PITFrequency / Channel0Frequency);
        bool Sleeping = false;
        uint CountDown = 0;

        public PIT()
        {
            INTs.SetIrqHandler(2, HandleIRQ);
        }

        public void Enable()
        {
            IO.Command.Byte = 0x34; //Mode 4, lobyte/hibyte, channel 0
            IO.Data0.Byte = (byte)(Channel0Divisor & 0xFF);
            IO.Data0.Byte = (byte)(Channel0Divisor >> 8);
        }

        public void Disable()
        {
            IO.Command.Byte = 0b11000; //Mode 0, lobyte/hibyte, channel 0
            ushort div = 0xFFFF;
            IO.Data0.Byte = (byte)(div & 0xFF);
            IO.Data0.Byte = (byte)(div >> 8);
        }

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
