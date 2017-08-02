using System;
using Sys = System;
using System.Collections.Generic;

namespace Cosmos.System.Network
{
    public class UdpClient
    {
        // TODO: Once we support more than just IPv4, we really need to base all the IPv4 classes on abstract classes
        // that represent the required functionality, then we can generalize the stack to be independent from IPv4 or IPv6
        internal class DataGram
        {
            internal byte[] data;
            internal IPv4.EndPoint source;

            internal DataGram(byte[] data, IPv4.EndPoint src)
            {
                this.data = data;
                this.source = src;
            }
        }

        private static TempDictionary<UdpClient> clients;

        protected Int32 localPort;
        protected IPv4.Address destination;
        protected Int32 destinationPort;

        private Queue<DataGram> rxBuffer;

        static UdpClient()
        {
            clients = new TempDictionary<UdpClient>();
        }

        internal static UdpClient Client(ushort destPort)
        {
            if (clients.ContainsKey((UInt32)destPort) == true)
            {
                return clients[(UInt32)destPort];
            }

            return null;
        }

        public UdpClient()
            :this(0)
        { }

        public UdpClient(Int32 localPort)
        {
            this.rxBuffer = new Queue<DataGram>(8);

            this.localPort = localPort;
            if (localPort > 0)
            {
                UdpClient.clients.Add((UInt32)localPort, this);
            }
        }

        public UdpClient(IPv4.Address dest, Int32 destPort)
            : this(0)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        public void Connect(IPv4.Address dest, Int32 destPort)
        {
            this.destination = dest;
            this.destinationPort = destPort;
        }

        public void Send(byte[] data)
        {
            if ((this.destination == null) ||
                (this.destinationPort == 0))
            {
                throw new Exception("Must establish a default remote host by calling Connect() before using this Send() overload");
            }

            Send(data, this.destination, this.destinationPort);
        }

        public void Send(byte[] data, IPv4.Address dest, Int32 destPort)
        {
            IPv4.Address source = IPv4.Config.FindNetwork(dest);
            IPv4.UDPPacket packet = new IPv4.UDPPacket(source, dest, (UInt16)this.localPort, (UInt16)destPort, data);

            Sys.Console.WriteLine("Sending " + packet.ToString());
            IPv4.OutgoingBuffer.AddPacket(packet);
        }

        public void Close()
        {
            if (UdpClient.clients.ContainsKey((UInt32)this.localPort) == true)
            {
                UdpClient.clients.Remove((UInt32)this.localPort);
            }
        }

        public byte[] Receive(ref IPv4.EndPoint source)
        {
            if (this.rxBuffer.Count < 1)
            {
                return null;
            }

            DataGram packet = rxBuffer.Dequeue();
            source.address = packet.source.address;
            source.port = packet.source.port;

            return packet.data;
        }

        internal void receiveData(IPv4.UDPPacket packet)
        {
            byte[] data = packet.UDP_Data;
            IPv4.EndPoint source = new IPv4.EndPoint(packet.SourceIP, packet.SourcePort);

            Sys.Console.WriteLine("Received " + data.Length + " bytes data from " + source.ToString());

            this.rxBuffer.Enqueue(new DataGram(data, source));
        }
    }
}
