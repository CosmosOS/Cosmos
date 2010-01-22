using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Debug {
    // Type of message sent to client debugger
    public enum MsgType { TracePoint = 1, Message = 2, BreakPoint = 3, Error = 4, Pointer = 5 }
    // These commands come from the client debugger to OS
    public enum Command { TraceOff = 1, TraceOn = 2, Break = 3, Step = 4, BreakOnAddress = 5 }
}
