/*
* PROJECT:          Aura Operating System Development
* CONTENT:          UDP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.System.Network.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4.UDP
{
    /// <summary>
    /// UdpClient class. Used to manage the UDP connection to a client.
    /// </summary>
    public class UdpClient : IDisposable
    {
        /// <summary>
        /// Clients dictionary.
        /// </summary>
        private static Dictionary<uint, UdpClient> clients;

        /// <summary>
        /// Local port.
        /// </summary>
        private int localPort;
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
        internal Queue<UDPPacket> rxBuffer;

        /// <summary>
        /// Assign clients dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static UdpClient()
        {
            clients = new Dictionary<uint, UdpClient>();
        }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="destPort">Destination port.</param>
        /// <returns>UdpClient</returns>
        internal static UdpClient GetClient(ushort destPort)
        {
            UdpClient client;

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
        /// Create new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient()
            : this(0)
        { }

        /// <summary>
        /// Create new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="localPort">Local port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
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
        /// Create new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        /// <param name="destPort">Destination port.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 0 exists.</exception>
        public UdpClient(Address dest, int destPort)
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
        public void Connect(Address dest, int destPort)
        {
            destination = dest;
            destinationPort = destPort;
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public void Close()
        {
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
            if (destination == null || destinationPort == 0)
            {
                throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, destination, destinationPort);
            NetworkStack.Update();
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
        public void Send(byte[] data, Address dest, int destPort)
        {
            Address source = IPConfig.FindNetwork(dest);
            var packet = new UDPPacket(source, dest, (ushort)localPort, (ushort)destPort, data);
            OutgoingBuffer.AddPacket(packet);
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

            var packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            return packet.UDP_Data;
        }

        /// <summary>
        /// Receive data from end point.
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <returns>byte array value.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public byte[] Receive(ref EndPoint source)
        {
            while (rxBuffer.Count < 1) ;

            var packet = new UDPPacket(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;
            source.Port = packet.SourcePort;

            return packet.UDP_Data;
        }

        /// <summary>
        /// Receive data from packet.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        internal void ReceiveData(UDPPacket packet)
        {
            rxBuffer.Enqueue(packet);
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
