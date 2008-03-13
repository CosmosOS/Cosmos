using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;
using Cosmos.Driver.RTL8139.Register;

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
                    uint address = (uint)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.MAC0 + i);
                    
                    bytes[i] = IOSpace.Read8(address);
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
            //Writes 0x00 to CONFIG_1 registers to enable card
            byte command = 0x00;
            ushort address = (ushort)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.Config1);
            IOSpace.Write8(address, command);
            return true;
        }

        /// <summary>
        /// Performs an internal system hardware reset of the network card.
        /// </summary>
        public void SoftReset()
        {
            byte command = 0x10;
            ushort address = (ushort)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd);
            IOSpace.Write8(address, command);
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
            byte mask = (byte)(Register.InterruptMaskRegister.Bit.ROK & Register.InterruptMaskRegister.Bit.TOK);
            ushort address = (ushort)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.IntrMask);
            IOSpace.Write8(address, mask);
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
            byte command = (byte)Register.CommandRegister.Bit.RE;
            ushort address = (ushort)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd);
            IOSpace.Write8(address, command);
        }

        /// <summary>
        /// Enable the NIC to be able to Transmit data.
        /// </summary>
        public void EnableTransmit()
        {
            byte command = (byte)Register.CommandRegister.Bit.TE;
            ushort address = (ushort)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.ChipCmd);
            IOSpace.Write8(address, command);
        }

        /// <summary>
        /// A general purpose timer. Writing to this will reset timer.
        /// </summary>
        public UInt32 TimerCount 
        {
            get 
            {
                return IOSpace.Read32((uint)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.Timer));
            } 
            set
            {
                uint address = (uint)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.Timer);
                IOSpace.Write32(address, 0x00); //Resets timer
            }
        }

        /// <summary>
        /// The Early TX Threshold specifies the threshold level in Tx FIFO register before transmission begins.
        /// The bytecount should not exceed 2048(2k bytes).
        /// The bytecount also needs to be dividable by 32.
        /// If bytecount 0 is set then NIC will use 8 bytes as threshold
        /// </summary>
        /// <param name="bytecount">Number zero or a number dividable by 32.</param>
        public void SetEarlyTXThreshold(uint bytecount)
        {
            if (bytecount != 0 & (bytecount%32 > 0))
                throw new ArgumentException("Early TX Threshold must be 0 or dividable by 32");

            uint address = (uint)(pciCard.BaseAddress1 + (byte)MainRegister.Bit.RxEarlyCnt); //TODO: Correct register??
            IOSpace.Write8(address, (byte)bytecount);
        }

        /// <summary>
        /// Transmits the given Packet
        /// </summary>
        /// <param name="packet"></param>
        public unsafe void Transmit(Packet packet)
        {
            //Tell the PCI card the address of body of the Packet.
            uint address = 
                (uint)(pciCard.BaseAddress1 + 
                (byte)MainRegister.Bit.TxAddr0) +
                TransmitStatusDescriptor.GetCurrentTSDescriptor();
            byte[] body = packet.PacketBody();

            fixed (byte* bodystart = &body[0])
            {
                IOSpace.Write32(address, (uint)bodystart);
            }

            /*
             * From Victor - an alternative way
             * fixed (byte* umanagedPointer = yourVariable)
             * {
             * IntPtr address = (IntPtr)umanagedPointer;
             * SomeFunction(address);
             * //or any other code
             * }
             */

            //Set the transmit status - which enables the transmit.
            this.SetEarlyTXThreshold(1024);
            this.ClearOWNBit();
            TransmitStatusDescriptor.IncrementTSDescriptor();
        }

        /// <summary>
        /// Clears the OWN bit in the Transmit Status Descriptor. This starts poring the data from the 
        /// buffer into the FIFO buffer on the PCI card. The data then moves from the FIFO to the network cable.
        /// </summary>
        private void ClearOWNBit()
        {
            uint address = (uint)(pciCard.BaseAddress1 + 
                (byte)MainRegister.Bit.TxStatus0 +
                TransmitStatusDescriptor.GetCurrentTSDescriptor() +
                (byte)TransmitStatusDescriptor.Bit.OWN);

            IOSpace.Write8(address, 0x00);
        }






    }
}
