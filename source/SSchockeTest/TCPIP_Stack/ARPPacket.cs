using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public class ARPPacket : EthernetPacket
    {
        protected UInt16 aHardwareType;
        protected UInt16 aProtocolType;
        protected byte aHardwareLen;
        protected byte aProtocolLen;
        protected UInt16 aOperation;

        public ARPPacket(byte[] rawData)
            : base(rawData)
        {
            aHardwareType = (UInt16)((rawData[14] << 8) | rawData[15]);
            aProtocolType = (UInt16)((rawData[16] << 8) | rawData[17]);
            aHardwareLen = rawData[18];
            aProtocolLen = rawData[19];
            aOperation = (UInt16)((rawData[20] << 8) | rawData[21]);
        }

        protected ARPPacket(HW.Network.MACAddress dest, HW.Network.MACAddress src, UInt16 hwType, UInt16 protoType,
            byte hwLen, byte protoLen, UInt16 operation, int packet_size)
            : base(dest, src, 0x0806, packet_size)
        {
            mRawData[14] = (byte)(hwType >> 8);
            mRawData[15] = (byte)(hwType >> 0);
            mRawData[16] = (byte)(protoType >> 8);
            mRawData[17] = (byte)(protoType >> 0);
            mRawData[18] = hwLen;
            mRawData[19] = protoLen;
            mRawData[20] = (byte)(operation >> 8);
            mRawData[21] = (byte)(operation >> 0);
        }

        public UInt16 Operation
        {
            get { return this.aOperation; }
        }
        public UInt16 HardwareType
        {
            get { return this.aHardwareType; }
        }
        public UInt16 ProtocolType
        {
            get { return this.aProtocolType; }
        }

        /*public ARPPacket(MACAddress srcMAC)
        {
            packet_data = new byte[60];
            for (int i = 0; i < 6; i++)
            {
                packet_data[i] = MACAddress.Broadcast.bytes[i];
            }
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 6] = srcMAC.bytes[i];
            }
            packet_data[12] = 0x08;
            packet_data[13] = 0x06;
            packet_data[14] = 0x00;
            packet_data[15] = 0x01;
            packet_data[16] = 0x08;
            packet_data[17] = 0x00;
            packet_data[18] = 0x06;
            packet_data[19] = 0x04;
            packet_data[20] = 0x00;
            packet_data[21] = 0x01;
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 22] = srcMAC.bytes[i];
            }
            packet_data[28] = 0xC0;
            packet_data[29] = 0xA8;
            packet_data[30] = 0x14;
            packet_data[31] = 0x7B;
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 32] = 0x00;
            }
            packet_data[38] = 0xC0;
            packet_data[39] = 0xA8;
            packet_data[40] = 0x14;
            packet_data[41] = 0x1C;
            for (int i = 42; i < 60; i++)
            {
                packet_data[i] = 0x00;
            }
        } */
    }
}
