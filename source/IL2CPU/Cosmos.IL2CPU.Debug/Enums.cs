using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.Debug {
    // Type of message sent to client debugger
    public enum MsgType { TracePoint = 0 }
    // These commands come from the client debugger to OS
    public enum Command { TraceOff = 1, TraceOn = 2, Break = 3, Step = 4 }
}
