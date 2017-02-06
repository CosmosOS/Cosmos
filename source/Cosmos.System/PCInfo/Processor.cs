using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.PCInfo
{
    public class Processor
    {
        /// <summary>
        /// Type of processor (e.g Inter i5)
        /// </summary>
        public string ProcessorVersion { get; set; }
        /// <summary>
        /// Manufacturer of the processor
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Number of the processor inside the cpu
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
        /// Processor family
        /// </summary>
        /// <param name="SMBIOSProcessor"></param>
        public string ProcessorFamily { get; set; }

        public Processor(Cosmos.HAL.PCInfo.Processor processor)
        {
            this.Manufacturer = processor.Manufacturer;
            this.ProcessorFamily = processor.ProcessorFamily;
            this.ProcessorType = processor.ProcessorType;
            this.ProcessorVersion = processor.ProcessorVersion;
            this.SocketDesignation = processor.SocketDesignation;
            this.Speed = processor.Speed;
        }
    }
}
