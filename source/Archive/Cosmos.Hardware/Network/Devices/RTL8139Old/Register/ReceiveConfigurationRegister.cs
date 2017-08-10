using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// Receive Configuration Register is used to set receive configuration.
    /// Offset 44h from main memory.
    /// Is 32 bits wide.
    /// </summary>
    public class ReceiveConfigurationRegister
    {
        private Kernel.MemoryAddressSpace xMem;

        public static ReceiveConfigurationRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new ReceiveConfigurationRegister(aMem);
        }

        private ReceiveConfigurationRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        public void Init()
        {
            UInt32 data = (UInt32)(BitValue.RBLEN0 | BitValue.MXDMA0 | BitValue.MXDMA1 | BitValue.AB | BitValue.AM | BitValue.APM);
            this.RCR = data;
        }

        /// <summary>
        /// Get or Sets all 32 bits in Receive Configuration Register
        /// </summary>
        public UInt32 RCR 
        { 
            get 
            {
                return xMem.Read32((UInt32)Register.MainRegister.Bit.RxConfig);
            } 
            set 
            {
                xMem.Write32((UInt32)Register.MainRegister.Bit.RxConfig, value);
            } 
        }

        public override string ToString()
        {
            return this.RCR.ToBinary(32);
        }

        /// <summary>
        /// Gets or Sets the promiscuous mode. When Promisuous mode set ALL detected packets on network are put into Receive buffer, not
        /// just the packets sent directly to us.
        /// </summary>
        public bool PromiscuousMode 
        {
            get
            {
                return BinaryHelper.CheckBit(this.RCR, 0);
            }
            set
            {
                this.RCR = BinaryHelper.FlipBit(this.RCR, 0);
            }
        }
        
        /// <summary>
        /// Enables the Rx buffer to act as a ring buffer: if a packet is being written near 
        /// the end of the buffer and the RTL8139 knows you've already handled data before this 
        /// (thanks to CAPR), the packet will continue at the beginning of the buffer. 
        /// </summary>
        public bool Wrap
        {
            get
            {
                return BinaryHelper.CheckBit(RCR,
                                             7);
            }
            set
            {
                RCR = BinaryHelper.FlipBit(RCR,
                                           7);
            }
        }

        public enum BitValue : uint
        {
            /// <summary>
            /// Accept Physical Address Packets. 0 rejects, 1 accepts.
            /// </summary>
            AAP = BinaryHelper.BitPos.BIT0,
            /// <summary>
            /// Accept Physical Match Packets. 0 rejects, 1 accepts.
            /// </summary>
            APM = BinaryHelper.BitPos.BIT1,
            /// <summary>
            /// Accept Multicast Packets. 0 rejects, 1 accepts.
            /// </summary>
            AM = BinaryHelper.BitPos.BIT2,
            /// <summary>
            /// Accept Broadcast Packets. 0 rejects, 1 accepts.
            /// </summary>
            AB = BinaryHelper.BitPos.BIT3,
            /// <summary>
            /// Accept Runt Packets (packets smaller than 64 bytes - but over 8 bytes.)
            /// </summary>
            AR = BinaryHelper.BitPos.BIT4,
            /// <summary>
            /// Accept Error Packets (Packets with CRC error, alignment error and/or collided fragments).
            /// </summary>
            AER = BinaryHelper.BitPos.BIT5,
            /// <summary>
            /// EEPROM used. 0 = 9346, 1 = 9356.
            /// </summary>
            EEPROM = BinaryHelper.BitPos.BIT6,
            /// <summary>
            /// (Only C mode) 0: Wrap incoming packet to beginning of next RxBuffer.
            /// 1: Overflow packet even after coming to end of buffer.
            /// </summary>
            WRAP = BinaryHelper.BitPos.BIT7,
            /// <summary>
            /// Three bits wide.
            /// Max DMA Burst Size per Rx DMA Burst. 010 = 64 bytes, 011 = 128 bytes, 100 = 256 bytes.
            /// </summary>
            MXDMA0 = BinaryHelper.BitPos.BIT8,
            MXDMA1 = BinaryHelper.BitPos.BIT9,
            MXDMA2 = BinaryHelper.BitPos.BIT10,
            /// <summary>
            /// RxBuffer Length.
            /// 00 = 8k + 16 byte
            /// 01 = 16k + 16 byte
            /// 10 = 32k + 16 byte
            /// 11 = 64k + 16 byte
            /// </summary>
            RBLEN0 = BinaryHelper.BitPos.BIT11,
            RBLEN1 = BinaryHelper.BitPos.BIT12,
            /// <summary>
            /// Rx FIFO Threshold. Three bits wide.
            /// When received byte count matches this level the incoming data will
            /// be transferred from FIFO to host memory.
            /// See 8139C+ specs for valid values.
            /// </summary>
            RXFTH0 = BinaryHelper.BitPos.BIT13,
            /// <summary>
            /// Receive Error Packets Larger than 8 bytes. Yes if 1. If 0 (default) then 
            /// 64-byte error packets are received. Also depends on AER or AR bits.
            /// </summary>
            RER8 = BinaryHelper.BitPos.BIT16,
            /// <summary>
            /// Multiple Early Interrupt Select. 1 bit wide.
            /// </summary>
            MULERINT = BinaryHelper.BitPos.BIT17,
            /// <summary>
            /// Early Rx Threshold. 4 bits wide.
            /// </summary>
            ERTH0 = BinaryHelper.BitPos.BIT24
        }
    }
}
