using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    public class DebugConnectorSerial : DebugConnector {
        protected override void SendData(byte[] aBytes)
        {
            throw new NotImplementedException();
        }

        protected override void Next(int aPacketSize, DebugConnector.PacketReceivedDelegate aCompleted)
        {
            throw new NotImplementedException();
        }

        protected override void PacketText(byte[] aPacket)
        {
            throw new NotImplementedException();
        }

        protected override void PacketTracePoint(byte[] aPacket)
        {
            throw new NotImplementedException();
        }
    }
}
