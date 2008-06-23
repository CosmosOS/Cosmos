using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class Frame {
        public byte[] mData;

        public Frame() {
        }

        public void InitTest() {
            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                0x0A00020F // 10.0.2.15
                , 0x0449
                , 0xFFFFFFFF // 255.255.255.255, Broadcast
                , 2222
                , new byte[] { 0x16 });
            var xEthernet = new Cosmos.Sys.Network.EthernetPacket(xUDP.GetData());
            mData = xEthernet.GetData();
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
