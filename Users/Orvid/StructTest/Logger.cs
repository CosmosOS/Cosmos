using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Core;

namespace StructTest
{
    /// <summary>
    /// Enables logging to a Com Port.
    /// </summary>
    public class Logger
    {
        private readonly Cosmos.Core.IOGroup.COM iop;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="comPort">This should be either 1, 2, 3, or 4.</param>
        public Logger(byte comPort)
        {
            iop = new Cosmos.Core.IOGroup.COM(comPort);
            Initialize();
        }

        private void Initialize()
        {
            iop.InterruptEnable.Byte = 0x00;
            iop.LineControl.Byte = 0x80;
            iop.Data.Byte = 0x03;
            iop.InterruptEnable.Byte = 0x00;
            iop.LineControl.Byte = 0x03;
            iop.FIFOControl.Byte = 0xC7;
            iop.ModemControl.Byte = 0x0B;
        }

        public void WriteLine(string s)
        {
            WriteString(s + "\r\n");
        }

        private int IsTransmitEmpty()
        {
            return iop.LineStatus.Byte & 0x20;
        }

        /// <summary>
        /// Writes the specified byte to the Log.
        /// </summary>
        /// <param name="b">The byte to write.</param>
        public void WriteData(byte b)
        {
            // Empty loop, will allow us to timeout
            for (uint i = 0; i < 1000 && (IsTransmitEmpty() == 0); i++) ;
            iop.Data.Byte = b;
        }

        /// <summary>
        /// Writes the specified string to the Log.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public void WriteString(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                WriteData((byte)s[i]);
            }
        }

    }
}
