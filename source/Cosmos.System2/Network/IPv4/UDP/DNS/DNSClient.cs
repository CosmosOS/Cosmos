using Cosmos.System.Network.Config;
using Cosmos.HAL;
using System;

namespace Cosmos.System.Network.IPv4.UDP.DNS
{
    /// <summary>
    /// Used to manage a DNS connection to a server.
    /// </summary>
    public class DnsClient : UdpClient
    {
        private string queryUrl;

        /// <summary>
        /// Create new instance of the <see cref="DnsClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public DnsClient() : base(53)
        {
        }

        /// <summary>
        /// Connects to a client.
        /// </summary>
        /// <param name="address">The destination address.</param>
        public void Connect(Address address)
        {
            Connect(address, 53);
        }

        /// <summary>
        /// Sends a DNS query for the given domain name string.
        /// </summary>
        /// <param name="url">The domain name string to query the DNS for.</param>
        public void SendAsk(string url)
        {
            Address source = IPConfig.FindNetwork(destination);
            queryUrl = url;
            var askpacket = new DNSPacketAsk(source, destination, url);

            OutgoingBuffer.AddPacket(askpacket);
            NetworkStack.Update();
        }

        /// <summary>
        /// Receives data from the DNS remote host.
        /// </summary>
        /// <param name="timeout">The timeout value - by default 5000ms.</param>
        /// <returns>The address corresponding to the previously specified domain name.</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error.</exception>
        public Address Receive(int timeout = 5000)
        {
            int second = 0;
            int deltaT = 0;

            while (rxBuffer.Count < 1)
            {
                if (second > timeout / 1000)
                {
                    return null;
                }
                if (deltaT != RTC.Second)
                {
                    second++;
                    deltaT = RTC.Second;
                }
            }

            var packet = new DNSPacketAnswer(rxBuffer.Dequeue().RawData);

            if ((ushort)(packet.DNSFlags & 0x0F) == (ushort)ReplyCode.OK)
            {
                if (packet.Queries.Count > 0 && packet.Queries[0].Name == queryUrl)
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
