using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Cosmos.Debug.Common.CDebugger
{
    //public class DebugConnectorSerial : DebugConnectorStream {
    //    private SerialPort mPort;

    //    public DebugConnectorSerial(byte aPort) { 
    //        // TODO: MtW - Make COM port configurable
    //        mPort = new SerialPort("COM" + aPort, 9600, Parity.None, 8, StopBits.One);
    //        mPort.Handshake = Handshake.None;
    //        mPort.Open();
    //        Start(mPort.BaseStream);
    //    }

    //    public override void WaitConnect() {    
    //        //TODO: Serial we cant detect connection, but we can wait for first byte...
    //        throw new NotImplementedException();
    //    }
    //}
}
