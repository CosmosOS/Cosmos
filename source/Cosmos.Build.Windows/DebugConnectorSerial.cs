using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Cosmos.Build.Windows {
    public class DebugConnectorSerial : DebugConnectorStream {
        private SerialPort mPort;
        public DebugConnectorSerial() { 
            // TODO: MtW - Make COM port configurable
            mPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            mPort.Handshake = Handshake.None;
            mPort.Open();
            Start(mPort.BaseStream);
        }
    }
}
