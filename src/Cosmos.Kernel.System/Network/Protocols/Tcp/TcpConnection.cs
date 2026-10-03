// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          TCP Connection
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Buffers;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Timers;

namespace Cosmos.Kernel.System.Network.Protocols.Tcp;

/// <summary>
/// Used to manage the TCP state machine.
/// Handle received packets according to current TCP connection Status. Also contains TCB (Transmission Control Block) information.
/// </summary>
/// <remarks>
/// See <a href="https://datatracker.ietf.org/doc/html/rfc793">RFC 793</a> for more information.
/// </remarks>
internal class TcpConnection : IDisposable
{
    /// <summary>
    /// The TCP window size.
    /// </summary>
    public const ushort TcpWindowSize = 8192;

    /// <summary>
    /// The first port of the dynamic range <see cref="GetDynamicPort"/> hands out.
    /// </summary>
    public const ushort DynamicPortStart = 49152;

    private static ushort s_nextPort = DynamicPortStart;

    // A plain counter, not the clock-driven initial sequence number generator RFC 793 describes.
    private static uint s_sequenceCounter = 1000;

    /// <summary>
    /// A list of currently active connections.
    /// </summary>
    private static List<TcpConnection> Connections { get; } = [];

    /// <summary>
    /// String / enum correspondence (used for debugging)
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

    #region TCB

    /// <summary>
    /// The local end-point.
    /// </summary>
    public EndPoint LocalEndPoint { get; }

    /// <summary>
    /// The remote end-point.
    /// </summary>
    public EndPoint RemoteEndPoint { get; }

    /// <summary>
    /// The connection Transmission Control Block.
    /// </summary>
    public TransmissionControlBlock TCB { get; }

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
    /// The received data buffer. A plain array the connection owns, not one
    /// from <see cref="ArrayPool{T}.Shared"/>: the receive path grows it on
    /// the kit worker with interrupts masked, and the shared pool takes a
    /// lock there that an app thread preempted inside it would never release,
    /// hanging the machine.
    /// </summary>
    private byte[] _data = [];
    /// <summary>
    /// Holds real data length as _data is usually longer.
    /// </summary>
    private int _dataLength;

    private int _dataOffset;

    /// <summary>
    /// The received bytes not consumed yet, unsynchronized: the receive path
    /// appends to them on the kit worker and may move them to a bigger array
    /// at any moment. The hosted tests read it, as they cannot mask
    /// interrupts; kernel code uses <see cref="DataLength"/> and
    /// <see cref="ReadData"/>.
    /// </summary>
    public ReadOnlySpan<byte> Data => _data.AsSpan().Slice(_dataOffset, _dataLength);

    /// <summary>
    /// How many received bytes wait to be consumed. A single read of one
    /// field, so any thread may take it without masking interrupts: it never
    /// sees a half-updated buffer, only a count the receive path may raise
    /// right after.
    /// </summary>
    public int DataLength => Volatile.Read(ref _dataLength);

