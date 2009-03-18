using System;
using HW = Cosmos.Hardware;

namespace Cosmos.Sys.Network.TCPIP
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
            initFields();
        }

        protected virtual void initFields()
        {
            destMAC = new HW.Network.MACAddress(mRawData, 0);
            srcMAC = new HW.Network.MACAddress(mRawData, 6);
            aType = (UInt16)((mRawData[12] << 8) | mRawData[13]);
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

        public HW.Network.MACAddress SourceMAC
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
        public HW.Network.MACAddress DestinationMAC
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
