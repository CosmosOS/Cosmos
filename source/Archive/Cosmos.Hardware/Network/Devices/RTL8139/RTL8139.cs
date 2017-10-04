using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.Network;
using Cosmos.Kernel;
using Cosmos.Kernel.ManagedMemory;

namespace Cosmos.Hardware2.Network.Devices.RTL8139
{
    public class RTL8139 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected AddressSpace io;
        protected MACAddress mac;
        protected bool mInitDone;

        protected ManagedMemorySpace rxBuffer;
        protected int rxBufferOffset;
        protected UInt16 capr;

        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        const UInt16 RxBufferSize = 32768;

        public RTL8139(PCIDevice device)
        {
            if (device == null) {
                throw new ArgumentException("PCI Device is null. Unable to get Realtek 8139 card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            // Setup interrupt handling
            //Interrupts.IRQ11 += HandleNetworkInterrupt;
            //Interrupts.AddIRQHandler(device.InterruptLine, HandleNetworkInterrupt);

            // Get IO Address from PCI Bus
            io = pciCard.GetAddressSpace(0);

            // Enable the card
            pciCard.EnableDevice();

            // Turn on the card
            io.Write8(0x52, 0x01);

            //Do a software reset
            SoftwareReset();

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = io.Read8(b);
            }

            this.mac = new MACAddress(eeprom_mac);

            // Get a receive buffer and assign it to the card
            rxBuffer = new ManagedMemorySpace(RxBufferSize + 2048 + 16, 4);

            RBStartRegister = rxBuffer.Offset;

            // Setup receive Configuration
            RecvConfigRegister = 0xF381;
            // Setup Transmit Configuration
            TransmitConfigRegister = 0x3000300;

            // Setup Interrupts
            IntMaskRegister = 0x7F;
            IntStatusRegister = 0xFFFF;

            // Setup our Receive and Transmit Queues
            mRecvBuffer = new Queue<byte[]>();
            mTransmitBuffer = new Queue<byte[]>();
        }

        public static void InitDriver()
        {
            Device.AddDriverInit(FindAll);
        }

        public static void FindAll()
        {
            Console.WriteLine("Scanning for Realtek 8139 cards...");
            for(int i = 0; i < PCIBus.Devices.Length; i++)
            {
                var xDevice = PCIBus.Devices[i];
                //DebugWriteLine("VendorID: " + device.VendorID + " - DeviceID: " + device.DeviceID);
                if ((xDevice.VendorID == 0x10EC) && (xDevice.DeviceID == 0x8139) && (xDevice.Claimed == false))
                {
                    RTL8139 nic = new RTL8139(xDevice);
                    NetworkDevice.Add(nic);
                }
            }
        }

        //protected void HandleNetworkInterrupt(ref IRQContext aContext)
        //{
        //    UInt16 cur_status = IntStatusRegister;

        //    //Console.WriteLine("RTL8139 Interrupt: ISR=" + cur_status.ToString());
        //    if ((cur_status & 0x01) != 0)
        //    {
        //        while ((CommandRegister & 0x01) == 0)
        //        {
        //            //UInt32 packetHeader = BitConverter.ToUInt32(rxBuffer, rxBufferOffset + capr);
        //            UInt32 packetHeader = rxBuffer.Read32(capr);
        //            UInt16 packetLen = (UInt16)(packetHeader >> 16);
        //            if ((packetHeader & 0x3E) != 0x00)
        //            {
        //                CommandRegister = 0x04; // TX Only;
        //                capr = CurBufferAddressRegister;
        //                CommandRegister = 0x0C; // RX and TX Enabled
        //            }
        //            else if ((packetHeader & 0x01) == 0x01)
        //            {
        //                ReadRawData(packetLen);
        //            }

        //            CurAddressPointerReadRegister = (UInt16)(capr - 0x10);
        //        }
        //    }
        //    if ((cur_status & 0x10) != 0)
        //    {
        //        CurAddressPointerReadRegister = (UInt16)(CurBufferAddressRegister - 0x10);
        //        cur_status = (UInt16)(cur_status | 0x01);
        //    }

        //    IntStatusRegister = cur_status;
        //}

        #region Register Access
        protected UInt32 RBStartRegister
        {
            get { return io.Read32(0x30); }
            set { io.Write32(0x30, value); }
        }
        internal UInt32 RecvConfigRegister
        {
            get { return io.Read32(0x44); }
            set { io.Write32(0x44, value); }
        }
        internal UInt16 CurAddressPointerReadRegister
        {
            get { return io.Read16(0x38); }
            set { io.Write16(0x38, value); }
        }
        internal UInt16 CurBufferAddressRegister
        {
            get { return io.Read16(0x3A); }
            set { io.Write16(0x3A, value); }
        }

        internal UInt16 IntMaskRegister
        {
            get { return io.Read16(0x3C); }
            set { io.Write16(0x3C, value); }
        }
        internal UInt16 IntStatusRegister
        {
            get { return io.Read16(0x3E); }
            set { io.Write16(0x3E, value); }
        }

        internal byte CommandRegister
        {
            get { return io.Read8(0x37); }
            set { io.Write8(0x37, value); }
        }
        protected byte MediaStatusRegister
        {
            get { return io.Read8(0x58); }
            set { io.Write8(0x58, value); }
        }

        protected byte Config1Register
        {
            get { return io.Read8(0x52); }
            set { io.Write8(0x52, value); }
        }

        internal UInt32 TransmitConfigRegister
        {
            get { return io.Read32(0x40); }
            set { io.Write32(0x40, value); }
        }

        internal UInt32 TransmitAddress1Register
        {
            get { return io.Read32(0x20); }
            set { io.Write32(0x20, value); }
        }
        internal UInt32 TransmitAddress2Register
        {
            get { return io.Read32(0x24); }
            set { io.Write32(0x24, value); }
        }
        internal UInt32 TransmitAddress3Register
        {
            get { return io.Read32(0x28); }
            set { io.Write32(0x28, value); }
        }
        internal UInt32 TransmitAddress4Register
        {
            get { return io.Read32(0x2C); }
            set { io.Write32(0x2C, value); }
        }
        internal UInt32 TransmitDescriptor1Register
        {
            get { return io.Read32(0x10); }
            set { io.Write32(0x10, value); }
        }
        internal UInt32 TransmitDescriptor2Register
        {
            get { return io.Read32(0x14); }
            set { io.Write32(0x14, value); }
        }
        internal UInt32 TransmitDescriptor3Register
        {
            get { return io.Read32(0x18); }
            set { io.Write32(0x18, value); }
        }
        internal UInt32 TransmitDescriptor4Register
        {
            get { return io.Read32(0x1C); }
            set { io.Write32(0x1C, value); }
        }
        #endregion

        protected bool CmdBufferEmpty
        {
            get { return ((CommandRegister & 0x01) == 0x01); }
        }

        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return this.mac; }
        }

        public override bool Enable()
        {
            // Enable Receiving and Transmitting of data
            CommandRegister = 0x0C;

            while (this.Ready == false)
            { }

            return base.Enable();
        }

        public override bool Ready
        {
            get { return ((Config1Register & 0x20) == 0); }
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            for (int b = 0; b < length; b++)
            {
                data[b] = buffer[b + offset];
            }

            Console.WriteLine("Try sending");

            if (SendBytes(ref data) == false)
            {
                Console.WriteLine("Queuing");
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
            get { return "Realtek 8139 Chipset NIC"; }
        }
        #endregion

        #region Helper Functions
        private void ReadRawData(UInt16 packetLen)
        {
            int recv_size = packetLen - 4;
            byte[] recv_data = new byte[recv_size];
            for (uint b = 0; b < recv_size; b++)
            {
                recv_data[b] = rxBuffer[(uint)(capr + 4 + b)];
            }
            if (DataReceived != null)
            {
                DataReceived(recv_data);
            }
            else
            {
                if (mRecvBuffer == null)
                {
                }

                mRecvBuffer.Enqueue(recv_data);
            }

            capr += (UInt16)((packetLen + 4 + 3) & 0xFFFFFFFC);
            if (capr > RxBufferSize)
            {
                capr -= RxBufferSize;
            }
        }

        protected void SoftwareReset()
        {
            CommandRegister = 0x10;
            while ((CommandRegister & 0x10) != 0)
            { }
        }

        protected bool SendBytes(ref byte[] aData)
        {
            int txd = mNextTXDesc++;
            if (mNextTXDesc >= 4)
            {
                mNextTXDesc = 0;
            }

            ManagedMemorySpace txBuffer;
            if (aData.Length < 64)
            {
                txBuffer = new ManagedMemorySpace(64);
                for (uint b = 0; b < aData.Length; b++)
                {
                    txBuffer[b] = aData[b];
                }
            }
            else
            {
                txBuffer = new ManagedMemorySpace(aData);
            }

            switch (txd)
            {
                case 0:
                    TransmitAddress1Register = txBuffer.Offset;
                    TransmitDescriptor1Register = txBuffer.Size;
                    break;
                case 1:
                    TransmitAddress2Register = txBuffer.Offset;
                    TransmitDescriptor2Register = txBuffer.Size;
                    break;
                case 2:
                    TransmitAddress3Register = txBuffer.Offset;
                    TransmitDescriptor3Register = txBuffer.Size;
                    break;
                case 3:
                    TransmitAddress4Register = txBuffer.Offset;
                    TransmitDescriptor4Register = txBuffer.Size;
                    break;
                default:
                    return false;
            }

            return true;
        }

        #endregion
    }
}
