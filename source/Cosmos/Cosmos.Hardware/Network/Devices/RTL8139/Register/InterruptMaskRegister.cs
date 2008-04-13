using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware.Extension.NumberSystem;

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

        #region Constructor

        private MemoryAddressSpace xMem;
        public static InterruptMaskRegister Load(MemoryAddressSpace aMem)
        {
            return new InterruptMaskRegister(aMem);
        }

        private InterruptMaskRegister(MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        public UInt16 IMR
        {
            get
            {
                return xMem.Read16((UInt32)Register.MainRegister.Bit.IntrMask);
            }
            set
            {
                xMem.Write16((UInt32)Register.MainRegister.Bit.IntrMask, value);
            }
        }

        public override string ToString()
        {
            return this.IMR.ToBinary(16);
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

        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.IMR, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.IMR = (byte)(this.IMR | (byte)bit);
            else
                this.IMR = (byte)(this.IMR & ~(byte)bit);
        }

        #endregion

        #region Bits

        [Flags]
        public enum BitPosition : byte
        {
            //TODO: CHECK THESE VALUES!!
            ROK = 0,     //Receive (Rx) OK
            RER = 1,     //Receive (Rx) Error
            TOK = 2,     //Transmit (Tx) OK
            TER = 3,     //Transmit (Tx) Error
            RXOVW = 4,   //Rx Buffer Overflow
            PUNLC = 5,   //Packed Underrun/Link Change
            FOVW = 6,    //FIFO Overflow
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
            LENCHG = BinaryHelper.BitPos.BIT13,
            TIMEOUT = BinaryHelper.BitPos.BIT14,
            SERR = BinaryHelper.BitPos.BIT15     
        }

        #endregion
    }
}
