using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public class EthernetPacket
    {
        protected byte[] mRawData;
        protected HW.Network.MACAddress srcMAC;
        protected HW.Network.MACAddress destMAC;
        protected UInt16 aType;

        public EthernetPacket(byte[] rawData)
        {
            mRawData = rawData;
            destMAC = new HW.Network.MACAddress(rawData, 0);
            srcMAC = new HW.Network.MACAddress(rawData, 6);
            aType = (UInt16)((rawData[12] << 8) | rawData[13]);
        }

        protected EthernetPacket(HW.Network.MACAddress dest, HW.Network.MACAddress src, UInt16 type, int packet_size)
        {
            mRawData = new byte[packet_size];
            for (int i = 0; i < 6; i++)
            {
                mRawData[i] = dest.bytes[i];
                mRawData[6 + i] = src.bytes[i];
            }

            mRawData[12] = (byte)(type >> 8);
            mRawData[13] = (byte)(type >> 0);
        }

        public HW.Network.MACAddress SourceMAC
        {
            get { return this.srcMAC; }
        }
        public HW.Network.MACAddress DestinationMAC
        {
            get { return this.destMAC; }
        }
        public UInt16 Type
        {
            get { return this.aType; }
        }

        public byte[] GetBytes()
        {
            return this.mRawData;
        }

        internal byte[] RawData
        {
            get { return this.mRawData; }
        }
    }
}
