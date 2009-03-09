using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace Cosmos.Playground.SSchocke
{
    public class AMDPCNet : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected Kernel.IOAddressSpace io;
        protected MACAddress mac;
        protected bool mInitDone;

        protected List<byte[]> mRxBuffers;
        protected List<byte[]> mTxBuffers;
        private UInt32 mTxDescriptorAddress;
        private UInt32 mRxDescriptorAddress;

        public AMDPCNet(PCIDevice device)
        {
            if (device == null) {
                throw new ArgumentException("PCI Device is null. Unable to get AMD PCNet card");
            }
            pciCard = device;
            Interrupts.IRQ09 += HandleNetworkInterrupt;
            io = pciCard.GetAddressSpace(0) as Kernel.IOAddressSpace;
            pciCard.EnableDevice();

            io.Write32(0x10, 0); // Set the device into 32-bit mode

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

            UInt32 address = Heap.MemAlloc(0x20);
            UInt32 aligned_address = address + (4 - (address % 4));
            Console.Write("Allocated 32 bytes for initialization block @ 0x" + address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + aligned_address.ToHex(8) + ")");

            UInt32 rd_address = Heap.MemAlloc(0x140);
            UInt32 tx_address = Heap.MemAlloc(0x140);
            mRxDescriptorAddress = rd_address + (16 - (rd_address % 16));
            mTxDescriptorAddress = tx_address + (16 - (tx_address % 16));
            Console.Write("Allocated 320 bytes for RX ring descriptors @ 0x" + rd_address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + mRxDescriptorAddress.ToHex(8) + ")");
            Console.Write("Allocated 320 bytes for TX ring descriptors @ 0x" + tx_address.ToHex(8));
            Console.WriteLine("(Aligned to 0x" + mTxDescriptorAddress.ToHex(8) + ")");

            unsafe
            {
                uint* init_block = (uint*)aligned_address;
                //init_block[0] = (uint)(BinaryHelper.BitPos.BIT15);
                init_block[0] = (0x04 << 28) | (0x04 << 30);
                init_block[1] = (uint)(eeprom_mac[0] | (eeprom_mac[1] << 8) | (eeprom_mac[2] << 16) | (eeprom_mac[3] << 24));
                init_block[2] = (uint)(eeprom_mac[4] | (eeprom_mac[5] << 8));
                init_block[3] = 0x00;
                init_block[4] = 0x00;
                init_block[5] = mRxDescriptorAddress;
                init_block[6] = mTxDescriptorAddress;
            }

            InitializationBlockAddress = aligned_address;
            SoftwareStyleRegister = 0x03; // Set the device to PCNet-PCI II Controller mode (full 32-bit mode)

            mRxBuffers = new List<byte[]>();
            mTxBuffers = new List<byte[]>();
            unsafe
            {
                for (int rxd = 0; rxd < 16; rxd++)
                {
                    uint* rd_desc = (uint*)(mRxDescriptorAddress + (rxd * 16));
                    byte[] buffer = new byte[2048];
                    rd_desc[2] = GetMemoryAddress(ref buffer);
                    UInt16 buffer_len = (UInt16)(~buffer.Length);
                    buffer_len++;
                    rd_desc[1] = (UInt32)(buffer_len & 0x0FFF);
                    rd_desc[1] |= 0xF000;
                    rd_desc[1] |= (uint)BinaryHelper.BitPos.BIT31;
                    mRxBuffers.Add(buffer);
                }
                for (int txd = 0; txd < 16; txd++)
                {
                    uint* tx_desc = (uint*)(mTxDescriptorAddress + (txd * 16));
                    byte[] buffer = new byte[2048];
                    tx_desc[2] = GetMemoryAddress(ref buffer);
                    mTxBuffers.Add(buffer);
                }
            }
        }

        protected void HandleNetworkInterrupt(ref Interrupts.InterruptContext aContext)
        {
            UInt32 cur_status = StatusRegister;

            StatusRegister = cur_status;
            //Console.WriteLine("AMD PCNet Driver Interrupt - Status=" + cur_status.ToHex(4));
            if ((cur_status & (uint)BinaryHelper.BitPos.BIT8) != 0)
            {
                mInitDone = true;
            }
            if ((cur_status & (uint)BinaryHelper.BitPos.BIT9) != 0)
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
            if ((cur_status & (uint)BinaryHelper.BitPos.BIT10) != 0)
            {
                ReadRawData();
            }
        }

        public Queue<byte[]> mRecvBuffer;

        private void ReadRawData()
        {
            uint status;
            UInt16 recv_size;
            byte[] recv_data;

            Debug.Debugger.Break();
            if (mRecvBuffer == null)
            {
                mRecvBuffer = new Queue<byte[]>();
            }
            unsafe
            {
                for (int rxd = 0; rxd < 16; rxd++)
                {
                    uint* rd_desc = (uint*)(mRxDescriptorAddress + (rxd * 16));
                    status = rd_desc[1];
                    if ((status & (uint)BinaryHelper.BitPos.BIT31) == 0)
                    {
                        recv_size = (UInt16)(rd_desc[0] & 0xFFF);
                        recv_data = new byte[recv_size];
                        for (int b = 0; b < recv_size; b++)
                        {
                            recv_data[b] = mRxBuffers[rxd][b];
                        }
                        mRecvBuffer.Enqueue(recv_data);
                        rd_desc[1] |= (uint)BinaryHelper.BitPos.BIT31;
                    }
                }
            }
        }

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
                if ((StatusRegister & (uint)BinaryHelper.BitPos.BIT1) != 0)
                {
                    return true;
                }

                return false;
            }
        }

        public UInt32 StatusRegister
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

        public UInt32 ModeRegister
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

        public UInt32 BurstBusControlRegister
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

        public UInt32 SoftwareStyleRegister
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

        public UInt32 InitializationBlockAddress
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

        public override MACAddress MACAddress
        {
            get { return mac; }
        }

        public override bool Enable()
        {
            StatusRegister = 0x43;

            return base.Enable();
        }

        public Queue<byte[]> mTransmitBuffer;
        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            if (mTransmitBuffer == null)
            {
                mTransmitBuffer = new Queue<byte[]>();
            }

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
            if ((mRecvBuffer == null) || (mRecvBuffer.Count < 1))
            {
                return null;
            }

            byte[] data = mRecvBuffer.Dequeue();
            return data;
        }

        public override int BytesAvailable()
        {
            if ((mRecvBuffer == null) || (mRecvBuffer.Count < 1))
            {
                return 0;
            }

            return mRecvBuffer.Peek().Length;
        }

        public override bool IsSendBufferFull()
        {
            throw new NotImplementedException();
        }

        public override bool IsReceiveBufferFull()
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return "Generic AMD PCNet Network device"; }
        }

        /// <summary>
        /// Get the 32-bit address where the bytearray is stored.
        /// </summary>
        private unsafe UInt32 GetMemoryAddress(ref byte[] bytearray)
        {
            fixed (byte* bodystart = bytearray)
            {
                return (UInt32)bodystart;
            }
        }

        protected bool SendBytes(ref byte[] aData)
        {
            unsafe
            {
                for (int txd = 0; txd < 16; txd++)
                {
                    uint* tx_desc = (uint*)(mTxDescriptorAddress + (txd * 16));
                    if ((tx_desc[1] & (uint)BinaryHelper.BitPos.BIT31) == 0)
                    {
                        for (int b = 0; b < aData.Length; b++)
                        {
                            mTxBuffers[txd][b] = aData[b];
                        }
                        UInt16 buffer_len = (UInt16)(~aData.Length);
                        buffer_len++;
                        tx_desc[1] = (UInt32)(buffer_len & 0x0FFF);
                        tx_desc[1] |= (uint)(BinaryHelper.BitPos.BIT25 | BinaryHelper.BitPos.BIT24) | 0xF000;
                        tx_desc[1] |= (uint)BinaryHelper.BitPos.BIT31;
                        return true;
                    }
                }
            }

            return false;
        }

        public UInt32[] GetTXDesc0()
        {
            UInt32[] tx_desc0 = new UInt32[4];
            unsafe
            {
                uint* tx_desc = (uint*)mTxDescriptorAddress;
                for (int i = 0; i < 4; i++)
                {
                    tx_desc0[i] = tx_desc[i];
                }
            }

            return tx_desc0;
        }
    }
}
