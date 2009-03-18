using System;
using HW = Cosmos.Hardware;

namespace Cosmos.Sys.Network.TCPIP.ARP
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
        {}

        protected override void initFields()
        {
            base.initFields();
            aHardwareType = (UInt16)((mRawData[14] << 8) | mRawData[15]);
            aProtocolType = (UInt16)((mRawData[16] << 8) | mRawData[17]);
            aHardwareLen = mRawData[18];
            aProtocolLen = mRawData[19];
            aOperation = (UInt16)((mRawData[20] << 8) | mRawData[21]);
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

            initFields();
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
    }
}
