using System;
using System.Collections.Generic;
using System.Linq;
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


        #region Packet Header

        public byte Version 
        {
            get { return 4;}
            private set { ;}
        }

        public byte HeaderLength
        {
            get { throw new NotImplementedException();}
            set { ;}
        }

        public byte TypeOfService
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public UInt16 TotalLength
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public UInt16 Identification
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public Fragmentation FragmentFlags
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public UInt16 FragmentOffset
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public byte TimeToLive
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public Protocols Protocol
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public UInt16 HeaderChecksum
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public Address SourceAddress
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public Address DestinationAddress
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }
        
        public UInt32 Options
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        #endregion

        public byte[] Data
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        /// <summary>
        /// Returns the entire packet as a byte array.
        /// </summary>
        public byte[] RawBytes()
        {
            throw new NotImplementedException();
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
