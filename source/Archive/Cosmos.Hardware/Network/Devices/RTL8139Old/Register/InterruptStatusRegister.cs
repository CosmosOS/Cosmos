using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// The InterruptStatusRegister is used to indicate why an IRQ was raised. Used in conjunction with the InterruptMaskRegister.
    /// Offset 0x3E - 0x3F from base memory.
    /// 16 bit wide.
    /// </summary>
    public class InterruptStatusRegister
    {

        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static InterruptStatusRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new InterruptStatusRegister(aMem);
        }

        private InterruptStatusRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        public UInt16 ISR
        {
            get
            {
                return xMem.Read16((UInt32)Register.MainRegister.Bit.IntrStatus);
            }
            set
            {
                xMem.Write16((UInt32)Register.MainRegister.Bit.IntrStatus, value);
            }
        }

        public override string ToString()
        {
            return this.ISR.ToBinary(16);
        }

        #endregion

        #region Data

        public bool ReceiveOK
        {
            get { return GetBit(BitPosition.ROK); }
            set { SetBit(BitValue.ROK, value); }
        }

        public bool ReceiveError
        {
            get { return GetBit(BitPosition.RER); }
            set { SetBit(BitValue.RER, value); }
        }

        public bool TransmitOK
        {
            get { return GetBit(BitPosition.TOK); }
            set { SetBit(BitValue.TOK, value); }
        }

        public bool TransmitError
        {
            get { return GetBit(BitPosition.TER); }
            set { SetBit(BitValue.TER, value); }
        }

        public bool RxBufferOverflow
        {
            get { return GetBit(BitPosition.RXOVW); }
            set { SetBit(BitValue.RXOVW, value); }
        }

        public bool PacketUnderrun
        {
            get { return GetBit(BitPosition.PUNLC); }
            set { SetBit(BitValue.PUNLC, value); }
        }

        public bool RxFifoOverflow
        {
            get { return GetBit(BitPosition.FOVW); }
            set { SetBit(BitValue.FOVW, value); }
        }

        public bool TxDescriptorUnavailable
        {
            get { return GetBit(BitPosition.TDU); }
            set { SetBit(BitValue.TDU, value); }
        }

        public bool SoftwareInterrupt
        {
            get { return GetBit(BitPosition.SWINT); }
            set { SetBit(BitValue.SWINT, value); }
        }

        public bool CableLengthChange
        {
            get { return GetBit(BitPosition.LENCHG); }
            set { SetBit(BitValue.LENCHG, value); }
        }

        public bool TimeOut
        {
            get { return GetBit(BitPosition.TIMEOUT); }
            set { SetBit(BitValue.TIMEOUT, value); }
        }

        public bool SystemError
        {
            get { return GetBit(BitPosition.SERR); }
            set { SetBit(BitValue.SERR, value); }
        }

        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.ISR, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.ISR = (byte)(this.ISR | (byte)bit);
            else
                this.ISR = (byte)(this.ISR & ~(byte)bit);
        }

        #endregion

        #region Bits

        [Flags]
        public enum BitPosition : byte
        {
            ROK = 0,     //Receive (Rx) OK
            RER = 1,     //Receive (Rx) Error
            TOK = 2,     //Transmit (Tx) OK
            TER = 3,     //Transmit (Tx) Error
            RXOVW = 4,   //Rx Buffer Overflow
            PUNLC = 5,   //Packed Underrun/Link Change
            FOVW = 6,    //FIFO Overflow
            TDU = 7,     //Tx Descriptor Unavailable
            SWINT = 8,   //Software Interrupt
            LENCHG = 13,  //Cable Length Changed
            TIMEOUT = 14, //Raised when TCTR register matches TimeInt register
            SERR = 15     //System Error. Might cause a reset.
        }

        public enum BitValue : uint
        {
            ROK = BinaryHelper.BitPos.BIT0,
            RER = BinaryHelper.BitPos.BIT1,
            TOK = BinaryHelper.BitPos.BIT2,
            TER = BinaryHelper.BitPos.BIT3,
            RXOVW = BinaryHelper.BitPos.BIT4,
            PUNLC = BinaryHelper.BitPos.BIT5,
            FOVW = BinaryHelper.BitPos.BIT6,
            TDU = BinaryHelper.BitPos.BIT7,
            SWINT = BinaryHelper.BitPos.BIT8,
            LENCHG = BinaryHelper.BitPos.BIT13,
            TIMEOUT = BinaryHelper.BitPos.BIT14,
            SERR = BinaryHelper.BitPos.BIT15
        }

        #endregion
    }
}
