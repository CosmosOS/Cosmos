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

    }
}
