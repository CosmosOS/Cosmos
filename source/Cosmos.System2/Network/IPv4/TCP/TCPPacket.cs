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
    /// TCP Flags
    /// </summary>
    public enum Flags : byte
    {
        /// <summary>
        /// No more data from sender.
        /// </summary>
        FIN = 1 << 0,

        /// <summary>
        /// Synchronize sequence numbers.
        /// </summary>
        SYN = 1 << 1,

        /// <summary>
        /// Reset the connection.
        /// </summary>
        RST = 1 << 2,

        /// <summary>
        /// Push Function.
        /// </summary>
        PSH = 1 << 3,

        /// <summary>
        /// Acknowledgment field significant.
        /// </summary>
        ACK = 1 << 4,

        /// <summary>
        /// Urgent Pointer field significant.
        /// </summary>
        URG = 1 << 5
    }

    /// <summary>
    /// TCP Option
    /// </summary>
    public class TCPOption
    {
        public byte Kind { get; set; }
        public byte Length { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// TCP Packet Class
    /// </summary>
    public class TCPPacket : IPPacket
    {
        /// <summary>
        /// TCP handler.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="sys.ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.OverflowException">Thrown if packetData array length is greater than Int32.MaxValue.</exception>
        internal static void TCPHandler(byte[] packetData)
        {
            var packet = new TCPPacket(packetData);

            if (packet.CheckCRC())
            {
                var connection = Tcp.GetConnection(packet.DestinationPort, packet.SourcePort, packet.DestinationIP, packet.SourceIP);

                if (connection != null)
                {
                    connection.ReceiveData(packet);
                }
            }
            else
            {
                Global.mDebugger.Send("Checksum incorrect! Packet passed.");
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="TCPPacket"/> class.
        /// </summary>
        internal TCPPacket()
            : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="TCPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public TCPPacket(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Create new instance of the <see cref="TCPPacket"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="srcPort">Source port.</param>
        /// <param name="destPort">Destination port.</param>
        /// <param name="sequencenumber">Sequence number.</param>
        /// <param name="acknowledgmentnb">Acknowledgment Number.</param>
        /// <param name="Headerlenght">TCP Header length.</param>
        /// <param name="Flags">TCP flags.</param>
        /// <param name="WSValue">Windows size.</param>
        /// <param name="UrgentPointer">Urgent Pointer.</param>
        /// <param name="data">Raw data.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, byte Flags,
            ushort WSValue, ushort UrgentPointer, byte[] data)
            : base((ushort)(20 + data.Length), 6, source, dest, 0x40)
        {
            AddRawData(data);
            MakePacket(source, dest, srcPort, destPort, sequencenumber,
            acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer);
        }

        /// <summary>
        /// Create new instance of the <see cref="TCPPacket"/> class.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="srcPort">Source port.</param>
        /// <param name="destPort">Destination port.</param>
        /// <param name="sequencenumber">Sequence number.</param>
        /// <param name="acknowledgmentnb">Acknowledgment Number.</param>
        /// <param name="Headerlenght">TCP Header length.</param>
        /// <param name="Flags">TCP flags.</param>
        /// <param name="WSValue">Windows size.</param>
        /// <param name="UrgentPointer">Urgent Pointer.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        public TCPPacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, byte Flags,
            ushort WSValue, ushort UrgentPointer)
            : base(20, 6, source, dest, 0x40)
        {
            MakePacket(source, dest, srcPort, destPort, sequencenumber,
            acknowledgmentnb, Headerlenght, Flags, WSValue, UrgentPointer);
        }

        /// <summary>
        /// Make TCP Packet.
        /// </summary>
        /// <param name="source">Source address.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="srcPort">Source port.</param>
        /// <param name="destPort">Destination port.</param>
        /// <param name="sequencenumber">Sequence number.</param>
        /// <param name="acknowledgmentnb">Acknowledgment Number.</param>
        /// <param name="Headerlenght">TCP Header length.</param>
        /// <param name="Flags">TCP flags.</param>
        /// <param name="WSValue">Windows size.</param>
        /// <param name="UrgentPointer">Urgent Pointer.</param>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        private void MakePacket(Address source, Address dest, ushort srcPort, ushort destPort,
            ulong sequencenumber, ulong acknowledgmentnb, ushort Headerlenght, byte Flags,
            ushort WSValue, ushort UrgentPointer)
        {
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

        /// <summary>
        /// Init TCPPacket fields.
        /// </summary>
        /// <exception cref="sys.ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitFields()
        {
            base.InitFields();
            SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
            DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
            SequenceNumber = (uint)((RawData[DataOffset + 4] << 24) | (RawData[DataOffset + 5] << 16) | (RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
            AckNumber = (uint)((RawData[DataOffset + 8] << 24) | (RawData[DataOffset + 9] << 16) | (RawData[DataOffset + 10] << 8) | RawData[DataOffset + 11]);
            TCPHeaderLength = (byte)((RawData[DataOffset + 12] >> 4) * 4);
            TCPFlags = RawData[DataOffset + 13];
            WindowSize = (ushort)((RawData[DataOffset + 14] << 8) | RawData[DataOffset + 15]);
            Checksum = (ushort)((RawData[DataOffset + 16] << 8) | RawData[DataOffset + 17]);
            UrgentPointer = (ushort)((RawData[DataOffset + 18] << 8) | RawData[DataOffset + 19]);

            SYN = (RawData[47] & (byte)Flags.SYN) != 0;
            ACK = (RawData[47] & (byte)Flags.ACK) != 0;
            FIN = (RawData[47] & (byte)Flags.FIN) != 0;
            PSH = (RawData[47] & (byte)Flags.PSH) != 0;
            RST = (RawData[47] & (byte)Flags.RST) != 0;
            URG = (RawData[47] & (byte)Flags.URG) != 0;

            if (TCPHeaderLength > 20) //options
            {
                Options = new List<TCPOption>();

                for (int i = 0; i < TCP_DataLength; i++)
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

                        Options.Add(option);

                        i += option.Length - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Add Option to TCP Packet.
        /// </summary>
        /// <param name="option">TCP Option.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal void AddOption(TCPOption option)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add raw data to TCP Packet.
        /// </summary>
        /// <param name="raw">Raw data.</param>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        internal void AddRawData(byte[] raw)
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
            byte[] header = new byte[12 + TCPHeaderLength + TCP_DataLength];

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
            ushort tcplen = (ushort)(TCPHeaderLength + TCP_DataLength);
            //TCP Length
            header[10] = (byte)((tcplen >> 8) & 0xFF);
            header[11] = (byte)((tcplen >> 0) & 0xFF);

            /* TCP Packet */
            for (int i = 0; i < tcplen; i++)
            {
                header[12 + i] = RawData[DataOffset + i];
            }

            return header;
        }

        /// <summary>
        /// Check TCP Checksum
        /// </summary>
        /// <returns>True if checksum correct, False otherwise.</returns>
        private bool CheckCRC()
        {
            //byte[] header = MakeHeader();

            //return CalcOcCRC(header, 0, header.Length) == Checksum;

            return true;
        }

        /// <summary>
        /// TCP Options
        /// </summary>
        public List<TCPOption> Options { get; set; }

        /// <summary>
        /// Is SYN Flag set.
        /// </summary>
        internal bool SYN;
        /// <summary>
        /// Is ACK Flag set.
        /// </summary>
        internal bool ACK;
        /// <summary>
        /// Is FIN Flag set.
        /// </summary>
        internal bool FIN;
        /// <summary>
        /// Is PSH Flag set.
        /// </summary>
        internal bool PSH;
        /// <summary>
        /// Is RST Flag set.
        /// </summary>
        internal bool RST;
        /// <summary>
        /// Is URG Flag set.
        /// </summary>
        internal bool URG;

        /// <summary>
        /// Get destination port.
        /// </summary>
        public ushort DestinationPort { get; private set; }
        /// <summary>
        /// Get source port.
        /// </summary>
        public ushort SourcePort { get; private set; }
        /// <summary>
        /// Get acknowledge number.
        /// </summary>
        public uint AckNumber { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public uint SequenceNumber { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public byte TCPHeaderLength { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public byte TCPFlags { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public ushort WindowSize { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public ushort Checksum { get; private set; }
        /// <summary>
        /// Get sequence number.
        /// </summary>
        public ushort UrgentPointer { get; private set; }

        /// <summary>
        /// Get TCP data lenght.
        /// </summary>
        public ushort TCP_DataLength => (ushort)(IPLength - HeaderLength - TCPHeaderLength);

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
                    data[b] = RawData[DataOffset + TCPHeaderLength + b];
                }

                return data;
            }
        }

        /// <summary>
        /// Get string representation of TCP flags.
        /// </summary>
        /// <returns>string value.</returns>
        public string getFlags()
        {
            string flags = "";

            if (FIN)
            {
                flags += "FIN|";
            }
            if (SYN)
            {
                flags += "SYN|";
            }
            if (RST)
            {
                flags += "RST|";
            }
            if (PSH)
            {
                flags += "PSH|";
            }
            if (ACK)
            {
                flags += "ACK|";
            }
            if (URG)
            {
                flags += "URG|";
            }

            return flags.Remove(flags.Length - 1);
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return $"TCP Packet {SourceIP}:{SourcePort} -> {DestinationIP}:{DestinationPort} (flags={getFlags()}, seq={SequenceNumber}, ack={AckNumber})";
        }
    }
}
