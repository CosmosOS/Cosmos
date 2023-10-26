using System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Helpers;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// Represents a TCP connection status.
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
        /// Represents no connection state.
        /// </summary>
        CLOSED
    }

    /// <summary>
    /// Represents a Transmission Control Block (TCB).
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
    /// Used to manage the TCP state machine.
    /// Handle received packets according to current TCP connection Status. Also contains TCB (Transmission Control Block) information.
    /// </summary>
    /// <remarks>
    /// See <a href="https://datatracker.ietf.org/doc/html/rfc793">RFC 793</a> for more information.
    /// </remarks>
    public class Tcp
    {

        public static ushort DynamicPortStart = 49152;

        private static Random dynamicPortStartRandom = new Random();

        /// <summary>
        /// gets a random port
        /// </summary>
        /// <param name="tries"></param>
        /// <returns></returns>
        public static ushort GetDynamicPort(int tries = 10)
        {
            for (int i = 0; i < tries; i++)
            {

                var port = (ushort)dynamicPortStartRandom.Next(DynamicPortStart, ushort.MaxValue);
                var portInUse = false;
                foreach (var connection in Connections)
                {
                    if (connection.LocalEndPoint.Port == port)
                    {
                        portInUse = true;
                    }
                }

                if (!portInUse)
                {
                    return port;
                }
            }

            return 0;
        }

        /// <summary>
        /// The TCP window size.
        /// </summary>
        public const ushort TcpWindowSize = 8192;

        #region Static
        /// <summary>
        /// A list of currently active connections.
        /// </summary>
        internal static List<Tcp> Connections;

        /// <summary>
        /// String / enum correspondance (used for debugging)
        /// </summary>
        internal static string[] Table;

        static Tcp()
        {
            Connections = new List<Tcp>();

            Table = new string[11];
            Table[0] = "LISTEN";
            Table[1] = "SYN_SENT";
            Table[2] = "SYN_RECEIVED";
            Table[3] = "ESTABLISHED";
            Table[4] = "FIN_WAIT1";
            Table[5] = "FIN_WAIT2";
            Table[6] = "CLOSE_WAIT";
            Table[7] = "CLOSING";
            Table[8] = "LAST_ACK";
            Table[9] = "TIME_WAIT";
            Table[10] = "CLOSED";
        }

        /// <summary>
        /// Gets a TCP connection object that matches the specified local and remote ports and addresses.
        /// </summary>
        /// <param name="localPort">The local port number of the connection.</param>
        /// <param name="remotePort">The remote port number of the connection.</param>
        /// <param name="localIp">The local IP address of the connection.</param>
        /// <param name="remoteIp">The remote IP address of the connection.</param>
        /// <returns>A TCP connection object if a match is found, <see langword="null"/> otherwise.</returns>
        /// <remarks>
        /// If a connection is found that matches the local and remote ports and addresses, it will be returned.
        /// If no exact match is found, a connection that is listening on the specified local port will be returned.
        /// If no matching connection is found, <see langword="null"/> is returned.
        /// </remarks>
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
        /// Removes a TCP connection object that matches the specified local and remote ports and addresses.
        /// </summary>
        /// <param name="localPort">The local port number of the connection.</param>
        /// <param name="remotePort">The remote port number of the connection.</param>
        /// <param name="localIp">The local IP address of the connection.</param>
        /// <param name="remoteIp">The remote IP address of the connection.</param>
        /// <remarks>
        /// If a connection is found that matches the local and remote ports and addresses, it will be removed from the list of connections.
        /// </remarks>
        internal static void RemoveConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Equals(localPort, remotePort, localIp, remoteIp))
                {
                    Connections.RemoveAt(i);

                    NetworkStack.Debugger.Send("Connection removed!");

                    return;
                }
            }
        }

        #endregion

        #region TCB

        /// <summary>
        /// The local end-point.
        /// </summary>
        public EndPoint LocalEndPoint;

        /// <summary>
        /// The remote end-point.
        /// </summary>
        public EndPoint RemoteEndPoint;

        /// <summary>
        /// The connection Transmission Control Block.
        /// </summary>
        internal TransmissionControlBlock TCB { get; set; }

        #endregion

        /// <summary>
        /// The RX buffer queue.
        /// </summary>
        internal Queue<TCPPacket> RxBuffer;

        /// <summary>
        /// The connection status.
        /// </summary>
        public Status Status;

        /// <summary>
        /// The received data buffer.
        /// </summary>
        internal byte[] Data { get; set; }

        public Tcp(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            LocalEndPoint = new EndPoint(localIp, localPort);
            RemoteEndPoint = new EndPoint(remoteIp, remotePort);
            TCB = new TransmissionControlBlock();
        }

        /// <summary>
        /// Handles incoming TCP packets according to the current connection status.
        /// </summary>
        /// <param name="packet">The packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(TCPPacket packet)
        {
            NetworkStack.Debugger.Send("[" + Table[(int)Status] + "] " + packet.ToString());

            /*if (Status == Status.CLOSED)
            {
                //DO NOTHING
            }
            else*/ if (Status == Status.LISTEN)
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
                            NetworkStack.Debugger.Send("Unknown TCP connection state.");
                            break;
                    }
                }
                else
                {
                    if (!packet.RST)
                    {
                        SendEmptyPacket(Flags.ACK);
                    }

                    NetworkStack.Debugger.Send("Sequence number or segment data invalid, packet passed.");
                }
            }
        }

        #region Process Status

        /// <summary>
        /// Processes a TCP LISTEN state packet and updates the connection status accordingly.
        /// </summary>
        /// <param name="packet">The incoming TCP packet.</param>
        /// <remarks>
        /// This method handles various types of incoming TCP packets during the LISTEN state and updates the status of the connection accordingly.
        /// If an RST packet is received, the packet is passed and no action is taken.
        /// If a FIN packet is received, the TCP connection is closed.
        /// If an ACK packet is received, the TCP connection is established.
        /// If a SYN packet is received, the TCP connection is moved to the SYN_RECEIVED state and an empty packet with the SYN and ACK flags set is sent back.
        /// </remarks>
        public void ProcessListen(TCPPacket packet)
        {
            if (packet.RST)
            {
                NetworkStack.Debugger.Send("RST received at LISTEN state, packet passed.");

                return;
            }
            else if (packet.FIN)
            {
                Status = Status.CLOSED;

                NetworkStack.Debugger.Send("TCP connection closed! (FIN received on LISTEN state)");
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
        /// Processes a TCP SYN_RECEIVED state packet and updates the connection status accordingly.
        /// </summary>
        /// <param name="packet">The incoming TCP packet.</param>
        /// <remarks>
        /// This method handles an incoming TCP packet during the SYN_RECEIVED state and updates the status of the connection accordingly.
        /// If an ACK packet is received with a valid AckNumber, the TCP connection is established.
        /// If the AckNumber is invalid, a reset packet is sent back with the AckNumber set to the invalid value.
        /// </remarks>
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
        /// Processes a SYN_SENT state TCP packet and updates the connection state accordingly.
        /// </summary>
        /// <param name="packet">The TCP packet to process.</param>
        /// <remarks>
        /// If the packet has the SYN flag set, the method sets the initial receive sequence number and responds with an ACK packet
        /// if the ACK flag is also set. If the SYN flag is set but not the ACK flag, the method closes the connection and sends an
        /// error message. If the packet has only the ACK flag set, the method checks whether the acknowledgment number is within the
        /// valid range and updates the send and receive sequence numbers. If the packet has the FIN flag set, the method closes the
        /// connection. If the packet has the RST flag set, the method also closes the connection and sends an error message.
        /// </remarks>
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

                    NetworkStack.Debugger.Send("Simultaneous open not supported.");
                }
                else
                {
                    Status = Status.CLOSED;

                    NetworkStack.Debugger.Send("TCP connection closed! (" + packet.getFlags() + " received on SYN_SENT state)");
                }
            }
            else if (packet.ACK)
            {
                //Check for bad ACK packet
                if (packet.AckNumber - TCB.ISS < 0 || packet.AckNumber - TCB.SndNxt > 0)
                {
                    SendEmptyPacket(Flags.RST, packet.AckNumber);

                    NetworkStack.Debugger.Send("Bad ACK received at SYN_SENT.");
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

                NetworkStack.Debugger.Send("TCP connection closed! (FIN received on SYN_SENT state).");
            }
            else if (packet.RST)
            {
                Status = Status.CLOSED;

                NetworkStack.Debugger.Send("Connection refused by remote computer.");
            }
        }

        /// <summary>
        /// Processes a ESTABLISHED state TCP packet.
        /// </summary>
        /// <param name="packet">The received packet.</param>
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

                    RxBuffer.Enqueue(packet);

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

                NetworkStack.Debugger.Send("TCP Connection resetted!");
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
        /// Waits until remote receives an ACKnowledge of its connection termination request.
        /// </summary>
        private void WaitAndClose()
        {
            Status = Status.TIME_WAIT;

            HAL.Global.PIT.Wait(300); //TODO: Calculate time value

            Status = Status.CLOSED;
        }

        /// <summary>
        /// Waits for a new TCP connection status.
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
        /// Waits for a new TCP connection status (blocking).
        /// </summary>
        internal bool WaitStatus(Status status)
        {
            while (Status != status);

            return true;
        }

        /// <summary>
        /// Sends an empty packet.
        /// </summary>
        internal void SendEmptyPacket(Flags flag)
        {
            SendPacket(new TCPPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port, TCB.SndNxt, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
        }

        /// <summary>
        /// Sends an empty packet.
        /// </summary>
        internal void SendEmptyPacket(Flags flag, uint sequenceNumber)
        {
            SendPacket(new TCPPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port, sequenceNumber, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
        }

        /// <summary>
        /// Sends a TCP packet.
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

        internal bool Equals(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
        {
            return LocalEndPoint.Port.Equals(localPort) && RemoteEndPoint.Port.Equals(remotePort) && LocalEndPoint.Address.Hash.Equals(localIp.Hash) && RemoteEndPoint.Address.Hash.Equals(remoteIp.Hash);
        }

        #endregion
    }
}
