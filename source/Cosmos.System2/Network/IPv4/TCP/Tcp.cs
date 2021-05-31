using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.Helpers;
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
    /// Tcp class. Used to manage the TCP state machine.
    /// Handle received packets according to current TCP connection Status. Also contains TCB (Transmission Control Block) information.
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

        /// <summary>
        /// Is waiting for an acknowledgement packet.
        /// </summary>
        internal bool WaitingAck;

        /// <summary>
        /// TCP Received Data.
        /// </summary>
        internal byte[] Data;

        /// <summary>
        /// String / enum correspondance (used for debugging)
        /// </summary>
        internal string[] table;

        public Tcp()
        {
            WaitingAck = false;

            table = new string[11];
            table[0] = "LISTEN";
            table[1] = "SYN_SENT";
            table[2] = "SYN_RECEIVED";
            table[3] = "ESTABLISHED";
            table[4] = "FIN_WAIT1";
            table[5] = "FIN_WAIT2";
            table[6] = "CLOSE_WAIT";
            table[7] = "CLOSING";
            table[8] = "LAST_ACK";
            table[9] = "TIME_WAIT";
            table[10] = "CLOSED";
        }

        /// <summary>
        /// Handle incoming TCP packets according to current connection status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(TCPPacket packet)
        {
            Global.mDebugger.Send("[" + table[(int)Status] + "] " + packet.ToString());

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
        }

        /// <summary>
        /// Process ESTABLISHED Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessEstablished(TCPPacket packet)
        {
            if (packet.ACK && !packet.FIN)
            {
                if (packet.PSH)
                {
                    if (packet.SequenceNumber > LastSequenceNumber) //dup check
                    {
                        AckNumber += packet.TCP_DataLength;

                        LastSequenceNumber = packet.SequenceNumber;

                        Data = ArrayHelper.Concat(Data, packet.TCP_Data);

                        rxBuffer.Enqueue(packet);

                        SendEmptyPacket(Flags.ACK);
                    }
                }

                if (WaitingAck)
                {
                    if (packet.AckNumber == SequenceNumber)
                    {
                        WaitingAck = false;
                    }
                }
                else if (!packet.PSH)
                {
                    if (packet.SequenceNumber >= AckNumber && packet.TCP_DataLength > 0) //packet sequencing
                    {
                        AckNumber += packet.TCP_DataLength;

                        Data = ArrayHelper.Concat(Data, packet.TCP_Data);
                    }
                }
                return;
            }
            if (packet.RST)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP Connection resetted!");
            }
            else if (packet.FIN && packet.ACK)
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

                HAL.Global.PIT.Wait(300);

                SendEmptyPacket(Flags.FIN);

                Status = Status.LAST_ACK;
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
        }

        #endregion

        #region Utils

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
        /// Wait for new TCP connection status (blocking).
        /// </summary>
        internal bool WaitStatus(Status status)
        {
            while (Status != status);

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
