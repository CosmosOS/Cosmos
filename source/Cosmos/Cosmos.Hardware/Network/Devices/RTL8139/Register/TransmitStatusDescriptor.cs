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
    /// NB! All Write access to this register has to be in double-word (i.e 32-bit) chuncks.
    /// </summary>
    public class TransmitStatusDescriptor
    {
        private PCIDevice pci;
        private UInt32 tsdAddress;
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
            tsdAddress = adr;
        }

        /// <summary>
        /// Used to get the 32 bit value stored in the TransmitStatusDescriptor.
        /// </summary>
        private UInt32 TSD
        {
            get
            {
                return IOSpace.Read32(tsdAddress);
            }
            set
            {
                IOSpace.Write32(tsdAddress, value);
            }
        }

        /// <summary>
        /// Clears the OWN bit in the Transmit Status Descriptor. This starts poring the data from the 
        /// buffer into the FIFO buffer on the PCI card. The data then moves from the FIFO to the network cable.
        /// </summary>
        public void ClearOWNBit()
        {
            UInt32 data = this.TSD;

            Console.WriteLine("OWN bit status in TransmitStatusDescriptor: " + BinaryHelper.CheckBit(this.TSD, 13));
            
            //Turn off single OWN bit
            if (BinaryHelper.CheckBit(data, (ushort)(13))) //if OWN bit is HIGH
            {
                Console.WriteLine("Flipping OWN bit...");
                data = BinaryHelper.FlipBit(data, (ushort)(13));
            }

            this.TSD = data;
            Console.WriteLine("OWN bit after turning off (should be false): " + BinaryHelper.CheckBit(this.TSD, 13));
        }

        /// <summary>
        /// The total size in bytes of the data in the descriptor. Must not be longer then 1792 bytes (0x700h), this 
        /// will set Tx queue invalid.
        /// </summary>
        public int Size 
        {
            get
            {
                UInt16 mask = 8191; // 0001 1111 1111 1111
                return (int)(this.TSD & mask);
            }
            set
            {
                UInt32 data = this.TSD;

                //First AND all 13 SIZE bits to zero. Then OR together with the correct size.
                UInt32 zeromask = 524287; //1111 1111 1111 1111 1110 0000 0000 0000
                data = data & zeromask;
                data = (UInt32)(data | (UInt32)value);

                this.TSD = data;
            }
        }

        public enum BitValue : uint
        {
            SIZE = BinaryHelper.BitPos.BIT0,       //13 bit long. Must not contain value over 0x700h
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
