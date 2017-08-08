using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Sys.Network.TCPIP.TCP
{
    internal class TCPPacket : IPPacket
    {
        private const byte CWR = 0x80;
        private const byte ECE = 0x40;
        private const byte URG = 0x20;
        private const byte ACK = 0x10;
        private const byte PSH = 0x08;
        private const byte RST = 0x04;
        private const byte SYN = 0x02;
        private const byte FIN = 0x01;

        protected UInt16 sourcePort;
        protected UInt16 destPort;
        protected UInt32 seqNum;
        protected UInt32 ackNum;
        protected byte tcpHeaderDWords;
        protected byte tcpFlags;
        protected UInt16 windowSize;
        protected UInt16 tcpChecksum;
        protected UInt16 urgentPointer;
        protected UInt16 tcpDataOffset;
        protected int numOptions;
        protected int optionsLength;
        protected byte optionsOffset = 20;

        internal TCPPacket(byte[] rawData)
            : base(rawData)
        { }

        internal TCPPacket(TCPConnection connection, UInt32 seqNum, UInt32 ackNum, byte tcpFlags, UInt16 winSize)
            : this(connection.LocalIP, connection.RemoteIP, connection.LocalPort, connection.RemotePort, seqNum,
                    ackNum, tcpFlags, winSize)
        { }

        internal TCPPacket(TCPConnection connection, UInt32 seqNum, UInt32 ackNum, byte tcpFlags, UInt16 winSize, byte optionCount)
            : this(connection.LocalIP, connection.RemoteIP, connection.LocalPort, connection.RemotePort, seqNum,
                    ackNum, tcpFlags, winSize, null, 0, (optionCount * 4))
        { }

        internal TCPPacket(IPv4Address source, IPv4Address dest, UInt16 srcPort, UInt16 destPort,
            UInt32 seqNum, UInt32 ackNum, byte tcpFlags, UInt16 winSize)
            : this(source, dest, srcPort, destPort, seqNum, ackNum, tcpFlags, winSize, null, 0, 0)
        { }

        internal TCPPacket(IPv4Address source, IPv4Address dest, UInt16 srcPort, UInt16 destPort,
            UInt32 seqNum, UInt32 ackNum, byte tcpFlags, UInt16 winSize, byte[] data)
            : this(source, dest, srcPort, destPort, seqNum, ackNum, tcpFlags, winSize, data, data.Length, 0)
        { }

        internal TCPPacket(IPv4Address source, IPv4Address dest, UInt16 srcPort, UInt16 destPort, 
            UInt32 seqNum, UInt32 ackNum, byte tcpFlags, UInt16 winSize, byte[] data, int dataLength, int optionLength)
            : base((UInt16)(dataLength + 20 + optionLength), 6, source, dest)
        {
            numOptions = optionLength / 4;
            this.optionsLength = optionLength;

            mRawData[this.dataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
            mRawData[this.dataOffset + 3] = (byte)((destPort >> 0) & 0xFF);
            mRawData[this.dataOffset + 4] = (byte)((seqNum >> 24) & 0xFF);
            mRawData[this.dataOffset + 5] = (byte)((seqNum >> 16) & 0xFF);
            mRawData[this.dataOffset + 6] = (byte)((seqNum >> 8) & 0xFF);
            mRawData[this.dataOffset + 7] = (byte)((seqNum >> 0) & 0xFF);
            mRawData[this.dataOffset + 8] = (byte)((ackNum >> 24) & 0xFF);
            mRawData[this.dataOffset + 9] = (byte)((ackNum >> 16) & 0xFF);
            mRawData[this.dataOffset + 10] = (byte)((ackNum >> 8) & 0xFF);
            mRawData[this.dataOffset + 11] = (byte)((ackNum >> 0) & 0xFF);
            mRawData[this.dataOffset + 12] = (byte)((5 + numOptions) << 4);
            mRawData[this.dataOffset + 13] = tcpFlags;
            mRawData[this.dataOffset + 14] = (byte)((winSize >> 8) & 0xFF);
            mRawData[this.dataOffset + 15] = (byte)((winSize >> 0) & 0xFF);
            mRawData[this.dataOffset + 16] = 0x00;
            mRawData[this.dataOffset + 17] = 0x00;
            mRawData[this.dataOffset + 18] = 0x00;
            mRawData[this.dataOffset + 19] = 0x00;

            initFields();
            if (data != null)
            {
                for (int b = 0; b < data.Length; b++)
                {
                    mRawData[this.tcpDataOffset + b] = data[b];
                }
            }

            RecalcCRC();
        }

        private void RecalcCRC()
        {
            mRawData[this.dataOffset + 16] = 0x00;
            mRawData[this.dataOffset + 17] = 0x00;
            tcpChecksum = this.CalcTCPCRC();
            mRawData[this.dataOffset + 16] = (byte)((tcpChecksum >> 8) & 0xFF);
            mRawData[this.dataOffset + 17] = (byte)((tcpChecksum >> 0) & 0xFF);
        }

        private UInt16 CalcTCPCRC()
        {
            UInt16 crc;

            byte[] tempHeader = new byte[32 + TCP_DataLength + optionsLength];
            for (int b = 0; b < 8; b++)
            {
                tempHeader[b] = mRawData[26 + b];
            }
            tempHeader[9] = 6;
            tempHeader[10] = (byte)(this.TCP_Length >> 8);
            tempHeader[11] = (byte)(this.TCP_Length & 0xFF);
            for (int b = 0; b < 20 + optionsLength; b++)
            {
                tempHeader[12 + b] = mRawData[this.dataOffset + b];
            }
            for (int b = 0; b < this.TCP_DataLength; b++)
            {
                tempHeader[32 + optionsLength + b] = mRawData[this.tcpDataOffset + b];
            }

            crc = IPPacket.CalcOcCRC(tempHeader, 0, tempHeader.Length);

            return crc;
        }

        protected override void initFields()
        {
            base.initFields();
            sourcePort = (UInt16)((mRawData[this.dataOffset] << 8) | mRawData[this.dataOffset + 1]);
            destPort = (UInt16)((mRawData[this.dataOffset + 2] << 8) | mRawData[this.dataOffset + 3]);
            seqNum = (UInt32)((mRawData[this.dataOffset + 4] << 24) | (mRawData[this.dataOffset + 5] << 16) |
                                (mRawData[this.dataOffset + 6] << 8) | mRawData[this.dataOffset + 7]);
            ackNum = (UInt32)((mRawData[this.dataOffset + 8] << 24) | (mRawData[this.dataOffset + 9] << 16) |
                                (mRawData[this.dataOffset + 10] << 8) | mRawData[this.dataOffset + 11]);
            tcpHeaderDWords = (byte)((mRawData[this.dataOffset + 12] & 0xF0) >> 4);
            tcpFlags = mRawData[this.dataOffset + 13];
            windowSize = (UInt16)((mRawData[this.dataOffset + 14] << 8) | mRawData[this.dataOffset + 15]);
            tcpChecksum = (UInt16)((mRawData[this.dataOffset + 16] << 8) | mRawData[this.dataOffset + 17]);
            urgentPointer = (UInt16)((mRawData[this.dataOffset + 18] << 8) | mRawData[this.dataOffset + 19]);

            tcpDataOffset = (UInt16)(this.dataOffset + (this.tcpHeaderDWords * 4));
        }

        internal UInt16 DestinationPort
        {
            get { return this.destPort; }
        }
        internal UInt16 SourcePort
        {
            get { return this.sourcePort; }
        }
        internal UInt32 SequenceNumber
        {
            get { return this.seqNum; }
        }
        internal UInt32 AckNumber
        {
            get { return this.ackNum; }
        }
        internal byte TCP_Flags
        {
            get { return this.tcpFlags; }
        }
        internal UInt16 WindowSize
        {
            get { return this.windowSize; }
        }
        internal UInt16 TCPCRC
        {
            get { return this.tcpChecksum; }
        }
        internal UInt16 TCP_Length
        {
            get { return (UInt16)(this.ipLength - this.HeaderLength); }
        }
        internal UInt16 TCP_HeaderLength
        {
            get { return (UInt16)(this.tcpHeaderDWords * 4); }
        }
        internal UInt16 TCP_HeaderWords
        {
            get { return this.tcpHeaderDWords; }
        }
        internal UInt16 TCP_DataLength
        {
            get { return (UInt16)(this.TCP_Length - this.TCP_HeaderLength); }
        }
        internal bool Syn
        {
            get { return ((this.tcpFlags & SYN) != 0); }
        }
        internal bool Fin
        {
            get { return ((this.tcpFlags & FIN) != 0); }
        }
        internal bool Ack
        {
            get { return ((this.tcpFlags & ACK) != 0); }
        }
        internal byte[] TCP_Data
        {
            get
            {
                byte[] data = new byte[this.TCP_DataLength];

                for (int b = 0; b < data.Length; b++)
                {
                    data[b] = this.mRawData[this.tcpDataOffset + b];
                }

                return data;
            }
        }

        public override string ToString()
        {
            return ""; // "TCP Packet Src=" + sourceIP + ":" + sourcePort + ", Dest=" + destIP + ":" + destPort +
                 //",Flags=" + tcpFlags.ToHex(2) + ",SeqNo=" + seqNum + ",AckNum=" + ackNum + ", DataLen=" + TCP_DataLength;
        }

        internal void AddMSSOption(int mss)
        {
            mRawData[this.dataOffset + optionsOffset] = 0x02;
            mRawData[this.dataOffset + optionsOffset + 1] = 0x04;
            mRawData[this.dataOffset + optionsOffset + 2] = (byte)(mss >> 8);
            mRawData[this.dataOffset + optionsOffset + 3] = (byte)(mss & 0xFF);
            optionsOffset += 4;
            RecalcCRC();
        }
        internal void AddSACKOption()
        {
            mRawData[this.dataOffset + optionsOffset] = 0x01;
            mRawData[this.dataOffset + optionsOffset + 1] = 0x01;
            mRawData[this.dataOffset + optionsOffset + 2] = 0x04;
            mRawData[this.dataOffset + optionsOffset + 3] = 0x02;
            optionsOffset += 4;
            RecalcCRC();
        }
    }
}
