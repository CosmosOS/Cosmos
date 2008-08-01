using System;
using System.Collections.Generic;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Registers
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
        public bool DAC1InterruptEnabled
        {
            get { return GetBit(BitPosition.DAC1IntEn); }
            set { SetBit(BitValue.DAC1IntEn, value); }
        }

        public bool DAC2InterruptEnabled
        {
            get { return GetBit(BitPosition.DAC2IntEn); }
            set { SetBit(BitValue.DAC2IntEn, value); }
        }

        public bool UARTInterruptEnabled
        {
            get { return GetBit(BitPosition.UARTIntEn); }
            set { SetBit(BitValue.UARTIntEn, value); }
        }

        public bool CodecWriteInProgressEnabled
        {
            get { return GetBit(BitPosition.CodecWIPIntEn); }
            set { SetBit(BitValue.CodecWIPIntEn, value); }
        }

        public bool CodecBusyIntEnabled
        {
            get { return GetBit(BitPosition.CodecBusyIntEn); }
            set { SetBit(BitValue.CodecBusyIntEn, value); }
        }

        public bool CodecStatusIntEnabled
        {
            get { return GetBit(BitPosition.CodecStatIntEn); }
            set { SetBit(BitValue.CodecStatIntEn, value); }
        }

        public bool MCCBIntEnabled
        {
            get { return GetBit(BitPosition.MCCBIntEn); }
            set { SetBit(BitValue.MCCBIntEn, value); }
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
            DAC2IntEn = 1,
            DAC1IntEn = 2,
            UARTIntEn = 3,
            MCCBIntEn = 4,
            CodecWIPIntEn = 8,
            CodecBusyIntEn = 9,
            CodecStatIntEn = 10
        }

        public enum BitValue : uint
        {
            DAC2IntEn = BinaryHelper.BitPos.BIT1,
            DAC1IntEn = BinaryHelper.BitPos.BIT2,
            UARTIntEn = BinaryHelper.BitPos.BIT3,
            MCCBIntEn = BinaryHelper.BitPos.BIT4,
            CodecWIPIntEn = BinaryHelper.BitPos.BIT8,
            CodecBusyIntEn = BinaryHelper.BitPos.BIT9,
            CodecStatIntEn = BinaryHelper.BitPos.BIT10
        }

        #endregion
    }
}
