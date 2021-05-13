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
        private uint LastSequenceNumber;

        private bool WaitingAck = false;

        private byte[] data;

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
            if (clients.ContainsKey((uint)destPort))
            {
                return clients[(uint)destPort];
            }

            return null;
        }

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
            LastSequenceNumber = 0;

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
        /// <exception cref="Exception">Thrown if TCP Status is not CLOSED.</exception>
        public void Connect(Address dest, int destPort, int timeout = 5000)
        {
            if (Status != Status.CLOSED)
            {
                throw new Exception("Client must be closed before setting a new connection.");
            }

            destination = dest;
            destinationPort = destPort;

            source = IPConfig.FindNetwork(dest);

            //Generate Random Sequence Number
            var rnd = new Random();
            SequenceNumber = (uint)((rnd.Next(0, Int32.MaxValue)) << 32) | (uint)(rnd.Next(0, Int32.MaxValue));

            // Flags=0x02 -> Syn
            var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destPort, SequenceNumber, 0, 20, (byte)Flags.SYN, 0xFAF0, 0);

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
        /// <exception cref="Exception">Thrown if TCP Status is CLOSED.</exception>
        public void Close()
        {
            if (Status == Status.CLOSED)
            {
                throw new Exception("Client already closed.");
            }
            if (Status == Status.ESTABLISHED)
            {
                var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, (byte)(Flags.FIN | Flags.ACK), 0xFAF0, 0);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                SequenceNumber++;

                Status = Status.FIN_WAIT1;

                if (WaitStatus(Status.CLOSED, 5000) == false)
                {
                    throw new Exception("Failed to close TCP connection!");
                }
            }

            if (clients.ContainsKey((uint)localPort))
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
        /// <exception cref="Exception">Thrown if TCP Status is not ESTABLISHED.</exception>
        public void Send(byte[] data)
        {
            if ((destination == null) || (destinationPort == 0))
            {
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }
            if (Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before sending data.");
            }
            if (data.Length > 536)
            {
                var chunks = ArraySplit(data, 536);

                for (int i = 0; i < chunks.Length; i++)
                {
                    var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, i == chunks.Length - 2 ? (byte)(Flags.PSH | Flags.ACK) : (byte)(Flags.ACK), 0xFAF0, 0, chunks[i]);
                    OutgoingBuffer.AddPacket(packet);
                    NetworkStack.Update();

                    SequenceNumber += (uint)chunks[i].Length;
                }
            }
            else
            {
                var packet = new TCPPacket(source, destination, (ushort)localPort, (ushort)destinationPort, SequenceNumber, AckNumber, 20, (byte)(Flags.PSH | Flags.ACK), 0xFAF0, 0, data);
                OutgoingBuffer.AddPacket(packet);
                NetworkStack.Update();

                SequenceNumber += (uint)data.Length;
            }
            WaitingAck = true;
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
            if (Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before receiving data.");
            }
            if (rxBuffer.Count < 1)
            {
                return null;
            }

            var packet = rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            var tmp = data;
            data = null;
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
            if (Status != Status.ESTABLISHED)
            {
                throw new Exception("Client must be connected before receiving data.");
            }
            while (rxBuffer.Count < 1) ;

            var packet = rxBuffer.Dequeue();
            source.address = packet.SourceIP;
            source.port = packet.SourcePort;

            var tmp = data;
            data = null;
            return tmp;
        }

        /// <summary>
        /// Handle TCP discussions and data.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(TCPPacket packet)
        {
            if (Status == Status.LISTEN || Status == Status.CLOSED)
            {
                if (packet.SYN)
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
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSED||LISTEN)");
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

                    SequenceNumber++;
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=SYN_RECEIVED)");
                }
            }
            else if (Status == Status.SYN_SENT)
            {
                if (packet.SYN && packet.ACK)
                {
                    AckNumber = packet.SequenceNumber + 1;
                    SequenceNumber++;

                    LastSequenceNumber = packet.SequenceNumber;

                    SendEmptyPacket(Flags.ACK);

                    Status = Status.ESTABLISHED;
                }
                else if (packet.SYN)
                {
                    Status = Status.CLOSED;

                    throw new NotImplementedException("Simultaneous open not supported.");
                }
                else if (packet.RST)
                {
                    Status = Status.CLOSED;

                    throw new Exception("Connection refused by remote computer.");
                }
                else if (packet.ACK)
                {
                    AckNumber = packet.SequenceNumber;
                    SequenceNumber = packet.AckNumber;

                    Status = Status.ESTABLISHED;
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=SYN_SENT)");
                }
            }
            else if (Status == Status.ESTABLISHED)
            {
                if (packet.RST)
                {
                    Status = Status.CLOSED;

                    throw new Exception("TCP Connection resetted!");
                }
                if (packet.TCPFlags == (byte)Flags.ACK)
                {
                    if (WaitingAck)
                    {
                        if (SequenceNumber == packet.AckNumber)
                        {
                            WaitingAck = false;
                        }
                    }
                    else
                    {
                        if (packet.SequenceNumber >= AckNumber && packet.TCP_DataLength > 0) //packet sequencing
                        {
                            AckNumber += packet.TCP_DataLength;

                            data = Concat(data, packet.TCP_Data);
                        }
                    }
                }
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

                    Status = Status.CLOSE_WAIT;

                    SendEmptyPacket(Flags.FIN);

                    Status = Status.LAST_ACK;
                }
                else if (packet.PSH && packet.ACK)
                {
                    if (packet.SequenceNumber > LastSequenceNumber) //dup check
                    {
                        AckNumber += packet.TCP_DataLength;

                        LastSequenceNumber = packet.SequenceNumber;

                        data = Concat(data, packet.TCP_Data);

                        rxBuffer.Enqueue(packet);

                        SendEmptyPacket(Flags.ACK);
                    }
                }
                else if (packet.ACK)
                {
                    //DO NOTHING
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=ESTABLISHED)");
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
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=FIN_WAIT1)");
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
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=FIN_WAIT2)");
                }
            }
            else if (Status == Status.CLOSING)
            {
                if (packet.ACK)
                {
                    WaitAndClose();
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSING)");
                }
            }
            else if (Status == Status.CLOSE_WAIT || Status == Status.LAST_ACK)
            {
                if (packet.ACK)
                {
                    Status = Status.CLOSED;
                }
                else
                {
                    Status = Status.CLOSED;

                    throw new Exception("Received packet not supported. Is this an error? (Flag=" + packet.TCPFlags + ", Status=CLOSE_WAIT||LAST_ACK)");
                }
            }
        }

        #region Utils

        /// <summary>
        /// Contatenate two byte arrays. https://stackoverflow.com/a/45531730
        /// </summary>
        private byte[] Concat(byte[] a, byte[] b)
        {
            byte[] output;
            int alen = 0;

            if (a == null)
            {
                output = new byte[b.Length];
            }
            else
            {
                output = new byte[a.Length + b.Length];
                alen = a.Length;
                for (int i = 0; i < a.Length; i++)
                {
                    output[i] = a[i];
                }
            }
            for (int j = 0; j < b.Length; j++)
            {
                output[alen + j] = b[j];
            }
            return output;
        }

        /// <summary>
        /// Split byte array into chunks of a specified size.
        /// </summary>
        private byte[][] ArraySplit(byte[] buffer, int chunksize = 1000)
        {
            var size = buffer.Length;
            var chunkCount = (buffer.Length + chunksize - 1) / chunksize;
            var bufferArray = new byte[chunkCount][];
            int index = 0;

            for (var i = 0; i < chunkCount; i++)
            {
                bufferArray[i] = new byte[Math.Min(chunksize, buffer.Length - i * chunksize)];
            }
            for (var i = 0; i < chunkCount; i++)
            {
                for (var j = 0; j < bufferArray[i].Length; j++)
                {
                    bufferArray[i][j] = buffer[index];
                    index++;
                }
            }
            return bufferArray;
        }

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

        #endregion

        /// <summary>
        /// Is TCP Connected.
        /// </summary>
        /// <returns>Boolean value.</returns>
        public bool IsConnected()
        {
            return Status == Status.ESTABLISHED;
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
