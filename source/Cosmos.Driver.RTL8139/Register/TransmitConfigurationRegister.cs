using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Driver.RTL8139.Misc;

namespace Cosmos.Driver.RTL8139.Register
{
    /// <summary>
    /// The TransmitConfigurationRegister (TCR) defines transmit configuration. It controls functions as
    /// loopback, heartbeat, auto transmit padding, Programmable Interframe Gap, Fill and Drain thresholds
    /// and maximum DMS burst size.
    /// Is 32 bits wide. Offset 0x40h-0x43h from main memory.
    /// </summary>
    public class TransmitConfigurationRegister
    {
        private UInt32 tcr;
        private PCIDevice pci;
        private UInt32 tcrAddress;
        private TransmitConfigurationRegister(UInt32 data, PCIDevice hw, UInt32 adr)
        {
            tcr = data;
            pci = hw;
            tcrAddress = adr;
        }
        
        public static TransmitConfigurationRegister Load(PCIDevice pcicard)
        {
            UInt32 address = pcicard.BaseAddress1 + (byte)MainRegister.Bit.TxConfig;
            UInt32 foundbytes = IOSpace.Read32(address);
            return new TransmitConfigurationRegister(foundbytes, pcicard, address);
        }

        public void Init()
        {
            //Set Interframe Gap and Max Burst Size (to 128 bytes)
            UInt32 data = (UInt32)(BitValue.IFG0 | BitValue.IFG1 | BitValue.MAXDMA0 | BitValue.MAXDMA1);
            //Console.WriteLine("Data in INIT for TX:" + data);
            IOSpace.Write32(tcrAddress, data);
        }

        /// <summary>
        /// Retrieves 6 bits 
        /// </summary>
        /// <returns></returns>
        public byte GetHWVERID()
        {
            byte mask = 249; // 1111 1001
            byte hwverid = Misc.BinaryHelper.GetByteFrom32bit(tcr, (byte)(23));
            return (byte)(mask & hwverid);
        }
        
        //internal void SetLoopBack(bool value)
        //{
        //    //Change bits LBK0 and LBK1 to HIGH for Loopback, or LOW for Normal mode.
        //    if (value)



        //}

        public bool LoopbackMode 
        {
            get 
            {
                UInt32 data = IOSpace.Read32(tcrAddress);
                bool low = BinaryHelper.CheckBit(data, 17);
                bool high = BinaryHelper.CheckBit(data, 18);
                
                if (low != high)
                    Console.WriteLine("Warning: Loopback bits should always be the same!");

                if (low && high)
                    return true;
                else
                    return false;
            }
            set 
            { 
                UInt32 data = IOSpace.Read32(tcrAddress);
                if (value) //turn ON
                    data = (UInt32)(data | (uint)BitValue.LBK0 | (uint)BitValue.LBK1);
                else //turn OFF
                    data = (UInt32)(data & (uint)~BitValue.LBK0 & (uint)~BitValue.LBK1);

                IOSpace.Write32(tcrAddress, data); 
            }
        }

        public enum BitValue : uint
        {
            /// <summary>
            /// Setting to 1 will cause RTL8139 to retransmit packet. Only allowed in transmit abort state.
            /// </summary>
            CLRABT = BinaryHelper.BitPos.BIT0,
            /// <summary>
            /// Tx Retry Count - 4 bits wide. Tx retry count in multiple of 16. Retries = 16 + (TXRR * 16) times.
            /// </summary>
            TXRR = BinaryHelper.BitPos.BIT4,
            /// <summary>
            /// Max DMA Burst Size per Tx DMA Burst. Se documentation for value details.
            /// </summary>
            MAXDMA0 = BinaryHelper.BitPos.BIT8,
            MAXDMA1 = BinaryHelper.BitPos.BIT9,
            MAXDMA2 = BinaryHelper.BitPos.BIT10,
            /// <summary>
            /// Append CRC. 0 = CRC is appended. 1 = CRC not appended.
            /// </summary>
            CRC = BinaryHelper.BitPos.BIT16,
            /// <summary>
            /// Loopback test. 00 is normal. 11 is loopback mode.
            /// </summary>
            LBK0 = BinaryHelper.BitPos.BIT17,
            LBK1 = BinaryHelper.BitPos.BIT18,
            /// <summary>
            /// Revisision. If this bit is 1 then the network card is RTL8139 rev.G. All other revisions have bit set to 0.
            /// </summary>
            REVG = BinaryHelper.BitPos.BIT23,
            /// <summary>
            /// Interframe Gap Time. Adjusts time between frames. 9.6 micro sec for 10Mbps. 0,96 micro sec for 100Mbps.
            /// Only 0xFF is valid according to IEEE 802.3 standard. Two bits wide.
            /// </summary>
            IFG0 = BinaryHelper.BitPos.BIT24,
            IFG1 = BinaryHelper.BitPos.BIT25,
            /// <summary>
            /// Hardware Version ID. 5 bits wide (6 with bit 23). See separate method to convert to string.
            /// </summary>
            HWVERID = BinaryHelper.BitPos.BIT26
        }

        /// <summary>
        /// Get the hardware revision. F.instance RTL8139A or RTL8139C+.
        /// </summary>
        /// <param name="hwverid">Must be the byte from bit 23 to bit 30 in the TCR! Bit 24 and 25 must be 0.</param>
        /// <returns></returns>
        public static string GetHardwareRevision(byte hwverid)
        {
            switch (hwverid)
            {
                case 192: //11000000
                    return "RTL8139";
                case 224: //11100000
                    return "RTL8139A";
                case 225: //11100001
                    return "RTL8139A-G";
                case 232: //11101000
                    return "RTL8139C";
                case 233: //11101001
                    return "RTL8139C+";
                case 240: //11110000
                    return "RTL8139B";
                case 248: //11111000
                    return "RTL8130";
                default:
                    return "Unknown RTL813xxx revision (" + hwverid + ")";
            }

        }


    }
}
