using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Cosmos.Hardware.Extension.NumberSystem;

namespace Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4
{
    //See http://en.wikipedia.org/wiki/IPv4#Header
    public class IPv4Packet
    {
        public IPv4Packet()
        {
        }

        //public IPv4Packet(byte[] data)
        //{
        //    this.Data = data;
        //}

        public UInt16 CalculateHeaderChecksum()
        {
            //16 bits one's complement of the one's complement sum of all 16-bit words in the headers.
            //So if the header contains 200 one's then we one-complement that value.

            //TODO - add algorithm. Now we just return 0 to indicate that checksum is turned off.
            UInt16 checksum = 0;
            List<UInt16> words = new List<ushort>();

            byte[] header = this.GetHeaderBytes();

            //Assemble the bytes into 16-bit words
            for (int index = 0; index < header.Length; index += 2)
            {
                UInt16 word = (UInt16)(header[index + 1] + (header[index] << 8));
                words.Add(word);
            }

            //Reset the existing checksum in the header to 0.
            words[5] = 0;

            //Add together the 16-bit words
            UInt32 total = 0;
            for (int i = 0; i < words.Count; i++)
            {
                total = total + words[i];
                if (total > UInt16.MaxValue)
                {
                    total = total - UInt16.MaxValue; //move carry
                }
            }

            //Invert the bits
            checksum = (UInt16)~total;

            return checksum;
        }

        /// <summary>
        /// Internet Header Length (IHL) telling the number of 32-bit words in the header.
        /// Minimum length is 5 words. Maximum is 15 words when Options are set.
        /// </summary>
        /// <returns></returns>
        public byte CalculateHeaderLength()
        {
            return 5; //TODO, include Options
        }

        public byte CalculateTotalLength()
        {
            byte header = (byte)(CalculateHeaderLength() * (byte)4);
            byte body;

            if (this.Data != null)
                body = (byte)this.Data.Count;
            else
                body = 0;

            return (byte)(header + body);
        }


        #region Packet Header

        public byte Version 
        {
            get { return 4;}
            private set { ;}
        }

        private byte mHeaderLength = 0;
        /// <summary>
        /// The number of 32-bit words in the header. Does not include the length of the data. Use TotalLength for that. 
        /// Remember to multiply by four to get the byte count.
        /// </summary>
        public byte HeaderLength
        {
            get { return mHeaderLength; }
            set { mHeaderLength = value; }
        }

        private byte mTypeOfService = 0;
        public byte TypeOfService
        {
            get { return mTypeOfService; }
            set { mTypeOfService = value;}
        }

        private UInt16 mTotalLength = 0;
        public UInt16 TotalLength
        {
            get { return mTotalLength; }
            set { mTotalLength = value;}
        }

        private UInt16 mIdentification = 0;
        public UInt16 Identification
        {
            get { return mIdentification; }
            set { mIdentification = value;}
        }

        private Fragmentation mFragmentFlags;
        public Fragmentation FragmentFlags
        {
            get { return mFragmentFlags; }
            set { mFragmentFlags = value;}
        }

        public UInt16 mFragmentOffset = 0;
        public UInt16 FragmentOffset
        {
            get { return mFragmentOffset; }
            set 
            { 
                //TODO - only 13 bits, not all 16.
                mFragmentOffset = value;
            }
        }

        private byte mTimeToLive = 0xFF;
        public byte TimeToLive
        {
            get { return mTimeToLive; }
            set { mTimeToLive = value;}
        }

        private Protocols mProtocol;
        public Protocols Protocol
        {
            get { return mProtocol; }
            set { mProtocol = value ;}
        }

        private UInt16 mHeaderChecksum = 0;
        public UInt16 HeaderChecksum
        {
            get { return mHeaderChecksum; }
            set { mHeaderChecksum = value;}
        }

        private IPv4Address mSourceAddress;
        public IPv4Address SourceAddress
        {
            get { return mSourceAddress; }
            set { mSourceAddress = value;}
        }

        private IPv4Address mDestinationAddress;
        public IPv4Address DestinationAddress
        {
            get { return mDestinationAddress; }
            set { mDestinationAddress = value;}
        }
        
        public UInt32 Options
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        #endregion

        private List<byte> mData = new List<byte>(0);
        public List<byte> Data
        {
            get { return mData; }
            set { mData = value;}
        }

        /// <summary>
        /// Reverses a string.
        /// </summary>
        private string Reverse(string s)
        {
            char[] c = s.ToCharArray();
            string ts = string.Empty;

            for (int i = c.Length - 1; i >= 0; i--)
                ts += c[i].ToString();

            return ts;
        }

