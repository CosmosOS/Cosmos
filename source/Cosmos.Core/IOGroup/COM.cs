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
        public readonly IOPort Data;
        /// <summary>
        /// Interrupt enable register port.
        /// </summary>
        public readonly IOPort InterruptEnable;
        /// <summary>
        /// FIFO control register port.
        /// </summary>
        public readonly IOPort FIFOControl;
        /// <summary>
        /// Line control register port.
        /// </summary>
        public readonly IOPort LineControl;
        /// <summary>
        /// Modem control register port.
        /// </summary>
        public readonly IOPort ModemControl;
        /// <summary>
        /// Line status register port.
        /// </summary>
        public readonly IOPort LineStatus;
        /// <summary>
        /// Modem status register port.
        /// </summary>
        public readonly IOPort ModemStatus;
        /// <summary>
        /// Scratch register port.
        /// </summary>
        public readonly IOPort Scratch;

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
            Data = new IOPort(portBase);
            InterruptEnable = new IOPort(unchecked((ushort)(portBase + 1)));
            FIFOControl = new IOPort(unchecked((ushort)(portBase + 2)));
            LineControl = new IOPort(unchecked((ushort)(portBase + 3)));
            ModemControl = new IOPort(unchecked((ushort)(portBase + 4)));
            LineStatus = new IOPort(unchecked((ushort)(portBase + 5)));
            ModemStatus = new IOPort(unchecked((ushort)(portBase + 6)));
            Scratch = new IOPort(unchecked((ushort)(portBase + 7)));
        }
    }
}
