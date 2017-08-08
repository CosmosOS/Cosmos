using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network
{
    public class ICMPPacket : IP4Packet {
        protected int mICMPHead;

        public ICMPPacket(uint aSrcIP, uint aDestIP, ICMPType aType, byte[] aData, byte aCode)
        {
            mICMPHead = base.Initialize(
                    aData,  //Data in ICMP body
                    8,      //Size of ICMP header
                    0x01,   //ICMP
                    aSrcIP, 
                    aDestIP
                    );

            this.Type = aType;
            this.Code = aCode;
        }

        /// <summary>
        /// Concludes the ICMP Packet by setting checksum, etc.
        /// </summary>
        protected override void Conclude()
        {
            base.Conclude();
            this.SetChecksum();            
        }

        public ICMPType Type
        {
            get { return (ICMPType)mData[mICMPHead + 0]; }
            set { mData[mICMPHead + 0] = (byte)value; }
        }

        public byte Code
        {
            get { return mData[mICMPHead + 1]; }
            set { mData[mICMPHead + 1] = (byte)value; }
        }

        public UInt16 Checksum
        {
            //TODO: Calculate checksum for ICMP here
            get { return 0; }//dummy checksum
        }

        /// <summary>
        /// Calculates and saves the checksum for the ICMP packet
        /// </summary>
        private void SetChecksum()
        {
            mData[mICMPHead + 2] = (byte)(this.Checksum >> 8);
            mData[mICMPHead + 3] = (byte)this.Checksum;
        }

        public UInt16 ID
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public UInt16 Sequence
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public byte[] Padding
        {
            get { throw new NotImplementedException(); }
            set { ;}
        }

        public enum ICMPType : byte
        {
            EchoReply = 0,
            DestinationUnreachable = 3,
            SourceQuench = 4,
            RedirectMessage = 5,
            EchoRequest = 8,
            RouterAdvertisement = 9,
            RouterSolicitation = 10,
            TimeExeeded = 11,
            BadIPHeader = 12,
            Timestamp = 13,
            TimestampReply = 14,
            InformationRequest = 15,
            InformationReply = 16,
            AddressMaskRequest = 17,
            AddressMaskReply = 18,
            Traceroute = 30
        }

        //public string GetControlMessage(ICMPType type, byte code)
        //{
        //    switch (type)
        //    {
        //        case ICMPType.EchoReply:
        //            break;
        //        case ICMPType.DestinationUnreachable:
        //            break;
        //        case ICMPType.SourceQuench:
        //            break;
        //        case ICMPType.RedirectMessage:
        //            break;
        //        case ICMPType.EchoRequest:
        //            break;
        //        case ICMPType.RouterAdvertisement:
        //            break;
        //        case ICMPType.RouterSolicitation:
        //            break;
        //        case ICMPType.TimeExeeded:
        //            break;
        //        case ICMPType.BadIPHeader:
        //            break;
        //        case ICMPType.Timestamp:
        //            break;
        //        case ICMPType.TimestampReply:
        //            break;
        //        case ICMPType.InformationRequest:
        //            break;
        //        case ICMPType.InformationReply:
        //            break;
        //        case ICMPType.AddressMaskRequest:
        //            break;
        //        case ICMPType.AddressMaskReply:
        //            break;
        //        case ICMPType.Traceroute:
        //            break;
        //        default:
        //            break;
        //    }

        //    return "dummy";
        //}

 
    }
}
