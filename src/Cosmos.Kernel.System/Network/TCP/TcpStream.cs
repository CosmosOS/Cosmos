// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network.TCP;

/// <summary>
/// The server side of one TCP connection as a byte stream, for the kernel's
/// own services. <see cref="Listen"/> opens a passive connection, which
/// becomes the connection once a peer completes the handshake; a server
/// listens again for the next peer. Reads never block. Every touch of the
/// connection masks interrupts: the stack has no lock of its own, and its
/// receive path runs under the same mask.
/// </summary>
internal sealed class TcpStream
{
    /// <summary>
    /// Largest payload sent in one segment: the MSS a host may assume when
    /// the handshake carries no MSS option (RFC 879), which ours does not.
    /// </summary>
    private const int MaxSegmentSize = 536;

    private readonly Tcp _connection;

    /// <summary>The peer, once one connected.</summary>
    public EndPoint RemoteEndPoint => _connection.RemoteEndPoint;

    /// <summary>Whether the stream still waits for a peer to complete the handshake.</summary>
    public bool IsListening => _connection.Status is Status.LISTEN or Status.SYN_RECEIVED;

    /// <summary>Whether a peer started the handshake and has not completed it yet.</summary>
    public bool IsConnecting => _connection.Status == Status.SYN_RECEIVED;

    /// <summary>Signaled when data arrives or the connection's status changes; a reader blocks on it instead of polling.</summary>
    public InterruptEvent? ReceiveSignal
    {
        get => _connection.ReceiveSignal;
        set => _connection.ReceiveSignal = value;
    }

    /// <summary>Whether a peer connected and neither side has closed the connection since.</summary>
    public bool IsOpen => _connection.Status == Status.ESTABLISHED;

    private TcpStream(Tcp connection)
    {
        _connection = connection;
    }

    /// <summary>Opens a passive connection that waits for a peer on <paramref name="port"/>.</summary>
    public static TcpStream Listen(ushort port)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Tcp connection = Tcp.CreateConnection(port, 0, Address4.Zero, Address4.Zero);
            connection.Status = Status.LISTEN;
            return new TcpStream(connection);
        }
    }

    /// <summary>Takes up to <paramref name="buffer"/>'s length of the bytes received so far.</summary>
    /// <returns>How many bytes were taken; zero when none are waiting.</returns>
    public int Read(Span<byte> buffer)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            ReadOnlySpan<byte> received = _connection.Data;
            int count = Math.Min(received.Length, buffer.Length);
            received.Slice(0, count).CopyTo(buffer);
            _connection.AdvanceDataOffset(count);
            return count;
        }
    }

    /// <summary>
    /// Sends <paramref name="data"/>, split into segments. It does not wait
    /// for the peer's acknowledgement, and nothing is sent once the
    /// connection is no longer open.
    /// </summary>
    public void Write(ReadOnlySpan<byte> data)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            while (data.Length > 0 && IsOpen)
            {
                int length = Math.Min(data.Length, MaxSegmentSize);
                TransmissionControlBlock tcb = _connection.TCB;
                TcpPacket packet = new(
                    _connection.LocalEndPoint.Address, _connection.RemoteEndPoint.Address,
                    _connection.LocalEndPoint.Port, _connection.RemoteEndPoint.Port,
                    tcb.SndNxt, tcb.RcvNxt, TcpPacket.TcpHeaderMinimumLength,
                    (byte)(TcpFlags.PSH | TcpFlags.ACK), (ushort)tcb.RcvWnd, 0, data.Slice(0, length).ToArray());
                packet.Network.Enqueue();

                // Advanced before the queue is pumped, so an acknowledgement
                // the pump processes inline sees this segment as sent.
                tcb.SndNxt += (uint)length;
                NetworkStack.Update();

                data = data.Slice(length);
            }
        }
    }

    /// <summary>
    /// Closes the connection. An open one sends its FIN and is left to finish
    /// the handshake on its own, and the stack drops it once it is closed.
    /// Closing twice does nothing more.
    /// </summary>
    public void Close()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            switch (_connection.Status)
            {
                case Status.LISTEN:
                case Status.SYN_RECEIVED:
                case Status.CLOSED:
                    Tcp.RemoveConnection(_connection);
                    return;

                case Status.ESTABLISHED:
                    // The status goes first: the send pumps the stack, which
                    // can run the peer's FIN|ACK inline, and would find the
                    // connection still ESTABLISHED otherwise.
                    _connection.Status = Status.FIN_WAIT1;
                    _connection.SendEmptyPacket(TcpFlags.FIN | TcpFlags.ACK);
                    break;
            }

            if (_connection.Status == Status.CLOSED)
            {
                Tcp.RemoveConnection(_connection);
            }
            else
            {
                _connection.Detached = true;
            }
        }
    }
}
