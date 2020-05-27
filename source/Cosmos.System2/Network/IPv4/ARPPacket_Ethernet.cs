using System;
using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;
using Sys = System;

namespace Cosmos.System.Network.IPv4
{
    internal abstract class ARPPacket_Ethernet : ARPPacket
    {
        protected MACAddress mSenderMAC;
        protected MACAddress mTargetMAC;
        protected Address mSenderIP;
        protected Address mTargetIP;

        internal ARPPacket_Ethernet()
            : base()
        { }

        internal ARPPacket_Ethernet(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();
            mSenderMAC = new MACAddress(RawData, 22);
            mSenderIP = new Address(RawData, 28);
            if (SenderIP == null)
            {
                NetworkStack.debugger.Send("But its already null again");
            }
            mTargetMAC = new MACAddress(RawData, 32);
            mTargetIP = new Address(RawData, 38);
        }

        protected ARPPacket_Ethernet(UInt16 operation, MACAddress senderMAC, Address senderIP,
            MACAddress targetMAC, Address targetIP, int packet_size, MACAddress arpTargetMAC)
            : base(targetMAC, senderMAC, 1, 0x0800, 6, 4, operation, packet_size)
        {
            for (int i = 0; i < 6; i++)
            {
                RawData[22 + i] = senderMAC.bytes[i];
                RawData[32 + i] = arpTargetMAC.bytes[i];
            }
            for (int i = 0; i < 4; i++)
            {
                RawData[28 + i] = senderIP.address[i];
                RawData[38 + i] = targetIP.address[i];
            }

            initFields();
        }

        internal MACAddress SenderMAC
        {
            get { return this.mSenderMAC; }
        }
        internal MACAddress TargetMAC
        {
            get { return this.mTargetMAC; }
        }
        internal Address SenderIP
        {
            get { return this.mSenderIP; }
        }
        internal Address TargetIP
        {
            get { return this.mTargetIP; }
        }

        public override string ToString()
        {
            return "IPv4 Ethernet ARP Packet SenderMAC=" + mSenderMAC + ", TargetMAC=" + mTargetMAC + ", SenderIP=" + mSenderIP +
                ", TargetIP=" + mTargetIP + ", Operation=" + aOperation;
        }
    }

    internal class ARPReply_Ethernet : ARPPacket_Ethernet
    {
        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ARPReply_Ethernet();
        }

        internal ARPReply_Ethernet()
            : base()
        { }

        internal ARPReply_Ethernet(byte[] rawData)
            : base(rawData)
        { }

        internal ARPReply_Ethernet(MACAddress ourMAC, Address ourIP, MACAddress targetMAC, Address targetIP)
            : base(2, ourMAC, ourIP, targetMAC, targetIP, 42, MACAddress.None)
        { }

        public override string ToString()
        {
            return "ARP Reply Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }
    internal class ARPRequest_Ethernet : ARPPacket_Ethernet
    {
        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ARPRequest_Ethernet();
        }

        internal ARPRequest_Ethernet()
            : base()
        { }

        internal ARPRequest_Ethernet(byte[] rawData)
            : base(rawData)
        {
            if (SenderIP == null)
            {
                NetworkStack.debugger.Send("In ARPRequest_Ethernet, SenderIP is null!");
            }
        }

        internal ARPRequest_Ethernet(MACAddress ourMAC, Address ourIP, MACAddress targetMAC, Address targetIP, MACAddress arpTargetMAC)
            : base(1, ourMAC, ourIP, targetMAC, targetIP, 42, arpTargetMAC)
        { }

        public override string ToString()
        {
            return "ARP Request Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }
}
