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
            
            return 0;
        }

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
        public byte HeaderLength
        {
            get { return mHeaderLength;}
            set { mHeaderLength = value;}
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
        /// Returns the entire packet as a byte array.
        /// </summary>
        public byte[] RawBytes()
        {
            List<byte> bytes = new List<byte>();
            List<UInt32> fields = new List<UInt32>();

            //Add the packetsections together into 32-bit words
            fields.Add((UInt32)((this.Version << 0) | (this.HeaderLength << 4) | (this.TypeOfService << 8) | (this.TotalLength << 16)));
            fields.Add((UInt32)((this.Identification << 0) | (((byte)(this.FragmentFlags)) << 16) | (this.FragmentOffset << 19)));
            fields.Add((UInt32)((this.TimeToLive << 0) | (((byte)(this.Protocol)) << 8) | (this.HeaderChecksum << 16)));
            fields.Add(this.SourceAddress.To32BitNumber());
            fields.Add(this.DestinationAddress.To32BitNumber());
            //TODO - Options field

            //Split the 32-bit words into bytes
            //foreach (UInt32 field in fields)
            //{
            //    bytes.Add((byte)(field >> 0));
            //    bytes.Add((byte)(field >> 8));
            //    bytes.Add((byte)(field >> 16));
            //    bytes.Add((byte)(field >> 24));
            //}

            for (int i = 0; i < fields.Count; i++)
            {
                bytes.Add((byte)(fields[i] >> 0));
                bytes.Add((byte)(fields[i] >> 8));
                bytes.Add((byte)(fields[i] >> 16));
                bytes.Add((byte)(fields[i] >> 24));
            }

            //The main body of the packet
            if (this.Data != null)
            {
                foreach (byte b in this.Data.ToArray())
                {
                    bytes.Add(b);
                }
            }
            

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
            sb.Append("Header length: " + this.HeaderLength + " bytes");
            sb.Append(Environment.NewLine);
            sb.Append("Total length: " + this.TotalLength);
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
