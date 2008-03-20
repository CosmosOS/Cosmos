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
        /// <summary>
        /// Retrieve all Realtek 8139 network cards found on computer.
        /// </summary>
        /// <returns></returns>
        public static List<RTL8139> FindRTL8139Devices()
        {
            List<RTL8139> found = new List<RTL8139>();

            foreach (PCIDevice device in Cosmos.Hardware.PC.Bus.PCIBus.Devices)
            {
                if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    found.Add(new RTL8139(device));
            }

            return found;
        }

        private PCIDevice pciCard;
        private MemoryAddressSpace mem;
        private Register.MainRegister regs;

        public RTL8139(PCIDevice device)
        {
            pciCard = device;
            mem = device.GetAddressSpace(1) as MemoryAddressSpace;
            regs = new MainRegister(mem);
        }

        #region NetworkDevice members
        public override MACAddress MACAddress
        {
            get 
            { 
                //Polls the PCI device for the MAC address
                /*byte[] bytes = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    uint address = (uint)(pciCard.BaseAddress1 + i);
                    bytes[i] = IOSpace.Read8(address);
                }

                MACAddress mac = new MACAddress(bytes);
                return mac;            
                 * */

                return regs.Mac;
            }
        }

        public string GetHardwareRevision()
        {
            /*uint address = pciCard.BaseAddress1 + (uint)MainRegister.Bit.TxConfig;
            UInt32 tcrdata = IOSpace.Read32(address);
            TransmitConfigurationRegister tcr = new TransmitConfigurationRegister(tcrdata);
            return TransmitConfigurationRegister.GetHardwareRevision(tcr.GetHWVERID());
            */


            uint address = pciCard.BaseAddress1 + (uint)MainRegister.Bit.TxConfig;
            UInt32 tcrdataOld = IOSpace.Read32(address);
            Console.WriteLine("Old TxConfig: " + tcrdataOld);
            UInt32 tcrdata = regs.TxConfig;
            Console.WriteLine("New TxConfig: " + tcrdata);
            TransmitConfigurationRegister tcr = new TransmitConfigurationRegister(tcrdata);
            return TransmitConfigurationRegister.GetHardwareRevision(tcr.GetHWVERID());
        }

        /// <summary>
        /// Changes the Loopback mode. 
        /// </summary>
        /// <param name="value">True to enable Loopback. False for normal operation.</param>
        public void SetLoopbackMode(bool value)
        {
            throw new NotImplementedException();
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
            regs.Config1 = 0x00;            
            return true;

            
        }

        /// <summary>
        /// Performs an internal system hardware reset of the network card.
        /// </summary>
        public void SoftReset()
        {
            Console.WriteLine("Performing software reset of RTL8139");
           
            //Tell RTL chip to issue a Reset`
            regs.ChipCmd = MainRegister.ChipCommandFlags.RST;

            //Wait while RST bit is active
            while (regs.ChipCmdTest(MainRegister.ChipCommandFlags.RST))
            {
                Console.WriteLine("Reset in progress...");
            }

            Console.WriteLine("Reset Complete!");
        }

        /// <summary>
        /// The IRQMaskRegister
        /// </summary>
        private void SetIRQMaskRegister()
        {
            byte mask = (byte)
                (Register.InterruptMaskRegister.Bit.ROK & 
                Register.InterruptMaskRegister.Bit.TOK &
                Register.InterruptMaskRegister.Bit.RER &
                Register.InterruptMaskRegister.Bit.TER
                );
            UInt32 address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.IntrMask;
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
            regs.ChipCmd = MainRegister.ChipCommandFlags.TE;
        }

        /// <summary>
        /// Enable the NIC to be able to Transmit data.
        /// </summary>
        public void EnableTransmit()
        {
            regs.ChipCmd = MainRegister.ChipCommandFlags.TE;
        }

        /// <summary>
        /// A general purpose timer. Writing to this will reset timer.
        /// </summary>
        public UInt32 TimerCount 
        {
            get 
            {
                return IOSpace.Read32(pciCard.BaseAddress1 + (byte)MainRegister.Bit.Timer);
            } 
            set
            {
                UInt32 address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.Timer;
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
        public void SetEarlyTxThreshold(uint bytecount)
        {
            //TODO: This method should be in TransmitStatusDescriptos.cs
            if (bytecount != 0 & (bytecount % 32 > 0))
                throw new ArgumentException("Early TX Threshold must be 0 or dividable by 32");

            //Each of the four Transmit Status Descriptors (TSD) has its own EarlyTxThreshold.

            UInt32 address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.RxEarlyCnt;
            IOSpace.Write8(address, (byte)bytecount);
        }

        /// <summary>
        /// Initialize the Receive Buffer. The RBSTART register consists of 4 bytes (0x30h to 0x33h) which should contain
        /// the address of a buffer to save incoming data to.
        /// </summary>
        public void InitReceiveBuffer()
        {
            //Prepare a buffer area
            byte[] rxbuffer = new byte[2048];

            UInt32 address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.RxBuf;

            //Write the address of the buffer area to the RBSTART 
            WriteAddressToPCI(ref rxbuffer, address);

            Console.WriteLine("RxBuffer contains address: " + IOSpace.Read32(address));
            
        }

        /// <summary>
        /// Takes a byte array, and a memory address. 
        /// The memoryaddress of the begining of the bytearray is written to the memory address.
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="address"></param>
        private unsafe void WriteAddressToPCI(ref byte[] bytearray, uint address)
        {

            /* The data in the bytearray contains the actual bytes we want to transfer to the network.
             * This bytearray must be in a continous memoryarea on the computer.
             * We then write the address of this memoryarea to the network card.
             * The address is stored in the Transmit Start Address which corresponds to the Transmit Status Descriptor we are currently using (0-3).
             */


            fixed (byte* bodystart = &bytearray[0])
            {
                IntPtr bodyAddress = (IntPtr)bodystart;
                IOSpace.Write32(address, (uint)bodyAddress);
                Console.WriteLine("Address where packet body is stored: " + (uint)bodyAddress);
            }
        }

        /// <summary>
        /// Transmits the given Packet
        /// </summary>
        /// <param name="packet"></param>
        public unsafe void Transmit(Packet packet)
        {
            //Tell the PCI card the address of body of the Packet.
            UInt32 address = 
                pciCard.BaseAddress1 + 
                (byte)MainRegister.Bit.TSD0 + //I think this should be TSAD0, but then no packet is sent...
                TransmitStatusDescriptor.GetCurrentTSDescriptor();
            Console.WriteLine("Address of TSAD0: " + address);

            byte[] body = packet.PacketBody;

            WriteAddressToPCI(ref body, address);

            Console.Write("Data in Transmit Status Descriptor " + TransmitStatusDescriptor.GetCurrentTSDescriptor() + ":");
            Console.WriteLine(IOSpace.Read32(address));
            //At this point the TSDA0 should contain the address of the data.
            Console.WriteLine("The Data pointed to: " + IOSpace.Read32(IOSpace.Read32(address)));

            //Set the transmit status - which enables the transmit.
            TransmitStatusDescriptor tsd = TransmitStatusDescriptor.Load(pciCard);
            tsd.Size = body.Length;
            Console.WriteLine("Told NIC to send " + tsd.Size + " bytes.");
            SetEarlyTxThreshold(1024);
            Console.WriteLine("Sending...");
            tsd.ClearOWNBit();
            TransmitStatusDescriptor.IncrementTSDescriptor();
        }
    }
}
