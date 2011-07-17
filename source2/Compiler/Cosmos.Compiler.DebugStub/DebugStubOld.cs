using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Debug;
using Cosmos.Compiler.XSharp;

//TODO: The asm code here is not efficient. Our first priority is to make it functionally robust and working
// Later we can optimize it.
namespace Cosmos.Compiler.DebugStub {
  public class DebugStubOld : CodeBlock {
    public override void Assemble() { }
    protected UInt16 mComAddr;
    protected UInt16 mComStatusAddr;

    public static void EmitDataSection() {
      Assembler.Assembler.CurrentInstance.DataMembers.AddRange(new DataMember[]{
                // Tracing: 0=Off, 1=On
                new DataMember("DebugTraceMode", 0),
                // enum Status
                new DataMember("DebugStatus", 0),
                    
                // Nesting control for non steppable routines
                new DataMember("DebugSuspendLevel", 0),
                // Nesting control for non steppable routines 
                new DataMember("DebugResumeLevel", 0),
                // Ptr to the push all data. It points to the "bottom" after a PushAll op.
                // Walk up to find the 8 x 32 bit registers.
                new DataMember("DebugPushAllPtr", 0),
                new DataMember("InterruptsEnabledFlag", 0),
                
                // If set non 0, on next trace a break will occur
                new DataMember("DebugBreakOnNextTrace", (uint)DebugStub.StepTrigger.None),
                // For step out and over this is used to determine where the initial request was made
                // EBP is logged when the trace is started and can be used to determine 
                // what level we are "at" relative to the original step start location.
                new DataMember("DebugBreakEBP", 0),

                // Command ID of last command received
                new DataMember("DebugStub_CommandID", 0),
                // Breakpoint addresses
                new DataMember("DebugBPs", new int[256]),
                //TODO: Move to DebugStub (new)
                new DataMember("DebugWaitMsg", "Waiting for debugger connection...")
             });
    }

    public void Main(UInt16 aComAddr) {
      mComAddr = aComAddr;
      mComStatusAddr = (UInt16)(aComAddr + 5);
      EmitDataSection();
    }

  }

}