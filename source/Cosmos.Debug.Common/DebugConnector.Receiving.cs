﻿using System;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.Common
{
    partial class DebugConnector
    {
        protected void WaitForMessage()
        {
            Next(1, PacketMsg);
        }

        protected void PacketTextSize(byte[] aPacket)
        {
            Next(GetUInt16(aPacket, 0), PacketText);
        }

        protected void PacketOtherChannelCommand(byte aChannel, byte[] aPacket)
        {
            Next(4, data => PacketOtherChannelSize(aChannel, aPacket[0], data));
        }

        protected void PacketOtherChannelSize(byte aChannel, byte aCommand, byte[] aPacket)
        {
            var xPacketSize = (int)GetUInt32(aPacket, 0);
            xPacketSize &= 0xFFF;
            Next(xPacketSize, data => PacketChannel(aChannel, aCommand, data));
        }

        protected void PacketMessageBoxTextSize(byte[] aPacket)
        {
            Next(GetUInt16(aPacket, 0), PacketMessageBoxText);
        }

        protected void PacketMethodContext(byte[] aPacket)
        {
            MethodContextDatas.Add(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketMemoryData(byte[] aPacket)
        {
            MemoryDatas.Add(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketRegisters(byte[] aPacket)
        {
            if (CmdRegisters != null)
            {
                CmdRegisters(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketFrame(byte[] aPacket)
        {
            if (CmdFrame != null)
            {
                CmdFrame(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketPong(byte[] aPacket)
        {
            if (CmdPong != null)
            {
                CmdPong(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketChannel(byte channel, byte command, byte[] aPacket)
        {
            if (SigReceived)
            {
                if (CmdChannel != null)
                {
                    CmdChannel(channel, command, aPacket);
                }
            }
            WaitForMessage();
        }

        protected void PacketNullReferenceOccurred(byte[] aPacket)
        {
            if (CmdNullReferenceOccurred != null)
            {
                CmdNullReferenceOccurred(GetUInt32(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketSimpleNumber(byte[] aPacket)
        {
            if (CmdSimpleNumber != null)
            {
                CmdSimpleNumber(GetUInt32(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketComplexSingleNumber(byte[] aPacket)
        {
            if (CmdComplexSingleNumber != null)
            {
                CmdComplexSingleNumber(GetSingle(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketComplexDoubleNumber(byte[] aPacket)
        {
            if (CmdComplexDoubleNumber != null)
            {
                CmdComplexDoubleNumber(GetDouble(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketStackCorruptionOccurred(byte[] aPacket)
        {
            if (CmdStackCorruptionOccurred != null)
            {
                CmdStackCorruptionOccurred(GetUInt32(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketStack(byte[] aPacket)
        {
            if (CmdStack != null)
            {
                CmdStack(aPacket.ToArray());
            }
            WaitForMessage();
        }

        private int lastCmdCompletedID = -1;
        protected void PacketCmdCompleted(byte[] aPacket)
        {
            byte xCmdID = aPacket[0];
            DebugLog("DS Msg: Cmd " + xCmdID + " Complete");
            if (mCurrCmdID != xCmdID)
            {
                DebugLog("DebugStub CmdCompleted Mismatch. Expected " + mCurrCmdID + ", received " + xCmdID + ".");
            }
            // Release command
            lastCmdCompletedID = xCmdID;
            mCmdWait.Set();
            WaitForMessage();
        }

        protected void PacketTracePoint(byte[] aPacket)
        {
            // WaitForMessage must be first. CmdTrace issues
            // more commands and if we dont issue this, the pipe wont be waiting for a response.
            WaitForMessage();
            CmdTrace(GetUInt32(aPacket, 0));
        }

        protected void PacketBreakPoint(byte[] aPacket)
        {
            WaitForMessage();
            CmdBreak(GetUInt32(aPacket, 0));
        }

        protected void PacketText(byte[] aPacket)
        {
            WaitForMessage();
            CmdText(Encoding.ASCII.GetString(aPacket));
            //CmdChannel(129, 0, aPacket);
        }

        protected void PacketMessageBoxText(byte[] aPacket)
        {
            WaitForMessage();
            CmdMessageBox(Encoding.ASCII.GetString(aPacket));
        }

        protected void SizePacket(byte[] aPacket)
        {
            int xSize = aPacket[0] + (aPacket[1] << 8);
            Next(xSize, mCompletedAfterSize);
        }
    }
}
