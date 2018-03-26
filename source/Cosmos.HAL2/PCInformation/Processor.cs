using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.Core.PCInformation;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.PCInformation
{
    /// <summary>
    /// This class represents one processor of the machine
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// Manufacturer of the procesor (on intel: genuine intel)
        /// </summary>
        public string Manufacturer { get; private set; }
        /// <summary>
        /// Processor family number
        /// </summary>
        public int Family{ get; private set; }
        /// <summary>
        /// Flags of the processor (sse, fpu and so on)
        /// </summary>
        public List<int> Flags { get; private set; }
        /// <summary>
        /// Stepping of the processor
        /// </summary>
        public int Stepping { get; private set; }
        /// <summary>
        /// Model number
        /// </summary>
        public int ModelNumber { get; private set; }
        /// <summary>
        /// Frequency of the processor in mhz
        /// </summary>
        public double Frequency { get; private set; }

        private Debugger mDebugger = new Debugger("HAL", "Processor");

        public Processor()
        {
            //Old smbios calls
            /*
            Speed = SMBIOSProcessor.CurrentSpeed;
            ProcessorType = ParseType(SMBIOSProcessor.ProcessorType);
            ProcessorFamily = ParseFamily(SMBIOSProcessor.ProcessorFamily);
            SocketDesignation = SMBIOSProcessor.SocketDesignation;
            Manufacturer = SMBIOSProcessor.ProcessorManufacturer;
            ProcessorVersion = SMBIOSProcessor.ProcessorVersion;
            Flags = ParseFlags(SMBIOSProcessor.CPUIDEAX, SMBIOSProcessor.CPUIDEDX);
            */
            this.Manufacturer = GetVendorName();
            this.Flags = ParseFlags();
            //Parses stepping, model and family
            ParseInformation();
            this.Frequency = ProcessorInformation.GetFrequency();
        }

        /// <summary>
        /// Parses multiple information using eax = 1 and the returned eax register
        /// </summary>
        public void ParseInformation()
        {
            uint[] raw = ProcessorInformation.CPUID(CPUIDOperation.GetProcessorInformation);
            ///Position of the 96 bit signature
            uint eax = raw[0];
            //NOTE: these equations are taken from the intel manual
            //We need the AND to get only the important bits
            //Get the first 4 bits
            this.Stepping = (int) eax & 15;
            //Sum the extended model to the standard model
            //Extended model: get bits 19:16 (base zero) and shift them 4 to the left
            //Standard model: get bits 7:4.
            this.ModelNumber = (((int)eax >> 16) & 15) << 4 + ((int) eax >> 4) & 15;
            //Get bits 27:20 of the extended family
            //Get bits 11:8 of the standard family
            this.Family = (((int) eax >> 20) & 511) + (((int) eax >> 8) & 15);
        }

        public string GetVendorName()
        {
            if (ProcessorInformation.CanReadCPUID() > 0)
            {
                uint[] raw = ProcessorInformation.CPUID(CPUIDOperation.GetVendorID);
                uint ebx = raw[1];
                uint ecx = raw[2];
                uint edx = raw[3];
                //A little more inefficient but a lot clearer
                return ConvertIntegerToString(ebx) + ConvertIntegerToString(edx) + ConvertIntegerToString(ecx);
            }
            else
                return "\0";
        }

        public string GetBrandName()
        {
            if (ProcessorInformation.CanReadCPUID() > 0)
            {
                uint[] raw = ProcessorInformation.CPUID(CPUIDOperation.GetProcessorBrand);
                return ConvertIntegerToString(raw[0]) +
                       ConvertIntegerToString(raw[1]) +
                       ConvertIntegerToString(raw[2]) +
                       ConvertIntegerToString(raw[3]) +
                       ConvertIntegerToString(raw[4]) +
                       ConvertIntegerToString(raw[5]) +
                       ConvertIntegerToString(raw[6]) +
                       ConvertIntegerToString(raw[8]) +
                       ConvertIntegerToString(raw[9]) +
                       ConvertIntegerToString(raw[10]) +
                       ConvertIntegerToString(raw[11]);
            }
            else
                return "\0";
        }

        public string ConvertIntegerToString(uint integer)
        {
            return new string(new char[]
            {
                (char) (integer & 0xff),
                (char) ((integer >> 8) & 0xff),
                (char) ((integer >> 16) & 0xff),
                (char) (integer >> 24)
            });
        }

        /// <summary>
        /// Parse the flags for an x86 machine
        /// TODO: in the future will need to overload this method with a variant for amr32 and arm64.
        /// </summary>
        /// <returns></returns>
        public List<int> ParseFlags()
        {
            uint[] raw = ProcessorInformation.CPUID(CPUIDOperation.GetProcessorInformation);
            //List of the every possible flag
            //Its impossible to do a list of enums (il2cpu errors). 
            //You cannot cast by using methods like ToList()...
            //So we use the old friend "int".
            List<int> listOfFlags = new List<int>();

            uint ecx = raw[2];
            //Regular flags
            uint edx = raw[3];

            //We need to convert edx to something that can be traslated to a bit array safely
            //We can't do (int)eax
            var edxBytes = BitConverter.GetBytes(edx);
            BitArray bitArrayEdx = new BitArray(edxBytes);

            //See: https://en.wikipedia.org/wiki/CPUID (this is where i got the information).

            mDebugger.Send("Bits parsed. Adding enums to table");
            //TODO: this gives ilcpu error
            int offset = 0;
            //Foreach bit in ea
            for (int i = 0; i < 32; i++)
            {
                //Skip reserved flags
                if (i == 10 || i == 20)
                {
                    offset--; 
                    continue;
                }
                mDebugger.Send("EDX: " + ((int)edx & (1 << i)));
                if (((int)edx & (1 << i)) > 0) listOfFlags.Add(i+offset);
            }

            for (int i = 0; i < 32; i++)
            {
                //Skip reserved flag
                if (i == 16)
                {
                    offset--;
                }
                if (((int) ecx & (1 << i)) > 0) listOfFlags.Add(i + offset + 32);
            }
            mDebugger.Send("Strings added and parsed.");
            return listOfFlags;
        }
    }
}
