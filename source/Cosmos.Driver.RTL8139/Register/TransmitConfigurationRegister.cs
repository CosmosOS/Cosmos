using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Driver.RTL8139.Register
{
    /// <summary>
    /// The TransmitConfigurationRegister (TCR) defines transmit configuration. It controls functions as
    /// loopback, heartbeat, auto transmit padding, Programmable Interframe Gap, Fill and Drain thresholds
    /// and maximum DMS burst size.
    /// Is 32 bits wide. Offset 0x40h-0x43h from main memory.
    /// </summary>
    public class TransmitConfigurationRegister
    {
        private UInt32 tcr;
        public TransmitConfigurationRegister(UInt32 data)
        {
            tcr = data;
        }

        /// <summary>
        /// Retrieves 6 bits 
        /// </summary>
        /// <returns></returns>
        public byte GetHWVERID()
        {
            byte mask = 249; // 1111 1001
            byte hwverid = Misc.BinaryHelper.GetByteFrom32bit(tcr, (byte)(23));
            return (byte)(mask & hwverid);
        }

        public enum Bit
        {
            /// <summary>
            /// Setting to 1 will cause RTL8139 to retransmit packet. Only allowed in transmit abort state.
            /// </summary>
            CLRABT = 0,
            /// <summary>
            /// Tx Retry Count - 4 bits wide. Tx retry count in multiple of 16. Retries = 16 + (TXRR * 16) times.
            /// </summary>
            TXRR = 4,
            /// <summary>
            /// Max DMA Burst Size per Tx DMA Burst. Se documentation for value details.
            /// </summary>
            MAXDMA0 = 8,
            MAXDMA1 = 9,
            MAXDMA2 = 10,
            /// <summary>
            /// Append CRC. 0 = CRC is appended. 1 = CRC not appended.
            /// </summary>
            CRC = 16,
            /// <summary>
            /// Loopback test. 00 is normal. 11 is loopback mode.
            /// </summary>
            LBK0 = 17,
            LBK1 = 18,
            /// <summary>
            /// Revisision. If this bit is 1 then the network card is RTL8139 rev.G. All other revisions have bit set to 0.
            /// </summary>
            REVG = 23,
            /// <summary>
            /// Interframe Gap Time. Adjusts time between frames. 9.6 micro sec for 10Mbps. 0,96 micro sec for 100Mbps.
            /// Only 0xFF is valid according to IEEE 802.3 standard. Two bits wide.
            /// </summary>
            IFG = 24,
            /// <summary>
            /// Hardware Version ID. 5 bits wide (6 with bit 23). See separate method to convert to string.
            /// </summary>
            HWVERID = 26,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwverid">Must be the byte from bit 23 to bit 30 in the TCR! Bit 24 and 25 must be 0.</param>
        /// <returns></returns>
        public static string GetHardwareRevision(byte hwverid)
        {
            switch (hwverid)
            {
                case 192: //11000000
                    return "RTL8139";
                case 224: //11100000
                    return "RTL8139A";
                case 225: //11100001
                    return "RTL8139A-G";
                case 232: //11101000
                    return "RTL8139C";
                case 233:
                    return "RTL8139C+";
                case 240: //11110000
                    return "RTL8139B";
                case 248: //11111000
                    return "RTL8130";
                default:
                    return "Unknown RTL813xxx revision (" + hwverid + ")";
            }

        }
    }
}
