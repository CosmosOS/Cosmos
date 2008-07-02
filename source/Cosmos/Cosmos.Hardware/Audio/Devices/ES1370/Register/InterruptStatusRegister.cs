using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Register
{
    /// <summary>
    /// The InterruptStatusRegister is used to indicate why an IRQ was raised. Used in conjunction with the InterruptMaskRegister.
    /// </summary>
    class InterruptStatusRegister
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
        /*
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
        }*/

        #endregion

        #region Data

        /*
        public bool SoftwareInterrupt
        {
            get { return GetBit(BitPosition.SWINT); }
            set { SetBit(BitValue.SWINT, value); }
        }


        public bool SystemError
        {
            get { return GetBit(BitPosition.SERR); }
            set { SetBit(BitValue.SERR, value); }
          
        }*/

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
        }

        public enum BitValue : uint
        {
        }

        #endregion
    }
}
