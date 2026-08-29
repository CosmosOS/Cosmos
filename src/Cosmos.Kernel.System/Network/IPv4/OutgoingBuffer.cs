using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.ARP;
using Cosmos.Kernel.System.Network.Config;

namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// Represents an outgoing IPv4 buffer. for use by drivers
/// </summary>
internal static class OutgoingBuffer
{
    private class BufferEntry
    {
        internal enum EntryStatus
        {
            ADDED,
            ARP_SENT,
            ROUTE_ARP_SENT,
            JUST_SEND,
            DONE,
            DHCP_REQUEST
        };

        public INetworkDevice NIC;
        public IPPacket Packet;
        public EntryStatus Status;
        public Address? NextHop;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferEntry"/> class.
        /// </summary>
        /// <param name="nic">The network device.</param>
        /// <param name="packet">The IP packet.</param>
        public BufferEntry(INetworkDevice nic, IPPacket packet)
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
    /// The buffer s_queue. Initialized eagerly to avoid issues with interrupt context.
    /// </summary>
    private static List<BufferEntry> s_queue = new();

    /// <summary>
    /// Ensures the s_queue exists and is initialized.
    /// </summary>
    private static void EnsureQueueExists()
    {
        // Queue is now initialized at class load time, but keep this for safety
        s_queue ??= new List<BufferEntry>();
    }

    /// <summary>
    /// Adds a packet to the buffer, resolving the sending device from the
    /// packet's source address.
    /// </summary>
    /// <param name="packet">The IP packet.</param>
    /// <returns>False when no configured interface matches the packet's source address.</returns>
    public static bool AddPacket(IPPacket packet)
    {
        var device = IPConfig.FindInterface(packet.SourceIP);
        if (device == null)
        {
            return false;
        }

        AddPacket(packet, device);
        return true;
    }

    /// <summary>
    /// Adds a packet to the buffer.
    /// </summary>
    /// <param name="packet">The IP packet.</param>
    /// <param name="device">The Network Interface Controller.</param>
    public static void AddPacket(IPPacket packet, INetworkDevice device)
    {
        EnsureQueueExists();
        packet.SourceMac = device.MacAddress;
        s_queue.Add(new BufferEntry(device, packet));
    }

    /// <summary>
    /// Sends packets from the buffer.
    /// </summary>
    internal static void Send()
    {
        EnsureQueueExists();
        int iterations = 0;
        int maxIterations = 10000; // Spin-based timeout

        while (s_queue.Count > 0)
        {
            iterations++;
            if (iterations >= maxIterations)
            {
                Serial.WriteString("[OutgoingBuffer] ARP timeout\n");
                s_queue.Clear();
                break;
            }

            for (int e = s_queue.Count - 1; e >= 0; e--)
            {
                BufferEntry entry = s_queue[e];
                if (entry.Status == BufferEntry.EntryStatus.ADDED)
                {
                    if (IPConfig.IsLocalAddress(entry.Packet.DestinationIP) == false)
                    {
                        entry.NextHop = IPConfig.FindRoute(entry.Packet.DestinationIP);
                        if (entry.NextHop == null)
                        {
                            s_queue.RemoveAt(e);
                            continue;
                        }

                        if (ArpCache.Contains(entry.NextHop))
                        {
                            entry.Packet.DestinationMac = ArpCache.Resolve(entry.NextHop ?? throw new Exception($"{nameof(entry.NextHop)} can not be null"))
                                ?? throw new Exception("DestinationMac can not be null");
                            entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                            s_queue.RemoveAt(e);
                        }
                        else
                        {
                            var arpRequest = new ArpRequestEthernet(
                                entry.NIC.MacAddress,
                                entry.Packet.SourceIP,
                                MACAddress.Broadcast,
                                entry.NextHop,
                                MACAddress.None
                            );
                            entry.NIC.Send(arpRequest.RawData, arpRequest.RawData.Length);
                            entry.Status = BufferEntry.EntryStatus.ROUTE_ARP_SENT;
                        }
                        continue;
                    }

                    if (ArpCache.Contains(entry.Packet.DestinationIP))
                    {
                        entry.Packet.DestinationMac = ArpCache.Resolve(entry.Packet.DestinationIP)
                                                      ?? throw new Exception("DestinationMac can not be null");
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        Serial.WriteString("[OutgoingBuffer] Sent via ARP cache\n");
                        s_queue.RemoveAt(e);
                    }
                    else
                    {
                        Serial.WriteString("[OutgoingBuffer] Sending ARP request\n");
                        var arpRequest = new ArpRequestEthernet(
                            entry.NIC.MacAddress,
                            entry.Packet.SourceIP,
                            MACAddress.Broadcast,
                            entry.Packet.DestinationIP,
                            MACAddress.None
                        );
                        bool sent = entry.NIC.Send(arpRequest.RawData, arpRequest.RawData.Length);
                        Serial.WriteString("[OutgoingBuffer] ARP send result: ");
                        Serial.WriteString(sent ? "OK" : "FAIL");
                        Serial.WriteString(" len=");
                        Serial.WriteNumber((ulong)arpRequest.RawData.Length);
                        Serial.WriteString("\n");
                        entry.Status = BufferEntry.EntryStatus.ARP_SENT;
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ARP_SENT)
                {
                    if (ArpCache.Contains(entry.Packet.DestinationIP))
                    {
                        entry.Packet.DestinationMac = ArpCache.Resolve(entry.Packet.DestinationIP)
                            ?? throw new Exception("DestinationMac can not be null");
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        s_queue.RemoveAt(e);
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ROUTE_ARP_SENT)
                {
                    if (entry.NextHop is not null && ArpCache.Contains(entry.NextHop))
                    {
                        entry.Packet.DestinationMac = ArpCache.Resolve(entry.NextHop)
                                                      ?? throw new Exception("DestinationMac can not be null");
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        s_queue.RemoveAt(e);
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.DHCP_REQUEST)
                {
                    entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                    s_queue.RemoveAt(e);
                }
                else if (entry.Status == BufferEntry.EntryStatus.JUST_SEND)
                {
                    entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                    s_queue.RemoveAt(e);
                }
            }

            // Spin to allow interrupt processing (ARP replies)
            if (s_queue.Count > 0)
            {
                Thread.SpinWait(10000);
            }
        }
    }

    /// <summary>
    /// Updates the ARP cache with the given ARP reply.
    /// </summary>
    /// <param name="arpReply">The ARP reply.</param>
    internal static void UpdateARPCache(ArpReplyEthernet arpReply)
    {
        EnsureQueueExists();
        for (int e = 0; e < s_queue.Count; e++)
        {
            BufferEntry entry = s_queue[e];
            if (entry.Status == BufferEntry.EntryStatus.ARP_SENT)
            {
                if (entry.Packet.DestinationIP.CompareTo(arpReply.SenderIP) == 0)
                {
                    entry.Packet.DestinationMac = arpReply.SenderMac;
                    entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                }
            }
            else if (entry.Status == BufferEntry.EntryStatus.ROUTE_ARP_SENT)
            {
                if (entry.NextHop?.CompareTo(arpReply.SenderIP) == 0)
                {
                    entry.Packet.DestinationMac = arpReply.SenderMac;
                    entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                }
            }
        }
    }
}
