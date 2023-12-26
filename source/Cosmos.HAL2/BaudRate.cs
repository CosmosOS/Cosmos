using System;

namespace Cosmos.HAL
{
    /// <summary>
    /// Represents the baud rates for serial ports.
    /// </summary>
    public enum BaudRate : byte
    {
        /// <summary>
        /// 115200 bits per second.
        /// </summary>
        BaudRate115200 = 0x01,

        /// <summary>
        /// 57600 bits per second.
        /// </summary>
        BaudRate57600 = 0x02,

        /// <summary>
        /// 38400 bits per second.
        /// </summary>
        BaudRate38400 = 0x03,

        /// <summary>
        /// 19200 bits per second.
        /// </summary>
        BaudRate19200 = 0x06,

        /// <summary>
        /// 9600 bits per second.
        /// </summary>
        BaudRate9600 = 0x12,

        /// <summary>
        /// 7200 bits per second.
        /// </summary>
        BaudRate7200 = 0x10,

        /// <summary>
        /// 4800 bits per second.
        /// </summary>
        BaudRate4800 = 0x18,

        /// <summary>
        /// 2400 bits per second.
        /// </summary>
        BaudRate2400 = 0x30,

        /// <summary>
        /// 1200 bits per second.
        /// </summary>
        BaudRate1200 = 0x60,

        /// <summary>
        /// 600 bits per second.
        /// </summary>
        BaudRate600 = 0xC0,
    }
}
