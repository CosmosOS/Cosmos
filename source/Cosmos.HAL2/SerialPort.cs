using System;
using Cosmos.Core;
namespace Cosmos.HAL
{
    /// <summary>
    /// Provides mehods for interacting with serial ports
    /// </summary>
    public static class SerialPort
    {
        // com1 is used by qemu by default
        /// <summary>
        /// IO port for COM1 port
        /// </summary>
        public const ushort COM1 = 0x3F8;
        /// <summary>
        /// IO port for COM2 port
        /// </summary>
        public const ushort COM2 = 0x2F8;
        /// <summary>
        /// IO port for COM3 port
        /// </summary>
        public const ushort COM3 = 0x3E8;
        /// <summary>
        /// IO port for COM4 port
        /// </summary>
        public const ushort COM4 = 0x2E8;
        /// <summary>
        /// IO port for COM5 port
        /// </summary>
        public const ushort COM5 = 0x5F8;
        /// <summary>
        /// IO port for COM6 port
        /// </summary>
        public const ushort COM6 = 0x4F8;
        /// <summary>
        /// IO port for COM7 port
        /// </summary>
        public const ushort COM7 = 0x5E8;
        /// <summary>
        /// IO port for COM8 port
        /// </summary>
        public const ushort COM8 = 0x4E8; 

        /// <summary>
        /// Enables certain COM port
        /// </summary>
        /// <param name="aPort">COM port</param>
        public static void Enable(ushort aPort)
        {
            IOPort.Write8((ushort)(aPort + 1), 0x00);
            IOPort.Write8((ushort)(aPort + 3), 0x80);
            IOPort.Write8(aPort, 0x03);
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
        public static byte Receive(ushort aPort)
        {
            while ((IOPort.Read8((ushort)(aPort + 5)) & 1) == 0) { };
            return IOPort.Read8(aPort);
        }

        /// <summary>
        /// Reads byte (ASCII character) from COM1 port
        /// </summary>
        /// <returns></returns>
        public static byte Receive()
        {
            return Receive(COM1);
        }

        /// <summary>
        /// Sends character to COM port
        /// </summary>
        /// <param name="aText">Character to send</param>
        /// <param name="aPort">COM port</param>
        public static void Send(char aText, ushort aPort)
        {
            while ((IOPort.Read8((ushort)(aPort + 5)) & 0x20) == 0) { };
            IOPort.Write8(aPort, (byte)aText);
        }

        /// <summary>
        /// Sends character to COM1 port
        /// </summary>
        /// <param name="aText">Character to send</param>
        public static void Send(char aText)
        {
            Send(aText, COM1);
        }

        /// <summary>
        /// Sends string to COM port
        /// </summary>
        /// <param name="aText">String to send</param>
        /// <param name="aPort">COM port</param>
        public static void SendString(string aText, ushort aPort)
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
            SendString(aText, COM1);
        }
    }
}