    private TcpConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        LocalEndPoint = new EndPoint(localIp, localPort);
        RemoteEndPoint = new EndPoint(remoteIp, remotePort);
        TCB = new TransmissionControlBlock();
    }

    #region Static

    /// <summary>
    /// Creates a TCP connection object.
    /// </summary>
    /// <returns>The new <see cref="TcpConnection"/>, registered in the connection table.</returns>
    internal static TcpConnection CreateNewConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        TcpConnection tcp = new(localPort, remotePort, localIp, remoteIp);
        Connections.Add(tcp);
        return tcp;
    }

    /// <summary>
    /// Creates a connection and adds it to the table. Kernel callers mask
    /// interrupts around it: the kit worker looks connections up in the same
    /// table, and <see cref="List{T}.Add"/> counts the new slot before it
    /// stores into it.
    /// </summary>
    /// <returns>The new <see cref="TcpConnection"/>, registered in the connection table.</returns>
    public static TcpConnection CreateConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        TcpConnection tcp = new(localPort, remotePort, localIp, remoteIp);
        Connections.Add(tcp);
        return tcp;
    }

    /// <summary>
    /// Gets a dynamic port (simple incrementing approach for AOT compatibility).
    /// </summary>
    /// <returns>A port no registered connection uses locally, or 0 when all <paramref name="tries"/> ports tried are in use.</returns>
    public static ushort GetDynamicPort(int tries = 10)
    {
        for (int i = 0; i < tries; i++)
        {
            ushort port = s_nextPort++;
            if (s_nextPort >= 65535)
            {
                s_nextPort = DynamicPortStart;
            }

            if (!IsLocalPortInUse(port))
            {
                return port;
            }
        }

        return 0;
    }

    /// <summary>
    /// Whether a connection in the table uses <paramref name="port"/> locally.
    /// The scan runs with interrupts masked: the kit worker removes closed
    /// connections from the table, which would skip an entry or break an
    /// enumerator mid-scan. Nothing in it can throw, so the restore is an
    /// explicit call.
    /// </summary>
    private static bool IsLocalPortInUse(ushort port)
    {
        InternalCpu.InterruptScope mask = InternalCpu.DisableInterruptsScope();
        bool inUse = false;
        for (int i = 0; i < Connections.Count; i++)
        {
            if (Connections[i].LocalEndPoint.Port == port)
            {
                inUse = true;
                break;
            }
        }

        mask.Dispose();
        return inUse;
    }

    /// <summary>
    /// Gets a TCP connection object that matches the specified local and remote ports and addresses.
    /// </summary>
    internal static TcpConnection? GetConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            TcpConnection connection = Connections[i];
            if (connection.Equals(localPort, remotePort, localIp, remoteIp))
            {
                return connection;
            }
        }

        // A listener answers for any remote end, so it is only picked once no
        // established connection matched.
        for (int i = 0; i < Connections.Count; i++)
        {
            TcpConnection connection = Connections[i];
            if (connection.LocalEndPoint.Port == localPort && connection.Status == Status.LISTEN)
            {
                return connection;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes a TCP connection object that matches the specified local and remote ports and addresses.
    /// </summary>
    /// <returns>True when a connection was removed, false when none matched.</returns>
    public static bool RemoveConnection(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        // Masked from the search to the removal, for the same reason as the
        // overload below.
        InternalCpu.InterruptScope mask = InternalCpu.DisableInterruptsScope();
        bool removed = false;
        for (int i = 0; i < Connections.Count; i++)
        {
            TcpConnection conn = Connections[i];
            if (conn.Equals(localPort, remotePort, localIp, remoteIp))
            {
                conn.Dispose();
                Connections.RemoveAt(i);
                removed = true;
                break;
            }
        }

        mask.Dispose();
        return removed;
    }

    /// <summary>
    /// Removes a TCP connection object by reference.
    /// </summary>
    /// <returns>True when the connection was removed, false when it was not registered.</returns>
    public static bool RemoveConnection(TcpConnection connection)
    {
        // Masked from the search to the removal: the kit worker reaps closed
        // connections from the same table, and an index found before it
        // removed an earlier entry would take out the next connection
        // instead. Nothing in between can throw, so the restore is an
        // explicit call.
        InternalCpu.InterruptScope mask = InternalCpu.DisableInterruptsScope();
        bool removed = false;
        for (int i = 0; i < Connections.Count; i++)
        {
            TcpConnection conn = Connections[i];
            if (ReferenceEquals(conn, connection))
            {
                conn.Dispose();
                Connections.RemoveAt(i);
                removed = true;
                break;
            }
        }

        mask.Dispose();
        return removed;
    }

    #endregion

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
                        Serial.WriteString($"[TCP] Unknown TCP connection state = {(int)Status}\n");
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
            LocalEndPoint.Address = IPConfig.FindNetwork(packet.SourceIP) ?? throw new Exception("Address can not be null");
            RemoteEndPoint.Address = packet.SourceIP;
            RemoteEndPoint.Port = packet.SourcePort;

            uint sequenceNumber = s_sequenceCounter++;

            TCB.SndUna = sequenceNumber;
            TCB.SndNxt = sequenceNumber;
            TCB.SndWnd = TcpWindowSize;
            TCB.SndUp = 0;
            TCB.SndWl1 = packet.SequenceNumber - 1;
            TCB.SndWl2 = 0;
            TCB.ISS = sequenceNumber;

            TCB.RcvNxt = packet.SequenceNumber + 1;
            TCB.RcvWnd = TcpWindowSize;
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
                Serial.WriteString($"[TCP] Connection closed! ({packet.GetFlags()} received on SYN_SENT state)\n");
            }
        }
        else if (packet._ack)
        {
            // Check for bad ACK packet
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

                // Update Window Size
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

            // Data is taken from every segment that carries it, PSH or not: a
            // peer sets PSH on the last segment of a write only. Only the
            // segment that continues the stream is taken, since one after a
            // gap would be appended as if it came next. Every segment is
            // acknowledged, which moves the peer's window on and, after a gap,
            // has it resend from RcvNxt.
            if (packet.TcpDataLength > 0)
            {
                if (packet.SequenceNumber != TCB.RcvNxt)
                {
                    SendEmptyPacket(TcpFlags.ACK);
                    return;
                }

                TCB.RcvNxt += packet.TcpDataLength;

                AppendToData(packet.TcpData);

                if (packet._fin)
                {
                    Serial.WriteString("[TCP] Data+FIN received, closing\n");
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
            TimerManager.Wait(10);
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
            TimerManager.Wait(10);
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
            TimerManager.Wait(10);
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
            TCB.SndNxt, TCB.RcvNxt, 20, (byte)flag, (ushort)TCB.RcvWnd, 0));
    }

    /// <summary>
    /// Sends an empty packet.
    /// </summary>
    internal void SendEmptyPacket(TcpFlags flag, uint sequenceNumber)
    {
        SendPacket(new TcpPacket(LocalEndPoint.Address, RemoteEndPoint.Address, LocalEndPoint.Port, RemoteEndPoint.Port,
            sequenceNumber, TCB.RcvNxt, 20, (byte)flag, (ushort)TCB.RcvWnd, 0));
    }

    /// <summary>
    /// Sends a TCP packet.
    /// </summary>
    private void SendPacket(TcpPacket packet)
    {
        packet.Network.Enqueue();

        // Increment SndNxt BEFORE NetworkStack.Update() so that incoming packets
        // processed during Update() see the correct value
        if (packet._syn || packet._fin)
        {
            TCB.SndNxt++;
        }

        NetworkStack.Update();
    }

    /// <summary>
    /// Copies up to <paramref name="destination"/>'s length of the received
    /// bytes into it and consumes them, as one step with interrupts masked.
    /// The receive path runs on the kit worker, which can preempt any other
    /// thread: a copy and a consume made apart could read a buffer the worker
    /// just moved, or race its append and lose acknowledged bytes. Any thread.
    /// </summary>
    /// <param name="destination">Where the bytes go.</param>
    /// <returns>How many bytes were copied; zero when none are waiting.</returns>
    public int ReadData(Span<byte> destination)
    {
        // Nothing between the mask and its restore can throw: the count fits
        // both the destination and the live bytes, and every writer of the
        // buffer runs masked too. The restore is an explicit call, not a
        // using: the kernel skips finally blocks while an exception unwinds,
        // so the region is kept free of anything that could throw instead.
        InternalCpu.InterruptScope mask = InternalCpu.DisableInterruptsScope();
        int count = Math.Min(_dataLength, destination.Length);
        if (count > 0)
        {
            _data.AsSpan(_dataOffset, count).CopyTo(destination);
            ConsumeData(count);
        }

        mask.Dispose();
        return count;
    }

    /// <summary>
    /// Consumes bytes a reader took from <see cref="Data"/>, unsynchronized
    /// like it: for the hosted tests, while kernel code uses
    /// <see cref="ReadData"/>.
    /// </summary>
    /// <param name="offset">How many bytes were taken.</param>
    public void AdvanceDataOffset(int offset)
    {
        if (offset == 0)
        {
            return;
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, _dataLength);

        ConsumeData(offset);
    }

    /// <summary>
    /// Drops the first <paramref name="count"/> received bytes, at most
    /// <see cref="_dataLength"/>. Once all of them are gone the next append
    /// starts again at the front of the same array. Never throws.
    /// </summary>
    private void ConsumeData(int count)
    {
        _dataOffset += count;
        _dataLength -= count;
        if (_dataLength == 0)
        {
            _dataOffset = 0;
        }
    }

    /// <summary>
    /// Appends bytes to <see cref="_data"/>. Called by the receive path, which
    /// runs with interrupts masked, so a reader's <see cref="ReadData"/> never
    /// sees it half-done.
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
        if (_dataOffset + _dataLength + other.Length <= _data.Length)
        {
            target = _data.AsSpan(_dataOffset + _dataLength);
            other.CopyTo(target);
            _dataLength += other.Length;
            return;
        }

        // _dataLength already excludes the bytes a reader consumed, so the
        // live data is _dataLength bytes from _dataOffset. When the array
        // holds them plus the new bytes, they move to its front (CopyTo
        // copies overlapping spans correctly); otherwise they move to an array
        // twice as large, which keeps the copies linear in the bytes received.
        int requiredLength = _dataLength + other.Length;
        byte[] result = requiredLength <= _data.Length
            ? _data
            : new byte[Math.Max(requiredLength, _data.Length * 2)];
        _data.AsSpan(_dataOffset, _dataLength).CopyTo(result);
        target = result.AsSpan(_dataLength);
        other.CopyTo(target);

        _data = result;
        _dataOffset = 0;
        _dataLength = requiredLength;
    }

    internal bool Equals(ushort localPort, ushort remotePort, Address localIp, Address remoteIp)
    {
        return LocalEndPoint.Port.Equals(localPort) && RemoteEndPoint.Port.Equals(remotePort) &&
               LocalEndPoint.Address.Equals(localIp) && RemoteEndPoint.Address.Equals(remoteIp);
    }

    #endregion

    /// <summary>
    /// Drops the receive buffer.
    /// </summary>
    public void Dispose()
    {
        // The buffer is dropped with interrupts masked: the kit worker can
        // preempt this thread to append, and between the three stores it
        // would find the empty array with the old offset and length.
        InternalCpu.InterruptScope mask = InternalCpu.DisableInterruptsScope();
        _data = [];
        _dataOffset = 0;
        _dataLength = 0;
        mask.Dispose();
    }
}
