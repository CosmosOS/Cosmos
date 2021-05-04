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
        internal uint AckNumber;

        /// <summary>
        /// Last Connection Sequence number.
        /// </summary>
        internal uint SequenceNumber;

        /// <summary>
        /// Next Connection Sequence number.
        /// </summary>
        internal uint NextSequenceNumber;

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

            //Generate Random Sequence Number
            var rnd = new Random();
            ulong sequencenumber = (ulong)((rnd.Next(0, Int32.MaxValue)) << 32) | (ulong)(rnd.Next(0, Int32.MaxValue)); 

            // Flags=0x02 -> Syn
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destPort, sequencenumber, 0, 20, (byte)Flags.SYN, 0xFAF0, 0);

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            Status = Status.SYN_SENT;

            if (WaitStatus(Status.ESTABLISHED, timeout) == false)
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
            if (Status == Status.ESTABLISHED)
            {
                var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, (byte)Flags.FIN, 0xFAF0, 0);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                SequenceNumber++;

                Status = Status.FIN_WAIT1;

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

            SequenceNumber += (uint)data.Length;

            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, 0x18, 0xFAF0, 0, (ushort)data.Length, data);
            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();

            /*if (WaitStatus(Status.OPENED, 5000) == false)
            {
                throw new Exception("Failed to send TCP data!");
            }*/
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
            if (Status == Status.LISTEN)
            {
                if (packet.SYN)
                {
                    Status = Status.SYN_RECEIVED;

                    source = IPConfig.FindNetwork(packet.SourceIP);

                    AckNumber = packet.SequenceNumber + 1;
                    SequenceNumber = 0xbeefcafe; //TODO: Generate this

                    destination = packet.SourceIP;
                    destinationPort = packet.SourcePort;

                    SendEmptyPacket(Flags.SYN | Flags.ACK);
                }
            }
            else if (Status == Status.SYN_RECEIVED)
            {
                if (packet.RST)
                {
                    Status = Status.LISTEN;
                }
                else if (packet.ACK)
                {
                    Status = Status.ESTABLISHED;

                    SequenceNumber = packet.SequenceNumber;
                }
            }
            else if (Status == Status.SYN_SENT)
            {
                if (packet.SYN && packet.ACK)
                {
                    AckNumber = packet.SequenceNumber + 1;
                    SequenceNumber++;

                    SendEmptyPacket(Flags.ACK);

                    Status = Status.ESTABLISHED;
                }
            }
            else if (Status == Status.ESTABLISHED)
            {
                if (packet.FIN)
                {
                    AckNumber++;

                    SendEmptyPacket(Flags.ACK);

                    Status = Status.CLOSE_WAIT;

                    SendEmptyPacket(Flags.FIN);
                }
            }
            else if (Status == Status.FIN_WAIT1)
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
            else if (Status == Status.FIN_WAIT2)
            {
                if (packet.FIN)
                {
                    AckNumber++;

                    SendEmptyPacket(Flags.ACK);

                    WaitAndClose();
                }
            }
            else if (Status == Status.CLOSING)
            {
                if (packet.ACK)
                {
                    WaitAndClose();
                }
            }
            else if (Status == Status.CLOSE_WAIT)
            {
                if (packet.ACK)
                {
                    Status = Status.CLOSED;
                }
            }
            
            
            /*else if (Status == Status.DATASENT && packet.ACK)
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

                SendEmptyPacket(LastACK, LastSEQ + 1, Flags.ACK);
            }*/
        }

        private void WaitAndClose()
        {
            Status = Status.TIME_WAIT;

            Thread.Sleep(100); //100ms for now

            Status = Status.CLOSED;
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
        private void SendEmptyPacket(Flags flag)
        {
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, (byte)flag, 0xFAF0, 0);

            OutgoingBuffer.AddPacket(packet);
            NetworkStack.Update();
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
