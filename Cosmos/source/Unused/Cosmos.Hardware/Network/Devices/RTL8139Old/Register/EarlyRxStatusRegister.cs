using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// The Early Receive Status Register (ERSR) is used as an indicator when incoming data is received.
    /// Is 1 byte wide, but only 4 bits used.
    /// Offset 0x36h from main memory.
    /// </summary>
    public class EarlyRxStatusRegister
    {
        private byte ersr;
        public EarlyRxStatusRegister(byte data)
        {
            ersr = data;
        }

        public bool IsEarlyRXOkay()
        {
            return BinaryHelper.CheckBit((ushort)ersr, (ushort)Bit.EROK);
        }

        public bool IsEarlyRXOverwrite()
        {
            return BinaryHelper.CheckBit((ushort)ersr, (ushort)Bit.EROVW);
        }

        public bool IsEarlyRXBadPacket()
        {
            return BinaryHelper.CheckBit((ushort)ersr, (ushort)Bit.ERBAD);
        }

        public bool IsEarlyRXGoodPacket()
        {
            return BinaryHelper.CheckBit((ushort)ersr, (ushort)Bit.ERGOOD);
        }

        public enum Bit : ushort
        {
            /// <summary>
            /// Early RX OK. Initial value 0. Set when Rx byte count exceeds Rx threshold.
            /// When whole packet is received the ROK or RER is set in ISR, and this bit is cleared.
            /// </summary>
            EROK = 0x00,
            /// <summary>
            /// Early Rx Overwrite. Set when local address pointer is equal to CAPR. In the early mode
            /// this is different from buffer overflow.
            /// </summary>
            EROVW = 0x01,
            /// <summary>
            /// Set when a packet is completely received, but the packet is bad.
            /// </summary>
            ERBAD = 0x02,
            /// <summary>
            /// Set when packet is completely received, and the packet is good.
            /// </summary>
            ERGOOD = 0x03
        }
    }
}
