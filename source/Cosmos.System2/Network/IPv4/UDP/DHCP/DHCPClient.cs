using Cosmos.System.Network.IPv4;
using System;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.Config;
using System.Collections.Generic;
using Cosmos.HAL;

/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Core
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// DHCPClient class. Used to manage the DHCP connection to a server.
    /// </summary>
    public class DHCPClient : UdpClient
    {
        /// <summary>
        /// Is DHCP ascked check variable
        /// </summary>
        private bool asked = false;

        /// <summary>
        /// Get the IP address of the DHCP server
        /// </summary>
        /// <returns></returns>
        public static Address DHCPServerAddress(NetworkDevice networkDevice)
        {
            return NetworkConfiguration.Get(networkDevice).DefaultGateway;
        }

        /// <summary>
        /// Create new instance of the <see cref="DHCPClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentException">Thrown if UdpClient with localPort 53 exists.</exception>
        public DHCPClient() : base(68)
        {
        }

        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="timeout">timeout value, default 5000ms</param>
        /// <returns>time value (-1 = timeout)</returns>
        /// <exception cref="InvalidOperationException">Thrown on fatal error (contact support).</exception>
        private int Receive(int timeout = 5000)
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

            var packet = new DHCPPacket(rxBuffer.Dequeue().RawData);

            if (packet.MessageType == 2) //Boot Reply
            {
                if (packet.RawData[284] == 0x02) //Offer packet received
                {
                    Global.mDebugger.Send("Offer received.");
                    return SendRequestPacket(packet.Client);
                }
                else if (packet.RawData[284] == 0x05 || packet.RawData[284] == 0x06) //ACK or NAK DHCP packet received
                {
                    var ack = new DHCPAck(packet.RawData);
                    if (asked)
                    {
                        Apply(ack, true);
                    }
                    else
                    {
                        Apply(ack);
                    }
                }
            }

            return second;
        }

        /// <summary>
        /// Send a packet to the DHCP server to make the address available again
        /// </summary>
        public void SendReleasePacket()
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                Address source = IPConfig.FindNetwork(DHCPServerAddress(networkDevice));
                var dhcp_release = new DHCPRelease(source, DHCPServerAddress(networkDevice), networkDevice.MACAddress);

                OutgoingBuffer.AddPacket(dhcp_release);
                NetworkStack.Update();

                NetworkStack.RemoveAllConfigIP();

                IPConfig.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));
            }
            Close();
        }

        /// <summary>
        /// Send a packet to find the DHCP server and tell that we want a new IP address
        /// </summary>
        /// <returns>time value (-1 = timeout)</returns>
        public int SendDiscoverPacket()
        {
            NetworkStack.RemoveAllConfigIP();

            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                IPConfig.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));

                var dhcp_discover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcp_discover);
                NetworkStack.Update();

                asked = true;
            }

            return Receive();
        }

        /// <summary>
        /// Send a request to apply the new IP configuration
        /// </summary>
        /// <returns>time value (-1 = timeout)</returns>
        private int SendRequestPacket(Address RequestedAddress)
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                var dhcp_request = new DHCPRequest(networkDevice.MACAddress, RequestedAddress);
                OutgoingBuffer.AddPacket(dhcp_request);
                NetworkStack.Update();
            }
            return Receive();
        }

        /*
         * Method called to applied the differents options received in the DHCP packet ACK
         **/
        /// <summary>
        /// Apply the new IP configuration received.
        /// </summary>
        /// <param name="Options">DHCPOption class using the packetData from the received dhcp packet.</param>
        /// <param name="message">Enable/Disable the displaying of messages about DHCP applying and conf. Disabled by default.
        /// </param>
        private void Apply(DHCPAck packet, bool message = false)
        {
            NetworkStack.RemoveAllConfigIP();

            //cf. Roadmap. (have to change this, because some network interfaces are not configured in dhcp mode) [have to be done in 0.5.x]
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                if (packet.Client.ToString() == null ||
                    packet.Client.ToString() == null ||
                    packet.Client.ToString() == null ||
                    packet.Client.ToString() == null)
                {
                    throw new Exception("Parsing DHCP ACK Packet failed, can't apply network configuration.");
                }
                else
                {
                    if (message)
                    {
                        Global.mDebugger.Send("[DHCP ACK][" + networkDevice.Name + "] Packet received, applying IP configuration...");
                        Global.mDebugger.Send("   IP Address  : " + packet.Client.ToString());
                        Global.mDebugger.Send("   Subnet mask : " + packet.Subnet.ToString());
                        Global.mDebugger.Send("   Gateway     : " + packet.Server.ToString());
                        Global.mDebugger.Send("   DNS server  : " + packet.DNS.ToString());
                    }

                    IPConfig.Enable(networkDevice, packet.Client, packet.Subnet, packet.Server);
                    DNSConfig.Add(packet.DNS);

                    if (message)
                    {
                        Global.mDebugger.Send("[DHCP CONFIG][" + networkDevice.Name + "] IP configuration applied.");
                        asked = false;
                    }
                }
            }

            Close();
        }
    }
}
