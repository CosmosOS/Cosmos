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
    public class SendFrame : CodeBlock {
      public override void Assemble() {
        AL = (int)Ds2Vs.Frame;
        Call("DebugStub_ComWriteAL");

        EAX = 32;
        Call("DebugStub_ComWriteAX");

        ESI = CallerEBP.Value;
        ESI.Add(8); // Dont transmit EIP or old EBP
        ECX = 32;
        Call("DebugStub_ComWriteX");
      }
    }

    public class SendStack : CodeBlock {
      public override void Assemble() {
        AL = (int)Ds2Vs.Stack;
        Call("DebugStub_ComWriteAL");

        // Send size of bytes
        ESI = CallerESP.Value;
        EAX = CallerEBP.Value;
        EAX.Sub(ESI);
        Call("DebugStub_ComWriteAX");

        // Send actual bytes
        //
        // Need to reload ESI, WriteAXToCompPort modifies it
        ESI = CallerESP.Value;
        Label = ".SendByte";
        ESI.Compare(CallerEBP.Value);
        JumpIf(Flags.Equal, ".Exit");
        Call("DebugStub_ComWrite8");
        Jump(".SendByte");
      }
    }

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

    public class SendTrace : CodeBlock {
      // Modifies: EAX, ESI
      public override void Assemble() {
        DebugStatus.Value.Compare(Status.Run);
        JumpIf(Flags.Equal, ".Normal");
        AL = (int)Ds2Vs.BreakPoint;
        Jump(".Type");

        Label = ".Normal";
        AL = (int)Ds2Vs.TracePoint;

        Label = ".Type";
        Call("DebugStub_ComWriteAL");

        // Send Calling EIP.
        ESI = CallerEIP.Address;
        Call("DebugStub_ComWrite32");
      }
    }

    public class SendText : CodeBlock {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)Ds2Vs.Message;
        Call("DebugStub_ComWriteAL");

        // Write Length
        ESI = EBP;
        ESI = ESI + 12;
        ECX = ESI[0];
        Call("DebugStub_ComWrite16");

        // Address of string
        ESI = EBP[8];
        Label = ".WriteChar";
        ECX.Compare(0);
        JumpIf(Flags.Equal, ".Exit");
        Call("DebugStub_ComWrite8");
        ECX--;
        // We are storing as 16 bits, but for now I will transmit 8 bits
        // So we inc again to skip the 0
        ESI++;
        Jump(".WriteChar");
      }
    }

    public class SendPtr : CodeBlock {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)Ds2Vs.Pointer;
        Call("DebugStub_ComWriteAL");

        // pointer value
        ESI = EBP[8];
        Call("DebugStub_ComWrite32");
      }
    }
  }
}
