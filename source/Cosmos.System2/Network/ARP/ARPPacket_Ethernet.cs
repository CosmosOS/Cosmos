/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ARP Packet ethernet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.IPv4;
using Sys = System;

namespace Cosmos.System.Network.ARP
{
    /// <summary>
    /// ARPPacket_Ethernet abstract class. See also: <seealso cref="ARPPacket"/>
    /// </summary>
    internal abstract class ARPPacket_Ethernet : ARPPacket
    {
        /// <summary>
        /// Sender MAC address.
        /// </summary>
        protected MACAddress mSenderMAC;
        /// <summary>
        /// Target MAC address.
        /// </summary>
        protected MACAddress mTargetMAC;
        /// <summary>
        /// Sender IP address.
        /// </summary>
        protected Address mSenderIP;
        /// <summary>
        /// Target IP address.
        /// </summary>
        protected Address mTargetIP;

        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        internal ARPPacket_Ethernet()
            : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ARPPacket_Ethernet(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Init ARPPacket_Ethernet fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitFields()
        {
            base.InitFields();
            mSenderMAC = new MACAddress(RawData, 22);
            mSenderIP = new Address(RawData, 28);
            if (SenderIP == null)
            {
                Global.mDebugger.Send("But its already null again");
            }
            mTargetMAC = new MACAddress(RawData, 32);
            mTargetIP = new Address(RawData, 38);
        }

        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        /// <param name="operation">Operation.</param>
        /// <param name="senderMAC">Source MAC address.</param>
        /// <param name="senderIP">Source IP address.</param>
        /// <param name="targetMAC">Destination MAC address.</param>
        /// <param name="targetIP">Destination IP address.</param>
        /// <param name="packet_size">Packet size.</param>
        /// <param name="arpTargetMAC">ARP destination MAC address.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
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

            InitFields();
        }

        /// <summary>
        /// Get sender MAC.
        /// </summary>
        internal MACAddress SenderMAC
        {
            get { return this.mSenderMAC; }
        }

        /// <summary>
        /// Get target MAC.
        /// </summary>
        internal MACAddress TargetMAC
        {
            get { return this.mTargetMAC; }
        }

        /// <summary>
        /// Get sender IP.
        /// </summary>
        internal Address SenderIP
        {
            get { return this.mSenderIP; }
        }

        /// <summary>
        /// Get target IP.
        /// </summary>
        internal Address TargetIP
        {
            get { return this.mTargetIP; }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "IPv4 Ethernet ARP Packet SenderMAC=" + mSenderMAC + ", TargetMAC=" + mTargetMAC + ", SenderIP=" + mSenderIP +
                ", TargetIP=" + mTargetIP + ", Operation=" + aOperation;
        }
    }

    /// <summary>
    /// ARPRequest_Ethernet class. See also: <seealso cref="ARPReply_Ethernet"/>.
    /// </summary>
    internal class ARPReply_Ethernet : ARPPacket_Ethernet
    {
        /// <summary>
        /// Create new instance of the <see cref="ARPReply_Ethernet"/> class.
        /// </summary>
        internal ARPReply_Ethernet()
            : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="ARPReply_Ethernet"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ARPReply_Ethernet(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Create new instance of the <see cref="ARPReply_Ethernet"/> class.
        /// </summary>
        /// <param name="ourMAC">Source MAC address.</param>
        /// <param name="ourIP">Source IP address.</param>
        /// <param name="targetMAC">Destination MAC address.</param>
        /// <param name="targetIP">Destination IP address.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ARPReply_Ethernet(MACAddress ourMAC, Address ourIP, MACAddress targetMAC, Address targetIP)
            : base(2, ourMAC, ourIP, targetMAC, targetIP, 42, MACAddress.None)
        { }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ARP Reply Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }

    /// <summary>
    /// ARPRequest_Ethernet class. See also: <seealso cref="ARPPacket_Ethernet"/>.
    /// </summary>
    internal class ARPRequest_Ethernet : ARPPacket_Ethernet
    {
        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        internal ARPRequest_Ethernet()
            : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ARPRequest_Ethernet(byte[] rawData)
            : base(rawData)
        {
            if (SenderIP == null)
            {
                Global.mDebugger.Send("In ARPRequest_Ethernet, SenderIP is null!");
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="ARPRequest_Ethernet"/> class.
        /// </summary>
        /// <param name="ourMAC">Source MAC address.</param>
        /// <param name="ourIP">Source IP address.</param>
        /// <param name="targetMAC">Destination MAC address.</param>
        /// <param name="targetIP">Destination IP address.</param>
        /// <param name="arpTargetMAC">ARP destination MAC address.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ARPRequest_Ethernet(MACAddress ourMAC, Address ourIP, MACAddress targetMAC, Address targetIP, MACAddress arpTargetMAC)
            : base(1, ourMAC, ourIP, targetMAC, targetIP, 42, arpTargetMAC)
        { }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ARP Request Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }
}
