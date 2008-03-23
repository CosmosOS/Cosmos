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
        private UInt32 address = 0;

        public static CommandRegister Load(PCIDevice pci)
        {
            //pcicard = pci;
            UInt32 address = GetCmdAddress(pci);
            return new CommandRegister(pci, address);
        }

        private CommandRegister(PCIDevice pci, UInt32 adr)
        {
            pcicard = pci;
            address = adr;
        }

        public byte GetCmdRegister()
        {
            return IOSpace.Read8(GetCmdAddress());
        }

        public UInt32 GetCmdAddress()
        {
            return pcicard.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd;
        }

        public static UInt32 GetCmdAddress(PCIDevice pci)
        {
            return pci.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd;
        }

        /// <summary>
        /// Bits used to issue commands to the RTL. Used in conjunction with register CHIPCMD (0x37h)
        /// </summary>
        public enum BitPosition : byte
        {
            BUFE = 0x00,    //Buffer Empty, read-only
            TE = 0x02,      //Transmitter Enable
            RE = 0x03,      //Receiver Enable
            RST = 0x04      //Software Reset
        }

        public enum BitValue : byte
        {
            BUFE = 1,
            TE = 4,
            RE = 8,
            RST = 16
        }

        public bool IsResetStatus()
        {
            byte cmd = GetCmdRegister();
            return BinaryHelper.CheckBit(cmd, (byte)BitPosition.RST);
        }

    }
}
