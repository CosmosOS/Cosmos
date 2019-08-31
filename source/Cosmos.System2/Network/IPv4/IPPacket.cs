using Cosmos.HAL.Network;
using Cosmos.System.Network.ARP;

namespace Cosmos.System.Network.IPv4
{
    public class IPPacket : EthernetPacket
    {
        protected byte ipHeaderLength;

        private static ushort sNextFragmentID;

        internal static void IPv4Handler(byte[] packetData)
        {
            IPPacket ip_packet = new IPPacket(packetData);
            //Sys.Console.WriteLine("Received IP Packet");
            //Sys.Console.WriteLine(ip_packet.ToString());
            if (ip_packet.SourceIP == null)
            {
                NetworkStack.debugger.Send("SourceIP null in IPv4Handler!");
            }
            ARPCache.Update(ip_packet.SourceIP, ip_packet.SourceMAC);

            if ((NetworkStack.AddressMap.ContainsKey(ip_packet.DestinationIP.Hash) == true) ||
                (ip_packet.DestinationIP.address[3] == 255))
            {
                switch (ip_packet.Protocol)
                {
                    case 1:
                        ICMPPacket.ICMPHandler(packetData);
                        break;
                    case 6:
                        //TCPPacket.TCPHandler(packetData);
                        break;
                    case 17:
                        UDPPacket.UDPHandler(packetData);
                        break;
                }
            }
        }

        public static ushort NextIPFragmentID => sNextFragmentID++;

        internal IPPacket()
        {
        }

        internal IPPacket(byte[] rawData)
            : base(rawData)
        {
        }

        protected override void initFields()
        {
            base.initFields();
            IPVersion = (byte)((RawData[14] & 0xF0) >> 4);
            ipHeaderLength = (byte)(RawData[14] & 0x0F);
            TypeOfService = RawData[15];
            IPLength = (ushort)((RawData[16] << 8) | RawData[17]);
            FragmentID = (ushort)((RawData[18] << 8) | RawData[19]);
            Flags = (byte)((RawData[20] & 0xE0) >> 5);
            FragmentOffset = (ushort)(((RawData[20] & 0x1F) << 8) | RawData[21]);
            TTL = RawData[22];
            Protocol = RawData[23];
            IPCRC = (ushort)((RawData[24] << 8) | RawData[25]);
            SourceIP = new Address(RawData, 26);
            DestinationIP = new Address(RawData, 30);
            DataOffset = (ushort)(14 + HeaderLength);
        }

        protected IPPacket(ushort dataLength, byte protocol, Address source, Address dest, byte Flags)
            : this(MACAddress.None, MACAddress.None, dataLength, protocol, source, dest, Flags)
        { }

        public IPPacket(MACAddress srcMAC, MACAddress destMAC, ushort dataLength, byte protocol,
            Address source, Address dest, byte Flags)
            : base(destMAC, srcMAC, 0x0800, dataLength + 14 + 20)
        {
            RawData[14] = 0x45;
            RawData[15] = 0;
            IPLength = (ushort)(dataLength + 20);
            ipHeaderLength = 5;

            RawData[16] = (byte)((IPLength >> 8) & 0xFF);
            RawData[17] = (byte)((IPLength >> 0) & 0xFF);
            FragmentID = NextIPFragmentID;
            RawData[18] = (byte)((FragmentID >> 8) & 0xFF);
            RawData[19] = (byte)((FragmentID >> 0) & 0xFF);
            RawData[20] = Flags;
            RawData[21] = 0x00;
            RawData[22] = 0x80;
            RawData[23] = protocol;
            RawData[24] = 0x00;
            RawData[25] = 0x00;
            for (int b = 0; b < 4; b++)
            {
                RawData[26 + b] = source.address[b];
                RawData[30 + b] = dest.address[b];
            }
            IPCRC = CalcIPCRC(20);
            RawData[24] = (byte)((IPCRC >> 8) & 0xFF);
            RawData[25] = (byte)((IPCRC >> 0) & 0xFF);

            initFields();
        }

        protected ushort CalcOcCRC(ushort offset, ushort length) => CalcOcCRC(RawData, offset, length);

        protected static ushort CalcOcCRC(byte[] buffer, ushort offset, int length)
        {
            uint crc = 0;

            for (ushort w = offset; w < offset + length; w += 2)
            {
                crc += (ushort)((buffer[w] << 8) | buffer[w + 1]);
            }

            crc = (~((crc & 0xFFFF) + (crc >> 16)));

            return (ushort)crc;
        }

        protected ushort CalcIPCRC(ushort headerLength)
        {
            return CalcOcCRC(14, headerLength);
        }

        internal byte IPVersion { get; private set; }
        internal ushort HeaderLength => (ushort)(ipHeaderLength * 4);

        internal byte TypeOfService { get; private set; }

        internal ushort IPLength { get; private set; }
        internal ushort FragmentID { get; private set; }
        internal byte Flags { get; private set; }
        internal ushort FragmentOffset { get; private set; }
        internal byte TTL { get; private set; }
        internal byte Protocol { get; private set; }
        internal ushort IPCRC { get; private set; }
        internal Address SourceIP { get; private set; }
        internal Address DestinationIP { get; private set; }
        internal ushort DataOffset { get; private set; }

        internal ushort DataLength => (ushort)(IPLength - HeaderLength);

        public override string ToString()
        {
            return "IP Packet Src=" + SourceIP + ", Dest=" + DestinationIP + ", Protocol=" + Protocol + ", TTL=" + TTL + ", DataLen=" + DataLength;
        }
    }
}
