using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    class ConfigurationRegister1
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static ConfigurationRegister1 Load(Kernel.MemoryAddressSpace aMem)
        {
            return new ConfigurationRegister1(aMem);
        }

        private ConfigurationRegister1(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        /// <summary>
        /// Get or Sets the 8 bits in the Config_1 Register.
        /// </summary>
        public byte CONFIG1
        {
            get
            {
                return xMem.Read8((UInt32)Register.MainRegister.Bit.Config1);
            }
            set
            {
                xMem.Write8((UInt32)Register.MainRegister.Bit.Config1, value);
            }
        }

        #endregion

        #region Register Data

        public bool PowerEnabled
        {
            get { return GetBit(BitPosition.PMEN); }
            set { SetBit(BitValue.PMEN, value); }
        }

        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.CONFIG1, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.CONFIG1 = (byte)(this.CONFIG1 | (byte)bit);
            else
                this.CONFIG1 = (byte)(this.CONFIG1 & ~(byte)bit);
        }

        public override string ToString()
        {
            return this.CONFIG1.ToBinary(8);
        }

        #endregion

        #region Bits

        [Flags]
        public enum BitPosition : byte
        {
            PMEN = 0,   //Power Management Enable
            VPD = 1,    //Enable Vital Product Data
            IOMAP = 2,  //I/O Mapping
            MEMMAP = 3, //Memory mapping
            LWACT = 4,  //LWake Active Mode
            DVRLOAD = 5,//Driver Load
            LEDS0 = 6,
            LEDS1 = 7
        }

        [Flags]
        public enum BitValue : uint
        {
            PMEN = BinaryHelper.BitPos.BIT0,
            VPD = BinaryHelper.BitPos.BIT1,
            IOMAP = BinaryHelper.BitPos.BIT2,
            MEMMAP = BinaryHelper.BitPos.BIT3,
            LWACT = BinaryHelper.BitPos.BIT4,
            DVRLOAD = BinaryHelper.BitPos.BIT5,
            LEDS0 = BinaryHelper.BitPos.BIT6,
            LEDS1 = BinaryHelper.BitPos.BIT7
        }

        #endregion
    }
}
