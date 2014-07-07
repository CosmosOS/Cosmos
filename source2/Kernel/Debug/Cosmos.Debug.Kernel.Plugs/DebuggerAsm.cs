using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler;

namespace Cosmos.Debug.Kernel.Plugs {
  [Plug(Target = typeof(Cosmos.Debug.Kernel.Debugger))]
  public static class Debugger {
    [PlugMethod(Assembler = typeof(DebugBreak))]
    public static void Break(Kernel.Debugger aThis) { }

    [PlugMethod(Assembler = typeof(DebugSend))]
    public static unsafe void Send(Kernel.Debugger aThis, int aLength, char* aText) { }

    [PlugMethod(Assembler = typeof(DebugSendMessageBox))]
    public static unsafe void SendMessageBox(Kernel.Debugger aThis, int aLength, char* aText) { }

    [PlugMethod(Assembler = typeof(DebugSendPtr))]
    public static unsafe void SendPtr(Kernel.Debugger aThis, object aPtr) { }

    //[PlugMethod(Assembler = typeof(DebugTraceOff))]
    //public static void TraceOff() { }

    //[PlugMethod(Assembler = typeof(DebugTraceOn))]
    //public static void TraceOn() { }
  }

  //TODO: Make a new plug attrib that assembly plug methods dont need
  // an empty stub also, its just extra fluff - although they allow signature matching
  // Maybe could merge this into the same unit as the plug
  public class DebugTraceOff : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
      aAssembler.Add(new LiteralAssemblerCode("pushad"));
      aAssembler.Add(new LiteralAssemblerCode("Call DebugStub_TraceOff"));
      aAssembler.Add(new LiteralAssemblerCode("popad"));
      aAssembler.Add(new LiteralAssemblerCode("%endif"));
    }
  }

  public class DebugTraceOn : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
      aAssembler.Add(new LiteralAssemblerCode("pushad"));
      aAssembler.Add(new LiteralAssemblerCode("Call DebugStub_TraceOn"));
      aAssembler.Add(new LiteralAssemblerCode("popad"));
      aAssembler.Add(new LiteralAssemblerCode("%endif"));
    }
  }

  public class DebugBreak : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
      aAssembler.Add(new LiteralAssemblerCode("mov dword [DebugBreakOnNextTrace], 1"));
      aAssembler.Add(new LiteralAssemblerCode("%endif"));
    }
  }

  public class DebugSend : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
      aAssembler.Add(new LiteralAssemblerCode("pushad"));
      aAssembler.Add(new LiteralAssemblerCode("Call DebugStub_SendText"));
      aAssembler.Add(new LiteralAssemblerCode("popad"));
      aAssembler.Add(new LiteralAssemblerCode("%endif"));
    }
  }

  public class DebugSendMessageBox : AssemblerMethod
  {
      public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
      {
          aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
          aAssembler.Add(new LiteralAssemblerCode("pushad"));
          aAssembler.Add(new LiteralAssemblerCode("Call DebugStub_SendMessageBox"));
          aAssembler.Add(new LiteralAssemblerCode("popad"));
          aAssembler.Add(new LiteralAssemblerCode("%endif"));
      }
  }

  public class DebugSendPtr : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      aAssembler.Add(new LiteralAssemblerCode("%ifdef DEBUGSTUB"));
      aAssembler.Add(new LiteralAssemblerCode("pushad"));
      aAssembler.Add(new LiteralAssemblerCode("Call DebugStub_SendPtr"));
      aAssembler.Add(new LiteralAssemblerCode("popad"));
      aAssembler.Add(new LiteralAssemblerCode("%endif"));
    }
  }
}
