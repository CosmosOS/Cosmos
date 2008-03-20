using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test.Mock
{
    public class FakeBroadcastPacket
    {

        public static byte[] GetFakePacketAllHigh()
        {
            byte[] p = new byte[150];
            for (int i = 0; i < 149; i++)
            {
                p[i] = 0xFE;
            }

            return p;

        }
        
        //Builds a fake Broadcast packet.
        public static byte[] GetFakePacket()
        {

            // A  Broadcast packet
            //0000  ff ff ff ff ff ff 00 ff  63 08 fc e2 08 06 00 01   ........ c.......
            //0010  08 00 06 04 00 01 00 ff  63 08 fc e2 ac 1c 06 06   ........ c.......
            //0020  00 00 00 00 00 00 ac 1c  05 0a                     ........ ..      

            byte[] p = new byte[42];
            p[0] = 0xFF;
            p[1] = 0xFF;
            p[2] = 0xFF;
            p[3] = 0xFF;
            p[4] = 0xFF;
            p[5] = 0xFF;
            p[6] = 0x00;
            p[7] = 0xFF;

            p[8] = 0x63;
            p[9] = 0x08;
            p[10] = 0xFC;
            p[11] = 0xE2;
            p[12] = 0x08;
            p[13] = 0x06;
            p[14] = 0x00;
            p[15] = 0x01;

            p[16] = 0x08;
            p[17] = 0x00;
            p[18] = 0x06;
            p[19] = 0x04;
            p[20] = 0x00;
            p[21] = 0x01;
            p[22] = 0x00;
            p[23] = 0xFF;

            p[24] = 0x63;
            p[25] = 0x08;
            p[26] = 0xFC;
            p[27] = 0xE2;
            p[28] = 0xAC;
            p[29] = 0x1C;
            p[30] = 0x06;
            p[31] = 0x06;

            p[32] = 0x00;
            p[33] = 0x00;
            p[34] = 0x00;
            p[35] = 0x00;
            p[36] = 0x00;
            p[37] = 0x00;
            p[38] = 0xAC;
            p[39] = 0x1C;

            p[40] = 0x05;
            p[41] = 0x0A;

            return p;

        }

    }
}
