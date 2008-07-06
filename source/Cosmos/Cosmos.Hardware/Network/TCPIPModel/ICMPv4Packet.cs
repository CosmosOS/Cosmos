using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.ICMPv4
{
    // http://en.wikipedia.org/wiki/Internet_Control_Message_Protocol
    public class ICMPv4Packet
    {
        public ICMPv4Packet()
        {
        }

        public byte Type 
        {
            get { throw new NotImplementedException(); }
            set { ;} 
        }

        public byte Code 
        {
            get { throw new NotImplementedException(); }
            set { ;} 
        }

        public UInt16 Checksum
        {
            get { throw new NotImplementedException(); }
            set { ;} 
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

        public string GetControlMessage(ICMPType type, byte code)
        {
            switch (type)
            {
                case ICMPType.EchoReply:
                    break;
                case ICMPType.DestinationUnreachable:
                    break;
                case ICMPType.SourceQuench:
                    break;
                case ICMPType.RedirectMessage:
                    break;
                case ICMPType.EchoRequest:
                    break;
                case ICMPType.RouterAdvertisement:
                    break;
                case ICMPType.RouterSolicitation:
                    break;
                case ICMPType.TimeExeeded:
                    break;
                case ICMPType.BadIPHeader:
                    break;
                case ICMPType.Timestamp:
                    break;
                case ICMPType.TimestampReply:
                    break;
                case ICMPType.InformationRequest:
                    break;
                case ICMPType.InformationReply:
                    break;
                case ICMPType.AddressMaskRequest:
                    break;
                case ICMPType.AddressMaskReply:
                    break;
                case ICMPType.Traceroute:
                    break;
                default:
                    break;
            }

            return "dummy";
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
    }
}
