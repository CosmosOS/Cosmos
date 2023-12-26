/*
* PROJECT:          Aura Operating System Development
* CONTENT:          DHCP - DHCP Core
* PROGRAMMER(S):    Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*/

using Cosmos.System.Network.IPv4;
using System;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.Config;
using System.Collections.Generic;
using Cosmos.HAL;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// Used to manage the DHCP connection to a server.
    /// </summary>
    public class DHCPClient : UdpClient
    {
        /// <summary>
        /// Is DHCP ascked check variable
        /// </summary>
        private bool applied = false;

        /// <summary>
        /// Gets the IP address of the DHCP server.
        /// </summary>
        public static Address DHCPServerAddress(NetworkDevice networkDevice)
        {
            return NetworkConfiguration.Get(networkDevice).DefaultGateway;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPClient"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
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

            var packet = new DHCPPacket(rxBuffer.Dequeue().RawData);

            if (packet.MessageType == 2) //Boot Reply
            {
                if (packet.RawData[284] == 0x02) //Offer packet received
                {
                    NetworkStack.Debugger.Send("Offer received.");
                    return SendRequestPacket(packet.Client);
                }
                else if (packet.RawData[284] == 0x05 || packet.RawData[284] == 0x06) //ACK or NAK DHCP packet received
                {
                    if (applied == false)
                    {
                        Apply(packet, true);

                        Close();
                    }
                }
            }

            return second;
        }

        /// <summary>
        /// Sends a packet to the DHCP server in order to make the address available again.
        /// </summary>
        public void SendReleasePacket()
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                Address source = IPConfig.FindNetwork(DHCPServerAddress(networkDevice));
                var dhcpRelease = new DHCPRelease(source, DHCPServerAddress(networkDevice), networkDevice.MACAddress);

                OutgoingBuffer.AddPacket(dhcpRelease);
                NetworkStack.Update();

                NetworkStack.RemoveAllConfigIP();

                IPConfig.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));
            }

            Close();
        }

        /// <summary>
        /// Send a packet to find the DHCP server and inform the host that we
        /// are requesting a new IP address.
        /// </summary>
        /// <returns>The amount of time elapsed, or -1 if a timeout has been reached.</returns>
        public int SendDiscoverPacket()
        {
            NetworkStack.RemoveAllConfigIP();

            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                IPConfig.Enable(networkDevice, new Address(0, 0, 0, 0), new Address(0, 0, 0, 0), new Address(0, 0, 0, 0));

                var dhcpDiscover = new DHCPDiscover(networkDevice.MACAddress);
                OutgoingBuffer.AddPacket(dhcpDiscover);
                NetworkStack.Update();

                applied = false;
            }

            return Receive();
        }

        /// <summary>
        /// Sends a request to apply the new IP configuration.
        /// </summary>
        /// <returns>The amount of time elapsed, or -1 if a timeout has been reached.</returns>
        private int SendRequestPacket(Address requestedAddress)
        {
            foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
            {
                var dhcpRequest = new DHCPRequest(networkDevice.MACAddress, requestedAddress);
                OutgoingBuffer.AddPacket(dhcpRequest);
                NetworkStack.Update();
            }
            return Receive();
        }

        /*
         * Method called to applied the differents options received in the DHCP packet ACK
         **/
        /// <summary>
        /// Applies the newly received IP configuration.
        /// </summary>
        /// <param name="message">Enable/Disable the displaying of messages about DHCP applying and conf. Disabled by default.
        /// </param>
        private void Apply(DHCPPacket packet, bool message = false)
        {
            if (applied == false)
            {
                NetworkStack.RemoveAllConfigIP();

                //cf. Roadmap. (have to change this, because some network interfaces are not configured in dhcp mode) [have to be done in 0.5.x]
                foreach (NetworkDevice networkDevice in NetworkDevice.Devices)
                {
                    if (packet.Client.ToString() == null)
                    {
                        throw new Exception("Parsing DHCP ACK Packet failed, can't apply network configuration.");
                    }
                    else
                    {
                        HAL.Global.debugger.Send("[DHCP ACK][" + networkDevice.Name + "] Packet received, applying IP configuration...");
                        HAL.Global.debugger.Send("   IP Address  : " + packet.Client.ToString());
                        HAL.Global.debugger.Send("   Subnet mask : " + packet.Subnet.ToString());
                        HAL.Global.debugger.Send("   Gateway     : " + packet.Server.ToString());
                        HAL.Global.debugger.Send("   DNS server  : " + packet.DNS.ToString());

                        IPConfig.Enable(networkDevice, packet.Client, packet.Subnet, packet.Server);
                        DNSConfig.Add(packet.DNS);

                        HAL.Global.debugger.Send("[DHCP CONFIG][" + networkDevice.Name + "] IP configuration applied.");

                        applied = true;

                        return;
                    }
                }

                HAL.Global.debugger.Send("[DHCP CONFIG] No DHCP Config applied!");
            }
            else
            {
                HAL.Global.debugger.Send("[DHCP CONFIG] DHCP already applied.");
            }
        }
    }
}