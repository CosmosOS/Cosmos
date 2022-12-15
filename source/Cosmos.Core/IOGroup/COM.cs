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
        public readonly ushort Data;
        /// <summary>
        /// Interrupt enable register port.
        /// </summary>
        public readonly ushort InterruptEnable;
        /// <summary>
        /// FIFO control register port.
        /// </summary>
        public readonly ushort FIFOControl;
        /// <summary>
        /// Line control register port.
        /// </summary>
        public readonly ushort LineControl;
        /// <summary>
        /// Modem control register port.
        /// </summary>
        public readonly ushort ModemControl;
        /// <summary>
        /// Line status register port.
        /// </summary>
        public readonly ushort LineStatus;
        /// <summary>
        /// Modem status register port.
        /// </summary>
        public readonly ushort ModemStatus;
        /// <summary>
        /// Scratch register port.
        /// </summary>
        public readonly ushort Scratch;

        /// <summary>
        /// Initializes a new set of IOPorts for the specified COM port number.
        /// </summary>
        /// <param name="comPortNumber">Can be either 1,2,3, or 4.</param>
        public COM(byte comPortNumber)
        {
            if (comPortNumber > 4 && comPortNumber != 0)
            {
                throw new Exception("Cosmos.Core->IOGroup->COM.cs-> ERROR: Unknown COM Port.");
            }
            ushort portBase = 0;
            switch (comPortNumber)
            {
                case 1:
                    portBase = 0x3F8;
                    break;
                case 2:
                    portBase = 0x2F8;
                    break;
                case 3:
                    portBase = 0x3E8;
                    break;
                case 4:
                    portBase = 0x2E8;
                    break;
            }
            Data = portBase;
            InterruptEnable = unchecked((ushort)(portBase + 1));
            FIFOControl = unchecked((ushort)(portBase + 2));
            LineControl = unchecked((ushort)(portBase + 3));
            ModemControl = unchecked((ushort)(portBase + 4));
            LineStatus = unchecked((ushort)(portBase + 5));
            ModemStatus = unchecked((ushort)(portBase + 6));
            Scratch = unchecked((ushort)(portBase + 7));
        }
    }
}
