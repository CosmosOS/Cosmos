using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// TCP Connection status
    /// </summary>
    public enum Status
    {
        LISTEN,
        SYN_SENT,
        SYN_RECEIVED,
        ESTABLISHED,
        FIN_WAIT1,
        FIN_WAIT2,
        CLOSE_WAIT,
        CLOSING,
        LAST_ACK,
        TIME_WAIT,
        CLOSED
    }

    /// <summary>
    /// Tcp class. Used to manage the TCP state machine
    /// See <a href="https://datatracker.ietf.org/doc/html/rfc793">RFC 793</a> for more information.
    /// </summary>
    internal class Tcp
    {
        /// <summary>
        /// Local port.
        /// </summary>
        internal int localPort;
        /// <summary>
        /// Source address.
        /// </summary>
        internal Address source;
        /// <summary>
        /// Destination address.
        /// </summary>
        internal Address destination;
        /// <summary>
        /// Destination port.
        /// </summary>
        internal int destinationPort;

        /// <summary>
        /// RX buffer queue.
        /// </summary>
        internal Queue<TCPPacket> rxBuffer;

        /// <summary>
        /// Connection status.
        /// </summary>
        internal Status Status;

        /// <summary>
        /// Connection Acknowledgement number.
        /// </summary>
        internal uint AckNumber;

        /// <summary>
        /// Connection Sequence number.
        /// </summary>
        internal uint SequenceNumber;

        /// <summary>
        /// Last recveived Connection Sequence number.
        /// </summary>
        internal uint LastSequenceNumber;

        internal bool WaitingAck = false;

        internal byte[] data;

        /// <summary>
        /// Handle TCP discussions and data.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(TCPPacket packet)
        {
            switch (Status)
            {
                case Status.LISTEN:
                    ProcessListen(packet);
                    break;
                case Status.SYN_SENT:
                    ProcessSynSent(packet);
                    break;
                case Status.SYN_RECEIVED:
                    ProcessSynReceived(packet);
                    break;
                case Status.ESTABLISHED:
                    ProcessEstablished(packet);
                    break;
                case Status.FIN_WAIT1:
                    ProcessFinWait1(packet);
                    break;
                case Status.FIN_WAIT2:
                    ProcessFinWait2(packet);
                    break;
                case Status.CLOSE_WAIT:
                    ProcessCloseWait(packet);
                    break;
                case Status.CLOSING:
                    ProcessClosing(packet);
                    break;
                case Status.LAST_ACK:
                    ProcessCloseWait(packet);
                    break;
                case Status.TIME_WAIT:
                    break;
                case Status.CLOSED:
                    break;
                default:
                    throw new Exception("Unknown TCP connection state.");
            }
        }

        #region Process Status

        /// <summary>
        /// Process LISTEN Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessListen(TCPPacket packet)
        {
            if (packet.RST)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP connection resetted! (RST received on LISTEN state)");
            }
            else if (packet.FIN)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP connection closed! (FIN received on LISTEN state)");
            }
            else if (packet.ACK)
            {
                SendEmptyPacket(Flags.RST);

                Status = Status.CLOSED;

                throw new Exception("TCP connection closed! (ACK received on LISTEN state)");
            }
            else if (packet.SYN)
            {
                Status = Status.SYN_RECEIVED;

                source = IPConfig.FindNetwork(packet.SourceIP);

                AckNumber = packet.SequenceNumber + 1;

                var rnd = new Random();
                SequenceNumber = (uint)((rnd.Next(0, Int32.MaxValue)) << 32) | (uint)(rnd.Next(0, Int32.MaxValue));

                destination = packet.SourceIP;
                destinationPort = packet.SourcePort;

                SendEmptyPacket(Flags.SYN | Flags.ACK);
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSED||LISTEN)");
            }
        }

        /// <summary>
        /// Process SYN_RECEIVED Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessSynReceived(TCPPacket packet)
        {
            if (packet.RST)
            {
                Status = Status.LISTEN;
            }
            else if (packet.ACK)
            {
                LastSequenceNumber = packet.SequenceNumber - 1; //TODO: Fix this trick (for dup check when PSH ACK)

                SequenceNumber++;

                Status = Status.ESTABLISHED;
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=SYN_RECEIVED)");
            }
        }

        /// <summary>
        /// Process SYN_SENT Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessSynSent(TCPPacket packet)
        {
            if (packet.FIN)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP connection closed! (FIN received on SYN_SENT state)");
            }
            else if (packet.TCPFlags == (byte)Flags.ACK)
            {
                AckNumber = packet.SequenceNumber;
                SequenceNumber = packet.AckNumber;

                Status = Status.ESTABLISHED;
            }
            else if (packet.RST)
            {
                Status = Status.CLOSED;

                throw new Exception("Connection refused by remote computer.");
            }
            else if (packet.SYN)
            {
                if (packet.ACK)
                {
                    AckNumber = packet.SequenceNumber + 1;
                    SequenceNumber++;

                    LastSequenceNumber = packet.SequenceNumber;

                    SendEmptyPacket(Flags.ACK);

                    Status = Status.ESTABLISHED;
                }
                else if (packet.TCPFlags == (byte)Flags.SYN)
                {
                    Status = Status.CLOSED;

                    throw new NotImplementedException("Simultaneous open not supported.");
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("TCP connection closed! (Flag " + packet.TCPFlags + " received on SYN_SENT state)");
                }
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=SYN_SENT)");
            }
        }

        /// <summary>
        /// Process ESTABLISHED Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessEstablished(TCPPacket packet)
        {
            if (packet.RST)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP Connection resetted!");
            }
            if (packet.TCPFlags == (byte)Flags.ACK)
            {
                if (WaitingAck)
                {
                    if (SequenceNumber == packet.AckNumber)
                    {
                        WaitingAck = false;
                    }
                }
                else
                {
                    if (packet.SequenceNumber >= AckNumber && packet.TCP_DataLength > 0) //packet sequencing
                    {
                        AckNumber += packet.TCP_DataLength;

                        data = Concat(data, packet.TCP_Data);
                    }
                }
            }
            if (packet.FIN && packet.ACK)
            {
                AckNumber++;

                SendEmptyPacket(Flags.ACK);

                WaitAndClose();
            }
            else if (packet.FIN)
            {
                AckNumber++;

                SendEmptyPacket(Flags.ACK);

                Status = Status.CLOSE_WAIT;

                SendEmptyPacket(Flags.FIN);

                Status = Status.LAST_ACK;
            }
            else if (packet.PSH && packet.ACK)
            {
                if (packet.SequenceNumber > LastSequenceNumber) //dup check
                {
                    AckNumber += packet.TCP_DataLength;

                    LastSequenceNumber = packet.SequenceNumber;

                    data = Concat(data, packet.TCP_Data);

                    rxBuffer.Enqueue(packet);

                    SendEmptyPacket(Flags.ACK);
                }
            }
            else if (packet.ACK)
            {
                //DO NOTHING
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=ESTABLISHED)");
            }
        }

        /// <summary>
        /// Process FIN_WAIT1 Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessFinWait1(TCPPacket packet)
        {
            if (packet.FIN && packet.ACK)
            {
                AckNumber++;

                SendEmptyPacket(Flags.ACK);

                WaitAndClose();
            }
            else if (packet.FIN)
            {
                AckNumber++;

                SendEmptyPacket(Flags.ACK);

                Status = Status.CLOSING;
            }
            else if (packet.ACK)
            {
                Status = Status.FIN_WAIT2;
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=FIN_WAIT1)");
            }
        }

        /// <summary>
        /// Process FIN_WAIT2 Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessFinWait2(TCPPacket packet)
        {
            if (packet.FIN)
            {
                AckNumber++;

                SendEmptyPacket(Flags.ACK);

                WaitAndClose();
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=FIN_WAIT2)");
            }
        }

        /// <summary>
        /// Process CLOSING Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessClosing(TCPPacket packet)
        {
            if (packet.ACK)
            {
                WaitAndClose();
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSING)");
            }
        }

        /// <summary>
        /// Process Close_WAIT Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessCloseWait(TCPPacket packet)
        {
            if (packet.ACK)
            {
                Status = Status.CLOSED;
            }
            else
            {
                Status = Status.CLOSED;

                throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSE_WAIT||LAST_ACK)");
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Contatenate two byte arrays. https://stackoverflow.com/a/45531730
        /// </summary>
        /// <param name="first">First byte array.</param>
        /// <param name="second">Byte array to concatenate.</param>
        private byte[] Concat(byte[] first, byte[] second)
        {
            byte[] output;
            int alen = 0;

            if (first == null)
            {
                output = new byte[second.Length];
            }
            else
            {
                output = new byte[first.Length + second.Length];
                alen = first.Length;
                for (int i = 0; i < first.Length; i++)
                {
                    output[i] = first[i];
                }
            }
            for (int j = 0; j < second.Length; j++)
            {
                output[alen + j] = second[j];
            }
            return output;
        }

        /// <summary>
        /// Split byte array into chunks of a specified size.
        /// </summary>
        /// <param name="buffer">Byte array to split.</param>
        /// <param name="chunksize">Chunk size.</param>
        internal byte[][] ArraySplit(byte[] buffer, int chunksize = 1000)
        {
            var size = buffer.Length;
            var chunkCount = (buffer.Length + chunksize - 1) / chunksize;
            var bufferArray = new byte[chunkCount][];
            int index = 0;

            for (var i = 0; i < chunkCount; i++)
            {
                bufferArray[i] = new byte[Math.Min(chunksize, buffer.Length - i * chunksize)];
            }
            for (var i = 0; i < chunkCount; i++)
            {
                for (var j = 0; j < bufferArray[i].Length; j++)
                {
                    bufferArray[i][j] = buffer[index];
                    index++;
                }
            }
            return bufferArray;
        }

        /// <summary>
        /// Wait until remote receive ACK of its connection termination request.
        /// </summary>
        private void WaitAndClose()
        {
            Status = Status.TIME_WAIT;

            HAL.Global.PIT.Wait(300); //TODO: Calculate time value

            Status = Status.CLOSED;
        }

        /// <summary>
        /// Wait for new TCP connection status.
        /// </summary>
        internal bool WaitStatus(Status status, int timeout)
        {
            int second = 0;
            int _deltaT = 0;

            while (Status != status)
            {
                if (second > (timeout / 1000))
                {
                    return false;
                }
                if (_deltaT != RTC.Second)
                {
                    second++;
                    _deltaT = RTC.Second;
                }
            }
            return true;
        }

        /// <summary>
        /// Send acknowledgement packet
        /// </summary>
        private void SendEmptyPacket(Flags flag)
        {
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, (byte)flag, 0xFAF0, 0);

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();
        }

        #endregion
    }
}
