using System;
using System.Collections.Generic;
using Cosmos.Kernel;
using Cosmos.Kernel.ManagedMemory;

namespace Cosmos.Hardware2.Network.Devices.AMDPCNetII
{
    public class AMDPCNet : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected IOAddressSpace io;
        protected MACAddress mac;
        protected bool mInitDone;

        protected List<ManagedMemorySpace> mRxBuffers;
        protected List<ManagedMemorySpace> mTxBuffers;
        private ManagedMemorySpace mTxDescriptor;
        private ManagedMemorySpace mRxDescriptor;
        private ManagedMemorySpace mInitBlock;
        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        // Initialize a new instance of the AMD PCNet device driver
        public AMDPCNet(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get AMD PCNet card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            // Setup interrupt handling
            //Interrupts.IRQ09 += HandleNetworkInterrupt;
            //Interrupts.AddIRQHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Get IO Address from PCI Bus
            io = (Kernel.IOAddressSpace)pciCard.GetAddressSpace(0);
            // Enable the card
            pciCard.EnableDevice();
            // Set the device into 32-bit mode
            io.Write32(0x10, 0);

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

            // Allocate 32 bytes for the 28 byte Initialization block that has to be aligned to a 4 byte boundary
            //UInt32 address = Heap.MemAlloc(0x20);
            //mInitBlock = new ManagedUInt32Array(7); // 7 UInt32's, aligned on a 4byte boundary
            mInitBlock = new ManagedMemorySpace(28, 4);
            /*Console.Write("Allocated 32 bytes for initialization block @ 0x" + address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + aligned_address.ToHex(8) + ")");*/

            // Allocate 80 uints for the 16 RX and TX Descriptor rings. These addresses have to be aligned on a 16-byte boundary
            mTxDescriptor = new ManagedMemorySpace(256, 16);
            mRxDescriptor = new ManagedMemorySpace(256, 16);
            /*Console.Write("Allocated 320 bytes for RX ring descriptors @ 0x" + rd_address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + mRxDescriptorAddress.ToHex(8) + ")");
            Console.Write("Allocated 320 bytes for TX ring descriptors @ 0x" + tx_address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + mTxDescriptorAddress.ToHex(8) + ")");*/

            // Fill in the Initialization block
            mInitBlock.Write32(0x00, (0x4 << 28) | (0x4 << 20));
            mInitBlock.Write32(0x04, (uint)(eeprom_mac[0] | (eeprom_mac[1] << 8) | (eeprom_mac[2] << 16) | (eeprom_mac[3] << 24)));
            mInitBlock.Write32(0x08, (uint)(eeprom_mac[4] | (eeprom_mac[5] << 8)));
            mInitBlock.Write32(0x0C, 0x0);
            mInitBlock.Write32(0x10, 0x0);
            mInitBlock.Write32(0x14, mRxDescriptor.Offset);
            mInitBlock.Write32(0x18, mTxDescriptor.Offset);

            // Write the Initialization blocks address to the registers on the card
            InitializationBlockAddress = mInitBlock.Offset;
            // Set the device to PCNet-PCI II Controller mode (full 32-bit mode)
            SoftwareStyleRegister = 0x03;

            /* Initialize the RX and TX buffers, and set up the RX and TX descriptors to point
               to the buffers. Also, mark the RX descriptors as being owned by the card so data 
               can be received in them */
            mRxBuffers = new List<ManagedMemorySpace>();
            mTxBuffers = new List<ManagedMemorySpace>();
            for (uint rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = rxd * 16;

                ManagedMemorySpace buffer = new ManagedMemorySpace(2048);
                mRxDescriptor.Write32(xOffset + 8, buffer.Offset);
                UInt16 buffer_len = (UInt16)(~buffer.Size);
                buffer_len++;
                UInt32 flags = (UInt32)(buffer_len & 0x0FFF) | 0xF000 | 0x80000000;
                mRxDescriptor.Write32(xOffset + 4, flags);
                mRxBuffers.Add(buffer);
            }
            for (uint txd = 0; txd < 16; txd++)
            {
                uint xOffset = txd * 16;

                ManagedMemorySpace buffer = new ManagedMemorySpace(2048);
                mTxDescriptor.Write32(xOffset + 8, buffer.Offset);
                mTxBuffers.Add(buffer);
            }

            // Set TX Descriptor 0 as the first one to use... Increment this when we use one to use them in a circular fashion
            mNextTXDesc = 0;

            // Setup our Receive and Transmit Queues
            mTransmitBuffer = new Queue<byte[]>();
            mRecvBuffer = new Queue<byte[]>();
        }

