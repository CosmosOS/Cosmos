/*
using System;
using System.Collections.Generic;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;
namespace Cosmos.Hardware2.Audio.Devices.ES1370.Registers
{
    class ControlRegister
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static ControlRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new ControlRegister(aMem);
        }

        private ControlRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        /// <summary>
        /// Get or Sets the 8 bits in the Control Register.
        /// </summary>
        public byte CONTROL
        {
            get
            {
                return xMem.Read8((UInt32)Registers.MainRegister.Bit.Control);
            }
            set
            {
                xMem.Write8((UInt32)Registers.MainRegister.Bit.Control, value);
            }
        }

        #endregion

        #region Register Data

        public bool PowerEnabled
        {
            get
            {
                return !GetBit(BitPosition.SERRDis);
            }
            set
            {
                SetBit(BitValue.SERRDis, !value);
            }
        }

        public bool CodecEnabled
        {
            get
            {
                return GetBit(BitPosition.CodecEn);
            }
            set
            {
                SetBit(BitValue.CodecEn, value);
            }
        }

        public bool DAC1Enabled
        {
            get
            {
                return GetBit(BitPosition.DAC1En);
            }
            set
            {
                SetBit(BitValue.DAC1En, value);
            }
        }

        public bool DAC2Enabled
        {
            get
            {
                return GetBit(BitPosition.DAC2En);
            }
            set
            {
                SetBit(BitValue.DAC2En, value);
            }
        }

        public bool UARTEnabled
        {
            get
            {
                return GetBit(BitPosition.UARTEn);
            }
            set
            {
                SetBit(BitValue.UARTEn, value);
            }
        }
        public bool MemoryBusRequestEnabled
        {
            get
            {
                return GetBit(BitPosition.BREQEn);
            }
            set
            {
                SetBit(BitValue.BREQEn, value);
            }
        }

        public ClockSourceType ClockSourceTypeSelected
        {
            get
            {
                if (GetBit(BitPosition.MSBB))
                    return ClockSourceType.MPEGClock;
                else
                    return ClockSourceType.GenClock;
            }
            set
            {
                if (value.Equals(ClockSourceType.GenClock))
                    SetBit(BitValue.MSBB, true);
                else
                    SetBit(BitValue.MSBB, false);
            }
        }

        public MPEGDataType MPEGDataTypeSelected
        {
            get
            {
                if (GetBit(BitPosition.MSFMTSEL))
                    return MPEGDataType.I2S;
                else
                    return MPEGDataType.SONY;
            }
            set
            {
                if (value.Equals(MPEGDataType.I2S))
                    SetBit(BitValue.MSFMTSEL, true);
                else
                    SetBit(BitValue.MSFMTSEL, false);
            }
        }
        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.CONTROL, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.CONTROL = (byte)(this.CONTROL | (byte)bit);
            else
                this.CONTROL = (byte)(this.CONTROL & ~(byte)bit);
        }

        public override string ToString()
        {
            return this.CONTROL.ToBinary(8);
        }

        #endregion

    }
    #region Bits

    [Flags]
    public enum BitPosition : byte
    {
        SERRDis = 0,
        CodecEn = 1,
        UARTEn = 3,
        DAC2En = 5,
        DAC1En = 6,
        BREQEn = 7, //memory bus request enable
        MSBB = 14, //clock source for DAC: gen (0) - MPEG(1)
        MSFMTSEL = 15 //MPEG data SONY(0)/I2S(1)
    }

    [Flags]
    public enum BitValue : uint
    {
        SERRDis = BinaryHelper.BitPos.BIT0,
        CodecEn = BinaryHelper.BitPos.BIT1,
        UARTEn = BinaryHelper.BitPos.BIT3,
        DAC2En = BinaryHelper.BitPos.BIT5,
        DAC1En = BinaryHelper.BitPos.BIT6,
        BREQEn = BinaryHelper.BitPos.BIT7,
        MSBB = BinaryHelper.BitPos.BIT14,
        MSFMTSEL = BinaryHelper.BitPos.BIT15
    }

    public enum ClockSourceType : uint
    {
        GenClock = 0,
        MPEGClock = 1
    }

    public enum MPEGDataType : uint
    {
        SONY = 0,
        I2S = 1
    }
    #endregion
}
*/