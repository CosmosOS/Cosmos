using System.Linq;
using System.Text;

namespace Cosmos.Debug.DebugConnectors
{
    partial class DebugConnector
    {
        protected void WaitForMessage() => Next(1, PacketMsg);

        protected void PacketTextSize(byte[] aPacket) => Next(GetUInt16(aPacket, 0), PacketText);

        protected void PacketOtherChannelCommand(byte aChannel, byte[] aPacket) =>
            Next(4, data => PacketOtherChannelSize(aChannel, aPacket[0], data));

        protected void PacketOtherChannelSize(byte aChannel, byte aCommand, byte[] aPacket)
        {
            var xPacketSize = (int)GetUInt32(aPacket, 0);
            xPacketSize &= 0xFFF;
            Next(xPacketSize, data => PacketChannel(aChannel, aCommand, data));
        }

        protected void PacketMessageBoxTextSize(byte[] aPacket) => Next(GetUInt16(aPacket, 0), PacketMessageBoxText);

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
            CmdRegisters?.Invoke(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketFrame(byte[] aPacket)
        {
            CmdFrame?.Invoke(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketPong(byte[] aPacket)
        {
            CmdPong?.Invoke(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketChannel(byte channel, byte command, byte[] aPacket)
        {
            if (SigReceived)
            {
                CmdChannel?.Invoke(channel, command, aPacket);
            }

            WaitForMessage();
        }

        protected void PacketNullReferenceOccurred(byte[] aPacket)
        {
            CmdNullReferenceOccurred?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketSimpleNumber(byte[] aPacket)
        {
            CmdSimpleNumber?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketKernelPanic(byte[] aPacket)
        {
            CmdKernelPanic?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketSimpleLongNumber(byte[] aPacket)
        {
            CmdSimpleLongNumber?.Invoke(GetUInt64(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketComplexNumber(byte[] aPacket)
        {
            CmdComplexNumber?.Invoke(GetSingle(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketComplexLongNumber(byte[] aPacket)
        {
            CmdComplexLongNumber?.Invoke(GetDouble(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketStackCorruptionOccurred(byte[] aPacket)
        {
            CmdStackCorruptionOccurred?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketStackOverflowOccurred(byte[] aPacket)
        {
            CmdStackOverflowOccurred?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketInterruptOccurred(byte[] aPacket)
        {
            CmdInterruptOccurred?.Invoke(GetUInt32(aPacket, 0));
            WaitForMessage();
        }

        protected void PacketStack(byte[] aPacket)
        {
            CmdStack?.Invoke(aPacket.ToArray());
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

        protected void PacketCoreDump(byte[] aPacket)
        {
            CmdCoreDump?.Invoke(CoreDump.FromStackArray(aPacket));
            WaitForMessage();
        }

        protected void SizePacket(byte[] aPacket)
        {
            int xSize = aPacket[0] + (aPacket[1] << 8);
            Next(xSize, mCompletedAfterSize);
        }
    }
}
