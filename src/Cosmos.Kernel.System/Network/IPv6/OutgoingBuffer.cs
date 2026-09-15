// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// The outgoing IPv6 queue: a packet to a multicast group leaves at once, a
/// packet to a unicast neighbor waits for Neighbor Discovery to resolve its
/// MAC address, one solicitation per packet.
/// </summary>
internal static class OutgoingBuffer
{
    private sealed class Entry
    {
        internal Entry(INetworkDevice nic, IPv6Packet packet)
        {
            Nic = nic;
            Packet = packet;
        }

        internal INetworkDevice Nic { get; }

        internal IPv6Packet Packet { get; }

        internal bool SolicitationSent { get; set; }
    }

    /// <summary>Spins of the send loop before the queue is abandoned.</summary>
    private const int MaxIterations = 10000;

    /// <summary>
    /// The queue. Initialized eagerly, as the IPv4 queue is, so no class
    /// constructor runs from interrupt context.
    /// </summary>
    private static readonly List<Entry> s_queue = [];

    /// <summary>
    /// Queues a packet, resolving the sending device from the packet's source
    /// address.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <returns>False when no configured interface carries the packet's source address.</returns>
    internal static bool AddPacket(IPv6Packet packet)
    {
        if (!NetworkStack.AddressMap.TryGetValue(packet.SourceIP, out INetworkDevice? device))
        {
            return false;
        }

        packet.SourceMac = device.MacAddress;
        s_queue.Add(new Entry(device, packet));
        return true;
    }

    /// <summary>
    /// Sends what the queue holds, soliciting the neighbors it has no MAC
    /// address for and spinning until every packet left or the iteration
    /// cap is reached.
    /// </summary>
    internal static void Send()
    {
        int iterations = 0;

        while (s_queue.Count > 0)
        {
            iterations++;
            if (iterations >= MaxIterations)
            {
                Serial.WriteString("[IPv6] Neighbor resolution timeout\n");
                s_queue.Clear();
                break;
            }

            for (int e = s_queue.Count - 1; e >= 0; e--)
            {
                Entry entry = s_queue[e];
                IPv6Packet packet = entry.Packet;

                if (packet.DestinationIP.IsMulticast)
                {
                    entry.Nic.Send(packet.RawData, packet.RawData.Length);
                    s_queue.RemoveAt(e);
                    continue;
                }

                MACAddress? neighborMac = NeighborCache.Resolve(packet.DestinationIP);
                if (neighborMac is not null)
                {
                    packet.DestinationMac = neighborMac;
                    entry.Nic.Send(packet.RawData, packet.RawData.Length);
                    s_queue.RemoveAt(e);
                    continue;
                }

                if (!entry.SolicitationSent)
                {
                    Serial.WriteString("[IPv6] Sending neighbor solicitation for ");
                    Serial.WriteString(packet.DestinationIP.ToString());
                    Serial.WriteString("\n");

                    NeighborSolicitation solicitation = new(packet.SourceIP, packet.DestinationIP, entry.Nic.MacAddress);
                    entry.Nic.Send(solicitation.RawData, solicitation.RawData.Length);
                    entry.SolicitationSent = true;
                }
            }

            // Spin to allow interrupt processing (neighbor advertisements)
            if (s_queue.Count > 0)
            {
                Thread.SpinWait(10000);
            }
        }
    }
}
