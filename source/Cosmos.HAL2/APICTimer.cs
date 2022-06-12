using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;
using Cosmos.Core.MemoryGroup;

namespace Cosmos.HAL
{
    /// <summary>
    /// Local APIC Timer class.
    /// </summary>
    public static class APICTimer
    {
        /// <summary>
        /// Tick Counter.
        /// </summary>
        //public static uint Tick = 0;

        /// <summary>
        /// Tick Frequency.
        /// </summary>
        public static uint TickFrequency = 0;

        /// <summary>
        /// Initialize local APIC timer.
        /// </summary>
        public static void Initialize()
        {
            INTs.SetIrqHandler(0, HandleApicTimer);

            // setup timer, Intel IA manual 10-16 Vol. 3A
            LocalAPIC.Out(LocalAPIC.LAPIC_TDCR, 0xB); // divide timer counts by 1

            Calibrate();

            if (TickFrequency == 0)
            {
                throw new Exception("APIC timer is not calibrated");
            }

            //Start timer
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x20000 | 0x20); // periodic, bind to corresponding IRQ
            LocalAPIC.Out(LocalAPIC.LAPIC_TDCR, 0xB);
            SetTimerCount(TickFrequency / 100);

            Global.mDebugger.Send("Local APIC Timer Initialized");
        }

        public static void HandleApicTimer(ref INTs.IRQContext aContext)
        {
            //Tick++;
        }

        /// <summary>
        /// Stop local APIC timer.
        /// </summary>
        public static void Stop()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x10000);
        }

        public static void SetTimerCount(uint count)
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TICR, count);
        }

        public static void Calibrate()
        {
            SetTimerCount(0xFFFFFFFF); // Set APIC init counter to -1

            Global.PIT.Wait(1000); // PIT sleep for 1000ms (10000µs)

            Stop();

            ulong ticks = 0xFFFFFFFF - LocalAPIC.In(LocalAPIC.LAPIC_TCCR); // Now we know how often the APIC timer has ticked in 10ms

            Global.mDebugger.Send("APIC timer ticks per 10ms: " + ticks);

            TickFrequency = (uint)(ticks * 1000 / 1000);

            Global.mDebugger.Send("APIC timer tick frequency: " + TickFrequency);
        }
    }
}
