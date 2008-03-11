using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;

namespace Cosmos.Driver.RTL8139
{
    /// <summary>
    /// Driver for networkcards using the RTL8139 chip.
    /// Some documentation can be found at: http://www.osdev.org/wiki/RTL8139
    /// </summary>
    public class RTL8139 : NetworkDevice //, DeviceDriver interface
    {
        public static Device[] FindDevices()  // DeviceDriver interface
        {
            List<Device> found = new List<Device>();

            foreach (PCIDevice device in Cosmos.Hardware.PC.Bus.PCIBus.Devices)
                if (device.VendorID == 0x10EC && device.DeviceID == 0x0139)
                    found.Add(new RTL8139(device));

            return found.ToArray();
        }


        private PCIDevice pciCard;
        public RTL8139(PCIDevice device)
        {
            pciCard = device;
            // etc
        }

        #region NetworkDevice members
        public override MACAddress MACAddress
        {
            get 
            { 
                //Polls the PCI device for the MAC address
                byte[] bytes = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.Config0 + i);
                    bytes[i] = CPUBus.Read8(address);
                }

                MACAddress mac = new MACAddress(bytes);
                return mac;            
            }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override bool RecieveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override int BytesAvailable()
        {
            throw new NotImplementedException();
        }

        public override bool IsSendBufferFull()
        {
            throw new NotImplementedException();
        }

        public override bool IsRecieveBufferFull()
        {
            throw new NotImplementedException();
        }
        #endregion

        public override string Name
        {
            get { return "Generic RTL8139 Network device"; }
        }

        public override bool Enable()
        {
            //Writes 0x00 to CONFIG_1 registers
            byte command = 0x00;
            ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.Config1);
            CPUBus.Write8(address, command);
            return true;
        }

        /// <summary>
        /// Performs an internal system hardware reset of the network card.
        /// </summary>
        public void SoftReset()
        {
            byte command = 0x10;
            ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.ChipCmd);
            CPUBus.Write8(address, command);
            //TODO: Should check the RST bit afterwards. It is high while resetting, and low when reset complete.
        }

        /// <summary>
        /// Initialize the Receive Buffer. The RBSTART register consists of 4 bytes (0x30h to 0x33h) which should contain
        /// the address of a buffer to save incoming data to.
        /// </summary>
        private void InitReceiveBuffer()
        {
            //TODO: Really unsure of the types and math here...
            //char[] rx_buffer = new char[8192+16]; //8k + header
            //pciCard.Write8(RTL8139Register.RxBuf, (byte)rx_buffer);
        }

        /// <summary>
        /// The IRQMaskRegister
        /// </summary>
        private void SetIRQMaskRegister()
        {
            byte mask = (byte)(IRQMask.ROK & IRQMask.TOK);
            ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.IntrMask);
            CPUBus.Write8(address, mask);
        }

        /// <summary>
        /// This register indicates the source of an interrupt when the INTA pin goes active. 
        /// Enabling the corresponding bits in the Interrupt Mask Register (IMR) allows bits in this register to produce an interrupt. 
        /// When an interrupt is active, one of more bits in this register are set to a “1”. 
        /// The interrupt Status Register reflects all current pending interrupts, regardless of the state of the corresponding mask bit in the IMR. 
        /// Reading the ISR clears all interrupts. Writing to the ISR has no effect.
        /// </summary>
        private void GetIRQServiceRegister()
        {
            //Could perhaps be used to raise events?
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enable the NIC to be able to Recieve data.
        /// </summary>
        public void EnableRecieve()
        {
            byte command = (byte)CommandRegister.RE;
            ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.ChipCmd);
            CPUBus.Write8(address, command);
        }

        /// <summary>
        /// Enable the NIC to be able to Transmit data.
        /// </summary>
        public void EnableTransmit()
        {
            byte command = (byte)CommandRegister.TE;
            ushort address = (ushort)(pciCard.BaseAddress0 + (byte)RTL8139Register.ChipCmd);
            CPUBus.Write8(address, command);
        }

        /// <summary>
        /// The RTL8139 contains 64 x 16 bit EEPROM registers.
        /// </summary>
        private enum RTL8139Register : byte
        {
            MAC0 = 0x00,            // Ethernet hardware address
            MAR0 = 0x08,            // Multicast filter
            TxStatus0 = 0x10,       // Transmit status (Four 32bit registers)
            TxAddr0 = 0x20,         // Tx descriptors (also four 32bit)
            RxBuf = 0x30,
            RxEarlyCnt = 0x34,
            RxEarlyStatus = 0x36,
            ChipCmd = 0x37,
            RxBufPtr = 0x38,
            RxBufAddr = 0x3A,
            IntrMask = 0x3C,
            IntrStatus = 0x3E,
            TxConfig = 0x40,
            RxConfig = 0x44,
            Timer = 0x48,           // A general-purpose counter
            RxMissed = 0x4C,        // 24 bits valid, write clears
            Cfg9346 = 0x50,
            Config0 = 0x51,
            Config1 = 0x52,
            FlashReg = 0x54,
            GPPinData = 0x58,
            GPPinDir = 0x59,
            MII_SMI = 0x5A,
            HltClk = 0x5B,
            MultiIntr = 0x5C,
            TxSummary = 0x60,
            MII_BMCR = 0x62,
            MII_BMSR = 0x64,
            NWayAdvert = 0x66,
            NWayLPAR = 0x68,
            NWayExpansion = 0x6A
        }

        /// <summary>
        /// IRQ masks used in conjunction with IMR and ISR.
        /// </summary>
        private enum IRQMask : byte
        {
            ROK = 0x00,     //Receive (Rx) OK
            RER = 0x01,     //Receive (Rx) Error
            TOK = 0x02,     //Transmit (Tx) OK
            TER = 0x03,     //Transmit (Tx) Error
            RXOVW = 0x04,   //Rx Buffer Overflow
            PUNLC = 0x05,   //Packed Underrun/Link Change
            FOVW = 0x06,    //FIFO Overflow
            LENCHG = 0x0D,  //Cable Length Changed
            TIMEOUT = 0x0E, //Raised when TCTR register matches TimeInt register
            SERR = 0x0F     //System Error. Might cause a reset.
        }

        /// <summary>
        /// Bits used to issue commands to the RTL. Used in conjunction with register CHIPCMD (0x37h)
        /// </summary>
        private enum CommandRegister : byte
        {
            BUFE = 0x00,    //Buffer Empty, read-only
            TE = 0x02,      //Transmitter Enable
            RE = 0x03,      //Receiver Enable
            RST = 0x04      //Software Reset
        }
    }
}
