using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.RTL8139.Register
{
    /// <summary>
    /// Transmit Status Register is used to describe how the process of transmitting data is going/gone.
    /// The RTL8139 contains four of these descriptors. 
    /// Located at 0x10h, 0x14h, 0x18h and 0x1Ch, each is 4 bytes wide.
    /// NB! All Write access to this register has to be in double-word (i.e 32-bit) chuncks.
    /// </summary>
    public class TransmitStatusDescriptor
    {
        #region Constructor

        private Kernel.MemoryAddressSpace xMem;

        public static TransmitStatusDescriptor Load(Kernel.MemoryAddressSpace aMem)
        {
            return new TransmitStatusDescriptor(aMem);
        }

        private TransmitStatusDescriptor(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }

        /// <summary>
        /// Used to get the 32 bit value stored in the TransmitStatusDescriptor.
        /// </summary>
        private UInt32 TSD
        {
            get
            {
                UInt32 address;
                switch (GetCurrentTSDescriptor())
                {
                    case 0:
                        address = (UInt32)Register.MainRegister.Bit.TSD0;
                        break;
                    case 1:
                        address = (UInt32)Register.MainRegister.Bit.TSD1;
                        break;
                    case 2:
                        address = (UInt32)Register.MainRegister.Bit.TSD2;
                        break;
                    case 3:
                        address = (UInt32)Register.MainRegister.Bit.TSD3;
                        break;
                    default:
                        throw new Exception("Problem with Transmit Status Descriptor");

                }

                return xMem.Read32(address);
            }
            set
            {
                UInt32 address;
                switch (GetCurrentTSDescriptor())
                {
                    case 0:
                        address = (UInt32)Register.MainRegister.Bit.TSD0;
                        break;
                    case 1:
                        address = (UInt32)Register.MainRegister.Bit.TSD1;
                        break;
                    case 2:
                        address = (UInt32)Register.MainRegister.Bit.TSD2;
                        break;
                    case 3:
                        address = (UInt32)Register.MainRegister.Bit.TSD3;
                        break;
                    default:
                        throw new Exception("Problem with Transmit Status Descriptor");
                }

                xMem.Write32(address, value);
            }
        }

        #endregion

        #region Register data

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
                if (value > 1792)
                    throw new ArgumentOutOfRangeException("Tried to set Size to " + value.ToString() + ". The Size in the TransmitStatusDescriptor in RTL8139 can not be over 1792 bytes.");

                UInt32 data = this.TSD;

                //First AND all 13 SIZE bits to zero. Then OR together with the correct size.
                UInt32 zeromask = 524287; //1111 1111 1111 1111 1110 0000 0000 0000
                data = data & zeromask;
                data = (UInt32)(data | (UInt32)value);

                this.TSD = data;
            }
        }

        /// <summary>
        /// Clearing (set to false) the OWN bit in the Transmit Status Descriptor will start poring the data from the 
        /// buffer into the FIFO buffer on the PCI card. The data then moves from the FIFO to the network cable.
        /// </summary>
        public bool OWN
        {
            get { return GetBit(BitPosition.OWN); }
            set { SetBit(BitValue.OWN, value); }
        }

        #endregion

        #region Accessors

        private bool GetBit(BitPosition bit)
        {
            return BinaryHelper.CheckBit(this.TSD, (byte)bit);
        }

        private void SetBit(BitValue bit, bool value)
        {
            if (value)
                this.TSD = (byte)(this.TSD | (byte)bit);
            else
                this.TSD = (byte)(this.TSD & ~(byte)bit);
        }

        public override string ToString()
        {
            return this.TSD.ToBinary(32);
        }

        #endregion

        #region Bits

        public enum BitPosition
        {
            SIZE = 0,
            OWN = 13,
            TUN = 14,
            TOK = 15,
            ERTXTH0 = 16,
            ERTXTH1 = 17,
            ERTXTH2 = 18,
            ERTXTH3 = 19,
            ERTXTH4 = 20,
            ERTXTH5 = 21,
            NCC0 = 24,
            NCC1 = 25,
            NCC2 = 26,
            NCC3 = 27,
            CDH = 28,
            OWC = 29,
            TABT = 30,
            CRS = 31     
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

        #endregion

        #region Transmit Descriptors

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

        #endregion



    }
}
