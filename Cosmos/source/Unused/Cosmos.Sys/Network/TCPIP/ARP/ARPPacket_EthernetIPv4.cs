using System;
using HW = Cosmos.Hardware2;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    internal abstract class ARPPacket_EthernetIPv4 : ARPPacket
    {
        protected HW.Network.MACAddress mSenderMAC;
        protected HW.Network.MACAddress mTargetMAC;
        protected IPv4Address mSenderIP;
        protected IPv4Address mTargetIP;

        internal ARPPacket_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}

        protected override void initFields()
        {
            base.initFields();
            mSenderMAC = new HW.Network.MACAddress(mRawData, 22);
            mSenderIP = new IPv4Address(mRawData, 28);
            mTargetMAC = new HW.Network.MACAddress(mRawData, 32);
            mTargetIP = new IPv4Address(mRawData, 38);
        }

        protected ARPPacket_EthernetIPv4(UInt16 operation, HW.Network.MACAddress senderMAC, IPv4Address senderIP, 
            HW.Network.MACAddress targetMAC, IPv4Address targetIP, int packet_size)
            : base(targetMAC, senderMAC, 1, 0x0800, 6, 4, operation, packet_size)
        {
            for (int i = 0; i < 6; i++)
            {
                mRawData[22 + i] = senderMAC.bytes[i];
                mRawData[32 + i] = targetMAC.bytes[i];
            }
            for (int i = 0; i < 4; i++)
            {
                mRawData[28 + i] = senderIP.address[i];
                mRawData[38 + i] = targetIP.address[i];
            }

            initFields();
        }

        internal HW.Network.MACAddress SenderMAC
        {
            get { return this.mSenderMAC; }
        }
        internal HW.Network.MACAddress TargetMAC
        {
            get { return this.mTargetMAC; }
        }
        internal IPv4Address SenderIP
        {
            get { return this.mSenderIP; }
        }
        internal IPv4Address TargetIP
        {
            get { return this.mTargetIP; }
        }

        public override string ToString()
        {
            return "IPv4 Ethernet ARP Packet SenderMAC=" + mSenderMAC + ", TargetMAC=" + mTargetMAC + ", SenderIP=" + mSenderIP +
                ", TargetIP=" + mTargetIP + ", Operation=" + aOperation;
        }
    }
}
