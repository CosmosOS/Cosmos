using System;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Manages the Cosmos networking stack.
    /// </summary>
    public static class NetworkStack
    {
        /// <summary>
        /// Debugger instance of the "System" ring, with the "NetworkStack" tag.
        /// </summary>
        public static readonly Debugger Debugger = new("Network Stack");

        /// <summary>
        /// Maps IP (Internet Protocol) addresses to network devices.
        /// </summary>
        internal static Dictionary<uint, NetworkDevice> AddressMap { get; private set; }

        /// <summary>
        /// Maps MAC addresses to network devices.
        /// </summary>
        internal static Dictionary<uint, NetworkDevice> MACMap { get; private set; }

        /// <summary>
        /// Initializes the network stack.
        /// </summary>
        public static void Initialize()
        {
            AddressMap = new Dictionary<uint, NetworkDevice>();
            MACMap = new Dictionary<uint, NetworkDevice>();
        }

        /// <summary>
        /// Sets the IP configuration for the given network device.
        /// </summary>
        /// <param name="nic">The target network device.</param>
        /// <param name="config">The IP configuration to assign to the device.</param>
        private static void SetConfigIP(NetworkDevice nic, IPConfig config)
        {
            NetworkConfiguration.AddConfig(nic, config);
            AddressMap.Add(config.IPAddress.Hash, nic);
            MACMap.Add(nic.MACAddress.Hash, nic);
            IPConfig.Add(config);
            nic.DataReceived = HandlePacket;
        }

        /// <summary>
        /// Configures an IP configuration on the given network device.
        /// </summary>
        /// <remarks>
        /// Multiple IP configurations can be made, similar to *nix environments.
        /// </remarks>
        ///
        /// <param name="nic">The <see cref="NetworkDevice"/> that will have the assigned configuration.</param>
        /// <param name="config">The <see cref="Config"/> instance that defines the IP Address, Subnet Mask and Default Gateway for the device</param>
        ///
        /// <exception cref="ArgumentException">
        ///     <list type="bullet">
        ///         <item>Thrown if configuration with the given config.IPAddress.Hash already exists.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        public static void ConfigIP(NetworkDevice nic, IPConfig config)
        {
            if (NetworkConfiguration.ConfigsContainsDevice(nic))
            {
                RemoveIPConfig(nic);
                SetConfigIP(nic, config);
            }
            else
            {
                SetConfigIP(nic, config);
            }

            NetworkConfiguration.SetCurrentConfig(nic, config);
        }

        /// <summary>
        /// Check if the current network configuration is empty.
        /// </summary>
        public static bool ConfigEmpty()
        {
            if (NetworkConfiguration.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove all IP configurations.
        /// </summary>
        public static void RemoveAllConfigIP()
        {
            AddressMap.Clear();
            MACMap.Clear();
            IPConfig.RemoveAll();
            NetworkConfiguration.ClearConfigs();
        }

        /// <summary>
        /// Removes the IP configuration of a specific network device.
        /// </summary>
        /// <param name="nic">The target device.</param>
        public static void RemoveIPConfig(NetworkDevice nic)
        {
            IPConfig config = NetworkConfiguration.Get(nic);
            AddressMap.Remove(config.IPAddress.Hash);
            MACMap.Remove(nic.MACAddress.Hash);
            IPConfig.Remove(config);
            NetworkConfiguration.Remove(nic);
        }

        /// <summary>
        /// Handle a network packet.
        /// </summary>
        /// <param name="packetData">Packet data array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        public static void HandlePacket(byte[] packetData)
        {
            if (packetData == null)
            {
                Debugger.Send("Error packet data null");
                return;
            }

            ushort etherType = (ushort)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPPacket.ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPPacket.IPv4Handler(packetData);
                    break;
            }
        }

        /// <summary>
        /// Updates the network stack. This method should be called continously by
        /// other parts of the network stack.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if data length of any packet in the queue is bigger than <see cref="Int32.MaxValue"/>.</exception>
        public static void Update()
        {
            OutgoingBuffer.Send();
        }
    }
}