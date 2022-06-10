using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.IOGroup;

namespace Cosmos.Core
{
    /// <summary>
    /// LocalAPICTimer class. Used to manage Local APIC Timer.
    /// </summary>
    public partial class LocalAPICTimer
    {
        /// <summary>
        /// Local APIC Timer IO group.
        /// </summary>
        APICTimerIOGroup IO;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocalAPICTimer()
        {
            IO = new APICTimerIOGroup();
        }

        /// <summary>
        /// Ticks.
        /// </summary>
        public ulong Ticks => IO.TimerCurrentCount.DWord;

        /// <summary>
        /// Send Init Command to Local APIC timer.
        /// </summary>
        /// <param name="freq">Frequency.</param>
        /// <param name="vector">Vector.</param>
        public void StartTimer(ulong freq, uint vector)
        {
            IO.Timer.DWord = 0x00020000 | vector;
            IO.TimerDiv.DWord = 0x3;
            IO.TimerInitCount.DWord = (uint)freq;
            //INTs.EnableInterrupt(0x20); TODO
        }

        /// <summary>
        /// Send Stop command to Local APIC timer.
        /// </summary>
        public void StopTimer()
        {
            IO.Timer.DWord = 0x00020000 | 0x10000;
        }
    }
}
