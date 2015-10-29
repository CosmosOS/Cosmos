﻿using System.Collections.Generic;
using Cosmos.Core.Network;
using Cosmos.Hardware;
using Cosmos.System.Network.ARP;

namespace Cosmos.System.Network.IPv4
{
    internal static class OutgoingBuffer
    {
        private class BufferEntry
        {
            public enum EntryStatus { ADDED, ARP_SENT, ROUTE_ARP_SENT, JUST_SEND, DONE };

            public NetworkDevice NIC;
            public IPPacket Packet;
            public EntryStatus Status;
            public Address nextHop;

            public BufferEntry(NetworkDevice nic, IPPacket packet)
            {
                this.NIC = nic;
                this.Packet = packet;
                this.Status = EntryStatus.ADDED;
            }
        }

        private static List<BufferEntry> queue;

        private static void ensureQueueExists()
        {
            if (queue == null)
            {
                queue = new List<BufferEntry>();
            }
        }

        internal static void AddPacket(IPPacket packet)
        {
            ensureQueueExists();
            NetworkDevice nic = Config.FindInterface(packet.SourceIP);
            packet.SourceMAC = nic.MACAddress;
            queue.Add(new BufferEntry(nic, packet));
        }

        internal static void Send()
        {
            ensureQueueExists();
            if (queue.Count < 1)
            {
                return;
            }

            //foreach (BufferEntry entry in queue)
            for (int e = 0; e < queue.Count; e++)
            {
                BufferEntry entry = queue[e];
                if (entry.Status == BufferEntry.EntryStatus.ADDED)
                {
                    if (Config.IsLocalAddress(entry.Packet.DestinationIP) == false)
                    {
                        entry.nextHop = Config.FindRoute(entry.Packet.DestinationIP);
                        if (entry.nextHop == null)
                        {
                            entry.Status = BufferEntry.EntryStatus.DONE;
                            continue;
                        }

                        if (ARPCache.Contains(entry.nextHop) == true)
                        {
                            entry.Packet.DestinationMAC = ARPCache.Resolve(entry.nextHop);

                            entry.NIC.QueueBytes(entry.Packet.RawData);

                            entry.Status = BufferEntry.EntryStatus.DONE;
                        }
                        else
                        {
                            ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(entry.NIC.MACAddress, entry.Packet.SourceIP,
                                MACAddress.Broadcast, entry.nextHop);

                            entry.NIC.QueueBytes(arp_request.RawData);

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
                        ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(entry.NIC.MACAddress, entry.Packet.SourceIP,
                            MACAddress.Broadcast, entry.Packet.DestinationIP);

                        entry.NIC.QueueBytes(arp_request.RawData);

                        entry.Status = BufferEntry.EntryStatus.ARP_SENT;
                    }
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

        internal static void ARPCache_Update(ARPReply_Ethernet arp_reply)
        {
            ensureQueueExists();
            //foreach (BufferEntry entry in queue)
            for (int e = 0; e < queue.Count; e++)
            {
                BufferEntry entry = queue[e];
                if (entry.Status == BufferEntry.EntryStatus.ARP_SENT)
                {
                    if (entry.Packet.DestinationIP.CompareTo(arp_reply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arp_reply.SenderMAC;

                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
                else if (entry.Status == BufferEntry.EntryStatus.ROUTE_ARP_SENT)
                {
                    if (entry.nextHop.CompareTo(arp_reply.SenderIP) == 0)
                    {
                        entry.Packet.DestinationMAC = arp_reply.SenderMAC;

                        entry.Status = BufferEntry.EntryStatus.JUST_SEND;
                    }
                }
            }
        }
    }
}