        public static void InitDriver()
        {
            Device.AddDriverInit(FindAll);
        }

        /// <summary>
        /// Retrieve all AMD PCNet network cards found on computer.
        /// </summary>
        /// <returns>List of all AMD PCNet cards</returns>
        public static void FindAll()
        {
            Console.WriteLine("Scanning for AMD PCNet cards...");
            foreach (PCIDevice device in Cosmos.Hardware2.PCIBus.Devices)
            {
                if ((device.VendorID == 0x1022) && (device.DeviceID == 0x2000) && (device.Claimed == false))
                {
                    AMDPCNet nic = new AMDPCNet(device);

                    Console.WriteLine("Found AMD PCNet NIC on PCI " + device.Bus + ":" + device.Slot + ":" + device.Function);
                    Console.WriteLine("NIC IRQ: " + device.InterruptLine);
                    Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());

                    NetworkDevice.Add(nic);
                }
            }
        }

        //protected void HandleNetworkInterrupt(ref IRQContext aContext)
        //{
        //    UInt32 cur_status = StatusRegister;

        //    if ((cur_status & 0x100) != 0)
        //    {
        //        mInitDone = true;
        //    }
        //    if ((cur_status & 0x200) != 0)
        //    {
        //        if (mTransmitBuffer.Count > 0)
        //        {
        //            byte[] data = mTransmitBuffer.Peek();
        //            if (SendBytes(ref data) == true)
        //            {
        //                mTransmitBuffer.Dequeue();
        //            }
        //        }
        //    }
        //    if ((cur_status & 0x400) != 0)
        //    {
        //        ReadRawData();
        //    }

        //    StatusRegister = cur_status;
        //}

        public bool InitDone
        {
            get 
            {
                return mInitDone; 
            }
        }
        public bool Started
        {
            get
            {
                if ((StatusRegister & 0x02) != 0)
                {
                    return true;
                }

                return false;
            }
        }

        #region Register Access Properties
        protected UInt32 StatusRegister
        {
            get
            {
                io.Write32(0x14, 0);
                return io.Read32(0x10);
            }
            set
            {
                io.Write32(0x14, 0);
                io.Write32(0x10, value);
            }
        }
        protected UInt32 ModeRegister
        {
            get
            {
                io.Write32(0x14, 15);
                return io.Read32(0x10);
            }
            set
            {
                io.Write32(0x14, 15);
                io.Write32(0x10, value);
            }
        }
        protected UInt32 BurstBusControlRegister
        {
            get
            {
                io.Write32(0x14, 18);

                return io.Read32(0x1C);
            }
            set
            {
                io.Write32(0x14, 18);
                io.Write32(0x1C, value);
            }
        }
        protected UInt32 SoftwareStyleRegister
        {
            get
            {
                io.Write32(0x14, 20);

                return io.Read32(0x1C);
            }
            set
            {
                io.Write32(0x14, 20);
                io.Write32(0x1C, value);
            }
        }
        protected UInt32 InitializationBlockAddress
        {
            get
            {
                UInt32 result;

                io.Write32(0x14, 1);
                result = io.Read32(0x10);
                io.Write32(0x14, 2);
                result |= (io.Read32(0x10) << 16);

                return result;
            }
            set
            {
                io.Write32(0x14, 1);
                io.Write32(0x10, (value & 0xFFFF));
                io.Write32(0x14, 2);
                io.Write32(0x10, (value >> 16));
            }
        }
        #endregion

        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return mac; }
        }

        public override bool Enable()
        {
            StatusRegister = 0x43;

            return base.Enable();
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

        public override bool Ready
        {
            get { return this.Started; }
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
            get { return "Generic AMD PCNet Network device"; }
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
            UInt32 status = mTxDescriptor.Read32(xOffset + 4);
            if ((status & 0x80000000) == 0)
            {
                for (uint b = 0; b < aData.Length; b++)
                {
                    mTxBuffers[txd][b] = aData[b];
                }
                UInt16 buffer_len = (UInt16)(aData.Length < 64 ? 64 : aData.Length);
                buffer_len = (UInt16)(~buffer_len);
                buffer_len++;

                UInt32 flags = (UInt32)(buffer_len & 0x0FFF) | 0x0300F000 | 0x80000000;

                mTxDescriptor.Write32(xOffset + 4, flags);
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
                status = mRxDescriptor.Read32(xOffset + 4);
                if ((status & 0x80000000) == 0)
                {
                    recv_size = (UInt16)(mRxDescriptor[xOffset + 0] & 0xFFF);
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

                    mRxDescriptor.Write32(xOffset + 4, status | 0x80000000);
                }
            }
        }

        #endregion
    }
}
