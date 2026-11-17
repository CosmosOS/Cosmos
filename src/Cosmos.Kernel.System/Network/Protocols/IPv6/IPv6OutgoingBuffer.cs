// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Network;

namespace Cosmos.Kernel.System.Network.Protocols.IPv6;

/// <summary>
/// The outgoing IPv6 queue: a packet to a multicast group leaves at once, a
/// packet to a unicast neighbor waits for Neighbor Discovery to resolve its
/// MAC address, one solicitation per packet.
/// </summary>
internal static class IPv6OutgoingBuffer
{
    private sealed class Entry
    {
        internal INetworkDevice Nic { get; }

        internal IPv6Packet Packet { get; }

        internal bool SolicitationSent { get; set; }

        internal Entry(INetworkDevice nic, IPv6Packet packet)
        {
            Nic = nic;
            Packet = packet;
        }
    }

    /// <summary>Spins of the send loop before the queue is abandoned.</summary>
    private const int MaxIterations = 10_000;

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
        using (NetworkStackLock.Enter())
        {
            if (!NetworkStack.AddressMap.TryGetValue(packet.SourceIP, out INetworkDevice? device))
            {
                return false;
            }

            packet.SourceMac = device.MacAddress;
            s_queue.Add(new Entry(device, packet));
            return true;
        }
    }

    /// <summary>
    /// Sends what the queue holds, soliciting the neighbors it has no MAC
    /// address for and spinning until every packet left or the iteration
    /// cap is reached. Each pass runs under the stack's lock; the wait
    /// between two passes does not, since the advertisements a pass asked
    /// for come in on the kit worker, which needs the lock to deliver them.
    /// </summary>
    internal static void Send()
    {
        int iterations = 0;

        while (true)
        {
            using (NetworkStackLock.Enter())
            {
                if (s_queue.Count == 0)
                {
                    return;
                }

                iterations++;
                if (iterations >= MaxIterations)
                {
                    Serial.WriteString("[IPv6] Neighbor resolution timeout\n");
                    s_queue.Clear();
                    return;
                }

                SendPass();
                if (s_queue.Count == 0)
                {
                    return;
                }
            }

            // Spin to let the receive path take the neighbor advertisements.
            Thread.SpinWait(10_000);
        }
    }

    /// <summary>
    /// One pass over the queue: sends every packet whose neighbor resolved
    /// and solicits the others once. Under the stack's lock.
    /// </summary>
    private static void SendPass()
    {
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

            MacAddress? neighborMac = NeighborCache.Resolve(packet.DestinationIP);
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
    }
}
