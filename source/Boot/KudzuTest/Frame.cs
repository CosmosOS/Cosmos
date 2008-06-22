using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class Frame {
        public byte[] mData;

        public Frame() {
        }

        public void Init1() {
            // This one is used
            //0000   ff ff ff ff ff ff 00 50 56 22 22 0d 08 00 45 00  .......PV""...E.
            //0010   00 1d 5a f8 00 00 80 11 09 23 c0 a8 16 0d ff ff  ..Z......#......
            //0020   ff ff 04 49 08 ae 00 09 06 30 16                 ...I.....0.
            // Same UDP packet, different send for comparison
            //0000   ff ff ff ff ff ff 00 50 56 22 22 0d 08 00 45 00  .......PV""...E.
            //0010   00 1d 5a*f9 00 00 80 11 09*22 c0 a8 16 0d ff ff  ..Z......"......
            //0020   ff ff 04*4a 08 ae 00 09 06*2f 16                 ...J...../.

            mData = new byte[43];

            // 6 bytes - MAC Dest
            // 6 bytes - MAC Src
            // 2 bytes Type 0x80 0x00 (IP)
            // 46-1500 - Data
            // - Where CRC is? Stripped by driver? We need to add it to ours...? or HW does it?
            // 4 bytes - CRC 32 - of what? Data? all?

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

            // http://en.wikipedia.org/wiki/IPv4
            //Bits  0–3 	    4–7    	        8–15 	            16–18 	            19–31
            //0 	Version (4) Headerlength 	Type of Service 	Total Length
            //32 	Identification 	                                Flags 	            Fragment Offset
            //64 	Time to Live 	            Protocol 	        Header Checksum
            //96 	Source Address
            //128 	Destination Address
            //160 	Options
            //160 or 192+ Data
            // IP - Version + Header length
            mData[14] = 0x45;
            // IP - Differntiated Services Field
            mData[15] = 0x00;
            // IP - Total Length
            mData[16] = 0x00;
            mData[17] = 0x1d;
            // IP - Identification - Varies
            // This field is an identification field and is primarily used for uniquely identifying fragments of an original IP datagram. Some experimental work has suggested using the ID field for other purposes, such as for adding packet-tracing information to datagrams in order to help trace back datagrams with spoofed source addresses.
            mData[18] = 0x5a;
            mData[19] = 0xf8;
            // IP - Flags + Fragment Offset
            mData[20] = 0x00;
            mData[21] = 0x00;
            // IP - TTL
            mData[22] = 0x80;
            // IP - Protocol, x11 = UDP
            mData[23] = 0x11;
            // IP - Header Checksum - Varies
            // In 1's complement, there are 2 representations for zero: 0000000000000000 and 1111111111111111. 
            // Note that flipping the bits of the one gives you the other. A header checksum of "0"
            // (allowed for some protocols, e.g. UDP) denotes that the checksum was not calculated. 
            // Thus, implementations which do calculate a checksum make sure to give a result of 0xffff rather 
            // that 0, when the checksum is actually zero.
            mData[24] = 0x09;
            mData[25] = 0x23;
            // IP - Source IP
            mData[26] = 0xc0;
            mData[27] = 0xa8;
            mData[28] = 0x16;
            mData[29] = 0x0d;
            // IP - Destination IP
            mData[30] = 0xFF;
            mData[31] = 0xFF;
            mData[32] = 0xFF;
            mData[33] = 0xFF;
            // UDP - Source Port - Varies
            mData[34] = 0x04;
            mData[35] = 0x49;
            // UDP - Destination Port
            mData[36] = 0x08;
            mData[37] = 0xAE;
            // UDP - Length
            mData[38] = 0x00;
            mData[39] = 0x09;
            // UDP - Checksum - Varies
            mData[40] = 0x06;
            mData[41] = 0x30;
            // UDP Data
            mData[42] = 0x16;

            var xIPChecksum = UpdateIPChecksum();
            var xUDPChecksum = UpdateUDPChecksum();
            if (xIPChecksum != 0x0923) {
                string xMSG = "IP Checksum error. Was ";
                xMSG += xIPChecksum.ToString();
                throw new Exception(xMSG);
            }
            if (xUDPChecksum != 0x0630) {
                string xMSG = "UDP Checksum error. Was ";
                xMSG += xUDPChecksum.ToString();
                throw new Exception(xMSG);
            }
        }

        public void Init2() {
            // Checksums didnt work when running Cosmos so I made
            // this packet using our code on Widnows
            mData = new byte[43];
		    mData[0] = 255;
		    mData[1] = 255;
		    mData[2] = 255;
		    mData[3] = 255;
		    mData[4] = 255;
		    mData[5] = 255;
		    mData[6] = 82;
		    mData[7] = 84;
		    mData[8] = 0;
		    mData[9] = 18;
		    mData[10] = 52;
		    mData[11] = 87;
		    mData[12] = 8;
		    mData[13] = 0;
		    mData[14] = 69;
		    mData[15] = 0;
		    mData[16] = 0;
		    mData[17] = 29;
		    mData[18] = 90;
		    mData[19] = 248;
		    mData[20] = 0;
		    mData[21] = 0;
		    mData[22] = 128;
		    mData[23] = 17;
		    mData[24] = 211;
		    mData[25] = 201;
		    mData[26] = 10;
		    mData[27] = 0;
		    mData[28] = 2;
		    mData[29] = 15;
		    mData[30] = 255;
		    mData[31] = 255;
		    mData[32] = 255;
		    mData[33] = 255;
		    mData[34] = 4;
		    mData[35] = 73;
		    mData[36] = 8;
		    mData[37] = 174;
		    mData[38] = 0;
		    mData[39] = 9;
		    mData[40] = 208;
		    mData[41] = 214;
            mData[42] = 22;
        }

        public void SetEthSrcMAC(byte aMAC1, byte aMAC2, byte aMAC3, byte aMAC4, byte aMAC5, byte aMAC6) {
            mData[6] = aMAC1;
            mData[7] = aMAC2;
            mData[8] = aMAC3;
            mData[9] = aMAC4;
            mData[10] = aMAC5;
            mData[11] = aMAC6;
        }

        public void SetIPSrcAddr(byte aIP1, byte aIP2, byte aIP3, byte aIP4) {
            mData[26] = aIP1;
            mData[27] = aIP2;
            mData[28] = aIP3;
            mData[29] = aIP4;
        }

        public void SetIPDestAddr(byte aIP1, byte aIP2, byte aIP3, byte aIP4) {
            mData[30] = aIP1;
            mData[31] = aIP2;
            mData[32] = aIP3;
            mData[33] = aIP4;
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
