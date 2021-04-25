/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network Intialization + Packet Handler
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Implement a Network Stack for all network devices and protocols
    /// </summary>
    public static class NetworkStack
    {
        /// <summary>
        /// Debugger instance of the "System" ring, with the "NetworkStack" tag.
        /// </summary>
        public static Debugger debugger = new Debugger("System", "NetworkStack");

        /// <summary>
        /// Get address dictionary.
        /// </summary>
        internal static Dictionary<uint, NetworkDevice> AddressMap { get; private set; }
        /// <summary>
        /// Get address dictionary.
        /// </summary>
        internal static Dictionary<uint, NetworkDevice> MACMap { get; private set; }

        /// <summary>
        /// Initialize the Network Stack to prepare it for operation.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public static void Init()
        {
            AddressMap = new Dictionary<uint, NetworkDevice>();
            MACMap = new Dictionary<uint, NetworkDevice>();
        }

        /// <summary>
        /// Set ConfigIP for NetworkDevice
        /// </summary>
        /// <param name="nic">Network device.</param>
        ///  /// <param name="config">IP Config</param>
        private static void SetConfigIP(NetworkDevice nic, IPConfig config)
        {
            NetworkConfig.Add(nic, config);
            AddressMap.Add(config.IPAddress.Hash, nic);
            MACMap.Add(nic.MACAddress.Hash, nic);
            IPConfig.Add(config);
            nic.DataReceived = HandlePacket;
        }

        /// <summary>
        /// Configure a IP configuration on the given network device.
        /// <remarks>Multiple IP Configurations can be made, like *nix environments</remarks>
        /// </summary>
        /// <param name="nic"><see cref="NetworkDevice"/> that will have the assigned configuration</param>
        /// <param name="config"><see cref="Config"/> instance that defines the IP Address, Subnet
        /// Mask and Default Gateway for the device</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if configuration with the given config.IPAddress.Hash already exists.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        public static void ConfigIP(NetworkDevice nic, IPConfig config)
        {
            if (NetworkConfig.ContainsKey(nic))
            {
                RemoveIPConfig(nic);
                SetConfigIP(nic, config);
            }
            else
            {
                SetConfigIP(nic, config);
            }
            NetworkConfig.CurrentConfig = new KeyValuePair<NetworkDevice, IPConfig>(nic, config);
        }

        /// <summary>
        /// Check if Config is empty
        /// <returns>bool value.</returns>
        /// </summary>
        public static bool ConfigEmpty()
        {
            if (NetworkConfig.Keys.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove All IPConfig
        /// </summary>
        public static void RemoveAllConfigIP()
        {
            AddressMap.Clear();
            MACMap.Clear();
            IPConfig.RemoveAll();
            NetworkConfig.Clear();
        }

        /// <summary>
        /// Remove IPConfig
        /// </summary>
        /// <param name="nic">Network device.</param>
        public static void RemoveIPConfig(NetworkDevice nic)
        {
            IPConfig config = NetworkConfig.Get(nic);
            AddressMap.Remove(config.IPAddress.Hash);
            MACMap.Remove(nic.MACAddress.Hash);
            IPConfig.Remove(config);
            NetworkConfig.Remove(nic);
        }

        /// <summary>
        /// Handle packet.
        /// </summary>
        /// <param name="packetData">Packet data array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        internal static void HandlePacket(byte[] packetData)
        {
            if (packetData == null)
            {
                Global.mDebugger.Send("Error packet data null");
                return;
            }

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
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
        /// Called continously to keep the Network Stack going.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if data length of any packet in the queue is bigger than Int32.MaxValue.</exception>
        public static void Update()
        {
            OutgoingBuffer.Send();
        }
    }
}
