using Cosmos.Core;
using Cosmos.HAL.Network;
using System;
using System.Collections.Generic;
using static Cosmos.Core.INTs;

namespace Cosmos.HAL.Drivers.PCI.Network
{
    public class RTL8139 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected MACAddress mac;
        protected bool mInitDone;
        protected ManagedMemoryBlock rxBuffer;
        protected int rxBufferOffset;
        protected ushort capr;
        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;
        const ushort RxBufferSize = 32768;

        private uint Base;
        public RTL8139(PCIDevice device)
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get Realtek 8139 card");
            }
            pciCard = device;
            Base = device.BaseAddressBar[0].BaseAddress;
            // We are handling this device
            pciCard.Claimed = true;
            // Setup interrupt handling
            INTs.SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);
            // Get IO Address from PCI Bus
            // Enable the card
            pciCard.EnableDevice();
            // Turn on the card
            IOPort.Write8((ushort)(Base + 0x52), 0x01);
            //Do a software reset
            SoftwareReset();
            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = IOPort.Read8((ushort)(Base + b));
            }
            this.mac = new MACAddress(eeprom_mac);
            // Get a receive buffer and assign it to the card
            rxBuffer = new ManagedMemoryBlock(RxBufferSize + 2048 + 16, 4);
            RBStartRegister = (uint)rxBuffer.Offset;
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
        public static List<RTL8139> FindAll()
        {
            //Console.WriteLine("Scanning for Realtek 8139 cards...");

            List<RTL8139> cards = new List<RTL8139>();
            foreach (var xDevice in HAL.PCI.Devices)
            {
                if (xDevice.VendorID == 0x10EC && xDevice.DeviceID == 0x8139 && xDevice.Claimed == false)
                {
                    RTL8139 nic = new RTL8139(xDevice);
                    cards.Add(nic);
                }
            }
            return cards;
        }
        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {
            ushort cur_status = IntStatusRegister;
            //Console.WriteLine("RTL8139 Interrupt: ISR=" + cur_status.ToString());
            if ((cur_status & 0x01) != 0)
            {
                while ((CommandRegister & 0x01) == 0)
                {
                    //UInt32 packetHeader = BitConverter.ToUInt32(rxBuffer, rxBufferOffset + capr);
                    uint packetHeader = rxBuffer.Read32(capr);
                    ushort packetLen = (ushort)(packetHeader >> 16);
                    if ((packetHeader & 0x3E) != 0x00)
                    {
                        CommandRegister = 0x04; // TX Only;
                        capr = CurBufferAddressRegister;
                        CommandRegister = 0x0C; // RX and TX Enabled
                    }
                    else if ((packetHeader & 0x01) == 0x01)
                    {
                        ReadRawData(packetLen);
                    }
                    CurAddressPointerReadRegister = (ushort)(capr - 0x10);
                }
            }
            if ((cur_status & 0x10) != 0)
            {
                CurAddressPointerReadRegister = (ushort)(CurBufferAddressRegister - 0x10);
                cur_status = (ushort)(cur_status | 0x01);
            }
            IntStatusRegister = cur_status;
        }
        #region Register Access
        protected uint RBStartRegister
        {
            get { return IOPort.Read32((ushort)(Base + 0x30)); }
            set { IOPort.Write32((ushort)(Base + 0x30), value); }
        }
        internal uint RecvConfigRegister
        {
            get { return IOPort.Read32((ushort)(Base + 0x44)); }
            set { IOPort.Write32((ushort)(Base + 0x44), value); }
        }
        internal ushort CurAddressPointerReadRegister
        {
            get { return IOPort.Read16((ushort)(Base + 0x38)); }
            set { IOPort.Write16((ushort)(Base + 0x38), value); }
        }
        internal ushort CurBufferAddressRegister
        {
            get { return IOPort.Read16((ushort)(Base + 0x3A)); }
            set { IOPort.Write16((ushort)(Base + 0x3A), value); }
        }
        internal ushort IntMaskRegister
        {
            get { return IOPort.Read16((ushort)(Base + 0x3C)); }
            set { IOPort.Write16((ushort)(Base + 0x3C), value); }
        }
        internal ushort IntStatusRegister
        {
            get { return IOPort.Read16((ushort)(Base + 0x3E)); }
            set { IOPort.Write16((ushort)(Base + 0x3E), value); }
        }
        internal byte CommandRegister
        {
            get { return IOPort.Read8((ushort)(Base + 0x37)); }
            set { IOPort.Write8((ushort)(Base + 0x37), value); }
        }
        protected byte MediaStatusRegister
        {
            get { return IOPort.Read8((ushort)(Base + 0x58)); }
            set { IOPort.Write8((ushort)(Base + 0x58), value); }
        }
        protected byte Config1Register
        {
            get { return IOPort.Read8((ushort)(Base + 0x52)); }
            set { IOPort.Write8((ushort)(Base + 0x52), value); }
        }
        internal uint TransmitConfigRegister
        {
            get { return IOPort.Read32((ushort)(Base + 0x40)); }
            set { IOPort.Write32((ushort)(Base + 0x40), value); }
        }
        internal uint TransmitAddress1Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x20)); }
            set { IOPort.Write32((ushort)(Base + 0x20), value); }
        }
        internal uint TransmitAddress2Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x24)); }
            set { IOPort.Write32((ushort)(Base + 0x24), value); }
        }
        internal uint TransmitAddress3Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x28)); }
            set { IOPort.Write32((ushort)(Base + 0x28), value); }
        }
        internal uint TransmitAddress4Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x2C)); }
            set { IOPort.Write32((ushort)(Base + 0x2C), value); }
        }
        internal uint TransmitDescriptor1Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x10)); }
            set { IOPort.Write32((ushort)(Base + 0x10), value); }
        }
        internal uint TransmitDescriptor2Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x14)); }
            set { IOPort.Write32((ushort)(Base + 0x14), value); }
        }
        internal uint TransmitDescriptor3Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x18)); }
            set { IOPort.Write32((ushort)(Base + 0x18), value); }
        }
        internal uint TransmitDescriptor4Register
        {
            get { return IOPort.Read32((ushort)(Base + 0x1C)); }
            set { IOPort.Write32((ushort)(Base + 0x1C), value); }
        }
        #endregion
        protected bool CmdBufferEmpty
        {
            get { return (CommandRegister & 0x01) == 0x01; }
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
            return true;
        }
        public override bool Ready
        {
            get { return (Config1Register & 0x20) == 0; }
        }
        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            for (int b = 0; b < length; b++)
            {
                data[b] = buffer[b + offset];
            }
            //Console.WriteLine("Try sending");
            if (SendBytes(ref data) == false)
            {
                //Console.WriteLine("Queuing");
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

        public override CardType CardType => CardType.Ethernet;

        #endregion
        #region Helper Functions
        private void ReadRawData(ushort packetLen)
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
            capr += (ushort)((packetLen + 4 + 3) & 0xFFFFFFFC);
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
            ManagedMemoryBlock txBuffer;
            if (aData.Length < 64)
            {
                txBuffer = new ManagedMemoryBlock(64);
                for (uint b = 0; b < aData.Length; b++)
                {
                    txBuffer[b] = aData[b];
                }
            }
            else
            {
                txBuffer = new ManagedMemoryBlock((uint)aData.Length);
                for (uint i = 0; i < aData.Length; i++)
                {
                    txBuffer[i] = aData[i];
                }
            }
            switch (txd)
            {
                case 0:
                    TransmitAddress1Register = (uint)txBuffer.Offset;
                    TransmitDescriptor1Register = txBuffer.Size;
                    break;
                case 1:
                    TransmitAddress2Register = (uint)txBuffer.Offset;
                    TransmitDescriptor2Register = txBuffer.Size;
                    break;
                case 2:
                    TransmitAddress3Register = (uint)txBuffer.Offset;
                    TransmitDescriptor3Register = txBuffer.Size;
                    break;
                case 3:
                    TransmitAddress4Register = (uint)txBuffer.Offset;
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
