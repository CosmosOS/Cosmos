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
        /// <summary>
        /// Wait for a connection request from any remote TCP and port.
        /// </summary>
        LISTEN,

        /// <summary>
        /// Wait for a matching connection request after having sent a connection request.
        /// </summary>
        SYN_SENT,

        /// <summary>
        /// Wait for a confirming connection request acknowledgment after having both received and sent a connection request.
        /// </summary>
        SYN_RECEIVED,

        /// <summary>
        /// Represents an open connection, data received can be delivered to the user. The normal state for the data transfer phase of the connection.
        /// </summary>
        ESTABLISHED,

        /// <summary>
        /// Wait for a connection termination request from the remote TCP, or an acknowledgment of the connection termination request previously sent.
        /// </summary>
        FIN_WAIT1,

        /// <summary>
        /// Wait for a connection termination request from the remote TCP.
        /// </summary>
        FIN_WAIT2,

        /// <summary>
        /// Wait for a connection termination request from the local user.
        /// </summary>
        CLOSE_WAIT,

        /// <summary>
        /// Wait for a connection termination request acknowledgment from the remote TCP.
        /// </summary>
        CLOSING,

        /// <summary>
        /// Wait for an acknowledgment of the connection termination request previously sent to the remote TCP (which includes an acknowledgment of its connection termination request).
        /// </summary>
        LAST_ACK,

        /// <summary>
        /// Wait for enough time to pass to be sure the remote TCP received the acknowledgment of its connection termination request.
        /// </summary>
        TIME_WAIT,

        /// <summary>
        /// represents no connection state at all.
        /// </summary>
        CLOSED
    }

    /// <summary>
    /// Transmission Control Block (TCB).
    /// </summary>
    public class TransmissionControlBlock
    {
        /** Send Sequence Variables **/

        /// <summary>
        /// Send unacknowledged.
        /// </summary>
        public uint SndUna { get; set; }

        /// <summary>
        /// Send next.
        /// </summary>
        public uint SndNxt { get; set; }

        /// <summary>
        /// Send window.
        /// </summary>
        public ushort SndWnd { get; set; }

        /// <summary>
        /// Send urgent pointer.
        /// </summary>
        public uint SndUp { get; set; }

        /// <summary>
        /// Segment sequence number used for last window update.
        /// </summary>
        public uint SndWl1 { get; set; }

        /// <summary>
        /// Segment acknowledgment number used for last window update.
        /// </summary>
        public uint SndWl2 { get; set; }

        /// <summary>
        /// Initial send sequence number
        /// </summary>
        public uint ISS { get; set; }

        /** Receive Sequence Variables **/

        /// <summary>
        /// Receive next.
        /// </summary>
        public uint RcvNxt { get; set; }

        /// <summary>
        /// Receive window.
        /// </summary>
        public uint RcvWnd { get; set; }

        /// <summary>
        /// Receive urgent pointer.
        /// </summary>
        public uint RcvUp { get; set; }

        /// <summary>
        /// Initial receive sequence number.
        /// </summary>
        public uint IRS { get; set; }
    }

    /// <summary>
    /// Tcp class. Used to manage the TCP state machine.
    /// Handle received packets according to current TCP connection Status. Also contains TCB (Transmission Control Block) information.
    /// See <a href="https://datatracker.ietf.org/doc/html/rfc793">RFC 793</a> for more information.
    /// </summary>
    public class Tcp
    {
        /// <summary>
        /// TCP Window Size.
        /// </summary>
        public const ushort TcpWindowSize = 8192;

        #region Static

        /// <summary>
        /// Connection list.
        /// </summary>
        internal static List<Tcp> Connections;

        /// <summary>
        /// String / enum correspondance (used for debugging)
        /// </summary>
        internal static string[] table;

        /// <summary>
        /// Assign connection list.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static Tcp()
        {
            Connections = new List<Tcp>();

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
        /// Get TCP Connection.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <param name="remotePort">Destination port.</param>
        /// <param name="localIp">Local IPv4 Address.</param>
        /// <param name="remoteIp">Remote IPv4 Address.</param>
        /// <returns>Tcp</returns>
        internal static Tcp GetConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            foreach (var con in Connections)
            {
                if (con.Equals(localPort, remotePort, localIp, remoteIp))
                {
                    return con;
                }
                else if (con.LocalEndPoint.Port.Equals(localPort) && con.Status == Status.LISTEN)
                {
                    return con;
                }
            }
            return null;
        }

        /// <summary>
        /// Remove TCP Connection.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <param name="remotePort">Destination port.</param>
        /// <param name="localIp">Local IPv4 Address.</param>
        /// <param name="remoteIp">Remote IPv4 Address.</param>
        internal static void RemoveConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Equals(localPort, remotePort, localIp, remoteIp))
                {
                    Connections.RemoveAt(i);

                    Global.mDebugger.Send("Connection removed!");

                    return;
                }
            }
        }

        #endregion

        #region TCB

        /// <summary>
        /// Local EndPoint.
        /// </summary>
        public EndPoint LocalEndPoint;

        /// <summary>
        /// Remote EndPoint.
        /// </summary>
        public EndPoint RemoteEndPoint;

        /// <summary>
        /// Connection Transmission Control Block.
        /// </summary>
        internal TransmissionControlBlock TCB { get; set; }

        #endregion

        /// <summary>
        /// RX buffer queue.
        /// </summary>
        internal Queue<TCPPacket> rxBuffer;

        /// <summary>
        /// Connection status.
        /// </summary>
        public Status Status;

        /// <summary>
        /// TCP Received Data.
        /// </summary>
        internal byte[] Data { get; set; }

        public Tcp(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            LocalEndPoint = new EndPoint(localIp, localPort);
            RemoteEndPoint = new EndPoint(remoteIp, remotePort);
            TCB = new TransmissionControlBlock();
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

            if (Status == Status.CLOSED)
            {
                //DO NOTHING
            }
            else if (Status == Status.LISTEN)
            {
                ProcessListen(packet);
            }
            else if (Status == Status.SYN_SENT)
            {
                ProcessSynSent(packet);
            }
            else
            {
                // Check sequence number and segment data.
                if (TCB.RcvNxt <= packet.SequenceNumber && packet.SequenceNumber + packet.TCP_DataLength < TCB.RcvNxt + TCB.RcvWnd)
                {
                    switch (Status)
                    {
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
                        default:
                            Global.mDebugger.Send("Unknown TCP connection state.");
                            break;
                    }
                }
                else
                {
                    if (!packet.RST)
                    {
                        SendEmptyPacket(Flags.ACK);
                    }

                    Global.mDebugger.Send("Sequence number or segment data invalid, packet passed.");
                }
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
                Global.mDebugger.Send("RST received at LISTEN state, packet passed.");

                return;
            }
            else if (packet.FIN)
            {
                Status = Status.CLOSED;

                Global.mDebugger.Send("TCP connection closed! (FIN received on LISTEN state)");
            }
            else if (packet.ACK)
            {
                TCB.RcvNxt = packet.SequenceNumber;
                TCB.SndNxt = packet.AckNumber;

                Status = Status.ESTABLISHED;
            }
            else if (packet.SYN)
            {
                LocalEndPoint.Address = IPConfig.FindNetwork(packet.SourceIP);
                RemoteEndPoint.Address = packet.SourceIP;
                RemoteEndPoint.Port = packet.SourcePort;

                var rnd = new Random();
                var sequenceNumber = (uint)(rnd.Next(0, Int32.MaxValue) << 32) | (uint)rnd.Next(0, Int32.MaxValue);

                //Fill TCB
                TCB.SndUna = sequenceNumber;
                TCB.SndNxt = sequenceNumber;
                TCB.SndWnd = Tcp.TcpWindowSize;
                TCB.SndUp = 0;
                TCB.SndWl1 = packet.SequenceNumber - 1;
                TCB.SndWl2 = 0;
                TCB.ISS = sequenceNumber;

                TCB.RcvNxt = packet.SequenceNumber + 1;
                TCB.RcvWnd = Tcp.TcpWindowSize;
                TCB.RcvUp = 0;
                TCB.IRS = packet.SequenceNumber;

                SendEmptyPacket(Flags.SYN | Flags.ACK);

                Status = Status.SYN_RECEIVED;
            }
        }

        /// <summary>
        /// Process SYN_RECEIVED Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessSynReceived(TCPPacket packet)
        {
            if (packet.ACK)
            {
                if (TCB.SndUna <= packet.AckNumber && packet.AckNumber <= TCB.SndNxt)
                {
                    TCB.SndWnd = packet.WindowSize;
                    TCB.SndWl1 = packet.SequenceNumber;
                    TCB.SndWl2 = packet.SequenceNumber;

                    Status = Status.ESTABLISHED;
                }
                else
                {
                    SendEmptyPacket(Flags.RST, packet.AckNumber);
                }
            }
        }

        /// <summary>
        /// Process SYN_SENT Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessSynSent(TCPPacket packet)
        {
            if (packet.SYN)
            {
                TCB.IRS = packet.SequenceNumber;
                TCB.RcvNxt = packet.SequenceNumber + 1;

                if (packet.ACK)
                {
                    TCB.SndUna = packet.AckNumber;
                    TCB.SndWnd = packet.WindowSize;
                    TCB.SndWl1 = packet.SequenceNumber;
                    TCB.SndWl2 = packet.AckNumber;

                    SendEmptyPacket(Flags.ACK);

                    Status = Status.ESTABLISHED;
                }
                else if (packet.TCPFlags == (byte)Flags.SYN)
                {
                    Status = Status.CLOSED;

                    Global.mDebugger.Send("Simultaneous open not supported.");
                }
                else
                {
                    Status = Status.CLOSED;

                    Global.mDebugger.Send("TCP connection closed! (" + packet.getFlags() + " received on SYN_SENT state)");
                }
            }
            else if (packet.ACK)
            {
                //Check for bad ACK packet
                if (packet.AckNumber - TCB.ISS < 0 || packet.AckNumber - TCB.SndNxt > 0)
                {
                    SendEmptyPacket(Flags.RST, packet.AckNumber);

                    Global.mDebugger.Send("Bad ACK received at SYN_SENT.");
                }
                else
                {
                    TCB.RcvNxt = packet.SequenceNumber;
                    TCB.SndNxt = packet.AckNumber;

                    Status = Status.ESTABLISHED;
                }
            }
            else if (packet.FIN)
            {
                Status = Status.CLOSED;

                Global.mDebugger.Send("TCP connection closed! (FIN received on SYN_SENT state).");
            }
            else if (packet.RST)
            {
                Status = Status.CLOSED;

                Global.mDebugger.Send("Connection refused by remote computer.");
            }
        }

        /// <summary>
        /// Process ESTABLISHED Status.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        public void ProcessEstablished(TCPPacket packet)
        {
            if (packet.ACK)
            {
                if (TCB.SndUna < packet.AckNumber && packet.AckNumber <= TCB.SndNxt)
                {
                    TCB.SndUna = packet.AckNumber;

                    //Update Window Size
                    if (TCB.SndWl1 < packet.SequenceNumber || (TCB.SndWl1 == packet.SequenceNumber && TCB.SndWl2 <= packet.AckNumber))
                    {
                        TCB.SndWnd = packet.WindowSize;
                        TCB.SndWl1 = packet.SequenceNumber;
                        TCB.SndWl2 = packet.AckNumber;
                    }
                }

                // Check for duplicate packet
                if (packet.AckNumber < TCB.SndUna)
                {
                    return;
                }

                // Something not yet sent
                if (packet.AckNumber > TCB.SndNxt)
                {
                    SendEmptyPacket(Flags.ACK);
                    return;
                }

                if (packet.PSH)
                {
                    TCB.RcvNxt += packet.TCP_DataLength;

                    Data = ArrayHelper.Concat(Data, packet.TCP_Data);

                    rxBuffer.Enqueue(packet);

                    SendEmptyPacket(Flags.ACK);
                    return;
                }
                else if (packet.FIN)
                {
                    TCB.RcvNxt++;

                    SendEmptyPacket(Flags.ACK);

                    WaitAndClose();

                    return;
                }

                if (packet.TCP_DataLength > 0 && packet.SequenceNumber >= TCB.RcvNxt) //packet sequencing
                {
                    TCB.RcvNxt += packet.TCP_DataLength;

                    Data = ArrayHelper.Concat(Data, packet.TCP_Data);
                }
            }
            if (packet.RST)
            {
                Status = Status.CLOSED;

                Global.mDebugger.Send("TCP Connection resetted!");
            }
            else if (packet.FIN)
            {
                TCB.RcvNxt++;

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
            if (packet.ACK)
            {
                if (packet.FIN)
                {
                    TCB.RcvNxt++;

                    SendEmptyPacket(Flags.ACK);

                    WaitAndClose();
                }
                else
                {
                    Status = Status.FIN_WAIT2;
                }
            }
            else if (packet.FIN)
            {
                TCB.RcvNxt++;

                SendEmptyPacket(Flags.ACK);

                Status = Status.CLOSING;
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
                TCB.RcvNxt++;

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
                if (second > timeout / 1000)
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
        /// Send empty packet.
        /// </summary>
        internal void SendEmptyPacket(Flags flag)
        {
            SendPacket(new TCPPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port, TCB.SndNxt, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
        }

        /// <summary>
        /// Send empty packet.
        /// </summary>
        internal void SendEmptyPacket(Flags flag, uint sequenceNumber)
        {
            SendPacket(new TCPPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port, sequenceNumber, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
        }

        /// <summary>
        /// Send TCP packet.
        /// </summary>
        private void SendPacket(TCPPacket packet)
        {
            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            if (packet.SYN || packet.FIN)
            {
                TCB.SndNxt++;
            }
        }

        /// <summary>
        /// Equals connection.
        /// </summary>
        internal bool Equals(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            return LocalEndPoint.Port.Equals(localPort) && RemoteEndPoint.Port.Equals(remotePort) && LocalEndPoint.Address.Hash.Equals(localIp.Hash) && RemoteEndPoint.Address.Hash.Equals(remoteIp.Hash);
        }

        #endregion
    }
}
