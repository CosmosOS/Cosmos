﻿using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Assembler;

namespace Cosmos.Debug.Kernel.Plugs
{
  [Plug(Target = typeof(Cosmos.Debug.Kernel.Debugger))]
  public static class Debugger
  {
    [PlugMethod(Assembler = typeof(DebugBreak))]
    public static void Break(Kernel.Debugger aThis) { }

    //[PlugMethod(Assembler = typeof(DebugActualSend))]
    //public static unsafe void ActualSend(int aLength, char* aText) { }

    [PlugMethod(Assembler = typeof(DebugDoSend))]
    public static void DoSend(string aText) { }

    [PlugMethod(Assembler = typeof(DebugDoSendNumber))]
    public static void DoSendNumber(uint aNumber) { }

    [PlugMethod(Assembler = typeof(DebugDoSendComplexSingleNumber))]
    public static void DoSendNumber(float aNumber) { }

    [PlugMethod(Assembler = typeof(DebugDoSendComplexDoubleNumber))]
    public static void DoSendNumber(double aNumber) { }

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
  public class DebugTraceOff : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("pushad");
      new LiteralAssemblerCode("Call DebugStub_TraceOff");
      new LiteralAssemblerCode("popad");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugTraceOn : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("pushad");
      new LiteralAssemblerCode("Call DebugStub_TraceOn");
      new LiteralAssemblerCode("popad");
      new LiteralAssemblerCode("%endif");
    }
  }

  /// <summary>
  /// Assembler for SendChannelCommand
  /// </summary>
  /// <remarks>
  /// AL contains channel
  /// BL contains command
  /// ECX contains number of bytes to send as command data
  /// ESI contains data start pointer
  /// </remarks>
  public class DebugSendChannelCommand : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("mov AL, [EBP + 20]");
      new LiteralAssemblerCode("mov BL, [EBP + 16]");
      new LiteralAssemblerCode("mov ECX, [EBP + 12]");
      new LiteralAssemblerCode("mov ESI, [EBP + 8]");
      new LiteralAssemblerCode("call DebugStub_SendCommandOnChannel");
      new LiteralAssemblerCode("%endif");
    }
  }

  /// <summary>
  /// Assembler for SendChannelCommandNoData
  /// </summary>
  /// <remarks>
  /// AL contains channel
  /// BL contains command
  /// ECX contains number of bytes to send as command data
  /// ESI contains data start pointer
  /// </remarks>
  public class DebugSendChannelCommandNoData : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("mov AL, [EBP + 12]");
      new LiteralAssemblerCode("mov BL, [EBP + 8]");
      new LiteralAssemblerCode("mov ECX, 0");
      new LiteralAssemblerCode("mov ESI, EBP");
      new LiteralAssemblerCode("call DebugStub_SendCommandOnChannel");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugBreak : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("mov dword [DebugStub_DebugBreakOnNextTrace], 1");
      new LiteralAssemblerCode("%endif");
    }
  }

  /// <summary>
  /// Assembler for DoSend
  /// </summary>
  /// <remarks>
  /// EBP + 12 contains length
  /// EBP + 8 contains first char pointer
  /// </remarks>
  public class DebugDoSend : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new Label(".BeforeArgumentsPrepare");
      new LiteralAssemblerCode("mov EBX, [EBP + 8]");
      new LiteralAssemblerCode("mov EBX, [EBX]");
      new LiteralAssemblerCode("push dword [EBX + 12]");
      new LiteralAssemblerCode("add EBX, 16");
      new LiteralAssemblerCode("push dword EBX");
      new Label(".BeforeCall");
      new LiteralAssemblerCode("Call DebugStub_SendText");
      new LiteralAssemblerCode("add ESP, 8");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugDoSendNumber : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("push dword [EBP + 8]");
      new LiteralAssemblerCode("Call DebugStub_SendSimpleNumber");
      new LiteralAssemblerCode("add ESP, 4");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugDoSendComplexSingleNumber : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("push dword [EBP + 8]");
      new LiteralAssemblerCode("Call DebugStub_SendComplexSingleNumber");
      new LiteralAssemblerCode("add ESP, 4");
      new LiteralAssemblerCode("%endif");
    }
  }

  public class DebugDoSendComplexDoubleNumber : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("push dword [EBP + 12]");
      new LiteralAssemblerCode("push dword [EBP + 8]");
      new LiteralAssemblerCode("Call DebugStub_SendComplexDoubleNumber");
      new LiteralAssemblerCode("add ESP, 8");
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

  public class DebugSendPtr : AssemblerMethod
  {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
    {
      new LiteralAssemblerCode("%ifdef DEBUGSTUB");
      new LiteralAssemblerCode("pushad");
      new LiteralAssemblerCode("Call DebugStub_SendPtr");
      new LiteralAssemblerCode("popad");
      new LiteralAssemblerCode("%endif");
    }
  }
}
