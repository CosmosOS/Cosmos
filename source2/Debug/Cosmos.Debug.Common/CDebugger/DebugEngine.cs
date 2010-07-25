using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;

namespace Cosmos.Debug.Common.CDebugger {
    public class DebugEngine {
        private DebugConnector mDebugConnector;
        public DebugConnector DebugConnector {
            get {
                return mDebugConnector;
            }
            set {
                if (mDebugConnector != null) {
                    throw new Exception("Cannot set debug connector after it has already been set!");
                }
                mDebugConnector = value;
                mDebugConnector.CmdTrace += OnTrace;
                mDebugConnector.CmdText += OnText;
            }
        }

        private void OnText(string aText) {
            if (TextReceived != null) {
                TextReceived(aText);
            }
        }

        private void OnTrace(MsgType aMessage, UInt32 aData) {
            if (TraceReceived != null)
            {
                TraceReceived(aMessage, aData);
            }
        }

        public event Action<MsgType, uint> TraceReceived;
        public event Action<string> TextReceived;
    }
}