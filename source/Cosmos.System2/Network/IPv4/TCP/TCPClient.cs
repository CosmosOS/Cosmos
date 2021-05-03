/*
* PROJECT:          Aura Operating System Development
* CONTENT:          TCP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

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
        OPENED,
        OPENING, //SYN sent or received
        DATASENT,
        CLOSED,
        CLOSING //FIN sent or received
    }

    /// <summary>
    /// TCPClient class. Used to manage the TCP connection to a client.
    /// </summary>
    public class TcpClient : IDisposable
    {
        /// <summary>
        /// Clients dictionary.
        /// </summary>
        private static Dictionary<uint, TcpClient> clients;

        /// <summary>
        /// Local port.
        /// </summary>
        private int localPort;
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
        private int destinationPort;

        /// <summary>
        /// RX buffer queue.
        /// </summary>
        internal Queue<TCPPacket> rxBuffer;

        /// <summary>
        /// Connection status.
        /// </summary>
        internal Status Status;

        /// <summary>
        /// Last Connection Acknowledgement number.
        /// </summary>
        internal uint LastACK;

        /// <summary>
        /// Last Connection Sequence number.
        /// </summary>
        internal uint LastSEQ;

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
            if (clients.ContainsKey((uint)destPort) == true)
            {
                return clients[(uint)destPort];
            }

            return null;
        }

        /// <summary>
        /// Create new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if TcpClient with localPort 0 exists.</exception>
        public TcpClient()
            : this(0)
        { }

        /// <summary>
        /// Create new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public TcpClient(int localPort)
        {
            rxBuffer = new Queue<TCPPacket>(8);
            Status = Status.CLOSED;

            this.localPort = localPort;
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
            destination = dest;
            destinationPort = destPort;
        }

        /// <summary>
        /// Connect to client.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        public void Connect(Address dest, int destPort, int timeout = 5000)
        {
            destination = dest;
            destinationPort = destPort;

            source = IPConfig.FindNetwork(dest);

            byte[] options = new byte[]
            {
                0x02, 0x04, 0x05, 0xB4, 0x01, 0x03, 0x03, 0x08, 0x01, 0x01, 0x04, 0x02
            };

            //Generate Random Sequence Number
            var rnd = new Random();
            ulong sequencenumber = (ulong)((rnd.Next(0, Int32.MaxValue)) << 32) | (ulong)(rnd.Next(0, Int32.MaxValue)); 

            // Flags=0x02 -> Syn
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destPort, sequencenumber, 0, (ushort)(20 + options.Length), 0x02, 0xFAF0, 0, (ushort)options.Length, options);

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            Status = Status.OPENING;

            if (WaitStatus(Status.OPENED, timeout) == false)
            {
                throw new Exception("Failed to open TCP connection!");
            }
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public void Close()
        {
            if (Status == Status.OPENED)
            {
                var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, LastSEQ, LastACK, 20, 0x11, 0xFAF0, 0);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                Status = Status.CLOSING;

                if (WaitStatus(Status.CLOSED, 5000) == false)
                {
                    throw new Exception("Failed to close TCP connection!");
                }
            }

            if (clients.ContainsKey((uint)localPort) == true)
            {
                clients.Remove((uint)localPort);
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
        public void Send(byte[] data)
        {
            if ((destination == null) || (destinationPort == 0))
            {
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, LastSEQ, LastACK, 20, 0x18, 0xFAF0, 0, (ushort)data.Length, data);
            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            Status = Status.DATASENT;

            if (WaitStatus(Status.OPENED, 5000) == false)
            {
                throw new Exception("Failed to send TCP data!");
            }
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            if (rxBuffer.Count < 1)
            {
                return null;
            }

            var packet = new TCPPacket(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.TCP_Data;
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1);

            var packet = new TCPPacket(rxBuffer.Dequeue().RawData);
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            return packet.TCP_Data;
        }

        /// <summary>
        /// Receive data from packet.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(TCPPacket packet)
        {
            if (packet.RST)
            {
                Status = Status.CLOSED;

                throw new Exception("TCP Connection Reseted!");
            }

            if (Status == Status.OPENED && packet.FIN)
            {
                Status = Status.CLOSING;
                SendAck(LastACK, LastSEQ + 1);

                //TODO: Send FIN Packet
            }
            else if (Status == Status.CLOSING && packet.FIN && packet.ACK)
            {
                Status = Status.CLOSED;

                LastACK = packet.AckNumber;
                LastSEQ = packet.SequenceNumber;

                SendAck(LastACK, LastSEQ + 1);
            }
            else if (Status == Status.OPENING && packet.SYN && packet.ACK)
            {
                Status = Status.OPENED;

                LastACK = packet.AckNumber;
                LastSEQ = packet.SequenceNumber;

                SendAck(LastACK, LastSEQ + 1);
            }
            else if (Status == Status.DATASENT && packet.ACK)
            {
                Status = Status.OPENED;

                LastACK = packet.SequenceNumber;
                LastSEQ = packet.AckNumber;
            }
            else if (Status == Status.OPENED && packet.PSH && packet.ACK)
            {
                LastACK = packet.AckNumber;
                LastSEQ = packet.SequenceNumber;

                rxBuffer.Enqueue(packet);

                SendAck(LastACK, LastSEQ + 1);
            }
        }

        /// <summary>
        /// Close Client
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Wait for new TCP connection status.
        /// </summary>
        private bool WaitStatus(Status status, int timeout)
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
        private void SendAck(uint lastSEQ, uint lastACK)
        {
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, lastSEQ, lastACK, 20, 0x10, 0xFAF0, 0);

            LastACK = packet.AckNumber;
            LastSEQ = packet.SequenceNumber;

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();
        }
    }
}
