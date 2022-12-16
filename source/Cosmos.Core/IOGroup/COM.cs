using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// Communication port (COM) class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class COM : IOGroup
    {
        /// <summary>
        /// Data register port.
        /// </summary>
        public readonly int Data;
        /// <summary>
        /// Interrupt enable register port.
        /// </summary>
        public readonly int InterruptEnable;
        /// <summary>
        /// FIFO control register port.
        /// </summary>
        public readonly int FIFOControl;
        /// <summary>
        /// Line control register port.
        /// </summary>
        public readonly int LineControl;
        /// <summary>
        /// Modem control register port.
        /// </summary>
        public readonly int ModemControl;
        /// <summary>
        /// Line status register port.
        /// </summary>
        public readonly int LineStatus;
        /// <summary>
        /// Modem status register port.
        /// </summary>
        public readonly int ModemStatus;
        /// <summary>
        /// Scratch register port.
        /// </summary>
        public readonly int Scratch;

        /// <summary>
        /// Initializes a new set of IOPorts for the specified COM port number.
        /// </summary>
        /// <param name="comPortNumber">Can be either 1,2,3, or 4.</param>
        public COM(byte comPortNumber)
        {
            if (comPortNumber > 4)
            {
                throw new Exception("Cosmos.Core->IOGroup->COM.cs-> ERROR: Unknown COM Port.");
            }

            ushort portBase = comPortNumber switch
            {
                1 => 0x3F8,
                2 => 0x2F8,
                3 => 0x3E8,
                4 => 0x2E8,
                _ => 0
            };

            Data = portBase;
            InterruptEnable = unchecked(portBase + 1);
            FIFOControl = unchecked(portBase + 2);
            LineControl = unchecked(portBase + 3);
            ModemControl = unchecked(portBase + 4);
            LineStatus = unchecked(portBase + 5);
            ModemStatus = unchecked(portBase + 6);
            Scratch = unchecked(portBase + 7);
        }
    }
}
