using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.ARP;
using System;
using Cosmos.System.Network.Config;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Represents an outgoing IPv4 buffer. for use by drivers
    /// </summary>
    public static class OutgoingBuffer
    {
        private class BufferEntry
        {
            public enum EntryStatus
            {
                ADDED,
                ARP_SENT,
                ROUTE_ARP_SENT,
                JUST_SEND,
                DONE,
                DHCP_REQUEST
            };

            public NetworkDevice NIC;
            public IPPacket Packet;
            public EntryStatus Status;
            public Address NextHop;

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferEntry"/> class.
            /// </summary>
            /// <param name="nic">The network device.</param>
            /// <param name="packet">The IP packet.</param>
            public BufferEntry(NetworkDevice nic, IPPacket packet)
            {
                this.NIC = nic;
                this.Packet = packet;

                if (Packet.DestinationIP.IsBroadcastAddress())
                {
                    this.Status = EntryStatus.DHCP_REQUEST;
                }
                else
                {
                    this.Status = EntryStatus.ADDED;
                }
            }
        }

        /// <summary>
        /// The buffer queue.
        /// </summary>
        private static List<BufferEntry> queue;

        /// <summary>
        /// Ensures the queue exists and is initialized.
        /// </summary>
        private static void EnsureQueueExists()
        {
            if (queue == null)
            {
                queue = new List<BufferEntry>();
            }
        }

        /// <summary>
        /// Adds a packet to the buffer. for use by drivers
        /// </summary>
        /// <param name="packet">The IP packet.</param>
        public static void AddPacket(IPPacket packet) =>
            AddPacket(packet, IPConfig.FindInterface(packet.SourceIP));

        /// <summary>
        /// Adds a packet to the buffer. for use by drivers
        /// </summary>
        /// <param name="packet">The IP packet.</param>
        /// <param name="device">The Network Interface Controller.</param>
        public static void AddPacket(IPPacket packet, NetworkDevice device)
        {
            EnsureQueueExists();
            packet.SourceMAC = device.MACAddress;
            queue.Add(new BufferEntry(device, packet));
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown if RawData length is bigger than Int32.MaxValue.</exception>
        internal static void Send()
        {
            EnsureQueueExists();
            int deltaT = 0;
            int second = 0;

            while (queue.Count > 0)
            {
                if (deltaT != RTC.Second)
                {
                    second++;
                    deltaT = RTC.Second;
                }

                if (second >= 4)
                {
                    NetworkStack.Debugger.Send("No response in 4 secondes...");
                    break;
                }

                for (int e = 0; e < queue.Count; e++)
                {
                    BufferEntry entry = queue[e];
                    if (entry.Status == BufferEntry.EntryStatus.ADDED)
                    {
                        if (IPConfig.IsLocalAddress(entry.Packet.DestinationIP) == false)
                        {
                            entry.NextHop = IPConfig.FindRoute(entry.Packet.DestinationIP);
                            if (entry.NextHop == null)
                            {
                                entry.Status = BufferEntry.EntryStatus.DONE;
                                continue;
                            }

                            if (ARPCache.Contains(entry.NextHop) == true)
                            {
                                entry.Packet.DestinationMAC = ARPCache.Resolve(entry.NextHop);
                                entry.NIC.QueueBytes(entry.Packet.RawData);
                                entry.Status = BufferEntry.EntryStatus.DONE;
                            }
                            else
                            {
                                var arpRequest = new ARPRequestEthernet(
                                    entry.NIC.MACAddress,
                                    entry.Packet.SourceIP,
                                    MACAddress.Broadcast,
                                    entry.NextHop,
                                    MACAddress.None
                                );

                                entry.NIC.QueueBytes(arpRequest.RawData);
                                entry.Status = BufferEntry.EntryStatus.ROUTE_ARP_SENT;
                            }

                            continue;
                        }

                        if (ARPCache.Contains(entry.Packet.DestinationIP) == true)
                        {
                            entry.Packet.DestinationMAC = ARPCache.Resolve(entry.Packet.DestinationIP);
                            entry.NIC.QueueBytes(entry.Packet.RawData);
                            entry.Status = BufferEntry.EntryStatus.DONE;
                        }
                        else
                        {
                            var arpRequest = new ARPRequestEthernet(
                                entry.NIC.MACAddress,
                                entry.Packet.SourceIP,
                                MACAddress.Broadcast,
                                entry.Packet.DestinationIP,
                                MACAddress.None
                            );

                            entry.NIC.QueueBytes(arpRequest.RawData);
                            entry.Status = BufferEntry.EntryStatus.ARP_SENT;
                        }
                    }
                    else if (entry.Status == BufferEntry.EntryStatus.DHCP_REQUEST)
                    {
                        entry.NIC.QueueBytes(entry.Packet.RawData);
                        entry.Status = BufferEntry.EntryStatus.DONE;
                    }
                    else if (entry.Status == BufferEntry.EntryStatus.JUST_SEND)
                    {
                        entry.NIC.QueueBytes(entry.Packet.RawData);
                        entry.Status = BufferEntry.EntryStatus.DONE;
                    }
                }
                int i = 0;
                while (i < queue.Count)
                {
                    if (queue[i].Status == BufferEntry.EntryStatus.DONE)
                    {
                        queue.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ARP cache with the given ARP reply.
        /// </summary>
        /// <param name="arpReply">The ARP reply.</param>
        /// <exception cref="sys.ArgumentException">Thrown if arpReply.SenderIP is not a IPv4Address.</exception>
        internal static void UpdateARPCache(ARPReplyEthernet arpReply)
        {
            EnsureQueueExists();
            //foreach (BufferEntry entry in queue)
            for (int e = 0; e < queue.Count; e++)
            {
                BufferEntry entry = queue[e];
                if (entry.Status == BufferEntry.EntryStatus.ARP_SENT)
                {
                    if (entry.Packet.DestinationIP.CompareTo(arpReply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arpReply.SenderMAC;
                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ROUTE_ARP_SENT)
                {
                    if (entry.NextHop.CompareTo(arpReply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arpReply.SenderMAC;
                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
            }
        }
    }
}
