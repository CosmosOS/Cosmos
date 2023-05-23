using System;
using Cosmos.Core;
namespace Cosmos.HAL
{
    /// <summary>
    /// Provides mehods for interacting with serial ports
    /// </summary>
    public static class SerialPort
    {
        /// <summary>
        /// Enables certain COM port
        /// </summary>
        /// <param name="aPort">COM port</param>
        public static void Enable(COMPort aPort, BaudRate aBaudRate)
        {
            IOPort.Write8((ushort)(aPort + 1), 0x00);
            IOPort.Write8((ushort)(aPort + 3), 0x80);
            IOPort.Write8((ushort)aPort, (byte)aBaudRate);
            IOPort.Write8((ushort)(aPort + 1), 0x00);
            IOPort.Write8((ushort)(aPort + 3), 0x03);
            IOPort.Write8((ushort)(aPort + 2), 0xC7);
            IOPort.Write8((ushort)(aPort + 4), 0x0B);
        }

        /// <summary>
        /// Reads byte (ASCII character) from COM port
        /// </summary>
        /// <param name="aPort">COM port</param>
        /// <returns>ASCII character as byte</returns>
        public static byte Receive(COMPort aPort)
        {
            while ((IOPort.Read8((ushort)(aPort + 5)) & 1) == 0) { };
            return IOPort.Read8((ushort)aPort);
        }

        /// <summary>
        /// Reads byte (ASCII character) from COM1 port
        /// </summary>
        /// <returns></returns>
        public static byte Receive()
        {
            return Receive(COMPort.COM1);
        }

        /// <summary>
        /// Sends character to COM port
        /// </summary>
        /// <param name="aText">Character to send</param>
        /// <param name="aPort">COM port</param>
        public static void Send(char aText, COMPort aPort)
        {
            while ((IOPort.Read8((ushort)(aPort + 5)) & 0x20) == 0) { };
            IOPort.Write8((ushort)aPort, (byte)aText);
        }

        /// <summary>
        /// Sends character to COM1 port
        /// </summary>
        /// <param name="aText">Character to send</param>
        public static void Send(char aText)
        {
            Send(aText, COMPort.COM1);
        }

        /// <summary>
        /// Sends string to COM port
        /// </summary>
        /// <param name="aText">String to send</param>
        /// <param name="aPort">COM port</param>
        public static void SendString(string aText, COMPort aPort)
        {
            for (int i = 0; i < aText.Length; ++i)
            {
                Send(aText[i], aPort);
            }
        }
        /// <summary>
        /// Sends string to COM1 port
        /// </summary>
        /// <param name="aText">String to send</param>
        public static void SendString(string aText)
        {
            SendString(aText, COMPort.COM1);
        }
    }
}
