using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.DebugConnectors
{
    public class DebugConnectorEdison : DebugConnectorSerial
    {
        private readonly string mKernelFile;

        public DebugConnectorEdison(string aPort, string kernelFile) : base(aPort)
        {
            mKernelFile = kernelFile;
            //			mKernelFile = @"c:\Data\Sources\OpenSource\Edison\CosmosEdison\TestKernel\WriteLineViaUBootApi\kernel";
        }

        // "boot > "
        private byte[] mBootPrompt = new byte[7] { 98, 111, 111, 116, 32, 62, 32 };
        private byte[] mBootPromptCheck = new byte[7];

        //protected override void BeforeSendCmd()
        //{
        //    SendRawData(new byte[1]
        //          {
        //            Vs2Ds.Noop
        //          });
        //    SendRawData(new byte[1]
        //          {
        //            Vs2Ds.Noop
        //          });
        //    SendRawData(new byte[1]
        //          {
        //            Vs2Ds.Noop
        //          });
        //}

        private int mBootStage = 0;

        protected override void WaitForSignature(byte[] aPacket)
        {
            //if (mWaitForBootPrompt)
            //{
            //    WaitForBootPrompt(aPacket);
            //    return;
            //}

            switch (mBootStage)
            {
                // start by sending out a message to the window
                case 0:
                    base.SendTextToConsole("Waiting for U-Boot bootloader prompt...\r\n");
                    mBootStage = 1;
                    break;
                // the bootloader should be configured to wait for input:
                case 1:
                    if (WaitForBootPrompt(aPacket))
                    {
                        SendTextToConsole("Sending kernel now\r\n");
                        SendTextToConsole("Filename = '" + mKernelFile + "'\r\n");
                        SendRawData("\0loady 0x1000000\r\n");
                        mBootStage = 2;
                    }
                    break;
                case 2:
                    if (HandleFileSending(aPacket))
                    {
                        SendTextToConsole("Done sending kernel file\r\n");
                        mBootStage = 4;
                    }
                    break;
                //case 3:
                //    if (WaitForBootPrompt(aPacket))
                //    {
                //        SendTextToConsole("mw.l 0xFF009000 0x11F8 1\r\n");
                //        SendRawData("mw.l 0xFF009000 0x11F8 1\r\n");
                //        mBootStage = 4;
                //    }
                //    break;
                case 4:
                    if (WaitForBootPrompt(aPacket))
                    {
                        SendTextToConsole("mw.l 0xFF009000 0x10F8 1\r\n");
                        SendRawData("mw.l 0xFF009000 0x10F8 1\r\n");
                        mBootStage = 5;
                    }
                    break;
                case 5:
                    if (WaitForBootPrompt(aPacket))
                    {
                        // now at boot prompt.
                        SendTextToConsole("Now starting kernel\r\n");
                        mBootStage = 6;
                        SendRawData("go 0x1000000\r\n");
                    }
                    break;
                case 6:
                    //if (WaitForBootPrompt(aPacket))
                    //{
                    //    SendRawData("\0A");
                    //}
                    base.WaitForSignature(aPacket);
                    return;
            }


            SendPacketToConsole(aPacket);
            // keep processing
            Next(1, WaitForSignature);
        }

        private bool WaitForBootPrompt(byte[] aPacket)
        {
            // wait for "boot > "
            mBootPromptCheck[0] = mBootPromptCheck[1];
            mBootPromptCheck[1] = mBootPromptCheck[2];
            mBootPromptCheck[2] = mBootPromptCheck[3];
            mBootPromptCheck[3] = mBootPromptCheck[4];
            mBootPromptCheck[4] = mBootPromptCheck[5];
            mBootPromptCheck[5] = mBootPromptCheck[6];
            mBootPromptCheck[6] = aPacket[0];
            //SendPacketToConsole(aPacket);
            if (mBootPromptCheck.SequenceEqual(mBootPrompt))
            {
                // boot prompt received!
                SendTextToConsole("Boot prompt received!\r\n");
                //Next(1, WaitForSignature);
                return true;
            }
            else
            {
                // Sig not found, keep looking
                //Next(1, WaitForSignature);
                return false;
            }
        }


        private FileStream mKernelFileStream;

        private bool mKernelSent = false;

        private bool HandleFileSending(byte[] aPacket)
        {

            if (mKernelFileStream == null)
            {
                // wait for packet with C (67)
                if (aPacket[0] == 67)
                {
                    // start sending
                    mKernelFileStream = File.Open(mKernelFile, FileMode.Open);
                    SendRawData(ConstructNextLargePacket());
                    SendTextToConsole(".");
                    return false;
                }
                return false;
            }
            if (mKernelFileStream.Position < mKernelFileStream.Length)
            {
                // send next packet
                if (aPacket[0] == YModem_ACK)
                {
                    SendRawData(ConstructNextLargePacket());
                    SendTextToConsole(".");
                    return false;
                }
                if (aPacket[0] == YModem_NACK)
                {
                    SendRawData(mLastPacket);
                    SendTextToConsole("R");
                    return false;
                }
                // weird character, just retransmit for now
                SendTextToConsole("Character value " + aPacket[0] + " found!\r\n");
                SendRawData(mLastPacket);
                return false;
            }
            else
            {
                // end of transmission
                if (aPacket[0] == YModem_ACK)
                {
                    if (mLastPacket != null)
                    {
                        SendRawData(new[]
                                    {
                                        YModem_EOT
                                    });
                        // done!
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (aPacket[0] == YModem_NACK)
                {
                    SendRawData(new[] { YModem_EOT });
                    SendTextToConsole("R");
                    return false;
                }
                //if (aPacket[0] == 67)
                {
                    // send null item
                    SendRawData(ConstructNullPacket());
                    SendTextToConsole("!");
                    mLastPacket = null;
                    return false;
                }

                // weird character, just retransmit for now
                SendTextToConsole("Character value " + aPacket[0] + " found!\r\n");
                SendRawData(new[] { YModem_EOT });
                return false;
            }
        }

        private byte mLastPacketIdx = 0;
        private byte[] mLastPacket;
        private byte[] ConstructNextLargePacket()
        {
            using (var xMS = new MemoryStream(1030))
            {
                xMS.WriteByte(YModem_STX);
                mLastPacketIdx++;
                xMS.WriteByte(mLastPacketIdx);
                xMS.WriteByte((byte)(255 - mLastPacketIdx));
                var xBuff = new byte[1024];
                var xLength = mKernelFileStream.Read(xBuff, 0, xBuff.Length);
                if (xLength < xBuff.Length)
                {
                    for (int i = xLength; i < xBuff.Length; i++)
                    {
                        xBuff[i] = YModem_EOF;
                    }
                }
                xMS.Write(xBuff, 0, xBuff.Length);
                var xCrc = Crc16(xBuff);
                xMS.WriteByte((byte)(xCrc >> 8));
                xMS.WriteByte((byte)(xCrc & 0xFF));
                mLastPacket = xMS.ToArray();
                return mLastPacket;
            }
        }

        private byte[] ConstructNullPacket()
        {
            using (var xMS = new MemoryStream(1030))
            {
                xMS.WriteByte(YModem_STX);
                mLastPacketIdx++;
                xMS.WriteByte(mLastPacketIdx);
                xMS.WriteByte((byte)(255 - mLastPacketIdx));
                var xBuff = new byte[1024];
                xMS.Write(xBuff, 0, xBuff.Length);
                var xCrc = Crc16(xBuff);
                xMS.WriteByte((byte)(xCrc >> 8));
                xMS.WriteByte((byte)(xCrc & 0xFF));
                return xMS.ToArray();
            }
        }

        private const byte YModem_STX = 0x02;
        private const byte YModem_ACK = 0x06;
        private const byte YModem_NACK = 0x15;
        private const byte YModem_EOT = 0x04;
        private const byte YModem_EOF = 0x1A;

        private static ushort Crc16(byte[] data)
        {
            int i;
            ushort cksum;

            cksum = 0;
            for (i = 0; i < data.Length; i++)
            {
                cksum = (ushort)(Crc16Constants[((cksum >> 8) ^ data[i]) & 0xFF] ^ (cksum << 8));
            }
            return cksum;
        }

        private static readonly ushort[] Crc16Constants = new ushort[]
                                                          {
                                                            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7,
                                                            0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef,
                                                            0x1231, 0x0210, 0x3273, 0x2252, 0x52b5, 0x4294, 0x72f7, 0x62d6,
                                                            0x9339, 0x8318, 0xb37b, 0xa35a, 0xd3bd, 0xc39c, 0xf3ff, 0xe3de,
                                                            0x2462, 0x3443, 0x0420, 0x1401, 0x64e6, 0x74c7, 0x44a4, 0x5485,
                                                            0xa56a, 0xb54b, 0x8528, 0x9509, 0xe5ee, 0xf5cf, 0xc5ac, 0xd58d,
                                                            0x3653, 0x2672, 0x1611, 0x0630, 0x76d7, 0x66f6, 0x5695, 0x46b4,
                                                            0xb75b, 0xa77a, 0x9719, 0x8738, 0xf7df, 0xe7fe, 0xd79d, 0xc7bc,
                                                            0x48c4, 0x58e5, 0x6886, 0x78a7, 0x0840, 0x1861, 0x2802, 0x3823,
                                                            0xc9cc, 0xd9ed, 0xe98e, 0xf9af, 0x8948, 0x9969, 0xa90a, 0xb92b,
                                                            0x5af5, 0x4ad4, 0x7ab7, 0x6a96, 0x1a71, 0x0a50, 0x3a33, 0x2a12,
                                                            0xdbfd, 0xcbdc, 0xfbbf, 0xeb9e, 0x9b79, 0x8b58, 0xbb3b, 0xab1a,
                                                            0x6ca6, 0x7c87, 0x4ce4, 0x5cc5, 0x2c22, 0x3c03, 0x0c60, 0x1c41,
                                                            0xedae, 0xfd8f, 0xcdec, 0xddcd, 0xad2a, 0xbd0b, 0x8d68, 0x9d49,
                                                            0x7e97, 0x6eb6, 0x5ed5, 0x4ef4, 0x3e13, 0x2e32, 0x1e51, 0x0e70,
                                                            0xff9f, 0xefbe, 0xdfdd, 0xcffc, 0xbf1b, 0xaf3a, 0x9f59, 0x8f78,
                                                            0x9188, 0x81a9, 0xb1ca, 0xa1eb, 0xd10c, 0xc12d, 0xf14e, 0xe16f,
                                                            0x1080, 0x00a1, 0x30c2, 0x20e3, 0x5004, 0x4025, 0x7046, 0x6067,
                                                            0x83b9, 0x9398, 0xa3fb, 0xb3da, 0xc33d, 0xd31c, 0xe37f, 0xf35e,
                                                            0x02b1, 0x1290, 0x22f3, 0x32d2, 0x4235, 0x5214, 0x6277, 0x7256,
                                                            0xb5ea, 0xa5cb, 0x95a8, 0x8589, 0xf56e, 0xe54f, 0xd52c, 0xc50d,
                                                            0x34e2, 0x24c3, 0x14a0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                                                            0xa7db, 0xb7fa, 0x8799, 0x97b8, 0xe75f, 0xf77e, 0xc71d, 0xd73c,
                                                            0x26d3, 0x36f2, 0x0691, 0x16b0, 0x6657, 0x7676, 0x4615, 0x5634,
                                                            0xd94c, 0xc96d, 0xf90e, 0xe92f, 0x99c8, 0x89e9, 0xb98a, 0xa9ab,
                                                            0x5844, 0x4865, 0x7806, 0x6827, 0x18c0, 0x08e1, 0x3882, 0x28a3,
                                                            0xcb7d, 0xdb5c, 0xeb3f, 0xfb1e, 0x8bf9, 0x9bd8, 0xabbb, 0xbb9a,
                                                            0x4a75, 0x5a54, 0x6a37, 0x7a16, 0x0af1, 0x1ad0, 0x2ab3, 0x3a92,
                                                            0xfd2e, 0xed0f, 0xdd6c, 0xcd4d, 0xbdaa, 0xad8b, 0x9de8, 0x8dc9,
                                                            0x7c26, 0x6c07, 0x5c64, 0x4c45, 0x3ca2, 0x2c83, 0x1ce0, 0x0cc1,
                                                            0xef1f, 0xff3e, 0xcf5d, 0xdf7c, 0xaf9b, 0xbfba, 0x8fd9, 0x9ff8,
                                                            0x6e17, 0x7e36, 0x4e55, 0x5e74, 0x2e93, 0x3eb2, 0x0ed1, 0x1ef0,
                                                          };
    }
}
