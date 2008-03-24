using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class RTL8139 {
        public Frame mFrame;

        public void Init() {
            mFrame = new Frame();
            mFrame.SetEthSrcMAC(0x52, 0x54, 0x00, 0x12, 0x34, 0x57);
            mFrame.SetIPSrcAddr(10, 0, 2, 15);
            //mFrame.SetIPDestAddr(255, 255, 255, 255);
            mFrame.UpdateIPChecksum();
            mFrame.UpdateUDPChecksum();
            // Ethernet frame CRC - Done by card?
        }
    }
}
