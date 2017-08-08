using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    public class MediaStatusRegister
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static MediaStatusRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new MediaStatusRegister(aMem);
        }

        private MediaStatusRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        /// <summary>
        /// Get or Sets the 8 bits in the Media Status Register.
        /// </summary>
        public byte MSR
        {
            get
            {
                return xMem.Read8((UInt32)Register.MainRegister.Bit.MSR);
            }
            set
            {
                xMem.Write8((UInt32)Register.MainRegister.Bit.MSR, value);
            }
        }

        #endregion

        #region Register data


        /// <summary>
        /// Returns inverse of Link Status in Basic Mode Status Register (false means Link OK, true means Link Fail).
        /// </summary>
        public bool LinkStatusInverse
        {
            get { return GetBit(BitPosition.LINKB); }
            private set { ; }
        }

        /// <summary>
        /// True = 10Mb, False = 100Mb
        /// </summary>
        public bool Speed10MB
        {
            get { return GetBit(BitPosition.SPEED10); }
            private set { ; }
        }

        public bool AuxPowerPresent
        {
            get { return GetBit(BitPosition.AUXSTATUS); }
            private set { ; }
        }

        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.MSR, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.MSR = (byte)(this.MSR | (byte)bit);
            else
                this.MSR = (byte)(this.MSR & ~(byte)bit);
        }

        public override string ToString()
        {
            return this.MSR.ToBinary(8);
        }

        #endregion

        #region Bits

        [Flags]
        public enum BitPosition : byte
        {
            RXPF = 0,
            TXPF = 1,
            LINKB = 2,
            SPEED10 = 3,
            AUXSTATUS = 4,
            RXFCE = 6,
            TXFCE = 7
        }

        [Flags]
        public enum BitValue : uint
        {
            RXPF = BinaryHelper.BitPos.BIT0,
            TXPF = BinaryHelper.BitPos.BIT1,
            LINKB = BinaryHelper.BitPos.BIT2,
            SPEED10 = BinaryHelper.BitPos.BIT3,
            AUXSTATUS = BinaryHelper.BitPos.BIT4,
            RXFCE = BinaryHelper.BitPos.BIT6,
            TXFCE = BinaryHelper.BitPos.BIT7
        }
        #endregion

    }
}
