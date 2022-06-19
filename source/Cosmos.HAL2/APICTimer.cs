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
        /// Tick Frequency.
        /// </summary>
        public static uint TickFrequency = 0;

        /// <summary>
        /// Tick Frequency.
        /// </summary>
        public static ulong BaseFrequency = 0;

        /// <summary>
        /// Local Apic Timer Divisor.
        /// </summary>
        public const int Divisor = 4;

        /// <summary>
        /// Initialize local APIC timer.
        /// </summary>
        public static void Initialize()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TDCR, 0xb0001); // divide timer counts by 4

            Calibrate();

            if (TickFrequency == 0)
            {
                throw new Exception("APIC timer is not calibrated");
            }

            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x20000 | 0x20); // periodic, bind to IRQ0

            Global.mDebugger.Send("Local APIC Timer Initialized");
        }

        public static void Start()
        {
            SetTimerFrequency(TickFrequency);
        }

        /// <summary>
        /// Stop local APIC timer.
        /// </summary>
        public static void Stop()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x10000);
        }

        private static void SetTimerCount(uint count)
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TICR, count);
        }

        public static void SetTimerFrequency(uint frequency)
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TICR, (uint)(BaseFrequency / (frequency * Divisor)));
        }

        private static void Calibrate()
        {
            Global.mDebugger.Send("Calibrating APIC timer...");

            SetTimerCount(0xFFFFFFFF); // Set APIC init counter to -1

            Global.PIT.Wait(50); // PIT sleep for 50ms

            BaseFrequency = ((0xFFFFFFFF - LocalAPIC.In(LocalAPIC.LAPIC_TCCR)) * 2) * Divisor;

            Stop();

            Global.mDebugger.Send("APIC timer base frequency: " + BaseFrequency + "Hz");

            TickFrequency = 1000000000 / 1000000; //freq in Hz = 1000000000 / 1ms

            Global.mDebugger.Send("APIC timer frequency: " + TickFrequency + "Hz");
        }
    }
}
