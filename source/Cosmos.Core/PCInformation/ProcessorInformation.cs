using Cosmos.Core.IOGroup;
using IL2CPU.API.Attribs;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.Core.PCInformation
{
    public unsafe class ProcessorInformation
    {
        private static int irq0Counter = 0;

        /// <summary>
        /// Gets the information related to a certain cpuid operation
        /// The order of the registers returned is as follows (a register can be omited if it is not returned):
        /// EAX, EBX, ECX, EDX. If there is more than one call the order stays the same: EAX, EBX, ECX, EDX, EAX...
        /// </summary>
        /// <param name="operation"></param>
        /// <remarks>There is not a 1:1 correspondence between the enum and the cpuid call.</remarks>
        /// <returns></returns>
        public static uint[] CPUID(CPUIDOperation operation)
        {
            uint ptr = 0;
            uint eax;
            uint ebx;
            uint ecx;
            uint edx;
            uint[] returnValue;
            switch (operation)
            {
                //Special case: this requires more than one call
                case CPUIDOperation.GetProcessorBrand:
                    returnValue = new uint[12];
                    uint eax1, eax2, eax3, ebx1, ebx2, ebx3, ecx1, ecx2, ecx3, edx1, edx2, edx3;
                    //If greater than four the string is present
                    Debugger.DoSend("Highest func: " + GetHighestExtendedFunctionSupported());
                    if (GetHighestExtendedFunctionSupported() > 0x80000004)
                    {
                        CPUID(0x80000002, &eax1, &ebx1, &ecx1, &edx1);
                        CPUID(0x80000003, &eax2, &ebx2, &ecx2, &edx2);
                        CPUID(0x80000004, &eax3, &ebx3, &ecx3, &edx3);
                        returnValue[0] = eax1;
                        returnValue[1] = ebx1;
                        returnValue[2] = ecx1;
                        returnValue[3] = edx1;

                        returnValue[4] = eax2;
                        returnValue[5] = ebx2;
                        returnValue[6] = ecx2;
                        returnValue[7] = edx2;

                        returnValue[8] = eax2;
                        returnValue[9] = ebx2;
                        returnValue[10] = ecx2;
                        returnValue[11] = edx2;
                    }
                    return returnValue;
                default:
                    //In some cases this will return garbage. Caller's problem.
                    CPUID((uint) operation, &eax, &ebx, &ecx, &edx);
                    returnValue = new uint[4];
                    returnValue[0] = eax;
                    returnValue[1] = ebx;
                    returnValue[2] = ecx;
                    returnValue[3] = edx;
                    return returnValue;
            }
        }

        #region frequencyMethods
        /// <summary>
        /// WARNING: may produce inacurate results (specially in emulators).
        /// Get the frequency of the processor without using interrupts.
        /// Notes: needs bochs: "clock: sync=slowdown, time0=local\n" line to work (well, seems to).
        /// </summary>
        /// <remarks>Credit to: http://wiki.osdev.org/Detecting_CPU_Speed#Without_Interrupts . In emulators the PIT
        /// can go much more quicker than expected giving absurdly big values.
        /// </remarks>
        /// <returns>Frequency in MHz</returns>
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

        // TODO: implement this method correctly
        /// <summary>
        /// Get the frequency of the processor (using interrupts)
        /// </summary>
        /// <remarks>Credit to: http://wiki.osdev.org/Detecting_CPU_Speed#Without_Interrupts </remarks>
        /// <returns></returns>
        /*
        public static double GetFrequency()
        {
            uint eaxPrev, edxPrev, eaxNext, edxNext;
            //INTs.SetIntHandler(0x20, aHandler: AHandler);
            CPU.EnableInterrupts();
            INTs.SetIrqHandler(0, AHandler);
            PIT pit = new PIT();
            int currentCounter;

            //Put a frequency of 100 hz

            pit.Command.Byte = 0;
            pit.Data0.Byte = 0;
            pit.Data0.Byte = 0;

            pit.Command.Byte = 0x32;
            pit.Data0.Byte = 11931 >> 8;
            pit.Data0.Byte = 11931 & 0xff;

            GetCurrentTimeStampCounter(&edxPrev, &eaxPrev);
            while (irq0Counter < 5) { Debugger.DoSend("Counter" + irq0Counter); }
            currentCounter = irq0Counter;
            GetCurrentTimeStampCounter(&edxNext, &eaxNext);

            ulong ciclos = ((edxNext << 32) + eaxNext) - ((edxPrev << 32) + eaxPrev);
            return ciclos / (currentCounter * 0.01);
        }

        private static void AHandler(ref INTs.IRQContext aContext)
        {
            Debugger.DoSend("Interrupt Called");
            irq0Counter++;
        }
        */
        #endregion


        /// <summary>
        /// Get the current cicle count using rtdsc instruction
        /// </summary>
        /// <param name="eax">eax containing the low </param>
        /// <param name="edx"></param>
        [PlugMethod(PlugRequired = true)]
        private static void GetCurrentTimeStampCounter(uint* eax, uint* edx)
        { }

        /// <summary>
        /// Calls cpuid and returns the registers modified by the instruction.
        /// </summary>
        /// <param name="eaxOperation">Initial value of eax (before calling cpuid)</param>
        /// <param name="eax">Returned eax</param>
        /// <param name="ebx">Returned ebx</param>
        /// <param name="ecx">Returned ecx</param>
        /// <param name="edx">Returned edx</param>
        [PlugMethod(PlugRequired = true)]
        private static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx) { }

        [PlugMethod(PlugRequired = true)]
        private static void RDMSR(uint ecxOperation, uint* eax, uint* edx) { }

        [PlugMethod(PlugRequired = true)]
        public static int CanReadCPUID() => 0; //plugged

        /// <summary>
        /// Returns the highest extended function supported by cpuid
        /// </summary>
        /// <returns>The highest function supported (eax)</returns>
        [PlugMethod(PlugRequired = true)]
        public static uint GetHighestExtendedFunctionSupported() => 0; //plugged

        /// <summary>
        /// Read the specific rdmsr register.
        /// The value will be returned as an integer array
        /// Can be used to get the frequency but probably will return 0
        /// </summary>
        /// <param name="operation">Retuned eax and edx (in that order)</param>
        public static uint[] RDMSR(RDMSROperation operation)
        {
            uint eax, edx;
            uint[] returnValue = new uint[2];
            RDMSR((uint)operation, &eax, &edx);
            Debugger.DoSend("Sent eax: " + eax);
            Debugger.DoSend("Sent edx: " + edx);
            returnValue[0] = eax;
            returnValue[1] = edx;
            return returnValue;
        }
    }
}
