using System;
using Sys = System;

namespace Cosmos.System.Network.IPv4
{
    internal class ICMPPacket : IPPacket
    {
        protected byte icmpType;
        protected byte icmpCode;
        protected UInt16 icmpCRC;

        internal static void ICMPHandler(byte[] packetData)
        {
            ICMPPacket icmp_packet = new ICMPPacket(packetData);
            switch (icmp_packet.ICMP_Type)
            {
                case 0:
                    ICMPEchoReply recvd_reply = new ICMPEchoReply(packetData);
                    Sys.Console.WriteLine("Received ICMP Echo reply from " + recvd_reply.SourceIP.ToString());
                    break;
                case 8:
                    ICMPEchoRequest request = new ICMPEchoRequest(packetData);
                    Sys.Console.WriteLine("Received " + request.ToString());
                    ICMPEchoReply reply = new ICMPEchoReply(request);
                    Sys.Console.WriteLine("Sending ICMP Echo reply to " + reply.DestinationIP.ToString());
                    OutgoingBuffer.AddPacket(reply);
                    break;
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new ICMPPacket();
        }

        internal ICMPPacket()
            : base()
        { }

        internal ICMPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPPacket.initFields() called;");
            base.initFields();
            icmpType = mRawData[this.dataOffset];
            icmpCode = mRawData[this.dataOffset + 1];
            icmpCRC = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
        }

        internal ICMPPacket(Address source, Address dest, byte type, byte code, UInt16 id, UInt16 seq, UInt16 icmpDataSize)
            : base(icmpDataSize, 1, source, dest)
        {
            mRawData[this.dataOffset] = type;
            mRawData[this.dataOffset + 1] = code;
            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            mRawData[this.dataOffset + 4] = (byte)((id >> 8) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((id >> 0) & 0xFF);
            mRawData[this.dataOffset + 6] = (byte)((seq >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((seq >> 0) & 0xFF);

            icmpCRC = CalcICMPCRC((UInt16)(icmpDataSize + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
            initFields();
        }

        protected UInt16 CalcICMPCRC(UInt16 length)
        {
            return CalcOcCRC(this.dataOffset, length);
        }

        internal byte ICMP_Type
        {
            get { return this.icmpType; }
        }
        internal byte ICMP_Code
        {
            get { return this.icmpCode; }
        }
        internal UInt16 ICMP_CRC
        {
            get { return this.icmpCRC; }
        }
        internal UInt16 ICMP_DataLength
        {
            get { return (UInt16)(this.DataLength - 8); }
        }

        internal byte[] GetICMPData()
        {
            byte[] data = new byte[ICMP_DataLength];

            for (int b = 0; b < ICMP_DataLength; b++)
            {
                data[b] = mRawData[this.dataOffset + 8 + b];
            }

            return data;
        }

        public override string ToString()
        {
            return "ICMP Packet Src=" + sourceIP + ", Dest=" + destIP + ", Type=" + icmpType + ", Code=" + icmpCode;
        }
    }

    internal class ICMPEchoRequest : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoRequest()
            : base()
        { }

        internal ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {
        }

        internal ICMPEchoRequest(Address source, Address dest, UInt16 id, UInt16 sequence)
            : base(source, dest, 8, 0, id, sequence, 40)
        {
            for (byte b = 8; b < this.ICMP_DataLength; b++)
            {
                mRawData[this.dataOffset + b] = b;
            }

            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ICMPEchoRequest();
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoRequest.initFields() called;");
            base.initFields();
            icmpID = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        internal UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        }

        public override string ToString()
        {
            return "ICMP Echo Request Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
    internal class ICMPEchoReply : ICMPPacket
    {
        protected UInt16 icmpID;
        protected UInt16 icmpSequence;

        internal ICMPEchoReply()
            : base()
        { }

        internal ICMPEchoReply(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public new static void VMTInclude()
        {
            new ICMPEchoReply();
        }

        protected override void initFields()
        {
            //Sys.Console.WriteLine("ICMPEchoReply.initFields() called;");
            base.initFields();
            icmpID = (UInt16)((mRawData[this.dataOffset + 4] << 8) | mRawData[this.dataOffset + 5]);
            icmpSequence = (UInt16)((mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
        }

        internal ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationIP, request.SourceIP, 0, 0,
                    request.ICMP_ID, request.ICMP_Sequence, (UInt16)(request.ICMP_DataLength + 8))
        {
            for (int b = 0; b < this.ICMP_DataLength; b++)
            {
                mRawData[this.dataOffset + 8 + b] = request.RawData[this.dataOffset + 8 + b];
            }

            mRawData[this.dataOffset + 2] = 0x00;
            mRawData[this.dataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((UInt16)(this.ICMP_DataLength + 8));
            mRawData[this.dataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        internal UInt16 ICMP_ID
        {
            get { return this.icmpID; }
        }
        internal UInt16 ICMP_Sequence
        {
            get { return this.icmpSequence; }
        }

        public override string ToString()
        {
            return "ICMP Echo Reply Src=" + sourceIP + ", Dest=" + destIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
