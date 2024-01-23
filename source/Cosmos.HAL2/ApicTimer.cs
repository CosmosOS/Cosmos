using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// Local APIC Timer class.
    /// </summary>
    public static class ApicTimer
    {
        /// <summary>
        /// APIC base frequency
        /// </summary>
        public static ulong BaseFrequency;

        /// <summary>
        /// APIC frequency
        /// </summary>
        public static ulong Frequency;

        private static ulong Ticks => LocalAPIC.In(LocalAPIC.LAPIC_TCCR);

        /// <summary>
        /// Initialize local APIC timer.
        /// </summary>
        public static void Initialize()
        {
            Frequency = 1000;
            BaseFrequency = EstimateBusSpeed();

            Global.debugger.Send("APIC timer frequency: " + Frequency + "Hz, Divisor: " + 16 + ", IRQ: 8");
            Global.debugger.Send("Base frequency is " + BaseFrequency / 1048576 + "mhz");
            Global.debugger.Send("Local APIC Timer Initialized");
        }

        /// <summary>
        /// Start local APIC timer.
        /// </summary>
        public static void Start()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x00020000 | 0x28);
            LocalAPIC.Out(LocalAPIC.LAPIC_TDCR, 0x3); //Divide 16
            LocalAPIC.Out(LocalAPIC.LAPIC_TICR, (uint)((BaseFrequency / 16) / Frequency));
        }

        /// <summary>
        /// Stop local APIC timer.
        /// </summary>
        public static void Stop()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x20000 | 0x10000);
        }

        /// <summary>
        /// Calculate BUS Speed using PIT
        /// </summary>
        private static uint EstimateBusSpeed()
        {
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x10000);
            LocalAPIC.Out(LocalAPIC.LAPIC_TDCR, 0x3);
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0);

            uint T0 = 0xFFFFFFFF; // -1
            LocalAPIC.Out(0x380, T0);
            Global.PIT.Wait(100); //100ms
            LocalAPIC.Out(LocalAPIC.LAPIC_TIMER, 0x10000);

            ulong Freq = (T0 - Ticks) * 16;
            return (uint)(Freq * 1000000 / 100000);
        }
    }
}