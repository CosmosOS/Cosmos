using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network {
    public class EthernetPacket : Packet {
        public class PacketType {
            public const UInt16 IP = 0x800;
        }

        protected byte mHeaderSize = 14;
        protected int mHeaderBegin;

        public EthernetPacket(byte[] aData, UInt64 aSrcMAC, UInt64 aDestMAC, UInt16 aType) {
            mHeaderBegin = Initialize(aData, mHeaderSize);
            DestinationAddress = aDestMAC;
            SourceAddress = aSrcMAC;
            this.Type = aType;
        }

        public EthernetPacket(byte[] aRawData) {
            mData = aRawData;
        }

        public uint Type {
            get {
                return (uint)(mData[12] << 8 | mData[13]);
            }
            set {
                mData[12] = (byte)(value >> 8);
                mData[13] = (byte)value;
            }
        }

        public UInt64 DestinationAddress {
            get {
                return (UInt64)(
                    mData[0] << 40
                    | mData[1] << 32
                    | mData[2] << 24
                    | mData[3] << 16
                    | mData[4] << 8
                    | mData[5]);
            }
            set {
                mData[0] = (byte)(value >> 24);
                mData[1] = (byte)(value >> 24);
                mData[2] = (byte)(value >> 24);
                mData[3] = (byte)(value >> 16);
                mData[4] = (byte)(value >> 8);
                mData[5] = (byte)value;
            }
        }

//        public 

        public UInt64 SourceAddress {
            get {
                return (UInt64)(
                    mData[6] << 40
                    | mData[7] << 32
                    | mData[8] << 24
                    | mData[9] << 16
                    | mData[10] << 8
                    | mData[11]);
            }
            set {
                mData[6] = (byte)(value >> 24);
                mData[7] = (byte)(value >> 24);
                mData[8] = (byte)(value >> 24);
                mData[9] = (byte)(value >> 16);
                mData[10] = (byte)(value >> 8);
                mData[11] = (byte)value;
            }
        }

    }
}
