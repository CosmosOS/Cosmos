using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    public class DebugConnectorVMWare : DebugConnector {
        protected NamedPipeClientStream mPipeStream;
    
        public DebugConnectorVMWare() {
            // \\.\pipe\CosmosDebug
            mPipeStream = new NamedPipeClientStream(".", @"\pipe\CosmosDebug", PipeDirection.In);
            mPipeStream.Connect();
        }

        public override void SendCommand(byte aCmd) {
        }
    }
}
