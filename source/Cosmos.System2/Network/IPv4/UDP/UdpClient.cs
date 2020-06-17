using System;
using Sys = System;
using System.Collections.Generic;

namespace Cosmos.System.Network
{
    /// <summary>
    /// UdpClient class. Used to manage the UDP connection to a client.
    /// </summary>
    public class UdpClient
    {
        // TODO: Once we support more than just IPv4, we really need to base all the IPv4 classes on abstract classes
        // that represent the required functionality, then we can generalize the stack to be independent from IPv4 or IPv6
        /// <summary>
        /// Datagram class.
        /// </summary>
        internal class DataGram
        {
            /// <summary>
            /// Data array.
            /// </summary>
            internal byte[] data;
            /// <summary>
            /// Source end point.
            /// </summary>
            internal IPv4.EndPoint source;

            /// <summary>
            /// Create new inctanse of the <see cref="DataGram"/> class.
            /// </summary>
            /// <param name="data">Data array.</param>
            /// <param name="src">Source end point.</param>
            internal DataGram(byte[] data, IPv4.EndPoint src)
            {
                this.data = data;
                this.source = src;
            }
        }

        /// <summary>
        /// Clients dictionary.
        /// </summary>
        private static TempDictionary<UdpClient> clients;

        /// <summary>
        /// Local port.
        /// </summary>
        protected Int32 localPort;
        /// <summary>
        /// Destination address.
        /// </summary>
        protected IPv4.Address destination;
        /// <summary>
        /// Destination port.
        /// </summary>
        protected Int32 destinationPort;

        /// <summary>
        /// RX buffer queue.
        /// </summary>
        private Queue<DataGram> rxBuffer;

        /// <summary>
        /// Assign clients dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static UdpClient()
        {
            clients = new TempDictionary<UdpClient>();
        }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>UdpClient</returns>
        internal static UdpClient Client(ushort destPort)
        {
            if (clients.ContainsKey((UInt32)destPort) == true)
            {
                return clients[(UInt32)destPort];
            }

            return null;
        }

        /// <summary>
        /// Create new inctanse of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient()
            :this(0)
        { }

        /// <summary>
        /// Create new inctanse of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public UdpClient(Int32 localPort)
        {
            this.rxBuffer = new Queue<DataGram>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                UdpClient.clients.Add((UInt32)localPort, this);
            }
        }

        /// <summary>
        /// Create new inctanse of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient(IPv4.Address dest, Int32 destPort)
            : this(0)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        /// <summary>
        /// Connect to client.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        public void Connect(IPv4.Address dest, Int32 destPort)
        {
            this.destination = dest;
            this.destinationPort = destPort;
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
            if ((this.destination == null) ||
                (this.destinationPort == 0))
            {
                throw new Exception("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, this.destination, this.destinationPort);
        }

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="data">Data array.</param>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        public void Send(byte[] data, IPv4.Address dest, Int32 destPort)
        {
            IPv4.Address source = IPv4.Config.FindNetwork(dest);
            IPv4.UDPPacket packet = new IPv4.UDPPacket(source, dest, (UInt16)this.localPort, (UInt16)destPort, data);

            Sys.Console.WriteLine("Sending " + packet.ToString());
            IPv4.OutgoingBuffer.AddPacket(packet);
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public void Close()
        {
            if (UdpClient.clients.ContainsKey((UInt32)this.localPort) == true)
            {
                UdpClient.clients.Remove((UInt32)this.localPort);
            }
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public byte[] Receive(ref IPv4.EndPoint source)
        {
            if (this.rxBuffer.Count < 1)
            {
                return null;
            }

            DataGram packet = rxBuffer.Dequeue();
            source.address = packet.source.address;
            source.port = packet.source.port;

            return packet.data;
        }

        /// <summary>
        /// Receive data from packet.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void receiveData(IPv4.UDPPacket packet)
        {
            byte[] data = packet.UDP_Data;
            IPv4.EndPoint source = new IPv4.EndPoint(packet.SourceIP, packet.SourcePort);

            Sys.Console.WriteLine("Received " + data.Length + " bytes data from " + source.ToString());

            this.rxBuffer.Enqueue(new DataGram(data, source));
        }
    }
}
