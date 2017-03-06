using System;
using System.Runtime.CompilerServices;
using Cosmos.Core.IOGroup;
using Cosmos.IL2CPU.Plugs;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.Core.PCInformation
{
    public unsafe class ProcessorInformation
    {
        /// <summary>
        /// Returns the Processor's vendor name
        /// </summary>
        /// <returns>CPU Vendor name</returns>

        /// <summary>
        /// Gets the information related to a certain cpuid operation
        /// The order of the registers returned is as follows (a register can be omited if it is not returned):
        /// EAX, EBX, ECX, EDX
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static uint[] GetCPUID(CPUIDOperation operation)
        {

            uint ptr = 0;
            uint eax;
            uint ebx;
            uint ecx;
            uint edx;
            uint[] returnValue;
            GetFrequency();
            switch (operation)
            {
                case CPUIDOperation.GetVendorID:
                    returnValue = new uint[3];
                    CPUID(0, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = ebx;
                    returnValue[1] = ecx;
                    returnValue[2] = edx;
                    return returnValue;
                case CPUIDOperation.GetProcessorInformation:
                    //Returns the signature
                    returnValue = new uint[1];
                    CPUID(1, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = eax;
                    return returnValue;
                case CPUIDOperation.GetFlags:
                    Debug.Kernel.Debugger.DoSend("Parse flags");
                    returnValue = new uint[2];
                    CPUID(1, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = ecx;
                    returnValue[1] = edx;
                    return returnValue;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the frequency of the processor
        /// Notes: needs bochs: "clock: sync=slowdown, time0=local\n" line to work (well, seems to).
        /// </summary>
        /// <remarks>Credit to: http://wiki.osdev.org/Detecting_CPU_Speed#Without_Interrupts . In emulators the PIT
        /// can go much more quicker than expected giving absurdly big values.
        /// </remarks>
        /// <returns></returns>
        public static double GetFrequency()
        {
            uint eaxPrev, edxPrev, eaxNext, edxNext;
            byte countNextLo, countNextHi;
            ulong ciclesPrev, ciclesNext;
            int pitNext;
            double ticks = 0;

            //This variables take account of wasted cicles
            PIT pit = new PIT();

            //Tell the pit to decrease the counter at its top speed until it reaches 0
            pit.Command.Byte = 0x34;
            //Load the number first msb and next lsb
            //The counter will be 0x10000 (as 0 doesnt exist in pit, the counter will be 65536)
            pit.Data0.Byte = 0;
            pit.Data0.Byte = 0;
            
            //Get the cicles used in the next loop
            GetCurrentTimeStampCounter(&edxPrev, &eaxPrev);

            //Wait a Little
            //This avoids errors produced by cicles wasted trying to get the cicles and loading the pit
            //Putting much delay gets incorrect speed
            for (int i = 1; i < 2000; i++) { }

            GetCurrentTimeStampCounter(&edxNext, &eaxNext);
        
            //Get the current timer
            //Read using a latch in channel 0 (mode software triggered strobe)
            pit.Command.Byte = 0x4;
            //Read the lowest part
            countNextLo = pit.Data0.Byte;
            //Read the highest part
            countNextHi = pit.Data0.Byte;
            
            //Since we finished getting the values, now we calculate the cicles and time
            ciclesPrev = (edxPrev << 32) + eaxPrev;
            ciclesNext = (edxNext << 32) + eaxNext;
            pitNext = (countNextHi << 8) + countNextLo;

            //The cicles will be the substraction of the two read values
            ulong totalCicles = ciclesNext - ciclesPrev;
            //The elapsed ticks will be the stating counter minus the read pit.
            //0x10000 is the starting counter (which we arbitrarily put)
            ticks += (0x10000 - pitNext);

            //Since we know that a tic will happen every 1 / 1193180 sec, its easy to get the freq (cicles per second)
            //We use 1.19 to get mhz directly
            double frequency = (totalCicles) * 1.193180 / ticks;
            Debugger.DoSend("Frequency: " + frequency + " mhz");
               
            return frequency;
        }

            /*
        /// <summary>
        /// Get the frequency of the processor
        /// </summary>
        /// <remarks>Credit to: http://wiki.osdev.org/Detecting_CPU_Speed#Without_Interrupts </remarks>
        /// <returns></returns>
        public static double GetFrequency()
        {
            uint eaxPrev, edxPrev, eaxNext, edxNext;
            byte countNextLo, countNextHi;
            ulong ciclesPrev, ciclesNext;
            int pitPrev, pitNext;
            double ticks = 0;
            RTC rtc = new RTC();

            //Tell the cmos to get the seconds
            rtc.Address.Byte = (0 << 7) | 0x00;
            for (int i = 0; i < 500; i++) { }
            byte seconds = rtc.Data.Byte;
            rtc.Address.Byte = (0 << 7) | 0x02;
            for (int i = 0; i < 500; i++) { }
            byte minutes = rtc.Data.Byte;
            rtc.Address.Byte = (0 << 7) | 0x04;
            for (int i = 0; i < 500; i++) { }
            byte hour = rtc.Data.Byte;
            Debugger.DoSend("Current time: " + hour + ":" + minutes + ":" + seconds);
            
            GetCurrentTimeStampCounter(&edxPrev, &eaxPrev);

            //Wait a Little
            //This avoids errors produced by cicles wasted trying to get the cicles and loading the pit
            for (int i = 1; i < 20001; i++)
            {

            }

            GetCurrentTimeStampCounter(&edxNext, &eaxNext);
            
            //Since we finished getting the values, now we calculate the cicles and time
            ciclesPrev = (edxPrev << 32) + eaxPrev;
            ciclesNext = (edxNext << 32) + eaxNext;
            //pitNext = 

            //The cicles will be the substraction of the two read values
            ulong totalCicles = ciclesNext - ciclesPrev;
            //The elapsed ticks will be the stating counter minus the read pit.
            //ticks = (0x10000 - pitNext);
            Debugger.DoSend("Ticks: " + ticks);
            Debugger.DoSend("totalCicles: " + totalCicles);
            //Since we know that a tic will happen every 1 / 1193180 sec, its easy to get the freq (cicles per second)
            //We use 1.19 to get mhz directly
            double frequency = (totalCicles) * 1.193180 / ticks;
            Debugger.DoSend("Frequency: " + frequency + " mhz");
               
            return frequency;
        }
        */


        [PlugMethod(PlugRequired = true)]
        public static void GetCurrentTimeStampCounter(uint* eax, uint* edx)
        { }

        [PlugMethod(PlugRequired = true)]
        public static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx) { }

        [PlugMethod(PlugRequired = true)]
        public static int CanReadCPUID() => 0; //plugged

        /// <summary>
        /// Returns the number of CPU cycles since startup of the current CPU core
        /// </summary>
        /// <returns>Number of CPU cycles since startup</returns>
        public static long GetCycleCount() => 0; //plugged

        /// <summary>
        /// Returns the number of CPU cycles per seconds
        /// </summary>
        /// <returns>Number of CPU cycles per seconds</returns>
        public static long GetCycleRate() => 0; //plugged
    }
}
