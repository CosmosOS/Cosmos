using Cosmos.System.Network.Config;
using Cosmos.HAL;
using System;
using System.Collections.Generic;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Used to manage the ICMP connection to a client.
    /// </summary>
    public class ICMPClient : IDisposable
    {
        private readonly static Dictionary<uint, ICMPClient> clients;

        /// <summary>
        /// The destination address.
        /// </summary>
        protected Address destination;

        /// <summary>
        /// The RX buffer queue.
        /// </summary>
        protected Queue<ICMPPacket> rxBuffer;

        static ICMPClient()
        {
            clients = new Dictionary<uint, ICMPClient>();
        }

        /// <summary>
        /// Gets a client by its IP address hash.
        /// </summary>
        /// <param name="iphash">IP Hash.</param>
        internal static ICMPClient GetClient(uint iphash)
        {
            if (clients.TryGetValue(iphash, out var client)) {
                return client;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ICMPClient"/> class.
        /// </summary>
        public ICMPClient()
        {
            rxBuffer = new Queue<ICMPPacket>(8);
        }

        /// <summary>
        /// Connects to the given client.
        /// </summary>
        /// <param name="dest">The destination address.</param>
        public void Connect(Address dest)
        {
            destination = dest;
            clients.Add(dest.Hash, this);
        }

        /// <summary>
        /// Closes the active connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void Close()
        {
            if (clients.ContainsKey(destination.Hash) == true)
            {
                clients.Remove(destination.Hash);
            }
        }

        /// <summary>
        /// Sends an ICMP echo.
        /// </summary>
        public void SendEcho()
        {
            Address source = IPConfig.FindNetwork(destination);
            var request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
            OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
            NetworkStack.Update();
        }

        /// <summary>
        /// Receives data from the remote host.
        /// </summary>
        /// <param name="source">The source end point.</param>
        /// <param name="timeout">The timeout value; by default, 5000ms.</param>
        /// <returns>The address from the domain name.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        public int Receive(ref EndPoint source, int timeout = 5000)
        {
            int second = 0;
            int deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > timeout / 1000)
                {
                    return -1;
                }

                if (deltaT != RTC.Second)
                {
                    second++;
                    deltaT = RTC.Second;
                }
            }

            var packet = new ICMPEchoReply(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;

            return second;
        }

        /// <summary>
        /// Receives data from the given packet.
        /// </summary>
        /// <param name="packet">The packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        public void ReceiveData(ICMPPacket packet)
        {
            rxBuffer.Enqueue(packet);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
