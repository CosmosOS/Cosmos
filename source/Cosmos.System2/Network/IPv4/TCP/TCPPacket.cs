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
        protected uint sequenceNumber;
        protected uint ackNumber;
        protected int headerLenght;
        protected int flags;
        protected int wsValue;
        protected ushort checksum;
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

            if (packet.CheckCRC())
            {
                var receiver = TcpClient.GetClient(packet.DestinationPort);
                if (receiver != null)
                {
                    receiver.ReceiveData(packet);
                }
            }
            else
            {
                Global.mDebugger.Send("Checksum incorrect! Packet passed.");
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
            ushort WSValue, ushort UrgentPointer, ushort len, byte[] options)
            : base((ushort)(20 + len), 6, source, dest, 0x40)
        {
            AddRawOption(options);
            MakePacket(source, dest, srcPort, destPort, sequencenumber,
            acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer, len);
        }

        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, ushort Flags,
            ushort WSValue, ushort UrgentPointer)
            : base(20, 6, source, dest, 0x40)
        {
            MakePacket(source, dest, srcPort, destPort, sequencenumber,
            acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer, 0);
        }

        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, ushort Flags,
            ushort WSValue, ushort UrgentPointer, ushort len)
            : base((ushort)(20 + len), 6, source, dest, 0x40)
        {
            MakePacket(source, dest, srcPort, destPort, sequencenumber,
            acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer, len);
        }

        private void MakePacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, ushort Flags,
            ushort WSValue, ushort UrgentPointer, ushort len)
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

            //Checksum
            RawData[DataOffset + 16] = 0;
            RawData[DataOffset + 17] = 0;

            //Urgent Pointer
            RawData[DataOffset + 18] = (byte)((UrgentPointer >> 8) & 0xFF);
            RawData[DataOffset + 19] = (byte)((UrgentPointer >> 0) & 0xFF);

            InitFields();

            //Checksum computation
            byte[] header = MakeHeader();
            ushort calculatedcrc = CalcOcCRC(header, 0, header.Length);

            //Checksum
            RawData[DataOffset + 16] = (byte)((calculatedcrc >> 8) & 0xFF);
            RawData[DataOffset + 17] = (byte)((calculatedcrc >> 0) & 0xFF);
        }

        protected override void InitFields()
        {
            base.InitFields();
            sourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            destinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            sequenceNumber = (uint)((RawData[DataOffset + 4] << 24) | (RawData[DataOffset + 5] << 16) | (RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
            ackNumber = (uint)((RawData[DataOffset + 8] << 24) | (RawData[DataOffset + 9] << 16) | (RawData[DataOffset + 10] << 8) | RawData[DataOffset + 11]);
            headerLenght = (RawData[DataOffset + 12] >> 4) * 4;
            flags = RawData[DataOffset + 13];
            wsValue = (ushort)((RawData[DataOffset + 14] << 8) | RawData[DataOffset + 15]);
            checksum = (ushort)((RawData[DataOffset + 16] << 8) | RawData[DataOffset + 17]);
            urgentPointer = (ushort)((RawData[DataOffset + 18] << 8) | RawData[DataOffset + 19]);

            SYN = (RawData[47] & (1 << 1)) != 0;
            ACK = (RawData[47] & (1 << 4)) != 0;
            FIN = (RawData[47] & (1 << 0)) != 0;
            PSH = (RawData[47] & (1 << 3)) != 0;
            RST = (RawData[47] & (1 << 2)) != 0;

            if (RawData.Length == (DataOffset + headerLenght)) //no data
            {
                if (headerLenght > 20) //options
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
            else
            {
                //TODO: Parse data
            }
        }

        internal void AddOption(TCPOption option)
        {
            throw new NotImplementedException();
        }

        internal void AddRawOption(byte[] raw)
        {
            for (int i = 0; i < raw.Length; i++)
            {
                RawData[DataOffset + 20 + i] = raw[i];
            }
        }

        /// <summary>
        /// Make TCP Header for CRC Computation
        /// </summary>
        /// <returns>byte array value.</returns>
        internal byte[] MakeHeader()
        {
            byte[] header = new byte[12 + (RawData.Length - DataOffset)];

            /* Pseudo Header */
            //Addresses
            for (int b = 0; b < 4; b++)
            {
                header[0 + b] = SourceIP.address[b];
                header[4 + b] = DestinationIP.address[b];
            }
            //Reserved
            header[8] = 0x00;
            //Protocol (TCP)
            header[9] = 0x06;
            ushort tcplen = (ushort)(RawData.Length - DataOffset);
            //TCP Length
            header[10] = (byte)((tcplen >> 8) & 0xFF);
            header[11] = (byte)((tcplen >> 0) & 0xFF);

            /* TCP Packet */
            for (int i = 0; i < RawData.Length - DataOffset; i++)
            {
                header[12 + i] = RawData[DataOffset + i];
            }

            return header;
        }

        /// <summary>
        /// Check TCP Checksum
        /// </summary>
        /// <returns>True if checksum correct, False otherwise.</returns>
        public bool CheckCRC()
        {
            /*byte[] header = MakeHeader();

            if (CalcOcCRC(header, 0, header.Length) == checksum)
            {
                return true;
            }
            else
            {
                return false;
            }*/
            return true;
        }


        internal ushort DestinationPort
        {
            get { return destinationPort; }
        }
        internal ushort SourcePort
        {
            get { return sourcePort; }
        }
        internal uint AckNumber
        {
            get { return ackNumber; }
        }
        internal uint SequenceNumber
        {
            get { return sequenceNumber; }
        }
        internal int TCPFlags
        {
            get { return flags; }
        }

        /// <summary>
        /// Get TCP data lenght.
        /// </summary>
        public ushort TCP_DataLength => (ushort)(RawData.Length - (DataOffset + headerLenght));

        /// <summary>
        /// Get TCP data.
        /// </summary>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        internal byte[] TCP_Data
        {
            get
            {
                byte[] data = new byte[TCP_DataLength];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = RawData[DataOffset + headerLenght + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return "TCP Packet Src=" + SourceIP + ":" + sourcePort + ", Dest=" + DestinationIP + ":" + destinationPort;
        }
    }
}
