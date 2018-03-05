using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cosmos.System.PCInfo
{
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
        /// Brand of the processor
        /// </summary>
        public string Brand { get; private set; }

        /// <summary>
        /// Frequency in mhz
        /// </summary>
        public double Frequency { get; private set; }

        public Processor()
        {
            Cosmos.HAL.PCInformation.Processor processor = new HAL.PCInformation.Processor();
            this.Flags = processor.Flags;
            this.Manufacturer = processor.Manufacturer;
            this.Family = processor.Family;
            this.ModelNumber = processor.ModelNumber;
            this.Stepping = processor.Stepping;
            this.Brand = processor.GetBrandName();
            this.Frequency = processor.Frequency;
        }

    }
}
