using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Driver.RTL8139.Register;
using Cosmos.Driver.RTL8139.Misc;


namespace Cosmos.Driver.RTL8139.Register
{
    /// <summary>
    /// Transmit Status Register is used to describe how the process of transmitting data is going/gone.
    /// The RTL8139 contains four of these descriptors. 
    /// Located at 0x10h, 0x14h, 0x18h and 0x1Ch, each is 4 bytes wide.
    /// </summary>
    public class TransmitStatusDescriptor
    {
        private UInt32 tds;
        private PCIDevice pci;
        private UInt32 tdsAddress;
        public static TransmitStatusDescriptor Load(PCIDevice pciCard)
        {
            //Retrieve the 32 bits from the PCI card
            //and create a TSD object
            UInt32 address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.TSD0 + GetCurrentTSDescriptor();
            UInt32 foundbytes = IOSpace.Read32(address);
            return new TransmitStatusDescriptor(foundbytes, pciCard, address);
        }

        private TransmitStatusDescriptor(UInt32 data, PCIDevice hw, UInt32 adr)
        {
            tds = data;
            pci = hw;
            tdsAddress = adr;
        }

        /// <summary>
        /// Clears the OWN bit in the Transmit Status Descriptor. This starts poring the data from the 
        /// buffer into the FIFO buffer on the PCI card. The data then moves from the FIFO to the network cable.
        /// </summary>
        public void ClearOWNBit()
        {
            //Read byte from register
            byte offset = 8;
            byte data = BinaryHelper.GetByteFrom32bit(tds, offset);
            
            //Turn off single OWN bit
            data &= (byte)~(1 << (byte)(Bit.OWN - offset));

            //Write all 8 bits back
            IOSpace.Write8(tdsAddress + offset, data);
        }

        public UInt32 TSD()
        {
            return IOSpace.Read32(tdsAddress);
        }

        public static bool CheckBit(UInt32 data, Bit bit)
        {
            ushort mask = (ushort)(1 << (ushort)bit);
            return (data & mask) != 0;
        }

        public enum Bit
        {
            SIZE = 0,       //12 bit long. Must not contain value over 0x700h
            OWN = 13,       //Set to 1 when transmit complete. Defaults to 1.
            TUN = 14,       //Transmit FIFO Underrun. Is set to 1 if TxFIFO was exhausted during transmition.
            TOK = 15,       //Transmit OK.
            ERTXTH0 = 16,   //Early TX Threshold 0-5
            ERTXTH1 = 17,   
            ERTXTH2 = 18,   
            ERTXTH3 = 19,   
            ERTXTH4 = 20,   
            ERTXTH5 = 21,   
            NCC0 = 24,      //Number of Collision Count 0-3
            NCC1 = 25,      
            NCC2 = 26,
            NCC3 = 27,
            CDH = 28,       //CD Heart Beat. Cleared in 100Mbps mode.
            OWC = 29,       //Out of Window Collision
            TABT = 30,      //Transmition aborted
            CRS = 31        //Carrier Sense Lost
        }

        private static byte currentTSDescriptor = 0;
        /// <summary>
        /// Increments to the next Transmit Status Descriptor to use.
        /// There are four TSD's which are used in round-robin.
        /// </summary>
        /// <returns></returns>
        public static void IncrementTSDescriptor()
        {
            const byte NumberOfDescriptors = 4;
            if (currentTSDescriptor == (NumberOfDescriptors - 1))
                currentTSDescriptor = 0;
            else
                currentTSDescriptor++;
        }

        public static byte GetCurrentTSDescriptor()
        {
            return currentTSDescriptor;
        }


    }
}
