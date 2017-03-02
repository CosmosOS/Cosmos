using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cosmos.HAL.PCInfo;

namespace Cosmos.System.PCInfo
{
    public class Processor
    {
        /// <summary>
        /// Specific model of the processor. If cannot be found family is provided.
        /// </summary>
        public string ProcessorVersion { get; set; }
        /// <summary>
        /// Manufacturer of the processor. Can be null
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Number of the processor inside the cpu. 
        /// </summary>
        public string SocketDesignation { get; set; }
        /// <summary>
        /// Current speed in mhz
        /// </summary>
        public int Speed { get; set; }
        /// <summary>
        /// Processor type (central, math, dsp or video)
        /// </summary>
        public string ProcessorType { get; set; }
        /// <summary>
        /// Get the flags of the processor
        /// </summary>
        public List<int> Flags { get; set; }

        public Processor(Cosmos.HAL.PCInfo.Processor processor)
        {
            this.ProcessorType = processor.ProcessorType;
            if (processor.ProcessorVersion == null || processor.ProcessorVersion == "")
                ProcessorVersion = processor.ProcessorFamily;
            else
                ProcessorVersion = processor.ProcessorVersion;
            /*
            if (processor.Manufacturer == null || processor.Manufacturer == "")
            {
                var match = Regex.Match(this.ProcessorVersion, @".*[Ii]ntel");
                if (match.Success)
                    this.Manufacturer = "GenuineIntel";
                match = Regex.Match(this.ProcessorVersion, @".*AMD.*");
                if (match.Success)
                    this.Manufacturer = "AMD";
                match = Regex.Match(this.ProcessorVersion, @".*ARM.*");
                if(match.Success)
                    this.Manufacturer = "ARM";
                //TODO: complete the list
                if (processor.Manufacturer == null || processor.Manufacturer == "")
                    this.Manufacturer = "Unknown";
            }
            */
            this.SocketDesignation = processor.SocketDesignation;
            this.Speed = processor.Speed;
            this.Flags = processor.Flags;
        }

    }
}
