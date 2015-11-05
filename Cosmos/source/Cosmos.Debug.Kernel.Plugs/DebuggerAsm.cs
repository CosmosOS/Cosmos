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

    //[PlugMethod(Assembler = typeof(DebugActualSend))]
    //public static unsafe void ActualSend(int aLength, char* aText) { }

    [PlugMethod(Assembler = typeof(DebugDoSend))]
    public static void DoSend(string aText) { }

    [PlugMethod(Assembler = typeof(DebugDoSendNumber))]
    public static void DoSendNumber(uint aNumber) { }

    [PlugMethod(Assembler = typeof(DebugSendMessageBox))]
    public static unsafe void SendMessageBox(Kernel.Debugger aThis, int aLength, char* aText) { }

    [PlugMethod(Assembler = typeof(DebugSendPtr))]
    public static unsafe void SendPtr(Kernel.Debugger aThis, object aPtr) { }

    [PlugMethod(Assembler = typeof(DebugSendChannelCommand))]
    public static unsafe void SendChannelCommand(byte aChannel, byte aCommand, int aByteCount, byte* aData) { }

    [PlugMethod(Assembler = typeof(DebugSendChannelCommandNoData))]
    public static unsafe void SendChannelCommand(byte aChannel, byte aCommand) { }
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
     new LiteralAssemblerCode("%ifdef DEBUGSTUB");
     new LiteralAssemblerCode("pushad");
     new LiteralAssemblerCode("Call DebugStub_TraceOff");
     new LiteralAssemblerCode("popad");
     new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugTraceOn : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
     new LiteralAssemblerCode("%ifdef DEBUGSTUB");
     new LiteralAssemblerCode("pushad");
     new LiteralAssemblerCode("Call DebugStub_TraceOn");
     new LiteralAssemblerCode("popad");
     new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugSendChannelCommand : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      // AL contains channel
      new LiteralAssemblerCode("mov AL, [EBP + 20]");
      // BL contains command
      new LiteralAssemblerCode("mov BL, [EBP + 16]");
      // ECX contains number of bytes to send as command data
      new LiteralAssemblerCode("mov ECX, [EBP + 12]");
      // ESI contains data start pointer
      new LiteralAssemblerCode("mov ESI, [EBP + 8]");
      new LiteralAssemblerCode("call DebugStub_SendCommandOnChannel");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugSendChannelCommandNoData : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      // AL contains channel
      new LiteralAssemblerCode("mov AL, [EBP + 12]");
      // BL contains command
      new LiteralAssemblerCode("mov BL, [EBP + 8]");
      // ECX contains number of bytes to send as command data
      new LiteralAssemblerCode("mov ECX, 0");
      // ESI contains data start pointer
      new LiteralAssemblerCode("mov ESI, EBP");
      new LiteralAssemblerCode("call DebugStub_SendCommandOnChannel");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugBreak : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
     new LiteralAssemblerCode("%ifdef DEBUGSTUB");
     new LiteralAssemblerCode("mov dword [DebugBreakOnNextTrace], 1");
     new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugDoSend : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      //new LiteralAssemblerCode("pushad");
      new Label(".BeforeArgumentsPrepare");
      // length: will be at EBP+12 in DebugStub_SendText
      new LiteralAssemblerCode("mov ebx, [ebp+8]");
      new LiteralAssemblerCode("mov ebx, [ebx]");
      new LiteralAssemblerCode("push dword [ebx + 12]");
      // first char pointer, will be at EBP+8 in DebugStub_SendText
      new LiteralAssemblerCode("add ebx, 16");
      new LiteralAssemblerCode("push dword ebx");
      new Label(".BeforeCall");
      new LiteralAssemblerCode("Call DebugStub_SendText");
      // for x#, we need to cleanup after a call:
      new LiteralAssemblerCode("add esp, 8");
      //new LiteralAssemblerCode("popad");
      //new LiteralAssemblerCode("mov ecx, [ebp+12]");
      //new LiteralAssemblerCode("imul ecx, 2");

      //new LiteralAssemblerCode("mov al, 129");
      //new LiteralAssemblerCode("mov bl, 0");
      //new LiteralAssemblerCode("mov esi, [ebp+8]");
      //new LiteralAssemblerCode("add esi, 16");
      //new LiteralAssemblerCode("call DebugStub_SendCommandOnChannel");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugDoSendNumber : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("push dword [ebp+8]");
      new LiteralAssemblerCode("Call DebugStub_SendSimpleNumber");
      new LiteralAssemblerCode("add esp, 4");
      new LiteralAssemblerCode("%endif");
    }
  }
  public class DebugSendMessageBox : AssemblerMethod
  {
      public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
      {
         new LiteralAssemblerCode("%ifdef DEBUGSTUB");
         new LiteralAssemblerCode("pushad");
         new LiteralAssemblerCode("Call DebugStub_SendMessageBox");
         new LiteralAssemblerCode("popad");
         new LiteralAssemblerCode("%endif");
      }
  }

  public class DebugSendPtr : AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
     new LiteralAssemblerCode("%ifdef DEBUGSTUB");
     new LiteralAssemblerCode("pushad");
     new LiteralAssemblerCode("Call DebugStub_SendPtr");
     new LiteralAssemblerCode("popad");
     new LiteralAssemblerCode("%endif");
    }
  }
}
