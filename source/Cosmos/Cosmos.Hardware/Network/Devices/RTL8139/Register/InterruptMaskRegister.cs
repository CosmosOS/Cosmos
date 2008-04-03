using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// This register masks the interrupts that can be generated from the InterruptStatusRegister (ISR).
    /// Setting a bit to 1 will enable a corresponding bit in ISR to cause an interrupt.
    /// During a hardware reset all bits are set to 0.
    /// Offset 0x3C - 0x3D from base memory.
    /// 16 bit wide.
    /// </summary>
    class InterruptMaskRegister
    {
        [Flags]
        public enum Bit : byte
        {
            ROK = 0x00,     //Receive (Rx) OK
            RER = 0x01,     //Receive (Rx) Error
            TOK = 0x02,     //Transmit (Tx) OK
            TER = 0x03,     //Transmit (Tx) Error
            RXOVW = 0x04,   //Rx Buffer Overflow
            PUNLC = 0x05,   //Packed Underrun/Link Change
            FOVW = 0x06,    //FIFO Overflow
            LENCHG = 0x0D,  //Cable Length Changed
            TIMEOUT = 0x0E, //Raised when TCTR register matches TimeInt register
            SERR = 0x0F     //System Error. Might cause a reset.
        }

    }
}
