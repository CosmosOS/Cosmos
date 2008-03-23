using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;

namespace Cosmos.Hardware.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// Transmit Status Register is used to describe how the process of transmitting data is going/gone.
    /// The RTL8139 contains four of these descriptors. 
    /// Located at 0x10h, 0x14h, 0x18h and 0x1Ch, each is 4 bytes wide.
    /// </summary>
    public class TransmitStatusDescriptor
    {
        private PCIDevice pci;
        private UInt32 tdsAddress;
        public static TransmitStatusDescriptor Load(PCIDevice pciCard)
        {
            //Retrieve the 32 bits from the PCI card
            //and create a TSD object
            UInt32 address = 0;
            switch (GetCurrentTSDescriptor())
            {
                case 0:
                    address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.TSD0;
                    break;
                case 1:
                    address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.TSD1;
                    break;
                case 2:
                    address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.TSD2;
                    break;
                case 3:
                    address = pciCard.BaseAddress1 + (byte)MainRegister.Bit.TSD3;
                    break;
                default:
                    Console.WriteLine("Illegal TSDescriptor in RTL driver!");
                    break;
            }

            return new TransmitStatusDescriptor(pciCard, address);
        }

        private TransmitStatusDescriptor(PCIDevice hw, UInt32 adr)
        {
            pci = hw;
            tdsAddress = adr;
        }

        /// <summary>
        /// Used to get the 32 bit value stored in the TransmitStatusDescriptor.
        /// </summary>
        private UInt32 TSD
        {
            get
            {
                return IOSpace.Read32(tdsAddress);
            }
        }

        /// <summary>
        /// Clears the OWN bit in the Transmit Status Descriptor. This starts poring the data from the 
        /// buffer into the FIFO buffer on the PCI card. The data then moves from the FIFO to the network cable.
        /// </summary>
        public void ClearOWNBit()
        {
            //Read byte from register
            byte offset = 8;
            byte data = BinaryHelper.GetByteFrom32bit(this.TSD, offset);


            Console.WriteLine("OWN data before: " + BinaryHelper.CheckBit(this.TSD, 13));
            
            //Turn off single OWN bit
            //data &= (byte)~(1 << (byte)(BitValue.OWN - offset));
            data &= (byte)~(1 << (byte)(13 - offset));

            //TODO, change to this instead...
//            if (BinaryHelper.CheckBit(data, 13 - offset)) //OWN bit is HIGH
 //               BinaryHelper.FlipBit(data, 13 - offset);

            //Write all 8 bits back
            IOSpace.Write8(tdsAddress + offset, data);

            Console.WriteLine("OWN data after: " + BinaryHelper.CheckBit(this.TSD, 13));
        }

        /// <summary>
        /// The total size in bytes of the data in the descriptor. Must not be longer then 1792 bytes (0x700h), this 
        /// will set Tx queue invalid.
        /// </summary>
        public int Size 
        {
            get
            {
                byte offset = 0;
                //return (int)IOSpace.Read8(this.TSD + offset);
                //return (int)IOSpace.Read8(
                Console.WriteLine("TSD is: " + this.TSD);
                Console.WriteLine("First 8 bits: " + (byte)BinaryHelper.GetByteFrom32bit(this.TSD, offset));
                return (int)BinaryHelper.GetByteFrom32bit(this.TSD, offset);
            }
            set
            {
                //TODO: Check this - the register contains 12 bits. We only write 8 bits here.
                byte offset = 0;
                IOSpace.Write16(tdsAddress + offset, (UInt16)value);
                //Console.WriteLine("Wrote value " + (UInt16)value + " to TDSAddress: " + tdsAddress);
                //Console.WriteLine("Read again: " + IOSpace.Read8(tdsAddress + offset));
            }
        }

        public enum BitValue : uint
        {
            SIZE = BinaryHelper.BitPos.BIT0,       //12 bit long. Must not contain value over 0x700h
            OWN = BinaryHelper.BitPos.BIT13,       //Set to 1 when transmit complete. Defaults to 1.
            TUN = BinaryHelper.BitPos.BIT14,       //Transmit FIFO Underrun. Is set to 1 if TxFIFO was exhausted during transmition.
            TOK = BinaryHelper.BitPos.BIT15,       //Transmit OK.
            ERTXTH0 = BinaryHelper.BitPos.BIT16,   //Early TX Threshold 0-5
            ERTXTH1 = BinaryHelper.BitPos.BIT17,
            ERTXTH2 = BinaryHelper.BitPos.BIT18,
            ERTXTH3 = BinaryHelper.BitPos.BIT19,
            ERTXTH4 = BinaryHelper.BitPos.BIT20,
            ERTXTH5 = BinaryHelper.BitPos.BIT21,
            NCC0 = BinaryHelper.BitPos.BIT24,      //Number of Collision Count 0-3
            NCC1 = BinaryHelper.BitPos.BIT25,
            NCC2 = BinaryHelper.BitPos.BIT26,
            NCC3 = BinaryHelper.BitPos.BIT27,
            CDH = BinaryHelper.BitPos.BIT28,       //CD Heart Beat. Cleared in 100Mbps mode.
            OWC = BinaryHelper.BitPos.BIT29,       //Out of Window Collision
            TABT = BinaryHelper.BitPos.BIT30,      //Transmition aborted
            CRS = BinaryHelper.BitPos.BIT31        //Carrier Sense Lost
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
