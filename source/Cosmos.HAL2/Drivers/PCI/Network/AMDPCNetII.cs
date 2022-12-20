using System;
using System.Collections.Generic;
using Cosmos.Common;
using Cosmos.Core;
using Cosmos.HAL.Network;

namespace Cosmos.HAL.Drivers.PCI.Network
{
    public class AMDPCNetII : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected MACAddress mac;
        protected bool mInitDone;

        protected ManagedMemoryBlock mInitBlock;
        protected ManagedMemoryBlock mRxDescriptor;
        protected ManagedMemoryBlock mTxDescriptor;
        protected List<ManagedMemoryBlock> mRxBuffers;
        protected List<ManagedMemoryBlock> mTxBuffers;
        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        /// <summary>
        /// Register address port.
        /// </summary>
        public readonly int RegisterAddress;
        /// <summary>
        /// Register data port.
        /// </summary>
        public readonly int RegisterData;
        /// <summary>
        /// Bus data port.
        /// </summary>
        public readonly int BusData;
        /// <summary>
        /// MAC1 port.
        /// </summary>
        public readonly int MAC1;
        /// <summary>
        /// MAC2 port.
        /// </summary>
        public readonly int MAC2;

        public AMDPCNetII(PCIDevice device)
            : base()
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get AMD PCNet card");
            }

            this.pciCard = device;
            this.pciCard.Claimed = true;
            this.pciCard.EnableDevice();

            int baseAddress = (int)this.pciCard.BaseAddressBar[0].BaseAddress;
            RegisterAddress = baseAddress + 0x14;
            RegisterData = baseAddress + 0x10;
            BusData = baseAddress + 0x1C;
            MAC1 = baseAddress;
            MAC2 = baseAddress + 0x04;

            IOPort.Write32(RegisterData, 0);

            // Get the EEPROM MAC Address and set it as the devices MAC
            byte[] eeprom_mac = new byte[6];
            uint result = IOPort.Read32(MAC1);
            eeprom_mac[0] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[1] = BinaryHelper.GetByteFrom32bit(result, 8);
            eeprom_mac[2] = BinaryHelper.GetByteFrom32bit(result, 16);
            eeprom_mac[3] = BinaryHelper.GetByteFrom32bit(result, 24);
            result = IOPort.Read32(MAC2);
            eeprom_mac[4] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[5] = BinaryHelper.GetByteFrom32bit(result, 8);

            mac = new MACAddress(eeprom_mac);

            mInitBlock = new ManagedMemoryBlock(28, 4);
            mRxDescriptor = new ManagedMemoryBlock(256, 16);
            mTxDescriptor = new ManagedMemoryBlock(256, 16);

            mInitBlock.Write32(0x00, (0x4 << 28) | (0x4 << 20));
            mInitBlock.Write32(0x04,
                (uint)(eeprom_mac[0] | (eeprom_mac[1] << 8) | (eeprom_mac[2] << 16) | (eeprom_mac[3] << 24)));
            mInitBlock.Write32(0x08, (uint)(eeprom_mac[4] | (eeprom_mac[5] << 8)));
            mInitBlock.Write32(0x0C, 0x0);
            mInitBlock.Write32(0x10, 0x0);
            mInitBlock.Write32(0x14, (uint)mRxDescriptor.Offset);
            mInitBlock.Write32(0x18, (uint)mTxDescriptor.Offset);

            InitializationBlockAddress = (uint)mInitBlock.Offset;
            SoftwareStyleRegister = 0x03;

            mRxBuffers = new List<ManagedMemoryBlock>();
            mTxBuffers = new List<ManagedMemoryBlock>();
            for (uint rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = rxd * 16;

                ManagedMemoryBlock buffer = new ManagedMemoryBlock(2048);
                mRxDescriptor.Write32(xOffset + 8, (uint)buffer.Offset);
                ushort buffer_len = (ushort)~buffer.Size;
                buffer_len++;
                uint flags = (uint)(buffer_len & 0x0FFF) | 0xF000 | 0x80000000;
                mRxDescriptor.Write32(xOffset + 4, flags);
                mRxBuffers.Add(buffer);
            }
            for (uint txd = 0; txd < 16; txd++)
            {
                uint xOffset = txd * 16;

                ManagedMemoryBlock buffer = new ManagedMemoryBlock(2048);
                mTxDescriptor.Write32(xOffset + 8, (uint)buffer.Offset);
                mTxBuffers.Add(buffer);
            }

            mNextTXDesc = 0;

            // Setup our Receive and Transmit Queues
            mTransmitBuffer = new Queue<byte[]>();
            mRecvBuffer = new Queue<byte[]>();

            INTs.SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);
        }

        protected void HandleNetworkInterrupt(ref INTs.IRQContext aContext)
        {
            uint cur_status = StatusRegister;

            if ((cur_status & 0x100) != 0)
            {
                mInitDone = true;
            }
            if ((cur_status & 0x200) != 0)
            {
                if (mTransmitBuffer.Count > 0)
                {
                    byte[] data = mTransmitBuffer.Peek();
                    if (SendBytes(ref data) == true)
                    {
                        mTransmitBuffer.Dequeue();
                    }
                }
            }
            if ((cur_status & 0x400) != 0)
            {
                ReadRawData();
            }

            StatusRegister = cur_status;
            Cosmos.Core.Global.PIC.EoiSlave();
        }

        /// <summary>
        /// Retrieve all AMD PCNetII network cards found on computer.
        /// </summary>
        public static void FindAll()
        {
            Console.WriteLine("Scanning for AMD PCNetII cards...");
            PCIDevice device = Cosmos.HAL.PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (device != null)
            {
                AMDPCNetII nic = new AMDPCNetII((PCIDevice)device);

                Console.WriteLine("Found AMD PCNetII NIC on PCI " + device.bus + ":" + device.slot + ":" +
                                  device.function);
                Console.WriteLine("NIC IRQ: " + device.InterruptLine);
                Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());
            }
        }

        #region Register Access Properties

        protected uint StatusRegister
        {
            get
            {
                IOPort.Write32(RegisterAddress, 0x00);
                return IOPort.Read32(RegisterData);
            }
            set
            {
                IOPort.Write32(RegisterAddress, 0x00);
                IOPort.Write32(RegisterData, value);
            }
        }

        protected uint InitializationBlockAddress
        {
            get
            {
                IOPort.Write32(RegisterAddress, 0x01);
                var result = IOPort.Read32(RegisterData);
                IOPort.Write32(RegisterAddress, 0x02);
                result |= IOPort.Read32(RegisterData) << 16;

                return result;
            }
            set
            {
                IOPort.Write32(RegisterAddress, 0x01);
                IOPort.Write32(RegisterData, value & 0xFFFF);
                IOPort.Write32(RegisterAddress, 0x02);
                IOPort.Write32(RegisterData, value >> 16);
            }
        }

        protected uint SoftwareStyleRegister
        {
            get
            {
                IOPort.Write32(RegisterAddress, 0x14);
                return IOPort.Read32(BusData);
            }
            set
            {
                IOPort.Write32(RegisterAddress, 0x14);
                IOPort.Write32(BusData, value);
            }
        }

        #endregion

        #region Network Device Implementation

        public override MACAddress MACAddress
        {
            get { return mac; }
        }

        public override bool Ready
        {
            get { return mInitDone; }
        }

        public override CardType CardType => CardType.Ethernet;

        public override string Name => "PCNETII";

        public override bool Enable()
        {
            StatusRegister = 0x43;

            return true;
        }

        /// <summary>
        /// Add bytes to the transmit buffer queue.
        /// </summary>
        /// <param name="buffer">bytes array to queue.</param>
        /// <param name="offset">Offset of the data in the buffer.</param>
        /// <param name="length">Data length.</param>
        /// <returns>TRUE on success.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if length is bigger than Int32.MaxValue.</exception>
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

        #endregion

        #region Helper Functions

        /// <summary>
        /// Send bytes to memory block.
        /// </summary>
        /// <param name="aData">A data to send.</param>
        /// <returns>TRUE on success.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if aData length is bigger than Int32.MaxValue.</exception>
        protected bool SendBytes(ref byte[] aData)
        {
            int txd = mNextTXDesc++;
            if (mNextTXDesc >= 16)
            {
                mNextTXDesc = 0;
            }

            uint xOffset = (uint)(txd * 16);
            uint status = mTxDescriptor.Read32(xOffset + 4);
            if ((status & 0x80000000) == 0)
            {
                for (uint b = 0; b < aData.Length; b++)
                {
                    mTxBuffers[txd][b] = aData[b];
                }
                //UInt16 buffer_len = (UInt16)(aData.Length < 64 ? 64 : aData.Length);
                ushort buffer_len = (ushort)aData.Length;
                buffer_len = (ushort)~buffer_len;
                buffer_len++;

                uint flags = (uint)(buffer_len & 0x0FFF) | 0x0300F000 | 0x80000000;

                mTxDescriptor.Write32(xOffset + 4, flags);

                return true;
            }

            return false;
        }

        private void ReadRawData()
        {
            uint status;
            ushort recv_size;
            byte[] recv_data;

            for (int rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = (uint)(rxd * 16);
                status = mRxDescriptor.Read32(xOffset + 4);
                if ((status & 0x80000000) == 0)
                {
                    recv_size = (ushort)(mRxDescriptor.Read32(xOffset + 0) & 0xFFF);

                    if (recv_size > 64) // remove checksum
                    {
                        recv_size -= 4;
                    }

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
