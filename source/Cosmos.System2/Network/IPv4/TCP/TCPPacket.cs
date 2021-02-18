/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4.TCP
{

    /// <summary>
    /// DHCP Option
    /// </summary>
    public class TCPOption
    {
        public byte Kind { get; set; }
        public byte Length { get; set; }
        public byte[] Data { get; set; }
    }

    public class TCPPacket : IPPacket
    {
        protected ushort sourcePort;
        protected ushort destinationPort;
        protected ushort optionLen;
        protected ulong sequenceNumber;
        protected ulong ackNumber;
        protected int headerLenght;
        protected int flags;
        protected int wsValue;
        protected int checksum;
        protected int urgentPointer;

        protected List<TCPOption> options = null;

        public bool SYN;
        public bool ACK;
        public bool FIN;
        public bool PSH;
        public bool RST;

        

        internal static void TCPHandler(byte[] packetData)
        {
            var packet = new TCPPacket(packetData);

            Global.mDebugger.Send("[Received] TCP packet from " + packet.SourceIP.ToString() + ":" + packet.SourcePort.ToString());

            var receiver = TcpClient.GetClient(packet.DestinationPort);
            if (receiver != null)
            {
                receiver.ReceiveData(packet);
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new TCPPacket();
        }

        internal TCPPacket()
            : base()
        { }

        internal TCPPacket(byte[] rawData)
            : base(rawData)
        { }

        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, ushort Flags,
            ushort WSValue, ushort UrgentPointer, ushort len)
            : base((ushort)(20 + len), 6, source, dest, 0x40)
        {
            optionLen = len;

            //ports
            RawData[DataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            RawData[DataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

            RawData[DataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            RawData[DataOffset + 3] = (byte)((destPort >> 0) & 0xFF);

            //sequencenumber
            RawData[DataOffset + 4] = (byte)((sequencenumber >> 24) & 0xFF);
            RawData[DataOffset + 5] = (byte)((sequencenumber >> 16) & 0xFF);
            RawData[DataOffset + 6] = (byte)((sequencenumber >> 8) & 0xFF);
            RawData[DataOffset + 7] = (byte)((sequencenumber >> 0) & 0xFF);

            //Acknowledgment number
            RawData[DataOffset + 8] = (byte)((acknowledgmentnb >> 24) & 0xFF);
            RawData[DataOffset + 9] = (byte)((acknowledgmentnb >> 16) & 0xFF);
            RawData[DataOffset + 10] = (byte)((acknowledgmentnb >> 8) & 0xFF);
            RawData[DataOffset + 11] = (byte)((acknowledgmentnb >> 0) & 0xFF);

            //Header lenght
            RawData[DataOffset + 12] = (byte)(((Headerlenght >> 0) & 0xFF) * 4);

            //Flags
            RawData[DataOffset + 13] = (byte)((Flags >> 0) & 0xFF);

            //Window size value
            RawData[DataOffset + 14] = (byte)((WSValue >> 8) & 0xFF);
            RawData[DataOffset + 15] = (byte)((WSValue >> 0) & 0xFF);

            //Urgent Pointer
            RawData[DataOffset + 18] = (byte)((UrgentPointer >> 8) & 0xFF);
            RawData[DataOffset + 19] = (byte)((UrgentPointer >> 0) & 0xFF);

            byte[] header = MakeHeader(source.address, dest.address, (ushort)(20 + len), srcPort, destPort,
            sequencenumber, acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer);

            ushort calculatedcrc = CalcOcCRC(header, 0, header.Length);

            //Checksum
            RawData[DataOffset + 16] = (byte)((calculatedcrc >> 8) & 0xFF);
            RawData[DataOffset + 17] = (byte)((calculatedcrc >> 0) & 0xFF);

            InitFields();
        }

        protected override void InitFields()
        {
            base.InitFields();
            sourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            destinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            sequenceNumber = (uint)((RawData[DataOffset + 4] << 24) | (RawData[DataOffset + 5] << 16) | (RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
            ackNumber = (uint)((RawData[DataOffset + 8] << 24) | (RawData[DataOffset + 9] << 16) | (RawData[DataOffset + 10] << 8) | RawData[DataOffset + 11]);
            headerLenght = RawData[DataOffset + 12];
            flags = RawData[DataOffset + 13];
            wsValue = (ushort)((RawData[DataOffset + 14] << 8) | RawData[DataOffset + 15]);
            checksum = (ushort)((RawData[DataOffset + 16] << 8) | RawData[DataOffset + 17]);
            urgentPointer = (ushort)((RawData[DataOffset + 18] << 8) | RawData[DataOffset + 19]);

            SYN = (RawData[47] & (1 << 1)) != 0;
            ACK = (RawData[47] & (1 << 4)) != 0;
            FIN = (RawData[47] & (1 << 0)) != 0;
            PSH = (RawData[47] & (1 << 3)) != 0;
            RST = (RawData[47] & (1 << 2)) != 0;

            //options
            if (RawData[DataOffset + 20] != 0)
            {
                options = new List<TCPOption>();

                for (int i = 0; i < RawData.Length - (DataOffset + 20); i++)
                {
                    var option = new TCPOption();
                    option.Kind = RawData[DataOffset + 20 + i];

                    if (option.Kind != 1) //NOP
                    {
                        option.Length = RawData[DataOffset + 20 + i + 1];

                        if (option.Length != 2)
                        {
                            option.Data = new byte[option.Length - 2];
                            for (int j = 0; j < option.Length - 2; j++)
                            {
                                option.Data[j] = RawData[DataOffset + 20 + i + 2 + j];
                            }
                        }

                        options.Add(option);

                        i += option.Length - 1;
                    }
                }
            }
        }

        internal void AddOption(TCPOption option)
        {
            throw new NotImplementedException();
        }

        internal void AddOption(byte[] raw)
        {
            if (raw.Length > optionLen)
            {
                throw new Exception("Raw option is larger than max value.");
            }
            for (int i = 0; i < raw.Length; i++)
            {
                RawData[DataOffset + 20 + i] = raw[i];
            }
        }

        internal byte[] MakeHeader(byte[] sourceIP, byte[] destIP, ushort tcpLen,
        ushort sourcePort, ushort destPort, ulong sequencenumber, ulong acknowledgmentnb,
        int Headerlenght, int Flags, int WSValue, int UrgentPointer)
        {
            byte[] header = new byte[30 + RawData.Length - (DataOffset + 20)];

            header[0] = sourceIP[0];
            header[1] = sourceIP[1];
            header[2] = sourceIP[2];
            header[3] = sourceIP[3];

            header[4] = destIP[0];
            header[5] = destIP[1];
            header[6] = destIP[2];
            header[7] = destIP[3];

            header[8] = 0x00;

            header[9] = 0x06;

            header[10] = (byte)((tcpLen >> 8) & 0xFF);
            header[11] = (byte)((tcpLen >> 0) & 0xFF);

            header[12] = (byte)((sourcePort >> 8) & 0xFF);
            header[13] = (byte)((sourcePort >> 0) & 0xFF);

            header[14] = (byte)((destPort >> 8) & 0xFF);
            header[15] = (byte)((destPort >> 0) & 0xFF);

            //sequencenumber
            header[16] = (byte)((sequencenumber >> 24) & 0xFF);
            header[17] = (byte)((sequencenumber >> 16) & 0xFF);
            header[18] = (byte)((sequencenumber >> 8) & 0xFF);
            header[19] = (byte)((sequencenumber >> 0) & 0xFF);

            //Acknowledgment number
            header[20] = (byte)((acknowledgmentnb >> 24) & 0xFF);
            header[21] = (byte)((acknowledgmentnb >> 16) & 0xFF);
            header[22] = (byte)((acknowledgmentnb >> 8) & 0xFF);
            header[23] = (byte)((acknowledgmentnb >> 0) & 0xFF);

            //Header lenght
            header[24] = (byte)((Headerlenght >> 0) & 0xFF);

            //Flags
            header[25] = (byte)((Flags >> 0) & 0xFF);

            //Window size value
            header[26] = (byte)((WSValue >> 8) & 0xFF);
            header[27] = (byte)((WSValue >> 0) & 0xFF);

            //Urgent Pointer
            header[28] = (byte)((UrgentPointer >> 8) & 0xFF);
            header[29] = (byte)((UrgentPointer >> 0) & 0xFF);

            for (int i = 0; i < RawData.Length - (DataOffset + 20); i++)
            {
                header[30 + i] = RawData[DataOffset + 20 + i];
            }

            return header;
        }

        internal ushort DestinationPort
        {
            get { return destinationPort; }
        }
        internal ushort SourcePort
        {
            get { return sourcePort; }
        }
        internal ulong SequenceNumber
        {
            get { return sequenceNumber; }
        }

        public override string ToString()
        {
            return "TCP Packet Src=" + SourceIP + ":" + sourcePort + ", Dest=" + DestinationIP + ":" + destinationPort;
        }
    }

    public class TCPacketSyn : TCPPacket
    {
        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new TCPacketSyn();
        }

        internal TCPacketSyn()
            : base()
        { }

        public TCPacketSyn(Address Source, Address Destination, ushort SourcePort,
            ushort DestinationPort, ulong SequenceNumber, ulong ACKNumber,
            ushort Flags, ushort WSValue, List<TCPOption> options, ushort optionslen)
            : base(Source, Destination, SourcePort, DestinationPort, SequenceNumber,
                  ACKNumber, 0x50, Flags, WSValue, 0x0000, optionslen)
        {
            int counter = 0;
            foreach (var option in options)
            {
                RawData[DataOffset + 20 + counter] = option.Kind;

                if (option.Kind != 1) //NOP
                {
                    RawData[DataOffset + 20 + counter + 1] = option.Length;

                    if (option.Length != 2)
                    {
                        for (int j = 0; j < option.Length - 2; j++)
                        {
                            RawData[DataOffset + 20 + counter + 2 + j] = option.Data[j];
                        }
                    }
                }

                counter += option.Length;
            }
        }

        protected override void InitFields()
        {
            base.InitFields();
        }
    }
}
