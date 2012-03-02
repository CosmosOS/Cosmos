using System;
using Sys = System;

namespace Cosmos.System.Network
{
    public class UdpClient
    {
        protected Int32 localPort;
        protected IPv4.Address destination;
        protected Int32 destinationPort;

        public UdpClient()
            :this(0)
        { }

        public UdpClient(Int32 localPort)
        {
            this.localPort = localPort;
        }

        public UdpClient(IPv4.Address dest, Int32 destPort)
        {
            this.destination = dest;
            this.destinationPort = destPort;
            this.localPort = 0;
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

        }
    }
}
