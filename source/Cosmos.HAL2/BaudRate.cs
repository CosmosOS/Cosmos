using System;

namespace Cosmos.HAL
{
    /// <summary>
    /// Represents the baud rates for serial ports.
    /// </summary>
    public enum BaudRate
    {
        /// <summary>
        /// 115200 bits per second.
        /// </summary>
        BaudRate115200 = 0x001,

        /// <summary>
        /// 57600 bits per second.
        /// </summary>
        BaudRate57600 = 0x002,

        /// <summary>
        /// 38400 bits per second.
        /// </summary>
        BaudRate38400 = 0x003,

        /// <summary>
        /// 19200 bits per second.
        /// </summary>
        BaudRate19200 = 0x006,

        /// <summary>
        /// 9600 bits per second.
        /// </summary>
        BaudRate9600 = 0x012,

        /// <summary>
        /// 7200 bits per second.
        /// </summary>
        BaudRate7200 = 0x010,

        /// <summary>
        /// 4800 bits per second.
        /// </summary>
        BaudRate4800 = 0x018,

        /// <summary>
        /// 2400 bits per second.
        /// </summary>
        BaudRate2400 = 0x030,

        /// <summary>
        /// 1200 bits per second.
        /// </summary>
        BaudRate1200 = 0x060,

        /// <summary>
        /// 600 bits per second.
        /// </summary>
        BaudRate600 = 0x0C0,

        /// <summary>
        /// 300 bits per second.
        /// </summary>
        BaudRate300 = 0x180
    }
}
