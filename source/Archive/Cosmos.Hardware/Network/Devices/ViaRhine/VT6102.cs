using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware2.Network.Devices.ViaRhine
{
    public class VT6102 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected IOAddressSpace io;
        protected MACAddress mac;

        protected List<ManagedMemorySpace> mRxBuffers;
        private ManagedMemorySpace mRxDescriptors;
        private ManagedMemorySpace mTxDescriptors;
        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        public VT6102(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get VIA Rhine-II card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            // Setup interrupt handling
            //Interrupts.IRQ10 += HandleNetworkInterrupt;
            //Interrupts.AddIRQHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Get IO Address from PCI Bus
            io = pciCard.GetAddressSpace(0) as Kernel.IOAddressSpace;

            // Enable the card
            pciCard.EnableDevice();

            // Get the EEPROM MAC Address and set it as the devices MAC
            byte[] eeprom_mac = new byte[6];
            UInt32 result = io.Read32(0x00);
            eeprom_mac[0] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[1] = BinaryHelper.GetByteFrom32bit(result, 8);
            eeprom_mac[2] = BinaryHelper.GetByteFrom32bit(result, 16);
            eeprom_mac[3] = BinaryHelper.GetByteFrom32bit(result, 24);
            result = io.Read32(0x04);
            eeprom_mac[4] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[5] = BinaryHelper.GetByteFrom32bit(result, 8);

            mac = new MACAddress(eeprom_mac);

            // Software Reset device
            SoftwareReset();

            // Configure Receive Config
            ReceiveConfigRegister = 0x1C;
            // Configure Transmit Config
            TransmitConfigRegister = 0x04;

            // Setup RX Descriptors
            mRxDescriptors = new ManagedMemorySpace(256, 16);

            // Setup TX Descriptors
            mTxDescriptors = new ManagedMemorySpace(256, 16);

            /* Initialize the RX and TX buffers, and set up the RX  descriptors to point
               to the buffers. Also, mark the RX descriptors as being owned by the card so data 
               can be received in them */
            mRxBuffers = new List<ManagedMemorySpace>();
            for (uint rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = rxd * 16;

                ManagedMemorySpace buffer = new ManagedMemorySpace(2048);
                mRxDescriptors.Write32(xOffset + 12, mRxDescriptors.Offset + xOffset + 16);
                mRxDescriptors.Write32(xOffset + 8, buffer.Offset);
                mRxDescriptors.Write32(xOffset + 4, buffer.Size);
                mRxDescriptors.Write32(xOffset, 0x80000000);
                mRxBuffers.Add(buffer);
            }
            mRxDescriptors.Write32(252, mRxDescriptors.Offset);
            for (uint txd = 0; txd < 16; txd++)
            {
                uint xOffset = txd * 16;

                mTxDescriptors.Write32(xOffset + 12, mTxDescriptors.Offset + xOffset + 16);
                mTxDescriptors.Write32(xOffset + 8, 0);
                mTxDescriptors.Write32(xOffset + 4, 0);
                mTxDescriptors.Write32(xOffset, 0);
            }
            mTxDescriptors.Write32(252, mTxDescriptors.Offset);

            mNextTXDesc = 0;

            RxDescAddressRegister = mRxDescriptors.Offset;
            TxDescAddressRegister = mTxDescriptors.Offset;

            // Setup and clear interrupts
            IntMaskRegister = 0xFFFF;
            IntStatusRegister = 0xFFFF;

            // Setup our Receive and Transmit Queues
            mTransmitBuffer = new Queue<byte[]>();
            mRecvBuffer = new Queue<byte[]>();
        }

        private void SoftwareReset()
        {
            io.Write16(0x08, 0x8000);
            while ((io.Read16(0x08) & 0x8000) != 0)
            {
                // Wait for device to reset
            }
        }

        public static void InitDriver()
        {
            Device.AddDriverInit(FindAll);
        }

        /// <summary>
        /// Retrieve all VIA Rhine-II network cards found on computer.
        /// </summary>
        /// <returns>List of all VIA Rhine-II cards</returns>
        public static void FindAll()
        {
            Console.WriteLine("Scanning for VIA Rhine-II cards...");
            foreach (PCIDevice device in Cosmos.Hardware2.PCIBus.Devices)
            {
                if ((device.VendorID == 0x1106) && (device.DeviceID == 0x3065) && (device.Claimed == false))
                {
                    VT6102 nic = new VT6102(device);

                    Console.WriteLine("Found VIA Rhine-II NIC on PCI " + device.Bus + ":" + device.Slot + ":" + device.Function);
                    Console.WriteLine("NIC IRQ: " + device.InterruptLine);
                    Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());

                    NetworkDevice.Add(nic);
                }
            }
        }

        //protected void HandleNetworkInterrupt(ref IRQContext aContext)
        //{
        //    UInt16 cur_status = IntStatusRegister;

        //    if ((cur_status & 0x01) != 0)
        //    {
        //        ReadRawData();
        //    }

        //    IntStatusRegister = cur_status;
        //}

        #region Register Access
        protected UInt16 CommandRegister
        {
            get { return io.Read16(0x08); }
            set { io.Write16(0x08, value); }
        }
        protected UInt16 IntStatusRegister
        {
            get { return io.Read16(0x0C); }
            set { io.Write16(0x0C, value); }
        }
        protected UInt16 IntMaskRegister
        {
            get { return io.Read16(0x0E); }
            set { io.Write16(0x0E, value); }
        }
        protected byte ReceiveConfigRegister
        {
            get { return io.Read8(0x06); }
            set { io.Write8(0x06, value); }
        }
        protected byte TransmitConfigRegister
        {
            get { return io.Read8(0x07); }
            set { io.Write8(0x07, value); }
        }
        protected UInt32 RxDescAddressRegister
        {
            get { return io.Read32(0x18); }
            set { io.Write32(0x18, value); }
        }
        protected UInt32 TxDescAddressRegister
        {
            get { return io.Read32(0x1C); }
            set { io.Write32(0x1C, value); }
        }
        #endregion

        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return this.mac; }
        }

        public override bool Enable()
        {
            // Set start,rxon and txon bit on card
            CommandRegister = 0x1A;

            return base.Enable();
        }
        public override bool Ready
        {
            get { return ((CommandRegister & 0x02) != 0); }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            for (int b = 0; b < length; b++)
            {
                data[b] = buffer[b + offset];
            }

            if (SendBytes(ref data) == false)
            {
                mTransmitBuffer.Enqueue(data);
            }

            return true;
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceivePacket()
        {
            if (mRecvBuffer.Count < 1)
            {
                return null;
            }

            byte[] data = mRecvBuffer.Dequeue();
            return data;
        }

        public override int BytesAvailable()
        {
            if (mRecvBuffer.Count < 1)
            {
                return 0;
            }

            return mRecvBuffer.Peek().Length;
        }

        public override bool IsSendBufferFull()
        {
            return false;
        }

        public override bool IsReceiveBufferFull()
        {
            return false;
        }

        public override string Name
        {
            get { return "VIA Rhine-II Ethernet Adapter"; }
        }
        #endregion

        #region Helper Functions
        protected bool SendBytes(ref byte[] aData)
        {
            int txd = mNextTXDesc++;
            if (mNextTXDesc >= 16)
            {
                mNextTXDesc = 0;
            }

            uint xOffset = (uint)(txd * 16);
            UInt32 status = mTxDescriptors.Read32(xOffset);
            ManagedMemorySpace txBuffer = new ManagedMemorySpace(aData);
            if (((txBuffer.Offset % 4) != 0) || (txBuffer.Size < 64) )
            {
                txBuffer = new ManagedMemorySpace((uint)(aData.Length < 64 ? 64 : aData.Length), 4);
                for (uint b = 0; b < aData.Length; b++)
                {
                    txBuffer[b] = aData[b];
                }
            }
            if ((status & 0x80000000) == 0)
            {
                mTxDescriptors.Write32(xOffset + 8, txBuffer.Offset);
                mTxDescriptors.Write32(xOffset + 4, txBuffer.Size | 0x600000 );

                mTxDescriptors.Write32(xOffset, status | 0x80000000);
                return true;
            }

            return false;
        }

        private void ReadRawData()
        {
            uint status;
            UInt16 recv_size;
            byte[] recv_data;

            for (int rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = (uint)(rxd * 16);
                status = mRxDescriptors.Read32(xOffset);
                if ((status & 0x80000000) == 0)
                {
                    recv_size = (UInt16)((status & 0xFFFF0000) >> 16);
                    recv_data = new byte[recv_size];
                    for (uint b = 0; b < recv_size; b++)
                    {
                        recv_data[b] = mRxBuffers[rxd][b];
                    }

                    if (DataReceived != null)
                    {
                        DataReceived(recv_data);
                    }
                    else
                    {
                        mRecvBuffer.Enqueue(recv_data);
                    }

                    mRxDescriptors.Write32(xOffset, status | 0x80000000);
                }
            }
        }
        #endregion
    }
}
