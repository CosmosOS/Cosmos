/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Network.Config;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4.UDP.DNS
{
    /// <summary>
    /// DnsClient class. Used to manage the DNS connection to a server.
    /// </summary>
    public class DnsClient : UdpClient
    {
        /// <summary>
        /// Domain Name query string
        /// </summary>
        private string queryurl;

        /// <summary>
        /// Create new instance of the <see cref="DnsClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public DnsClient() : base(53)
        {
        }

        /// <summary>
        /// Connect to client.
        /// </summary>
        /// <param name="dest">Destination address.</param>
        public void Connect(Address address)
        {
            Connect(address, 53);
        }

        /// <summary>
        /// Send DNS Ask for Domain Name string
        /// </summary>
        /// <param name="url">Domain Name string.</param>
        public void SendAsk(string url)
        {
            Address source = IPConfig.FindNetwork(destination);

            queryurl = url;

            var askpacket = new DNSPacketAsk(source, destination, url);

            OutgoingBuffer.AddPacket(askpacket);

            NetworkStack.Update();
        }

        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="timeout">timeout value, default 5000ms</param>
        /// <returns>Address from Domain Name</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        public Address Receive(int timeout = 5000)
        {
            int second = 0;
            int _deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > timeout / 1000)
                {
                    return null;
                }
                if (_deltaT != RTC.Second)
                {
                    second++;
                    _deltaT = RTC.Second;
                }
            }

            var packet = new DNSPacketAnswer(rxBuffer.Dequeue().RawData);

            if ((ushort)(packet.DNSFlags & 0x0F) == (ushort)ReplyCode.OK)
            {
                if (packet.Queries.Count > 0 && packet.Queries[0].Name == queryurl)
                {
                    if (packet.Answers.Count > 0 && packet.Answers[0].Address.Length == 4)
                    {
                        return new Address(packet.Answers[0].Address, 0);
                    }
                }
            }
            return null;
        }

    }
}
