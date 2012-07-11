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
    public class SendMethodContext : CodeBlock {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      [XSharp(PreserveStack = true)]
      public override void Assemble() {
        AL = (int)Ds2Vs.MethodContext;
        Call("DebugStub_ComWriteAL");

        // offset relative to ebp
        // size of data to send
        Call("DebugStub_ComReadEAX");
        ECX = EAX;
        Call("DebugStub_ComReadEAX");

        // now ECX contains size of data (count)
        // EAX contains relative to EBP
        ESI = CallerEBP.Value;
        ESI.Add(EAX); //TODO: ESI = ESI + EAX

        Label = ".SendByte";
        ECX.Compare(0);
        JumpIf(Flags.Equal, ".AfterSendByte");
        Call("DebugStub_ComWrite8");
        ECX--;
        Jump(".SendByte");
        Label = ".AfterSendByte";
      }
    }

    public class SendMemory : CodeBlock {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      [XSharp(PreserveStack = true)]
      public override void Assemble() {
        Call("DebugStub_ComReadEAX");
        ECX = EAX;
        AL = (int)Ds2Vs.MemoryData;
        Call("DebugStub_ComWriteAL");

        Call("DebugStub_ComReadEAX");
        ESI = EAX;

        // now ECX contains size of data (count)
        // ESI contains address

        Label = "DebugStub_SendMemory_SendByte";
        new Compare { DestinationReg = Registers.ECX, SourceValue = 0 };
        JumpIf(Flags.Equal, "DebugStub_SendMemory_After_SendByte");
        Call("DebugStub_ComWrite8");
        ECX--;
        Jump("DebugStub_SendMemory_SendByte");

        Label = "DebugStub_SendMemory_After_SendByte";
      }
    }
  }
}