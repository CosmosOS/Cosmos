using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network
{
    // for more info, http://standards.ieee.org/about/get/802/802.3.html
    public class EthernetPacket
    {
        protected byte[] mRawData;
        protected MACAddress srcMAC;
        protected MACAddress destMAC;
        protected UInt16 aEtherType;

        protected EthernetPacket()
        { }

        protected EthernetPacket(byte[] rawData)
        {
            mRawData = rawData;
            initFields();
        }

        protected virtual void initFields()
        {
            destMAC = new MACAddress(mRawData, 0);
            srcMAC = new MACAddress(mRawData, 6);
            aEtherType = (UInt16)((mRawData[12] << 8) | mRawData[13]);
        }

        protected EthernetPacket(UInt16 type, int packet_size)
            : this(MACAddress.None, MACAddress.None, type, packet_size)
        {
        }

        protected EthernetPacket(MACAddress dest, MACAddress src, UInt16 type, int packet_size)
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

        internal MACAddress SourceMAC
        {
            get { return this.srcMAC; }
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    mRawData[6 + i] = value.bytes[i];
                }
                initFields();
            }
        }
        internal MACAddress DestinationMAC
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

        public byte[] GetBytes()
        {
            return this.mRawData;
        }

        public byte[] RawData
        {
            get { return this.mRawData; }
        }

        /// <summary>
        /// Calculate any checksums
        /// </summary>
        public virtual void PrepareForSending()
        {

            
        }

        public override string ToString()
        {
            return "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + aEtherType;
        }
    }
}
