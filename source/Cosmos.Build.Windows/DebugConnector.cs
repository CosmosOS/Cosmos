using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    public abstract class DebugConnector {
        public delegate void ConnectionLostDelegate(Exception ex);
        public delegate void CmdTraceDelegate(UInt32 aEIP);
        public delegate void CmdTextDelegate(string aText);
        
        //TODO: These should not be this way and should in fact
        // be checked or better yet done by constructor arguments
        // but that puts a dependency on where the sub classes
        // are created.
        public ConnectionLostDelegate ConnectionLost;
        public CmdTraceDelegate CmdTrace;
        public CmdTextDelegate CmdText;
        // Cannot use Dispatcher.CurrentDispatcher - it doesnt work
        // Must use same dispatcher as the Window. Could also change
        // delegates to catch them in a thread and then redispatch on their own
        public System.Windows.Threading.Dispatcher Dispatcher;
        
        public abstract void SendCommand(byte aCmd);
    }
}
