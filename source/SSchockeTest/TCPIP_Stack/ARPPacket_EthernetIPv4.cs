using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public abstract class ARPPacket_EthernetIPv4 : ARPPacket
    {
        protected HW.Network.MACAddress mSenderMAC;
        protected HW.Network.MACAddress mTargetMAC;
        protected IPv4Address mSenderIP;
        protected IPv4Address mTargetIP;

        public ARPPacket_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {
            mSenderMAC = new HW.Network.MACAddress(rawData, 22);
            mSenderIP = new IPv4Address(rawData, 28);
            mTargetMAC = new HW.Network.MACAddress(rawData, 32);
            mTargetIP = new IPv4Address(rawData, 38);
        }

        protected ARPPacket_EthernetIPv4(HW.Network.MACAddress dest, HW.Network.MACAddress src, UInt16 operation, 
            HW.Network.MACAddress senderMAC, IPv4Address senderIP, HW.Network.MACAddress targetMAC, IPv4Address targetIP, int packet_size)
            : base(dest, src, 1, 0x0800, 6, 4, operation, packet_size)
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
        }

        public HW.Network.MACAddress SenderMAC
        {
            get { return this.mSenderMAC; }
        }
        public HW.Network.MACAddress TargetMAC
        {
            get { return this.mTargetMAC; }
        }
        public IPv4Address SenderIP
        {
            get { return this.mSenderIP; }
        }
        public IPv4Address TargetIP
        {
            get { return this.mTargetIP; }
        }
    }
}
