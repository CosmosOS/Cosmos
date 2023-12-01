using Cosmos.System.Network.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.System.Network.IPv4.UDP
{
    /// <summary>
    /// Used to manage the UDP connection to a client.
    /// </summary>
    public class UdpClient : IDisposable
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
                if (!clients.ContainsKey(port)) return port;

            }

            return 0;
        }
        private readonly static Dictionary<uint, UdpClient> clients;
        private readonly int localPort;
        private int destinationPort;

        /// <summary>
        /// The destination address.
        /// </summary>
        internal Address destination;

        /// <summary>
        /// The RX buffer queue.
        /// </summary>
        internal Queue<UDPPacket> rxBuffer;

        static UdpClient()
        {
            clients = new Dictionary<uint, UdpClient>();
        }

        /// <summary>
        /// Gets a UDP client running on the given port.
        /// </summary>
        /// <param name="destPort">The destination port.</param>
        /// <returns>If a client is running on the given port, the <see cref="UdpClient"/>; otherwise, <see langword="null"/>.</returns>
        internal static UdpClient GetClient(ushort destPort)
        {
            if (clients.TryGetValue(destPort, out var client))
            {
                return client;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient()
            : this(0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if localPort already exists.</exception>
        public UdpClient(int localPort)
        {
            rxBuffer = new Queue<UDPPacket>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                clients.Add((uint)localPort, this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient(Address dest, int destPort)
            : this(0)
        {
            destination = dest;
            destinationPort = destPort;
        }

        /// <summary>
        /// Connects to the given client.
        /// </summary>
        /// <param name="dest">The destination address.</param>
        /// <param name="destPort">The destination port.</param>
        public void Connect(Address dest, int destPort)
        {
            destination = dest;
            destinationPort = destPort;
        }

        /// <summary>
        /// Closes the active connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void Close()
        {
            if (clients.ContainsKey((uint)localPort) == true)
            {
                clients.Remove((uint)localPort);
            }
        }

        /// <summary>
        /// Sends data to the client.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <exception cref="Exception">Thrown if destination is null or destinationPort is 0.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        public void Send(byte[] data)
        {
            if (destination == null || destinationPort == 0)
            {
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, destination, destinationPort);
            NetworkStack.Update();
        }

        /// <summary>
        /// Sends data to a remote host.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="dest">The destination address.</param>
        /// <param name="destPort">The destination port.</param>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown if data array length is greater than Int32.MaxValue.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        public void Send(byte[] data, Address dest, int destPort)
        {
            Address source = IPConfig.FindNetwork(dest);
            var packet = new UDPPacket(source, dest, (ushort)localPort, (ushort)destPort, data);
            OutgoingBuffer.AddPacket(packet);
        }

        /// <summary>
        /// Receives data from the given end-point.
        /// </summary>
        /// <param name="source">The source end point.</param>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        public byte[] NonBlockingReceive(ref EndPoint source)
        {
            if (rxBuffer.Count < 1)
            {
                return null;
            }

            var packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            return packet.UDPData;
        }

        /// <summary>
        /// Receives data from the given end-point.
        /// </summary>
        /// <param name="source">The source end point.</param>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1)
            {
                ;
            }

            var packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            return packet.UDPData;
        }

        /// <summary>
        /// Receives data from the given packet.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(UDPPacket packet)
        {
            rxBuffer.Enqueue(packet);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