        /// <summary>
        /// Converts the four bytes in a 32-bit word into using big-endian instead of little-endian.
        /// The four bytes retains their position, but the bits inside each of the bytes are reversed in order.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private UInt32 ConvertToBigEndian(UInt32 n)
        {
            string bin = n.ToBinary();
            //bin = bin.PadLeft(8, '0');
            while (bin.Length < 32)
                bin = "0" + bin;

            string first = Reverse(bin.Substring(0, 8));
            string second = Reverse(bin.Substring(8, 8));
            string third = Reverse(bin.Substring(16, 8));
            string fourth = Reverse(bin.Substring(24, 8));

            //Console.WriteLine("Little-endian: " + bin + " (as value: " + bin.FromBinary());
            bin = first + second + third + fourth;
            //Console.WriteLine("Big-endian:    " + bin + " (as value: " + bin.FromBinary());

            return bin.FromBinary();
        }

        /// <summary>
        /// Returns the entire packet as a byte array.
        /// The header is using big-endian for the bytes.
        /// </summary>
        public byte[] RawBytes()
        {
            List<byte> bytes = new List<byte>();
            
            // Header
            foreach (byte b in GetHeaderBytes())
            {
                bytes.Add(b);
            }
            
            // Main body of the packet
            if (this.Data != null)
            {
                foreach (byte b in this.Data.ToArray())
                {
                    bytes.Add(b);
                }
            }

            return bytes.ToArray();
        }

        private byte[] GetHeaderBytes()
        {
            List<byte> bytes = new List<byte>();
            List<UInt32> fields = new List<UInt32>();

            //Add the packetsections together into 32-bit words
            UInt32 field1 = (UInt32)((this.Version << 4) | (this.HeaderLength << 0) | (this.TypeOfService << 8) | (this.TotalLength << 24));
            //field1 = ConvertToBigEndian(field1);
            fields.Add(field1);

            UInt32 field2 = (UInt32)((this.Identification << 8) | (((byte)(this.FragmentFlags)) << 21) | (byte)(this.FragmentOffset << 24)); //TODO: FragmentOffset should be 13 bits.
            //fields.Add(ConvertToBigEndian(field2));
            fields.Add(field2);

            UInt32 field3 = (UInt32)((this.TimeToLive << 0) | (((byte)(this.Protocol)) << 8) | (this.HeaderChecksum << 24));
            //fields.Add(ConvertToBigEndian(field3));
            fields.Add(field3);
            
            //Split the 32-bit words into bytes
            for (int i = 0; i < fields.Count; i++)
            {
                bytes.Add((byte)(fields[i] >> 0));
                bytes.Add((byte)(fields[i] >> 8));
                bytes.Add((byte)(fields[i] >> 16));
                bytes.Add((byte)(fields[i] >> 24));
            }

            //Source and Destination
            foreach (byte b in SourceAddress.ToByteArray())
                bytes.Add(b);

            foreach (byte b in DestinationAddress.ToByteArray())
                bytes.Add(b);

            //TODO - Options field

            return bytes.ToArray();

        }

        public override string ToString()
        {
            //Outputs the IP packet like Wireshark displays it
            StringBuilder sb = new StringBuilder();
            sb.Append("----- IPv4 Packet -----");
            sb.Append(Environment.NewLine);
            sb.Append("Version: " + this.Version);
            sb.Append(Environment.NewLine);
            sb.Append("Header length: " + this.HeaderLength + " (" + this.HeaderLength*4 + " bytes)");
            sb.Append(Environment.NewLine);
            sb.Append("Total length: " + this.TotalLength + " bytes");
            sb.Append(Environment.NewLine);
            sb.Append("Identification: 0x" + this.Identification.ToHex() + " (" + this.Identification + ")");
            sb.Append(Environment.NewLine);
            sb.Append("Flags: 0x" + ((byte)(this.FragmentFlags)).ToHex());
            sb.Append(Environment.NewLine);
            sb.Append("Fragment offset: " + this.FragmentOffset);
            sb.Append(Environment.NewLine);
            sb.Append("Time to live: " + this.TimeToLive);
            sb.Append(Environment.NewLine);
            sb.Append("Protocol: " + this.Protocol + " (0x" + ((byte)(this.Protocol)).ToHex() + ")");
            sb.Append(Environment.NewLine);
            sb.Append("Header checksum: 0x" + this.HeaderChecksum.ToHex() + " [unknown]");
            sb.Append(Environment.NewLine);
            sb.Append("Source: " + this.SourceAddress);
            sb.Append(Environment.NewLine);
            sb.Append("Destination: " + this.DestinationAddress.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("Data size: " + this.Data.Count + " bytes");
            sb.Append(Environment.NewLine);
            sb.Append("--------------------");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }


        [Flags]
        public enum Fragmentation : int
        {
            Reserved = 0,
            DoNotFragment = 1,
            MoreFragments = 2
        }

        public enum Protocols
        {
            ICMP = 1,
            IGMP = 2,
            TCP = 6,
            UDP = 17,
            OSPF = 89,
            SCTP = 132
        }




    }
}
