/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cosmos.HAL;
using Cosmos.System.Helpers;
using Cosmos.System.Network.Config;

namespace Cosmos.System.Network.IPv4.TCP
{
    /// <summary>
    /// TcpClient class. Used to manage the TCP connection to a server.
    /// </summary>
    public class TcpClient : IDisposable
    {
        /// <summary>
        /// Tcp State machine.
        /// </summary>
        internal Tcp StateMachine;

        /// <summary>
        /// Clients dictionary.
        /// </summary>
        private static Dictionary<uint, TcpClient> clients;

        /// <summary>
        /// Assign clients dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static TcpClient()
        {
            clients = new Dictionary<uint, TcpClient>();
        }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>TcpClient</returns>
        internal static TcpClient GetClient(ushort destPort)
        {
            TcpClient client;

            if (clients.TryGetValue(destPort, out client))
            {
                return client;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="stateMachine">Tcp state machine.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        internal TcpClient(Tcp stateMachine)
        {
            StateMachine = stateMachine;

            if (StateMachine.localPort > 0)
            {
                clients.Add((uint)StateMachine.localPort, this);
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpClient(int localPort)
        {
            StateMachine = new Tcp();

            StateMachine.rxBuffer = new Queue<TCPPacket>(8);

            StateMachine.Status = Status.CLOSED;
            StateMachine.LastSequenceNumber = 0;

            StateMachine.localPort = localPort;
            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if TcpClient with localPort 0 exists.</exception>
        public TcpClient(Address dest, int destPort)
            : this(0)
        {
            StateMachine.destination = dest;
            StateMachine.destinationPort = destPort;
        }

        /// <summary>
        /// Connect to client.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="Exception">Thrown if TCP Status is not CLOSED.</exception>
        public void Connect(Address dest, int destPort, int timeout = 5000)
        {
            if (StateMachine.Status != Status.CLOSED)
            {
                throw new Exception("Client must be closed before setting a new connection.");
            }

            StateMachine.destination = dest;
            StateMachine.destinationPort = destPort;

            StateMachine.source = IPConfig.FindNetwork(dest);

            //Generate Random Sequence Number
            var rnd = new Random();
            StateMachine.SequenceNumber = (uint)((rnd.Next(0, Int32.MaxValue)) << 32) | (uint)(rnd.Next(0, Int32.MaxValue));

            // Flags=0x02 -> Syn
            var packet = new TCPPacket(StateMachine.source, StateMachine.destination, (ushort)StateMachine.localPort, (ushort)destPort, StateMachine.SequenceNumber, 0, 20, (byte)Flags.SYN, 0xFAF0, 0);

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            StateMachine.Status = Status.SYN_SENT;

            if (StateMachine.WaitStatus(Status.ESTABLISHED, timeout) == false)
            {
                throw new Exception("Failed to open TCP connection!");
            }
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">Thrown if TCP Status is CLOSED.</exception>
        public void Close()
        {
            if (StateMachine.Status == Status.CLOSED)
            {
                throw new Exception("Client already closed.");
            }
            if (StateMachine.Status == Status.ESTABLISHED)
            {
                var packet = new TCPPacket(StateMachine.source, StateMachine.destination, (ushort)StateMachine.localPort, (ushort)StateMachine.destinationPort, StateMachine.SequenceNumber, StateMachine.AckNumber, 20, (byte)(Flags.FIN | Flags.ACK), 0xFAF0, 0);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                StateMachine.SequenceNumber++;

                StateMachine.Status = Status.FIN_WAIT1;

                if (StateMachine.WaitStatus(Status.CLOSED, 5000) == false)
                {
                    throw new Exception("Failed to close TCP connection!");
                }
            }

            if (clients.ContainsKey((uint)StateMachine.localPort))
            {
                clients.Remove((uint)StateMachine.localPort);
            }
        }

        /// <summary>
        /// Send data to client.
        /// </summary>
        /// <param name="data">Data array to send.</param>
        /// <exception cref="Exception">Thrown if destination is null or destinationPort is 0.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public void Send(byte[] data)
        {
            if ((StateMachine.destination == null) || (StateMachine.destinationPort == 0))
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
                    var packet = new TCPPacket(StateMachine.source, StateMachine.destination, (ushort)StateMachine.localPort, (ushort)StateMachine.destinationPort, StateMachine.SequenceNumber, StateMachine.AckNumber, 20, i == chunks.Length - 2 ? (byte)(Flags.PSH | Flags.ACK) : (byte)(Flags.ACK), 0xFAF0, 0, chunks[i]);
                    OutgoingBuffer.AddPacket(packet);
                    NetworkStack.Update();

                    StateMachine.SequenceNumber += (uint)chunks[i].Length;
                }
            }
            else
            {
                var packet = new TCPPacket(StateMachine.source, StateMachine.destination, (ushort)StateMachine.localPort, (ushort)StateMachine.destinationPort, StateMachine.SequenceNumber, StateMachine.AckNumber, 20, (byte)(Flags.PSH | Flags.ACK), 0xFAF0, 0, data);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                StateMachine.SequenceNumber += (uint)data.Length;
            }
            StateMachine.WaitingAck = true;
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before receiving data.");
            }
            if (StateMachine.rxBuffer.Count < 1)
            {
                return null;
            }

            var packet = StateMachine.rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            var tmp = StateMachine.Data;
            StateMachine.Data = null;
            return tmp;
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public byte[] Receive(ref EndPoint source)
        {
            if (StateMachine.Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before receiving data.");
            }
            while (StateMachine.rxBuffer.Count < 1) ;

            var packet = StateMachine.rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            var tmp = StateMachine.Data;
            StateMachine.Data = null;
            return tmp;
        }

        /// <summary>
        /// Is TCP Connected.
        /// </summary>
        /// <returns>Boolean value.</returns>
        public bool IsConnected()
        {
            return StateMachine.Status == Status.ESTABLISHED;
        }

        /// <summary>
        /// Close Client
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
