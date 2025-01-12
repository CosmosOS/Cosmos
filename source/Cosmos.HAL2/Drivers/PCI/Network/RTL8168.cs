/*
* PROJECT:          Aura Operating System Development
* CONTENT:          RTL8168 Driver
* PROGRAMMERS:      Valentin Charbonnier <valentInBreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using static Cosmos.Core.INTs;

namespace Cosmos.HAL.Drivers.PCI.Network
{
    public class RTL8168 : NetworkDevice
    {
        protected PCIDevice pciCard;
        protected MACAddress mac;

        uint BaseAddress;

        public override CardType CardType => CardType.Ethernet;

        public override string Name => "RTL8168";

        protected ManagedMemoryBlock mRxDescriptor;
        protected ManagedMemoryBlock mTxDescriptor;

        protected List<ManagedMemoryBlock> mRxBuffers;
        protected List<ManagedMemoryBlock> mTxBuffers;

        protected Queue<byte[]> mRecvBuffer;
        protected Queue<byte[]> mTransmitBuffer;

        private int mNextTXDesc;

        uint OWN = 0x80000000, EOR = 0x40000000;

        uint GetMacVersion()
        {
            return Ports.InD((ushort)(BaseAddress + 0x40));
        }

        void InitBuffers()
        {

            mNextTXDesc = 0;

            mRxDescriptor = new ManagedMemoryBlock(32 * 16, 256);
            mTxDescriptor = new ManagedMemoryBlock(32 * 16, 256);

            mRxBuffers = new List<ManagedMemoryBlock>();
            mTxBuffers = new List<ManagedMemoryBlock>();

            mTransmitBuffer = new Queue<byte[]>();
            mRecvBuffer = new Queue<byte[]>();

            for (int i = 0; i < 32; i++)
            {
                uint xOffset = (uint)i * 16;
                var rxbuffer = new ManagedMemoryBlock(2048, 8);
                var txbuffer = new ManagedMemoryBlock(2048, 8);
                if (i == (32 - 1))
                {
                    mRxDescriptor.Write32(xOffset + 0, OWN | EOR | (2048 & 0x3FFF));
                    mTxDescriptor.Write32(xOffset + 0, EOR);
                }
                else
                {
                    mRxDescriptor.Write32(xOffset + 0, OWN | EOR | (2048 & 0x3FFF));
                    mTxDescriptor.Write32(xOffset + 0, 0);
                }

                mRxBuffers.Add(rxbuffer);
                mTxBuffers.Add(txbuffer);

                mRxDescriptor.Write32(xOffset + 4, 0);
                mTxDescriptor.Write32(xOffset + 4, 0);

                mRxDescriptor.Write32(xOffset + 8, rxbuffer.Offset);
                mTxDescriptor.Write32(xOffset + 8, txbuffer.Offset);

                mRxDescriptor.Write32(xOffset + 12, 0);
                mTxDescriptor.Write32(xOffset + 12, 0);

            }
        }

        public RTL8168(PCIDevice device) : base()
        {
            if (device == null)
            {
                throw new ArgumentException("PCI Device is null. Unable to get Realtek 8168 card");
            }
            pciCard = device;

            // We are handling this device
            pciCard.Claimed = true;

            BaseAddress = pciCard.BaseAddressBar[0].BaseAddress;

            // Enable the card
            pciCard.EnableDevice();

            SetIrqHandler(device.InterruptLine, HandleNetworkInterrupt);

            Ports.OutB((ushort)(BaseAddress + 0xE0), 0x08);

            Reset();

            // Get the MAC Address
            byte[] eeprom_mac = new byte[6];
            for (uint b = 0; b < 6; b++)
            {
                eeprom_mac[b] = Ports.InB((ushort)(BaseAddress + b));
            }

            mac = new MACAddress(eeprom_mac);

            InitBuffers();

            Ports.OutD((ushort)(BaseAddress + 0x44), 0x0000E70F); // Enable RX

            Ports.OutD((ushort)(BaseAddress + 0x37), 0x04);

            Ports.OutD((ushort)(BaseAddress + 0x40), 0x03000700); // Enable TX

            Ports.OutD((ushort)(BaseAddress + 0xDA), 2048); // Max rx packet size

            Ports.OutB((ushort)(BaseAddress + 0xEC), 0x3F); // No early transmit

            Ports.OutD((ushort)(BaseAddress + 0x20), mTxDescriptor.Offset);
            Console.WriteLine("addresstx desc: " + mTxDescriptor.Offset);

            Ports.OutD((ushort)(BaseAddress + 0xE4), mRxDescriptor.Offset);
            Console.WriteLine("addressrx desc: " + mRxDescriptor.Offset);

            if (((GetMacVersion() & 0x7cf00000) == 0x54100000) || ((GetMacVersion() & 0x7cf00000) == 0x54000000))
            {
                Console.WriteLine("8168H Detected!");

                Ports.OutD((ushort)(BaseAddress + 0x40), Ports.InD((ushort)(BaseAddress + 0x40)) | (1 << 7)); // AUTO TX FIFO
            }

            Ports.OutW((ushort)(BaseAddress + 0x3C), 0xC3FF); //Activating all Interrupts

            Ports.OutB((ushort)(BaseAddress + 0x37), 0x0C); // Enabling receive and transmit

            //Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7cf00000));
            //Console.WriteLine("Netcard version: 0x" + System.Utils.Conversion.DecToHex((int)GetMacVersion() & 0x7c800000));

        }

        public bool Reset()
        {
            Ports.OutB((ushort)(BaseAddress + 0x37), 0x10); /* Send the Reset bit to the Command register */
            while ((Ports.InB((ushort)(BaseAddress + 0x37)) & 0x10) != 0) { } /* Wait for the chip to finish resetting */
            return true;
        }

        protected void HandleNetworkInterrupt(ref IRQContext aContext)
        {

            ushort status = Ports.InW((ushort)(BaseAddress + 0x3E));

            if ((status & 0x0001) != 0)
            {
                ReadRawData();
            }
            if ((status & 0x0002) != 0) Console.WriteLine("Receive error");
            if (((status & 0x0004) != 0) && ((status & 0x0080) != 0))
            {
            }
            else
            {
                if ((status & 0x0004) != 0) Console.WriteLine("Transmit succesfull - descriptor not resetted");
                //if ((status & 0x0080) != 0) Console.WriteLine("Transmit descriptor unavailable");
            }
            if ((status & 0x0008) != 0) Console.WriteLine("Transmit error");
            if ((status & 0x0010) != 0)
            {
                Console.WriteLine("Receive descriptor unavailable");
            }
            if ((status & 0x0020) != 0)
            {
                if ((Ports.InB((ushort)(BaseAddress + 0x6C)) & 0x02) != 0)
                {
                    Console.WriteLine("\nLink is up with ");
                    if ((Ports.InB((ushort)(BaseAddress + 0x6C)) & 0x04) != 0) Console.WriteLine("10 Mbps and ");
                    if ((Ports.InB((ushort)(BaseAddress + 0x6C)) & 0x08) != 0) Console.WriteLine("100 Mbps and ");
                    if ((Ports.InB((ushort)(BaseAddress + 0x6C)) & 0x10) != 0) Console.WriteLine("1000 Mbps and ");
                    if ((Ports.InB((ushort)(BaseAddress + 0x6C)) & 0x01) != 0) Console.WriteLine("Full-duplex");
                    else Console.WriteLine("Half-duplex");
                }
                else
                {
                    Console.WriteLine("\nLink is down!");
                }
            }
            if ((status & 0x0040) != 0)
            {
                Console.WriteLine("RX FIFO overflow!");
                if ((status & 0x0200) != 0)
                {
                    Console.WriteLine("RX FIFO empty");
                }
            }
            if ((status & 0x0100) != 0)
            {
                Console.WriteLine("Software Interrupt");
            }
            if ((status & 0x0200) != 0)
            {
                Console.WriteLine("RX FIFO empty");
                Ports.OutW((ushort)(BaseAddress + status), 0x0040); //https://groups.google.com/forum/#!topic/fa.linux.kernel/Vo8-9W3LoQs
            }
            if ((status & 0x0400) != 0) Console.WriteLine("Unknown Status (reserved Bit 11)");
            if ((status & 0x0800) != 0) Console.WriteLine("Unknown Status (reserved Bit 12)");
            if ((status & 0x1000) != 0) Console.WriteLine("Unknown Status (reserved Bit 13)");
            if ((status & 0x2000) != 0) Console.WriteLine("Unknown Status (reserved Bit 14)");
            if ((status & 0x4000) != 0) Console.WriteLine("Timeout");
            if ((status & 0x8000) != 0) Console.WriteLine("Unknown Status (reserved Bit 16)");

            Ports.OutW((ushort)(BaseAddress + 0x3E), status);

        }

        #region Network Device Implementation
        public override MACAddress MACAddress
        {
            get { return this.mac; }
        }

        public override bool Enable()
        {
            return true;
        }

        public override bool Ready
        {
            get { return true; }
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
            return new byte[] { };
        }

        public override int BytesAvailable()
        {
            return 0;
        }

        public override bool IsSendBufferFull()
        {
            return false;
        }

        public override bool IsReceiveBufferFull()
        {
            return false;
        }

        protected bool SendBytes(ref byte[] aData)
        {
            if (aData.Length > 2048)
                return false; // Splitting packets not yet supported

            uint xOffset = (uint)(mNextTXDesc * 16);

            if ((mTxDescriptor.Read32(xOffset + 0) & (1 << 31)) != 0)
            {
                Console.WriteLine("The Tx buffer is busy");
                return false;
            }

            for (uint b = 0; b < aData.Length; b++)
            {
                mTxBuffers[mNextTXDesc][b] = aData[b];
            }

            mTxDescriptor.Write32(xOffset + 0, OWN | mTxDescriptor.Read32(xOffset + 0) & EOR | (uint)(aData.Length & 0x3FFF) | (1 << 29) | (1 << 28));

            mTxDescriptor.Write32(xOffset + 4, 0);

            Ports.OutB((ushort)(BaseAddress + 0x38), 1 << 6);

            mNextTXDesc++;
            mNextTXDesc %= 32;

            return true;

        }

        private void ReadRawData()
        {
            for (ushort i = 0; i < 32; i++)
            {
                uint xOffset = (uint)(i * 16);
                if ((mRxDescriptor.Read32(xOffset) & 0x80000000) == 0)
                {
                    uint length = mRxDescriptor.Read32(xOffset + 0) & 0x3FFF;
                    if (length > 4)
                    {

                        byte[] recv_data = new byte[length - 4];
                        for (uint b = 0; b < length; b++)
                        {
                            recv_data[b] = mRxBuffers[i][b];
                        }

                        if (DataReceived != null)
                        {
                            DataReceived(recv_data);
                        }
                        else
                        {
                            mRecvBuffer.Enqueue(recv_data);
                        }

                    }

                    // Reset descriptor
                    if (i == (32 - 1)) // Last descriptor? if so, set the EOR bit
                        mRxDescriptor.Write32(xOffset + 0, OWN | EOR | (2048 & 0x3FFF));
                    else
                        mRxDescriptor.Write32(xOffset + 0, OWN | (2048 & 0x3FFF));
                    mRxDescriptor.Write32(xOffset + 4, 0);
                }
            }
        }

        #endregion

    }
}
