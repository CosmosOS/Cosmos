using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// The CommandRegister is used for issuing commands to the RTL8139.
    /// Used for performing Software Reset, or enabling transmitter and receiver.
    /// 1 Byte wide. Only one 4 bits used. (Bit 1, 5, 6, 7 not used)
    /// Offset 0x37h from the base memory.
    /// </summary>
    public class CommandRegister
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;
        public static CommandRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new CommandRegister(aMem);
        }

        private CommandRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        /// <summary>
        /// Get or Sets the 8 bits in the Command Register.
        /// </summary>
        /// <returns></returns>
        public byte CR
        {
            get 
            {
                return xMem.Read8((UInt32)Register.MainRegister.Bit.ChipCmd);
            }
            set 
            {
                xMem.Write8((UInt32)Register.MainRegister.Bit.ChipCmd, value);
            }
        }

        #endregion

        #region Register data

        public bool Reset
        {
            get { return GetBit(BitPosition.RST); }
            set { SetBit(BitValue.RST, value); }
        }

        public bool RxEnabled
        {
            get { return GetBit(BitPosition.RE); }
            set { SetBit(BitValue.RE, value); }
        }

        public bool TxEnabled
        {
            get { return GetBit(BitPosition.TE); }
            set { SetBit(BitValue.TE, value); }
        }

        public bool RxBufferEmpty 
        {
            get { return GetBit(BitPosition.BUFE); }
            private set { ; }
        }
        // add bit 7, WRAP, for circular buffer


        #endregion

        public override string ToString()
        {
            return this.CR.ToBinary(8);
        }

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.CR, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.CR = (byte)(this.CR | (byte)bit);
            else
                this.CR = (byte)(this.CR & ~(byte)bit);
        }

        #endregion

        #region Bits

        /// <summary>
        /// Bits used to issue commands to the RTL. Used in conjunction with register CHIPCMD (0x37h)
        /// </summary>
        public enum BitPosition : byte
        {
            BUFE = 0,    //Buffer Empty, read-only
            TE = 2,      //Transmitter Enable
            RE = 3,      //Receiver Enable
            RST = 4      //Software Reset
        }

        public enum BitValue : uint
        {
            BUFE = BinaryHelper.BitPos.BIT0,
            TE = BinaryHelper.BitPos.BIT2,
            RE = BinaryHelper.BitPos.BIT3,
            RST = BinaryHelper.BitPos.BIT4
        }

        #endregion
    }
}
