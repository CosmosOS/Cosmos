using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Represents an ICMP packet.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="IPPacket"/>.
    /// </remarks>
    public class ICMPPacket : IPPacket
    {
        protected byte icmpType;
        protected byte icmpCode;
        protected ushort icmpCRC;

        /// <summary>
        /// Handles an ICMP packet.
        /// </summary>
        /// <param name="packetData">The data of the packet.</param>
        /// <exception cref="ArgumentException">Thrown if packetData is invalid.</exception>
        internal static void ICMPHandler(byte[] packetData)
        {
            var icmpPacket = new ICMPPacket(packetData);
            switch (icmpPacket.ICMPType)
            {
                case 0:
                    var receiver = ICMPClient.GetClient(icmpPacket.SourceIP.Hash);
                    receiver?.ReceiveData(icmpPacket);
                    NetworkStack.Debugger.Send("Received ICMP Echo reply from " + icmpPacket.SourceIP.ToString());
                    break;
                case 8:
                    var request = new ICMPEchoRequest(packetData);
                    var reply = new ICMPEchoReply(request);
                    NetworkStack.Debugger.Send("Sending ICMP Echo reply to " + reply.DestinationIP.ToString());
                    OutgoingBuffer.AddPacket(reply);
                    NetworkStack.Update();
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPPacket"/> class.
        /// </summary>
        internal ICMPPacket()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();
            icmpType = RawData[DataOffset];
            icmpCode = RawData[DataOffset + 1];
            icmpCRC = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPPacket"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="type">Type.</param>
        /// <param name="code">Code.</param>
        /// <param name="id">ID.</param>
        /// <param name="seq">SEQ.</param>
        /// <param name="icmpDataSize">Data size.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ICMPPacket(Address source, Address dest, byte type, byte code, ushort id, ushort seq, ushort icmpDataSize)
            : base(icmpDataSize, 1, source, dest, 0x00)
        {
            RawData[DataOffset] = type;
            RawData[DataOffset + 1] = code;
            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            RawData[DataOffset + 4] = (byte)((id >> 8) & 0xFF);
            RawData[DataOffset + 5] = (byte)((id >> 0) & 0xFF);
            RawData[DataOffset + 6] = (byte)((seq >> 8) & 0xFF);
            RawData[DataOffset + 7] = (byte)((seq >> 0) & 0xFF);

            icmpCRC = CalcICMPCRC((ushort)icmpDataSize);

            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
            InitializeFields();
        }

        /// <summary>
        /// Calculates the ICMP CRC3.
        /// </summary>
        /// <param name="length">The length of the packet.</param>
        protected ushort CalcICMPCRC(ushort length)
        {
            return CalcOcCRC(DataOffset, length);
        }

        /// <summary>
        /// The ICMP packet type.
        /// </summary>
        internal byte ICMPType => icmpType;

        /// <summary>
        /// The ICMP packet code.
        /// </summary>
        internal byte ICMPCode => icmpCode;

        /// <summary>
        /// The ICMP packet CRC.
        /// </summary>
        internal ushort ICMPCRC => icmpCRC;

        /// <summary>
        /// The ICMP packet data length.
        /// </summary>
        internal ushort ICMPDataLength => (ushort)(DataLength - 8);

        /// <summary>
        /// Returns the ICMP packet data.
        /// </summary>
        internal byte[] GetICMPData()
        {
            byte[] data = new byte[ICMPDataLength];

            for (int b = 0; b < ICMPDataLength; b++)
            {
                data[b] = RawData[DataOffset + 8 + b];
            }

            return data;
        }

        public override string ToString()
        {
            return "ICMP Packet Src=" + SourceIP + ", Dest=" + DestinationIP + ", Type=" + icmpType + ", Code=" + icmpCode;
        }
    }

    /// <summary>
    /// Represents an ICMP echo request packet.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="ICMPPacket"/>.
    /// </remarks>
    internal class ICMPEchoRequest : ICMPPacket
    {
        protected ushort icmpID;
        protected ushort icmpSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoRequest"/> class.
        /// </summary>
        internal ICMPEchoRequest()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoRequest"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPEchoRequest(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoRequest"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="id">ID.</param>
        /// <param name="sequence">Sequence.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ICMPEchoRequest(Address source, Address dest, ushort id, ushort sequence)
            : base(source, dest, 8, 0, id, sequence, 40)
        {
            for (int b = 8; b < ICMPDataLength; b++)
            {
                RawData[DataOffset + b] = (byte)b;
            }

            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((ushort)(ICMPDataLength + 8));
            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();
            icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        /// <summary>
        /// The ICMP packet ID.
        /// </summary>
        internal ushort ICMPID => icmpID;

        /// <summary>
        /// The ICMP packet Sequence.
        /// </summary>
        internal ushort ICMPSequence => icmpSequence;

        public override string ToString()
        {
            return "ICMP Echo Request Src=" + SourceIP + ", Dest=" + DestinationIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }

    /// <summary>
    /// Represents an ICMP echo reply packet.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="ICMPPacket"/>.
    /// </remarks>
    internal class ICMPEchoReply : ICMPPacket
    {
        protected ushort icmpID;
        protected ushort icmpSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        internal ICMPEchoReply()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal ICMPEchoReply(byte[] rawData)
            : base(rawData)
        {
        }

        protected override void InitializeFields()
        {
            //Sys.Console.WriteLine("ICMPEchoReply.InitFields() called;");
            base.InitializeFields();
            icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
            icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPEchoReply"/> class.
        /// </summary>
        /// <param name="request">ICMP echo request.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal ICMPEchoReply(ICMPEchoRequest request)
            : base(request.DestinationIP, request.SourceIP, 0, 0, request.ICMPID, request.ICMPSequence, (ushort)request.ICMPDataLength)
        {
            for (int b = 0; b < ICMPDataLength; b++)
            {
                RawData[DataOffset + 8 + b] = request.RawData[DataOffset + 8 + b];
            }

            RawData[DataOffset + 2] = 0x00;
            RawData[DataOffset + 3] = 0x00;
            icmpCRC = CalcICMPCRC((ushort)(ICMPDataLength + 8));
            RawData[DataOffset + 2] = (byte)((icmpCRC >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((icmpCRC >> 0) & 0xFF);
        }

        /// <summary>
        /// The ICMP packet ID.
        /// </summary>
        internal ushort ICMPID => icmpID;

        /// <summary>
        /// The ICMP packet sequence.
        /// </summary>
        internal ushort ICMPSequence => icmpSequence;

        public override string ToString()
        {
            return "ICMP Echo Reply Src=" + SourceIP + ", Dest=" + DestinationIP + ", ID=" + icmpID + ", Sequence=" + icmpSequence;
        }
    }
}
