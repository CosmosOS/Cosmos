using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;

namespace Cosmos.Hardware.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// The CommandRegister is used for issuing commands to the RTL8139.
    /// Used for performing Software Reset, or enabling transmitter and receiver.
    /// 1 Byte wide. Only one 4 bits used. (Bit 1, 5, 6, 7 not used)
    /// Offset 0x37h from the base memory.
    /// </summary>
    public class CommandRegister
    {
        //private byte cmd;

        private PCIDevice pcicard = null;
        private UInt32 crAddress = 0;

        public static CommandRegister Load(PCIDevice pci)
        {
            UInt32 address = pci.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd;
            return new CommandRegister(pci, address);
        }

        private CommandRegister(PCIDevice pci, UInt32 adr)
        {
            pcicard = pci;
            crAddress = adr;
        }

        /// <summary>
        /// Get or Sets the 8 bits in the Command Register.
        /// </summary>
        /// <returns></returns>
        public byte CR
        {
            get { return IOSpace.Read8(crAddress); }
            set { IOSpace.Write8(crAddress, value); }
        }

        public bool IsRxBufferEmpty()
        {
            return BinaryHelper.CheckBit(this.CR, (byte)BitPosition.BUFE);
        }

        public bool IsResetStatus()
        {
            return BinaryHelper.CheckBit(this.CR, (byte)BitPosition.RST);
        }

        public bool IsRxEnabled()
        {
            return BinaryHelper.CheckBit(this.CR, (byte)BitPosition.RE);
        }

        public bool IsTxEnabled()
        {
            return BinaryHelper.CheckBit(this.CR, (byte)BitPosition.TE);
        }

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
    }
}
