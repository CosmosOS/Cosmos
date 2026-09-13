/*
* PROJECT:          Cosmos OS Development
* CONTENT:          TCP Connection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Buffers;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network.Config;

namespace Cosmos.Kernel.System.Network.IPv4.TCP;

/// <summary>
/// Represents a TCP connection status.
/// </summary>
internal enum Status
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
internal class TransmissionControlBlock
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
internal class Tcp : IDisposable
{
    public static readonly ushort DynamicPortStart = 49152;

    private static ushort s_nextPort = 49152;
    /// <summary>
    /// Array pool used to rent buffers.
    /// </summary>
    private static readonly ArrayPool<byte> s_arrayPool = ArrayPool<byte>.Shared;

    /// <summary>
    /// Gets a dynamic port (simple incrementing approach for AOT compatibility).
    /// </summary>
    public static ushort GetDynamicPort(int tries = 10)
    {
        for (int i = 0; i < tries; i++)
        {
            ushort port = s_nextPort++;
            if (s_nextPort >= 65535)
            {
                s_nextPort = DynamicPortStart;
            }

            bool portInUse = false;
            foreach (var connection in Connections)
            {
                if (connection.LocalEndPoint.Port == port)
                {
                    portInUse = true;
                    break;
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

    // Simple sequence number generator
    private static uint s_sequenceCounter = 1000;

    #region Static
    /// <summary>
    /// A list of currently active connections.
    /// </summary>
    private static List<Tcp> Connections { get; } = new();

    /// <summary>
    /// String / enum correspondance (used for debugging)
    /// </summary>
    public static readonly string[] Table =
    [
        "LISTEN",
        "SYN_SENT",
        "SYN_RECEIVED",
        "ESTABLISHED",
        "FIN_WAIT1",
        "FIN_WAIT2",
        "CLOSE_WAIT",
        "CLOSING",
        "LAST_ACK",
        "TIME_WAIT",
        "CLOSED"
    ];

    /// <summary>
    /// Creates a TCP connection object.
    /// </summary>
    /// <param name="localPort"></param>
    /// <param name="remotePort"></param>
    /// <param name="localIp"></param>
    /// <param name="remoteIp"></param>
    /// <returns>A <see cref="Tcp"/> connection instance.</returns>
    internal static Tcp CreateNewConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        var tcp = new Tcp(localPort, remotePort, localIp, remoteIp);
        Connections.Add(tcp);
        return tcp;
    }

    public static Tcp CreateConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        var tcp = new Tcp(localPort, remotePort, localIp, remoteIp);
        Connections.Add(tcp);
        return tcp;
    }

    /// <summary>
    /// Gets a TCP connection object that matches the specified local and remote ports and addresses.
    /// </summary>
    internal static Tcp? GetConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            var con = Connections[i];
            if (con.Equals(localPort, remotePort, localIp, remoteIp))
            {
                return con;
            }
            // Is this correct if clause? Shouldn't be there another loop?
            if (con.LocalEndPoint.Port.Equals(localPort) && con.Status == Status.LISTEN)
            {
                return con;
            }
        }
        return null;
    }

    /// <summary>
    /// Removes a TCP connection object that matches the specified local and remote ports and addresses.
    /// </summary>
    /// <returns>True when connection was removed, false when one was not found and removed.</returns>
    public static bool RemoveConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            var conn = Connections[i];
            if (conn.Equals(localPort, remotePort, localIp, remoteIp))
            {
                conn.Dispose();
                Connections.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes a TCP connection object by reference.
    /// </summary>
    /// <returns>True when connection was removed, false when one was not found and removed.</returns>
    public static bool RemoveConnection(Tcp connection)
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            var conn = Connections[i];
            if (ReferenceEquals(conn, connection))
            {
                conn.Dispose();
                Connections.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    #endregion

    #region TCB

    /// <summary>
    /// The local end-point.
    /// </summary>
    public EndPoint LocalEndPoint { get; private set; }

    /// <summary>
    /// The remote end-point.
    /// </summary>
    public EndPoint RemoteEndPoint { get; private set; }

    /// <summary>
    /// The connection Transmission Control Block.
    /// </summary>
    public TransmissionControlBlock TCB { get; private set; }

    #endregion

    /// <summary>
    /// The connection status.
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// Whether the connection has been detached from its owning socket:
    /// Close() already returned but the peer has not finished the FIN
    /// handshake yet. A detached state machine keeps processing packets in
    /// the background and is removed from <see cref="Connections"/> as soon
    /// as it reaches <see cref="Status.CLOSED"/>.
    /// </summary>
    public bool Detached { get; set; }

    /// <summary>
    /// The received data buffer.
    /// </summary>
    private byte[] _data = [];
    /// <summary>
    /// Holds real data length as _data might be longer due to being rented.
    /// </summary>
    private int _dataLength = 0;

    private int _dataOffset = 0;
    public ReadOnlySpan<byte> Data => _data.AsSpan().Slice(_dataOffset, _dataLength);

    private Tcp(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        LocalEndPoint = new EndPoint(localIp, localPort);
        RemoteEndPoint = new EndPoint(remoteIp, remotePort);
        TCB = new TransmissionControlBlock();
    }

    public void AssignData(ReadOnlySpan<byte> source, int offset, int count, int length)
    {

    }

    /// <summary>
    /// Handles incoming TCP packets according to the current connection status.
    /// </summary>
    internal void ReceiveData(TcpPacket packet)
    {
        ReceiveDataInternal(packet);

        // A detached connection has no owner left to clean it up — reap it
        // from the connection table once the state machine lands on CLOSED.
        if (Detached && Status == Status.CLOSED)
        {
            Serial.WriteString("[TCP] Detached connection closed, reaping\n");
            RemoveConnection(this);
        }
    }

    private void ReceiveDataInternal(TcpPacket packet)
    {
        Serial.WriteString($"[{Table[(int)Status]}] {packet}\n");

        if (Status == Status.LISTEN)
        {
            ProcessListen(packet);
        }
        else if (Status == Status.SYN_SENT)
        {
            ProcessSynSent(packet);
        }
        else if (Status == Status.CLOSED)
        {
            // Connection is closed - just ignore all packets
            // Don't send RST as the connection state may be invalid
            Serial.WriteString("[TCP] Packet received on CLOSED connection, ignoring\n");
            return;
        }
        else if (Status == Status.TIME_WAIT)
        {
            // In TIME_WAIT, just ignore packets (we've already sent final ACK)
            Serial.WriteString("[TCP] Packet received in TIME_WAIT, ignoring\n");
        }
        else
        {
            // Check sequence number and segment data.
            if (TCB.RcvNxt <= packet.SequenceNumber && packet.SequenceNumber + packet.TcpDataLength < TCB.RcvNxt + TCB.RcvWnd)
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
                    default:
                        Serial.WriteString("[TCP] Unknown TCP connection state = " + (int)Status + "\n");
                        break;
                }
            }
            else
            {
                if (!packet._rst)
                {
                    SendEmptyPacket(TcpFlags.ACK);
                }

                Serial.WriteString("[TCP] Sequence number or segment data invalid, packet passed.\n");
            }
        }
    }

    #region Process Status

    /// <summary>
    /// Processes a TCP LISTEN state packet and updates the connection status accordingly.
    /// </summary>
    public void ProcessListen(TcpPacket packet)
    {
        if (packet._rst)
        {
            Serial.WriteString("[TCP] RST received at LISTEN state, packet passed.\n");
        }
        else if (packet._fin)
        {
            Serial.WriteString("[TCP] Connection closed! (FIN received on LISTEN state)\n");
        }
        else if (packet._ack)
        {
            TCB.RcvNxt = packet.SequenceNumber;
            TCB.SndNxt = packet.AckNumber;

            Status = Status.ESTABLISHED;
        }
        else if (packet._syn)
        {
            LocalEndPoint.Address = IPConfig.FindNetwork(packet.SourceIP) ?? throw new Exception($"Address can not be null");
            RemoteEndPoint.Address = packet.SourceIP;
            RemoteEndPoint.Port = packet.SourcePort;

            // Simple sequence number generation
            uint sequenceNumber = s_sequenceCounter++;

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

            SendEmptyPacket(TcpFlags.SYN | TcpFlags.ACK);

            Status = Status.SYN_RECEIVED;
        }
    }

    /// <summary>
    /// Processes a TCP SYN_RECEIVED state packet and updates the connection status accordingly.
    /// </summary>
    public void ProcessSynReceived(TcpPacket packet)
    {
        if (packet._ack)
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
                SendEmptyPacket(TcpFlags.RST, packet.AckNumber);
            }
        }
    }

    /// <summary>
    /// Processes a SYN_SENT state TCP packet and updates the connection state accordingly.
    /// </summary>
    public void ProcessSynSent(TcpPacket packet)
    {
        if (packet._syn)
        {
            TCB.IRS = packet.SequenceNumber;
            TCB.RcvNxt = packet.SequenceNumber + 1;

            if (packet._ack)
            {
                TCB.SndUna = packet.AckNumber;
                TCB.SndWnd = packet.WindowSize;
                TCB.SndWl1 = packet.SequenceNumber;
                TCB.SndWl2 = packet.AckNumber;

                SendEmptyPacket(TcpFlags.ACK);

                Status = Status.ESTABLISHED;
            }
            else if (packet.FlagBits == (byte)TcpFlags.SYN)
            {
                Status = Status.CLOSED;
                Serial.WriteString("[TCP] Simultaneous open not supported.\n");
            }
            else
            {
                Status = Status.CLOSED;
                Serial.WriteString("[TCP] Connection closed! (" + packet.GetFlags() + " received on SYN_SENT state)\n");
            }
        }
        else if (packet._ack)
        {
            //Check for bad ACK packet
            if ((int)packet.AckNumber - TCB.ISS < 0 || packet.AckNumber - TCB.SndNxt > 0)
            {
                SendEmptyPacket(TcpFlags.RST, packet.AckNumber);
                Serial.WriteString("[TCP] Bad ACK received at SYN_SENT.\n");
            }
            else
            {
                TCB.RcvNxt = packet.SequenceNumber;
                TCB.SndNxt = packet.AckNumber;

                Status = Status.ESTABLISHED;
            }
        }
        else if (packet._fin)
        {
            Status = Status.CLOSED;
            Serial.WriteString("[TCP] Connection closed! (FIN received on SYN_SENT state).\n");
        }
        else if (packet._rst)
        {
            Status = Status.CLOSED;
            Serial.WriteString("[TCP] Connection refused by remote computer.\n");
        }
    }

    /// <summary>
    /// Processes a ESTABLISHED state TCP packet.
    /// </summary>
    public void ProcessEstablished(TcpPacket packet)
    {
        if (packet._ack)
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
                Serial.WriteString("[TCP] Duplicate ACK, ignoring\n");
                return;
            }

            // Something not yet sent
            if (packet.AckNumber > TCB.SndNxt)
            {
                Serial.WriteString("[TCP] ACK for unsent data, sending ACK\n");
                SendEmptyPacket(TcpFlags.ACK);
                return;
            }

            if (packet._psh)
            {
                Serial.WriteString("[TCP] PSH received, data length: ");
                Serial.WriteNumber((ulong)packet.TcpDataLength);
                Serial.WriteString(", storing data\n");

                TCB.RcvNxt += packet.TcpDataLength;

                AppendToData(packet.TcpData);

                Serial.WriteString("[TCP] Data buffer now has ");
                Serial.WriteNumber((ulong)(_data?.Length ?? 0));
                Serial.WriteString(" bytes\n");

                // Handle FIN flag within PSH handling if both are set
                if (packet._fin)
                {
                    Serial.WriteString("[TCP] PSH+FIN received, closing\n");
                    TCB.RcvNxt++;

                    SendEmptyPacket(TcpFlags.ACK);

                    Status = Status.CLOSE_WAIT;

                    SimpleWait(300);

                    SendEmptyPacket(TcpFlags.FIN);

                    Status = Status.LAST_ACK;
                }
                else
                {
                    SendEmptyPacket(TcpFlags.ACK);
                }
                return;
            }
            else if (packet._fin)
            {
                Serial.WriteString("[TCP] FIN received, closing connection\n");
                TCB.RcvNxt++;

                SendEmptyPacket(TcpFlags.ACK);

                WaitAndClose();

                return;
            }

            if (packet.TcpDataLength > 0 && packet.SequenceNumber >= TCB.RcvNxt) //packet sequencing
            {
                TCB.RcvNxt += packet.TcpDataLength;

                AppendToData(packet.TcpData);
            }
        }
        if (packet._rst)
        {
            Status = Status.CLOSED;

            Serial.WriteString("[TCP] Connection reset!\n");
        }
        else if (packet._fin)
        {
            TCB.RcvNxt++;

            SendEmptyPacket(TcpFlags.ACK);

            Status = Status.CLOSE_WAIT;

            SimpleWait(300);

            SendEmptyPacket(TcpFlags.FIN);

            Status = Status.LAST_ACK;
        }
    }

    /// <summary>
    /// Process FIN_WAIT1 Status.
    /// </summary>
    public void ProcessFinWait1(TcpPacket packet)
    {
        if (packet._ack)
        {
            if (packet._fin)
            {
                TCB.RcvNxt++;

                SendEmptyPacket(TcpFlags.ACK);

                WaitAndClose();
            }
            else
            {
                Status = Status.FIN_WAIT2;
            }
        }
        else if (packet._fin)
        {
            TCB.RcvNxt++;

            SendEmptyPacket(TcpFlags.ACK);

            Status = Status.CLOSING;
        }
    }

    /// <summary>
    /// Process FIN_WAIT2 Status.
    /// </summary>
    public void ProcessFinWait2(TcpPacket packet)
    {
        if (packet._fin)
        {
            TCB.RcvNxt++;

            SendEmptyPacket(TcpFlags.ACK);

            WaitAndClose();
        }
        else if (packet._rst)
        {
            Status = Status.CLOSED;

            Serial.WriteString("[TCP] Connection reset in FIN_WAIT2!\n");
        }
    }

    /// <summary>
    /// Process CLOSING Status.
    /// </summary>
    public void ProcessClosing(TcpPacket packet)
    {
        if (packet._ack)
        {
            WaitAndClose();
        }
    }

    /// <summary>
    /// Process Close_WAIT Status.
    /// </summary>
    public void ProcessCloseWait(TcpPacket packet)
    {
        if (packet._ack)
        {
            Status = Status.CLOSED;
        }
    }

    #endregion

    #region Utils

    /// <summary>
    /// Simple busy wait (iteration-based for bare metal).
    /// </summary>
    private void SimpleWait(int iterations)
    {
        for (int i = 0; i < iterations * 10000; i++)
        {
            // Busy wait
        }
    }

    /// <summary>
    /// Waits until remote receives an ACKnowledge of its connection termination request.
    /// </summary>
    private void WaitAndClose()
    {
        Serial.WriteString("[TCP] WaitAndClose: entering TIME_WAIT\n");
        Status = Status.TIME_WAIT;

        SimpleWait(300);

        Serial.WriteString("[TCP] WaitAndClose: entering CLOSED\n");
        Status = Status.CLOSED;
    }

    /// <summary>
    /// Waits for a new TCP connection status (with timeout in milliseconds).
    /// </summary>
    public bool WaitStatus(Status status, int timeout)
    {
        int waited = 0;
        while (Status != status && waited < timeout)
        {
            Timer.TimerManager.Wait(10);
            waited += 10;
        }
        return Status == status;
    }

    /// <summary>
    /// Waits for a new TCP connection status (blocking).
    /// </summary>
    public bool WaitStatus(Status status)
    {
        while (Status != status)
        {
            Timer.TimerManager.Wait(10);
        }
        return true;
    }

    /// <summary>
    /// Waits until the connection leaves the given status (with timeout in milliseconds).
    /// </summary>
    public bool WaitLeaveStatus(Status status, int timeout)
    {
        int waited = 0;
        while (Status == status && waited < timeout)
        {
            Timer.TimerManager.Wait(10);
            waited += 10;
        }
        return Status != status;
    }

    /// <summary>
    /// Sends an empty packet.
    /// </summary>
    public void SendEmptyPacket(TcpFlags flag)
    {
        SendPacket(new TcpPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port,
            TCB.SndNxt, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
    }

    /// <summary>
    /// Sends an empty packet.
    /// </summary>
    internal void SendEmptyPacket(TcpFlags flag, uint sequenceNumber)
    {
        SendPacket(new TcpPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port,
            sequenceNumber, TCB.RcvNxt, 20, (byte)flag, TCB.SndWnd, 0));
    }

    /// <summary>
    /// Sends a TCP packet.
    /// </summary>
    private void SendPacket(TcpPacket packet)
    {
        OutgoingBuffer.AddPacket(packet);

        // Increment SndNxt BEFORE NetworkStack.Update() so that incoming packets
        // processed during Update() see the correct value
        if (packet._syn || packet._fin)
        {
            TCB.SndNxt++;
        }

        NetworkStack.Update();
    }

    public void AdvanceDataOffset(int offset)
    {
        if (offset == 0)
        {
            return;
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, _dataLength);

        if (offset == _dataLength && _dataLength != 0)
        {
            s_arrayPool.Return(_data);
            _data = [];
            _dataOffset = 0;
            _dataLength = 0;
            return;
        }

        _dataOffset += offset;
        _dataLength -= offset;
    }

    /// <summary>
    /// Appends bytes to <see cref="_data"/>.
    /// </summary>
    internal void AppendToData(ReadOnlySpan<byte> other)
    {
        if (other.Length == 0)
        {
            return;
        }

        Span<byte> target;
        // if new data fits into existing buffer, then no need to allocate a new one and write there
        // just append to existing buffer
        if (_dataOffset + _dataLength + other.Length <= _dataLength)
        {
            target = _data.AsSpan(_dataOffset + _dataLength);
            other.CopyTo(target);
            _dataLength += other.Length;
            return;
        }
        int realDataLength = _dataLength - _dataOffset;
        int requiredLength = realDataLength + other.Length;
        byte[] result = ArrayPool<byte>.Shared.Rent(requiredLength);
        Buffer.BlockCopy(_data, _dataOffset, result, 0, realDataLength);
        target = result.AsSpan(realDataLength);
        other.CopyTo(target);
        if (_data.Length > 0)
        {
            s_arrayPool.Return(_data);
        }

        _data = result;
        _dataOffset = 0;
        _dataLength = requiredLength;
    }

    internal bool Equals(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        return LocalEndPoint.Port.Equals(localPort) && RemoteEndPoint.Port.Equals(remotePort) &&
               LocalEndPoint.Address.Id.Equals(localIp.Id) && RemoteEndPoint.Address.Id.Equals(remoteIp.Id);
    }

    #endregion

    public void Dispose()
    {
        if (_data.Length > 0)
        {
            s_arrayPool.Return(_data);
        }
        // TODO release managed resources here
    }
}
