using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Debug {
  // Messages from Guest to Host
  static public class MsgType {
    public const byte Noop = 0;
    public const byte TracePoint = 1;
    public const byte Message = 2;
    public const byte BreakPoint = 3;
    public const byte Error = 4;
    public const byte Pointer = 5;
    // This is sent once on start up. The first call to debug stub sends this. 
    // Host can then respond with a series of set breakpoints etc, ie ones that were set before running.
    public const byte Started = 6;
    public const byte MethodContext = 7;
    public const byte MemoryData = 8;
    // Sent after commands to acknowledge receipt during batch mode
    public const byte CmdCompleted = 9;
  }

  // Messages from Host to Guest
  static public class Command {
    public const byte Noop = 0;
    public const byte TraceOff = 1;
    public const byte TraceOn = 2;
    public const byte Break = 3;
    public const byte Continue = 4; // After a Break
    public const byte Step = 5;
    public const byte BreakOnAddress = 6;
    public const byte BatchBegin = 7; // Not used yet
    public const byte BatchEnd = 8; // Not used yet
    public const byte SendMethodContext = 9; // sends data from stack, relative to EBP (in x86)
    public const byte SendMemory = 10;
    // Make sure this is always the last entry. Used by DebugStub to verify commands
    public const byte Max = 11;
  }
}
