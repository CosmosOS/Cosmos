/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ICMP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.System.Network.Config;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// ICMPClient class. Used to manage the ICMP connection to a client.
    /// </summary>
    public class ICMPClient : IDisposable
    {
        /// <summary>
        /// Clients dictionary.
        /// </summary>
        private static Dictionary<uint, ICMPClient> clients;

        /// <summary>
        /// Destination address.
        /// </summary>
        protected Address destination;

        /// <summary>
        /// RX buffer queue.
        /// </summary>
        protected Queue<ICMPPacket> rxBuffer;

        /// <summary>
        /// Assign clients dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        static ICMPClient()
        {
            clients = new Dictionary<uint, ICMPClient>();
        }

        /// <summary>
        /// Get client.
        /// </summary>
        /// <param name="iphash">IP Hash.</param>
        /// <returns>ICMPClient</returns>
        internal static ICMPClient GetClient(uint iphash)
        {
            ICMPClient client;

            if (clients.TryGetValue(iphash, out client))
            {
                return client;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="ICMPClient"/> class.
        /// </summary>
        public ICMPClient()
        {
            rxBuffer = new Queue<ICMPPacket>(8);
        }

        /// <summary>
        /// Connect to client.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        public void Connect(Address dest)
        {
            destination = dest;
            clients.Add(dest.Hash, this);
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public void Close()
        {
            if (clients.ContainsKey(destination.Hash) == true)
            {
                clients.Remove(destination.Hash);
            }
        }

        /// <summary>
        /// Send ICMP Echo
        /// </summary>
        public void SendEcho()
        {
            Address source = IPConfig.FindNetwork(destination);
            var request = new ICMPEchoRequest(source, destination, 0x0001, 0x50); //this is working
            OutgoingBuffer.AddPacket(request); //Aura doesn't work when this is called.
            NetworkStack.Update();
        }

        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="source">Source end point.</param>
        /// <param name="timeout">timeout value, default 5000ms</param>
        /// <returns>Address from Domain Name</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public int Receive(ref EndPoint source, int timeout = 5000)
        {
            int second = 0;
            int _deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > timeout / 1000)
                {
                    return -1;
                }
                if (_deltaT != RTC.Second)
                {
                    second++;
                    _deltaT = RTC.Second;
                }
            }

            var packet = new ICMPEchoReply(rxBuffer.Dequeue().RawData);
            source.Address = packet.SourceIP;

            return second;
        }

        /// <summary>
        /// Receive data from packet.
        /// </summary>
        /// <param name="packet">Packet to receive.</param>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        public void ReceiveData(ICMPPacket packet)
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
