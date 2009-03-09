using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Playground.SSchocke
{
    public class ARPPacket
    {
        protected byte[] packet_data;

        public ARPPacket(byte[] src_mac)
        {
            //0xC0A8147B /*192.168.20.123*/, 1234, 0xC0A8141C /*192.168.20.28*/
            packet_data = new byte[60];
            for (int i = 0; i < 6; i++)
            {
                packet_data[i] = 0xFF;
            }
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 6] = src_mac[i];
            }
            packet_data[12] = 0x08;
            packet_data[13] = 0x06;
            packet_data[14] = 0x00;
            packet_data[15] = 0x01;
            packet_data[16] = 0x08;
            packet_data[17] = 0x00;
            packet_data[18] = 0x06;
            packet_data[19] = 0x04;
            packet_data[20] = 0x00;
            packet_data[21] = 0x01;
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 22] = src_mac[i];
            }
            packet_data[28] = 0xC0;
            packet_data[29] = 0xA8;
            packet_data[30] = 0x14;
            packet_data[31] = 0x7B;
            for (int i = 0; i < 6; i++)
            {
                packet_data[i + 32] = 0x00;
            }
            packet_data[38] = 0xC0;
            packet_data[39] = 0xA8;
            packet_data[40] = 0x14;
            packet_data[41] = 0x1C;
            for (int i = 42; i < 60; i++)
            {
                packet_data[i] = 0x00;
            }
        }

        public byte[] GetData()
        {
            return packet_data;
        }
    }
}
