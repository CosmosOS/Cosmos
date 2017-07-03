using System;
using System.Collections.Generic;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Audio.Devices.ES1370.Registers
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

        public UInt16 ISR
        {
            get
            {
                return xMem.Read16((UInt32)Registers.MainRegister.Bit.Status);
            }
            set
            {
                xMem.Write16((UInt32)Registers.MainRegister.Bit.Status, value);
            }
        }

        public override string ToString()
        {
            return this.ISR.ToBinary(16);
        }

        #endregion

        #region Data
        public bool IsDAC1InterruptEnabled
        {
            get { return GetBit(BitPosition.IsDAC1IntEn); }
            set { SetBit(BitValue.IsDAC1IntEn, value); }
        }

        public bool IsDAC2InterruptEnabled
        {
            get { return GetBit(BitPosition.IsDAC2IntEn); }
            set { SetBit(BitValue.IsDAC2IntEn, value); }
        }

        public bool IsUARTInterruptEnabled
        {
            get { return GetBit(BitPosition.IsUARTIntEn); }
            set { SetBit(BitValue.IsUARTIntEn, value); }
        }

        public bool IsCodecWriteInProgressEnabled
        {
            get { return GetBit(BitPosition.IsCodecWIPIntEn); }
            set { SetBit(BitValue.IsCodecWIPIntEn, value); }
        }

        public bool IsCodecBusyIntEnabled
        {
            get { return GetBit(BitPosition.IsCodecBusyIntEn); }
            set { SetBit(BitValue.IsCodecBusyIntEn, value); }
        }

        public bool IsCodecStatusIntEnabled
        {
            get { return GetBit(BitPosition.IsCodecStatIntEn); }
            set { SetBit(BitValue.IsCodecStatIntEn, value); }
        }

        public bool IsMCCBIntEnabled
        {
            get { return GetBit(BitPosition.IsMCCBIntEn); }
            set { SetBit(BitValue.IsMCCBIntEn, value); }
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
            IsDAC2IntEn = 1,
            IsDAC1IntEn = 2,
            IsUARTIntEn = 3,
            IsMCCBIntEn = 4,
            IsCodecWIPIntEn = 8,
            IsCodecBusyIntEn = 9,
            IsCodecStatIntEn = 10
        }

        public enum BitValue : uint
        {
            IsDAC2IntEn = BinaryHelper.BitPos.BIT1,
            IsDAC1IntEn = BinaryHelper.BitPos.BIT2,
            IsUARTIntEn = BinaryHelper.BitPos.BIT3,
            IsMCCBIntEn = BinaryHelper.BitPos.BIT4,
            IsCodecWIPIntEn = BinaryHelper.BitPos.BIT8,
            IsCodecBusyIntEn = BinaryHelper.BitPos.BIT9,
            IsCodecStatIntEn = BinaryHelper.BitPos.BIT10
        }

        #endregion
    }
}
