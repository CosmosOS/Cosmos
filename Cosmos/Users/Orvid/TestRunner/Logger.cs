using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Core;

namespace TestRunner
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

        private int IsTransmitEmpty()
        {
            return iop.LineStatus.Byte & 0x20;
        }

        #region Write Data

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
        /// Writes the specified sbyte to the Log.
        /// </summary>
        /// <param name="b">The sbyte to write.</param>
        public void WriteData(sbyte b)
        {
            WriteData(unchecked((byte)b));
        }

        /// <summary>
        /// Writes the specified ushort to the Log.
        /// </summary>
        /// <param name="b">The ushort to write.</param>
        public void WriteData(ushort s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified short to the Log.
        /// </summary>
        /// <param name="b">The short to write.</param>
        public void WriteData(short s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified uint to the Log.
        /// </summary>
        /// <param name="b">The uint to write.</param>
        public void WriteData(uint s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
            WriteData(unchecked((byte)((s >> 16) & 0xFF)));
            WriteData(unchecked((byte)((s >> 24) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified int to the Log.
        /// </summary>
        /// <param name="b">The int to write.</param>
        public void WriteData(int s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
            WriteData(unchecked((byte)((s >> 16) & 0xFF)));
            WriteData(unchecked((byte)((s >> 24) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified ulong to the Log.
        /// </summary>
        /// <param name="b">The ulong to write.</param>
        public void WriteData(ulong s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
            WriteData(unchecked((byte)((s >> 16) & 0xFF)));
            WriteData(unchecked((byte)((s >> 24) & 0xFF)));
            WriteData(unchecked((byte)((s >> 32) & 0xFF)));
            WriteData(unchecked((byte)((s >> 40) & 0xFF)));
            WriteData(unchecked((byte)((s >> 48) & 0xFF)));
            WriteData(unchecked((byte)((s >> 56) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified long to the Log.
        /// </summary>
        /// <param name="b">The long to write.</param>
        public void WriteData(long s)
        {
            WriteData(unchecked((byte)(s & 0xFF)));
            WriteData(unchecked((byte)((s >> 8) & 0xFF)));
            WriteData(unchecked((byte)((s >> 16) & 0xFF)));
            WriteData(unchecked((byte)((s >> 24) & 0xFF)));
            WriteData(unchecked((byte)((s >> 32) & 0xFF)));
            WriteData(unchecked((byte)((s >> 40) & 0xFF)));
            WriteData(unchecked((byte)((s >> 48) & 0xFF)));
            WriteData(unchecked((byte)((s >> 56) & 0xFF)));
        }

        /// <summary>
        /// Writes the specified float to the Log.
        /// </summary>
        /// <param name="b">The float to write.</param>
        public void WriteData(float b)
        {
            byte[] s = BitConverter.GetBytes(b);
            WriteData(s[0]);
            WriteData(s[1]);
            WriteData(s[2]);
            WriteData(s[3]);
        }

        /// <summary>
        /// Writes the specified double to the Log.
        /// </summary>
        /// <param name="b">The double to write.</param>
        public void WriteData(double b)
        {
            byte[] s = BitConverter.GetBytes(b);
            WriteData(s[0]);
            WriteData(s[1]);
            WriteData(s[2]);
            WriteData(s[3]);
            WriteData(s[4]);
            WriteData(s[5]);
            WriteData(s[6]);
            WriteData(s[7]);
        }

        #endregion


        /// <summary>
        /// Writes the specified string to the Log, followed by EOL.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public void WriteLine(string s)
        {
            WriteString(s + "\r\n");
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
