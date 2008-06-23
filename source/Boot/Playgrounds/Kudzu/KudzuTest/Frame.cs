using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class Frame {
        public byte[] mData;

        public Frame() {
        }

        public void InitTest() {
            mData = new byte[43];

            // Ethernet - Destination
            mData[0] = 0xFF;
            mData[1] = 0xFF;
            mData[2] = 0xFF;
            mData[3] = 0xFF;
            mData[4] = 0xFF;
            mData[5] = 0xFF;
            // Ethernet - Source
            mData[6] = 0x00;
            mData[7] = 0x50;
            mData[8] = 0x56;
            mData[9] = 0x22;
            mData[10] = 0x22;
            mData[11] = 0x0d;
            // Ethernet - Type - 0800 = IP
            mData[12] = 0x08;
            mData[13] = 0x00;

            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                0x0A00020F // 10.0.2.15
                , 0x0449
                , 0xFFFFFFFF // 255.255.255.255, Broadcast
                , 2222
                , new byte[] { 0x16 });
            var xUDPData = xUDP.GetData();
            xUDPData.CopyTo(mData, 14);
        }

        public void SetEthSrcMAC(byte aMAC1, byte aMAC2, byte aMAC3, byte aMAC4, byte aMAC5, byte aMAC6) {
            mData[6] = aMAC1;
            mData[7] = aMAC2;
            mData[8] = aMAC3;
            mData[9] = aMAC4;
            mData[10] = aMAC5;
            mData[11] = aMAC6;
        }

        public UInt16 UpdateIPChecksum() {
            mData[24] = 0;
            mData[25] = 0;
            // TODO: Change this to a ASM and use 32 bit addition
            UInt32 xResult = 0;
            // TODO: 20 doesnt take into account options
            for (int i = 14; i < 34; i = i + 2) {
                xResult += (UInt16)((mData[i] << 8) + mData[i + 1]);
            }
            xResult = (~((xResult & 0xFFFF) + (xResult >> 16)));
            mData[24] = (byte)(xResult >> 8);
            mData[25] = (byte)(xResult & 0xFF);
            return (UInt16)xResult;
        }

        public UInt16 UpdateUDPChecksum() {
            mData[40] = 0;
            mData[41] = 0;

            // TODO: Change this to a ASM and use 32 bit addition
            UInt32 xResult = 0;
            // TODO: Adjust for length and odd sizes
            for (int i = 34; i < 42; i = i + 2) {
                xResult += (UInt16)((mData[i] << 8) + mData[i + 1]);
            }
			// Data
            xResult += (UInt16)((mData[42] << 8) + 0);
            // Pseudo header
            // --Protocol
            // TODO: Change to actually iterate data
            xResult += (UInt16)(mData[23]);
            // --IP Source
            xResult += (UInt16)((mData[26] << 8) + mData[27]);
            xResult += (UInt16)((mData[28] << 8) + mData[29]);
            // --IP Dest
            xResult += (UInt16)((mData[30] << 8) + mData[31]);
            xResult += (UInt16)((mData[32] << 8) + mData[33]);
            // --UDP Length
            xResult += (UInt16)((mData[38] << 8) + mData[39]);

            xResult = (~((xResult & 0xFFFF) + (xResult >> 16)));
            
            mData[40] = (byte)(xResult >> 8);
            mData[41] = (byte)(xResult & 0xFF);

            return (UInt16)xResult;
        }
    }
}
