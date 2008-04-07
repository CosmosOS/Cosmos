using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4
{
    //See http://en.wikipedia.org/wiki/IPv4#Header
    public class Packet
    {
        public Packet()
        {
        }

        public Packet(byte[] data)
        {
            this.Data = data;
        }

        public UInt16 CalculateHeaderChecksum()
        {
            //16 bits one's complement of the one's complement sum of all 16-bit words in the headers.
            //So if the header contains 200 one's then we one-complement that value.

            //TODO - add algorithm. Now we just return 0 to indicate that checksum is turned off.
            
            return 0;
        }

        public byte CalculateTotalLength()
        {
            return 0;
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

        private Address mSourceAddress;
        public Address SourceAddress
        {
            get { return mSourceAddress; }
            set { mSourceAddress = value;}
        }

        private Address mDestinationAddress;
        public Address DestinationAddress
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

        private byte[] mData;
        public byte[] Data
        {
            get { return mData; }
            set { mData = value;}
        }

        /// <summary>
        /// Returns the entire packet as a byte array.
        /// </summary>
        public byte[] RawBytes()
        {
            //NB! THIS METHOD CRASHES COSMOS BUILDER!

            List<byte> bytes = new List<byte>();
            List<UInt32> fields = new List<UInt32>();

            //Add the packetsections together into 32-bit words
            UInt32 field1 = (UInt32)((this.Version << 0) | (this.HeaderLength << 4) | (this.TypeOfService << 8) | (this.TotalLength << 16));
            fields.Add(field1);
            //UInt32 field2 = this.Identification + this.FragmentFlags + this.FragmentOffset;
            //UInt32 field3 = this.TimeToLive + this.Protocol + this.HeaderChecksum;

            //Split the 32-bit words into bytes
            foreach (UInt32 field in fields)
            {
                bytes.Add((byte)(field >> 0));
                bytes.Add((byte)(field >> 8));
                bytes.Add((byte)(field >> 16));
                bytes.Add((byte)(field >> 24));
            }

            return bytes.ToArray();
        }

        //[Flags]
        public enum Fragmentation
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
