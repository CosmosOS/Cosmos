// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.System.Network.Protocols.Arp;

namespace Cosmos.Kernel.System.Network.Protocols.IPv4;

/// <summary>
/// The outgoing IPv4 queue: a broadcast leaves at once, any other packet
/// waits for ARP to resolve the MAC address of its destination, or of its
/// gateway when the destination is off the link.
/// </summary>
internal static class IPv4OutgoingBuffer
{
    private sealed class BufferEntry
    {
        internal enum EntryStatus
        {
            Added,
            ArpSent,
            RouteArpSent,
            JustSend,
            Done,
            DhcpRequest
        }

        public INetworkDevice NIC { get; }
        public IPPacket Packet { get; }
        public EntryStatus Status { get; set; }
        public Address? NextHop { get; set; }

        public BufferEntry(INetworkDevice nic, IPPacket packet)
        {
            NIC = nic;
            Packet = packet;

            if (Packet.DestinationIP.IsBroadcastAddress)
            {
                Status = EntryStatus.DhcpRequest;
            }
            else
            {
                Status = EntryStatus.Added;
            }
        }
    }

    /// <summary>Spins of the send loop before the queue is abandoned.</summary>
    private const int MaxIterations = 10_000;

    /// <summary>
    /// The queue. Initialized eagerly to avoid issues with interrupt context.
    /// </summary>
    private static readonly List<BufferEntry> s_queue = [];

    /// <summary>
    /// Adds a packet to the buffer, resolving the sending device from the
    /// packet's source address.
    /// </summary>
    /// <param name="packet">The IP packet.</param>
    /// <returns>False when no configured interface matches the packet's source address.</returns>
    public static bool AddPacket(IPPacket packet)
    {
        INetworkDevice? device = IPConfig.FindInterface(packet.SourceIP);
        if (device is null)
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
        packet.SourceMac = device.MacAddress;
        s_queue.Add(new BufferEntry(device, packet));
    }

    /// <summary>
    /// Sends packets from the buffer.
    /// </summary>
    internal static void Send()
    {
        int iterations = 0;

        while (s_queue.Count > 0)
        {
            iterations++;
            if (iterations >= MaxIterations)
            {
                Serial.WriteString("[IPv4OutgoingBuffer] ARP timeout\n");
                s_queue.Clear();
                break;
            }

            for (int e = s_queue.Count - 1; e >= 0; e--)
            {
                BufferEntry entry = s_queue[e];
                if (entry.Status == BufferEntry.EntryStatus.Added)
                {
                    if (!IPConfig.IsLocalAddress(entry.Packet.DestinationIP))
                    {
                        Address? nextHop = IPConfig.FindRoute(entry.Packet.DestinationIP);
                        entry.NextHop = nextHop;
                        if (nextHop is null)
                        {
                            s_queue.RemoveAt(e);
                            continue;
                        }

                        MacAddress? nextHopMac = ArpCache.Resolve(nextHop);
                        if (nextHopMac is not null)
                        {
                            entry.Packet.DestinationMac = nextHopMac;
                            entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                            s_queue.RemoveAt(e);
                        }
                        else
                        {
                            ArpRequestEthernet arpRequest = new(
                                entry.NIC.MacAddress,
                                entry.Packet.SourceIP,
                                MacAddress.Broadcast,
                                nextHop,
                                MacAddress.None
                            );
                            entry.NIC.Send(arpRequest.RawData, arpRequest.RawData.Length);
                            entry.Status = BufferEntry.EntryStatus.RouteArpSent;
                        }
                        continue;
                    }

                    MacAddress? cachedMac = ArpCache.Resolve(entry.Packet.DestinationIP);
                    if (cachedMac is not null)
                    {
                        entry.Packet.DestinationMac = cachedMac;
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        Serial.WriteString("[IPv4OutgoingBuffer] Sent via ARP cache\n");
                        s_queue.RemoveAt(e);
                    }
                    else
                    {
                        Serial.WriteString("[IPv4OutgoingBuffer] Sending ARP request\n");
                        ArpRequestEthernet arpRequest = new(
                            entry.NIC.MacAddress,
                            entry.Packet.SourceIP,
                            MacAddress.Broadcast,
                            entry.Packet.DestinationIP,
                            MacAddress.None
                        );
                        bool sent = entry.NIC.Send(arpRequest.RawData, arpRequest.RawData.Length);
                        Serial.WriteString("[IPv4OutgoingBuffer] ARP send result: ");
                        Serial.WriteString(sent ? "OK" : "FAIL");
                        Serial.WriteString(" len=");
                        Serial.WriteNumber((ulong)arpRequest.RawData.Length);
                        Serial.WriteString("\n");
                        entry.Status = BufferEntry.EntryStatus.ArpSent;
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ArpSent)
                {
                    MacAddress? repliedMac = ArpCache.Resolve(entry.Packet.DestinationIP);
                    if (repliedMac is not null)
                    {
                        entry.Packet.DestinationMac = repliedMac;
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        s_queue.RemoveAt(e);
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.RouteArpSent)
                {
                    MacAddress? routedMac = entry.NextHop is null ? null : ArpCache.Resolve(entry.NextHop);
                    if (routedMac is not null)
                    {
                        entry.Packet.DestinationMac = routedMac;
                        entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                        s_queue.RemoveAt(e);
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.DhcpRequest)
                {
                    entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                    s_queue.RemoveAt(e);
                }
                else if (entry.Status == BufferEntry.EntryStatus.JustSend)
                {
                    entry.NIC.Send(entry.Packet.RawData, entry.Packet.RawData.Length);
                    s_queue.RemoveAt(e);
                }
            }

            // Spin to allow interrupt processing (ARP replies)
            if (s_queue.Count > 0)
            {
                Thread.SpinWait(10_000);
            }
        }
    }

    /// <summary>
    /// Hands an ARP reply to the queued entries waiting on it: each one whose
    /// destination, or next hop, is the reply's sender takes the sender's MAC
    /// address and leaves on the next pass of <see cref="Send"/>.
    /// </summary>
    /// <param name="arpReply">The ARP reply.</param>
    internal static void UpdateARPCache(ArpReplyEthernet arpReply)
    {
        for (int e = 0; e < s_queue.Count; e++)
        {
            BufferEntry entry = s_queue[e];
            if (entry.Status == BufferEntry.EntryStatus.ArpSent)
            {
                if (entry.Packet.DestinationIP.CompareTo(arpReply.SenderIP) == 0)
                {
                    entry.Packet.DestinationMac = arpReply.SenderMac;
                    entry.Status = BufferEntry.EntryStatus.JustSend;
                }
            }
            else if (entry.Status == BufferEntry.EntryStatus.RouteArpSent)
            {
                if (entry.NextHop?.CompareTo(arpReply.SenderIP) == 0)
                {
                    entry.Packet.DestinationMac = arpReply.SenderMac;
                    entry.Status = BufferEntry.EntryStatus.JustSend;
                }
            }
        }
    }
}
