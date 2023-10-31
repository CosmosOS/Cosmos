using System;
using System.Collections.Generic;
using Cosmos.System.Helpers;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// Represents a TCP client. Used to manage the TCP connection to a server.
    /// </summary>
    public class TcpClient : IDisposable
    {
        /// <summary>
        /// The TCP state machine.
        /// </summary>
        public Tcp StateMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if TcpClient with localPort 0 exists.</exception>
        public TcpClient() : this(Tcp.GetDynamicPort())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="stateMachine">The TCP state machine.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        internal TcpClient(Tcp stateMachine)
        {
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="localPort">The local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        internal TcpClient(int localPort)
        {
            StateMachine = new((ushort)localPort, 0, Address.Zero, Address.Zero);
            StateMachine.RxBuffer = new Queue<TCPPacket>(8);
            StateMachine.Status = Status.CLOSED;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if TcpClient with localPort 0 exists.</exception>
        public TcpClient(Address dest, int destPort)
            : this(Tcp.GetDynamicPort())
        {
            StateMachine.RemoteEndPoint.Address = dest;
            StateMachine.RemoteEndPoint.Port = (ushort)destPort;
        }

        /// <summary>
        /// Connects the client to the given server.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="Exception">Thrown if TCP Status is not CLOSED.</exception>
        public void Connect(Address dest, int destPort, int timeout = 5000)
        {
            if (StateMachine.Status == Status.ESTABLISHED)
            {
                throw new Exception("Client must be closed before setting a new connection.");
            }

            StateMachine.RemoteEndPoint.Address = dest;
            StateMachine.LocalEndPoint.Address = IPConfig.FindNetwork(dest);
            StateMachine.RemoteEndPoint.Port = (ushort)destPort;

            //Generate Random Sequence Number
            var rnd = new Random();
            var SequenceNumber = (uint)(rnd.Next(0, Int32.MaxValue) << 32) | (uint)rnd.Next(0, Int32.MaxValue);

            //Fill TCB
            StateMachine.TCB.SndUna = SequenceNumber;
            StateMachine.TCB.SndNxt = SequenceNumber;
            StateMachine.TCB.SndWnd = Tcp.TcpWindowSize;
            StateMachine.TCB.SndUp = 0;
            StateMachine.TCB.SndWl1 = 0;
            StateMachine.TCB.SndWl2 = 0;
            StateMachine.TCB.ISS = SequenceNumber;

            StateMachine.TCB.RcvNxt = 0;
            StateMachine.TCB.RcvWnd = Tcp.TcpWindowSize;
            StateMachine.TCB.RcvUp = 0;
            StateMachine.TCB.IRS = 0;

            Tcp.Connections.Add(StateMachine);

            StateMachine.SendEmptyPacket(Flags.SYN);

            StateMachine.Status = Status.SYN_SENT;

            if (StateMachine.WaitStatus(Status.ESTABLISHED, timeout) == false)
            {
                throw new Exception("Failed to open TCP connection!");
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown if TCP Status is CLOSED.</exception>
        public void Close()
        {
            if (StateMachine.Status == Status.ESTABLISHED)
            {
                StateMachine.SendEmptyPacket(Flags.FIN | Flags.ACK);

                StateMachine.TCB.SndNxt++;

                StateMachine.Status = Status.FIN_WAIT1;

                if (StateMachine.WaitStatus(Status.CLOSED, 5000) == false)
                {
                    throw new Exception("Failed to close TCP connection!");
                }
            }

            Tcp.RemoveConnection(StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address);
        }

        /// <summary>
        /// Sends data to the connected server.
        /// </summary>
        /// <param name="data">Data array to send.</param>
        /// <exception cref="Exception">Thrown if destination is null or destinationPort is 0.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public void Send(byte[] data)
        {
            if (StateMachine.RemoteEndPoint.Address == null || StateMachine.RemoteEndPoint.Port == 0)
            {
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before sending data.");
            }
            if (data.Length > 536)
            {
                var chunks = ArrayHelper.ArraySplit(data, 536);

                for (int i = 0; i < chunks.Length; i++)
                {
                    var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, i == chunks.Length - 2 ? (byte)(Flags.PSH | Flags.ACK) : (byte)Flags.ACK, StateMachine.TCB.SndWnd, 0, chunks[i]);
                    OutgoingBuffer.AddPacket(packet);
                    NetworkStack.Update();

                    StateMachine.TCB.SndNxt += (uint)chunks[i].Length;
                }
            }
            else
            {
                var packet = new TCPPacket(StateMachine.LocalEndPoint.Address, StateMachine.RemoteEndPoint.Address, StateMachine.LocalEndPoint.Port, StateMachine.RemoteEndPoint.Port, StateMachine.TCB.SndNxt, StateMachine.TCB.RcvNxt, 20, (byte)(Flags.PSH | Flags.ACK), StateMachine.TCB.SndWnd, 0, data);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                StateMachine.TCB.SndNxt += (uint)data.Length;
            }
        }

        /// <summary>
        /// Receives data from the end-point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before receiving data.");
            }

            if (StateMachine.RxBuffer.Count < 1)
            {
                return null;
            }

            var packet = StateMachine.RxBuffer.Dequeue();
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            var tmp = StateMachine.Data;
            StateMachine.Data = null;
            return tmp;
        }

        /// <summary>
        /// Receives data from the end-point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public byte[] Receive(ref EndPoint source)
        {
            while (StateMachine.RxBuffer.Count < 1)
            {
                if (StateMachine.Status != Status.ESTABLISHED)
                {
                    throw new Exception("Client must be connected before receiving data.");
                }
            }

            var packet = StateMachine.RxBuffer.Dequeue();
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            var tmp = StateMachine.Data;
            StateMachine.Data = null;
            return tmp;
        }

        /// <summary>
        /// Gets the remote hosts end-point (IP address and port).
        /// </summary>
        public EndPoint RemoteEndPoint => StateMachine.RemoteEndPoint;

        /// <summary>
        /// Gets the remote hosts end-point (IP address and port).
        /// </summary>
        public EndPoint LocalEndPoint => StateMachine.LocalEndPoint;

        /// <summary>
        /// Returns a value whether the TCP client is connected to a remote host.
        /// </summary>
        public bool IsConnected()
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        /// <summary>
        /// Returns a value whether the TCP client is closed
        /// </summary>
        public bool IsClosed()
        {
            return StateMachine.Status == Status.CLOSED;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
