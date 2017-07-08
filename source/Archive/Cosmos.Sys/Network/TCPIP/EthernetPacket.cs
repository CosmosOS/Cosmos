using System;
using HW = Cosmos.Hardware2;
using Cosmos.Kernel;

namespace Cosmos.Sys.Network.TCPIP
{
    internal class EthernetPacket
    {
        protected byte[] mRawData;
        protected HW.Network.MACAddress srcMAC;
        protected HW.Network.MACAddress destMAC;
        protected UInt16 aEtherType;

        internal EthernetPacket(byte[] rawData)
        {
            mRawData = rawData;
            initFields();
        }

        protected virtual void initFields()
        {
            destMAC = new HW.Network.MACAddress(mRawData, 0);
            srcMAC = new HW.Network.MACAddress(mRawData, 6);
            aEtherType = (UInt16)((mRawData[12] << 8) | mRawData[13]);
        }

        protected EthernetPacket(UInt16 type, int packet_size)
            : this(HW.Network.MACAddress.None, HW.Network.MACAddress.None, type, packet_size)
        {
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
            initFields();
        }

        internal HW.Network.MACAddress SourceMAC
        {
            get { return this.srcMAC; }
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    mRawData[6+i] = value.bytes[i];
                }
                initFields();
            }
        }
        internal HW.Network.MACAddress DestinationMAC
        {
            get { return this.destMAC; }
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    mRawData[i] = value.bytes[i];
                }
                initFields();
            }
        }
        internal UInt16 EthernetType
        {
            get { return this.aEtherType; }
        }

        internal byte[] GetBytes()
        {
            return this.mRawData;
        }

        internal byte[] RawData
        {
            get { return this.mRawData; }
        }

        public override string ToString()
        {
            return ""; // "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + aEtherType.ToHex(4);
        }
    }
}
