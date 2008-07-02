using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Register
{
    /// <summary>
    /// This register masks the interrupts that can be generated from the InterruptStatusRegister (ISR).
    /// Setting a bit to 1 will enable a corresponding bit in ISR to cause an interrupt.
    /// During a hardware reset all bits are set to 0.
    /// </summary>
    class InterruptMaskRegister
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static InterruptMaskRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new InterruptMaskRegister(aMem);
        }

        private InterruptMaskRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }
        /*
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
        */
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

        }

        public enum BitValue : uint
        {
         
        }

        #endregion
    }
}
