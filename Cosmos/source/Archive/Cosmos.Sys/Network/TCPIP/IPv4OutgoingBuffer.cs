using System.Collections.Generic;
using HW = Cosmos.Hardware2;
using System;

namespace Cosmos.Sys.Network.TCPIP
{
    internal static class IPv4OutgoingBuffer
    {
        private class BufferEntry
        {
            public enum EntryStatus { ADDED, ARP_SENT, ROUTE_ARP_SENT, JUST_SEND, DONE };

            public HW.Network.NetworkDevice NIC;
            public IPPacket Packet;
            public EntryStatus Status;
            public IPv4Address nextHop;

            public BufferEntry(HW.Network.NetworkDevice nic, IPPacket packet)
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
            HW.Network.NetworkDevice nic = TCPIPStack.FindInterface(packet.SourceIP);
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
                    if (TCPIPStack.IsLocalAddress(entry.Packet.DestinationIP) == false)
                    {
                        entry.nextHop = TCPIPStack.FindRoute(entry.Packet.DestinationIP);
                        if (entry.nextHop == null)
                        {
                            entry.Status = BufferEntry.EntryStatus.DONE;
                            continue;
                        }

                        if (ARP.ARPCache.Contains(entry.nextHop) == true)
                        {
                            entry.Packet.DestinationMAC = ARP.ARPCache.Resolve(entry.nextHop);

                            entry.NIC.QueueBytes(entry.Packet.RawData);

                            entry.Status = BufferEntry.EntryStatus.DONE;
                        }
                        else
                        {
                            ARP.ARPRequest_EthernetIPv4 arp_request = new ARP.ARPRequest_EthernetIPv4(entry.NIC.MACAddress, entry.Packet.SourceIP,
                                HW.Network.MACAddress.Broadcast, entry.nextHop);

                            entry.NIC.QueueBytes(arp_request.RawData);

                            entry.Status = BufferEntry.EntryStatus.ROUTE_ARP_SENT;
                        }
                        continue;
                    }

                    if (ARP.ARPCache.Contains(entry.Packet.DestinationIP) == true)
                    {
                        entry.Packet.DestinationMAC = ARP.ARPCache.Resolve(entry.Packet.DestinationIP);

                        entry.NIC.QueueBytes(entry.Packet.RawData);

                        entry.Status = BufferEntry.EntryStatus.DONE;
                    }
                    else
                    {
                        ARP.ARPRequest_EthernetIPv4 arp_request = new ARP.ARPRequest_EthernetIPv4(entry.NIC.MACAddress, entry.Packet.SourceIP,
                            HW.Network.MACAddress.Broadcast, entry.Packet.DestinationIP);

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

        internal static void ARPCache_Update(ARP.ARPReply_EthernetIPv4 arp_reply)
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
