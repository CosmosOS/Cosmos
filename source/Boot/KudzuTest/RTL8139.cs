using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class RTL8139 {
        public static UInt16 IPChecksum(byte[] aData) {
            // TODO: Change this to a ASM and use 32 bit addition
            UInt32 xResult = 0;
            // TODO: 20 doesnt take into account options
            for (int i = 14; i < 34; i = i + 2) {
                xResult += (UInt16)((aData[i] << 8) + aData[i + 1]);
            }
            return (UInt16)(~((xResult & 0xFFFF) + (xResult >> 16)));
        }

        public static UInt16 UDPChecksum(byte[] aData) {
            // TODO: Change this to a ASM and use 32 bit addition
            UInt32 xResult = 0;
            // TODO: Adjust for length and odd sizes
            for (int i = 34; i < 42; i = i + 2) {
                xResult += (UInt16)((aData[i] << 8) + aData[i + 1]);
            }
            // Data
            xResult += (UInt16)((aData[42] << 8) + 0);
            // Pseudo header
            // --Protocol
            // TODO: Change to actually iterate data
            xResult += (UInt16)(aData[23]);
            // --IP Source
            xResult += (UInt16)((aData[26] << 8) + aData[27]);
            xResult += (UInt16)((aData[28] << 8) + aData[29]);
            // --IP Dest
            xResult += (UInt16)((aData[30] << 8) + aData[31]);
            xResult += (UInt16)((aData[32] << 8) + aData[33]);
            // --UDP Length
            xResult += (UInt16)((aData[38] << 8) + aData[39]);
            return (UInt16)(~((xResult & 0xFFFF) + (xResult >> 16)));
        }

        public static byte[] SampleUDPBroadcast() {
// This one is used
//0000   ff ff ff ff ff ff 00 50 56 22 22 0d 08 00 45 00  .......PV""...E.
//0010   00 1d 5a f8 00 00 80 11 09 23 c0 a8 16 0d ff ff  ..Z......#......
//0020   ff ff 04 49 08 ae 00 09 06 30 16                 ...I.....0.
// Same UDP packet, different send for comparison
//0000   ff ff ff ff ff ff 00 50 56 22 22 0d 08 00 45 00  .......PV""...E.
//0010   00 1d 5a*f9 00 00 80 11 09*22 c0 a8 16 0d ff ff  ..Z......"......
//0020   ff ff 04*4a 08 ae 00 09 06*2f 16                 ...J...../.

            var xResult = new byte[43];

            // 6 bytes - MAC Dest
            // 6 bytes - MAC Src
            // 2 bytes Type 0x80 0x00 (IP)
            // 46-1500 - Data
            // - Where CRC is? Stripped by driver? We need to add it to ours...? or HW does it?
            // 4 bytes - CRC 32 - of what? Data? all?

            // Ethernet - Destination
            xResult[0] = 0xFF;
            xResult[1] = 0xFF;
            xResult[2] = 0xFF;
            xResult[3] = 0xFF;
            xResult[4] = 0xFF;
            xResult[5] = 0xFF;
            // Ethernet - Source
            xResult[6] = 0x00;
            xResult[7] = 0x50;
            xResult[8] = 0x56;
            xResult[9] = 0x22;
            xResult[10] = 0x22;
            xResult[11] = 0x0d;
            // Ethernet - Type - 0800 = IP
            xResult[12] = 0x08;
            xResult[13] = 0x00;

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
            xResult[14] = 0x45;
            // IP - Differntiated Services Field
            xResult[15] = 0x00;
            // IP - Total Length
            xResult[16] = 0x00;
            xResult[17] = 0x1d;
            // IP - Identification - Varies
            // This field is an identification field and is primarily used for uniquely identifying fragments of an original IP datagram. Some experimental work has suggested using the ID field for other purposes, such as for adding packet-tracing information to datagrams in order to help trace back datagrams with spoofed source addresses.
            xResult[18] = 0x5a;
            xResult[19] = 0xf8;
            // IP - Flags + Fragment Offset
            xResult[20] = 0x00;
            xResult[21] = 0x00;
            // IP - TTL
            xResult[22] = 0x80;
            // IP - Protocol, x11 = UDP
            xResult[23] = 0x11;
            // IP - Header Checksum - Varies
            // In 1's complement, there are 2 representations for zero: 0000000000000000 and 1111111111111111. 
            // Note that flipping the bits of the one gives you the other. A header checksum of "0"
            // (allowed for some protocols, e.g. UDP) denotes that the checksum was not calculated. 
            // Thus, implementations which do calculate a checksum make sure to give a result of 0xffff rather 
            // that 0, when the checksum is actually zero.
            xResult[24] = 0x09;
            xResult[25] = 0x23;
            // IP - Source IP
            xResult[26] = 0xc0;
            xResult[27] = 0xa8;
            xResult[28] = 0x16;
            xResult[29] = 0x0d;
            // IP - Destination IP
            xResult[30] = 0xFF;
            xResult[31] = 0xFF;
            xResult[32] = 0xFF;
            xResult[33] = 0xFF;
            // UDP - Source Port - Varies
            xResult[34] = 0x04;
            xResult[35] = 0x49;
            // UDP - Destination Port
            xResult[36] = 0x08;
            xResult[37] = 0xAE;
            // UDP - Length
            xResult[38] = 0x00;
            xResult[39] = 0x09;
            // UDP - Checksum - Varies
            xResult[40] = 0x06;
            xResult[41] = 0x30;
            // UDP Data
            xResult[42] = 0x16;

            xResult[24] = 0;
            xResult[25] = 0;
            var xIPChecksum = IPChecksum(xResult);
            if (xIPChecksum != 0x0923) {
                throw new Exception("IP Checksum error");
            }
            xResult[24] = (byte)(xIPChecksum >> 8);
            xResult[25] = (byte)(xIPChecksum & 0xFF);

            xResult[40] = 0;
            xResult[41] = 0;
            var xUDPChecksum = UDPChecksum(xResult);
            if (xUDPChecksum != 0x0630) {
                throw new Exception("UDP Checksum error - " + xUDPChecksum.ToString("X"));
            }
            xResult[40] = (byte)(xUDPChecksum >> 8);
            xResult[41] = (byte)(xUDPChecksum & 0xFF);

            return xResult;
        }
    }
}
