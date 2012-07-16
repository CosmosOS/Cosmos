using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Debug.Consts;
using Cosmos.Assembler.XSharp;

namespace Cosmos.Debug.DebugStub {
  public partial class DebugStub : CodeGroup {
    // Caller's Registers
    static public DataMember32 CallerEBP;
    static public DataMember32 CallerEIP;
    static public DataMember32 CallerESP;

    // Tracing: 0=Off, 1=On
    static protected DataMember32 TraceMode;
    // enum Status
    static protected DataMember32 DebugStatus;
    // Pointer to the push all data. It points to the bottom after PushAll.
    // Walk up to find the 8 x 32 bit registers.
    static protected DataMember32 PushAllPtr;
    // If set non 0, on next trace a break will occur
    static protected DataMember32 DebugBreakOnNextTrace;
    // For step out and over this is used to determine where the initial request was made
    // EBP is logged when the trace is started and can be used to determine 
    // what level we are "at" relative to the original step start location.
    static protected DataMember32 BreakEBP;
    // Command ID of last command received
    static protected DataMember32 CommandID;

  }
}
