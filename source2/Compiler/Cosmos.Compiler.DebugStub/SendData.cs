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
    public class SendRegisters : Inlines {
      public override void Assemble() {
        AL = (int)DsVsip.Registers; // Send the actual started signal
        Call<WriteALToComPort>();

        ESI = DebugPushAllPtr.Value;
        WriteBytesToComPort(32);
        ESI = CallerESP.Address;
        WriteBytesToComPort(4);
        ESI = CallerEIP.Address;
        WriteBytesToComPort(4);
      }
    }

    public class SendFrame : Inlines {
      public override void Assemble() {
        AL = (int)DsVsip.Frame;
        Call<WriteALToComPort>();

        int xCount = 8 * 4;
        EAX = (uint)xCount;
        Call<WriteAXToComPort>();

        ESI = CallerEBP.Value;
        ESI.Add(8); // Dont transmit EIP or old EBP
        WriteBytesToComPort(xCount);
      }
    }

    public class SendStack : CodeBlock {
      public override void Assemble() {
        AL = (int)DsVsip.Stack;
        Call<WriteALToComPort>();

        // Send size of bytes
        ESI = CallerESP.Value;
        EAX = CallerEBP.Value;
        EAX.Sub(ESI);
        Call<WriteAXToComPort>();

        // Send actual bytes
        //
        // Need to reload ESI, WriteAXToCompPort modifies it
        ESI = CallerESP.Value;
        Label = ".SendByte";
        ESI.Compare(CallerEBP.Value);
        JumpIf(Flags.Equal, ".Exit");
        Call<WriteByteToComPort>();
        Jump(".SendByte");
      }
    }

    public class SendMethodContext : Inlines {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      [XSharp(PreserveStack = true)]
      public override void Assemble() {
        AL = (int)DsVsip.MethodContext;
        Call<WriteALToComPort>();

        // offset relative to ebp
        // size of data to send
        ReadComPortX32toStack(2);
        ECX.Pop();
        EAX.Pop();

        // now ECX contains size of data (count)
        // EAX contains relative to EBP
        ESI = CallerEBP.Value;
        ESI.Add(EAX); //TODO: ESI = ESI + EAX

        Label = ".SendByte";
        ECX.Compare(0);
        JumpIf(Flags.Equal, ".AfterSendByte");
        Call<WriteByteToComPort>();
        ECX--;
        Jump(".SendByte");
        Label = ".AfterSendByte";
      }
    }

    public class SendMemory : Inlines {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      [XSharp(PreserveStack = true)]
      public override void Assemble() {
        ReadComPortX32toStack(1);
        Label = "DebugStub_SendMemory_1";
        AL = (int)DsVsip.MemoryData;
        Call<WriteALToComPort>();

        ReadComPortX32toStack(1);
        Label = "DebugStub_SendMemory_2";
        ECX.Pop();
        ESI.Pop();

        // now ECX contains size of data (count)
        // ESI contains address

        Label = "DebugStub_SendMemory_3";
        Label = "DebugStub_SendMemory_SendByte";
        new Compare { DestinationReg = Registers.ECX, SourceValue = 0 };
        JumpIf(Flags.Equal, "DebugStub_SendMemory_After_SendByte");
        Call<WriteByteToComPort>();
        ECX--;
        Jump("DebugStub_SendMemory_SendByte");

        Label = "DebugStub_SendMemory_After_SendByte";
      }
    }

    public class SendTrace : Inlines {
      // Modifies: EAX, ESI
      public override void Assemble() {
        DebugStatus.Value.Compare(Status.Run);
        JumpIf(Flags.Equal, ".Normal");
        AL = (int)DsVsip.BreakPoint;
        Jump(".Type");

        Label = ".Normal";
        AL = (int)DsVsip.TracePoint;

        Label = ".Type";
        Call<WriteALToComPort>();

        // Send Calling EIP.
        ESI = CallerEIP.Address;
        WriteBytesToComPort(4);
      }
    }

    public class SendText : Inlines {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)DsVsip.Message;
        Call<WriteALToComPort>();

        // Write Length
        ESI = EBP;
        ESI = ESI + 12;
        ECX = ESI[0];
        WriteBytesToComPort(2);

        // Address of string
        ESI = EBP[8];
        Label = ".WriteChar";
        ECX.Compare(0);
        JumpIf(Flags.Equal, ".Exit");
        Call<WriteByteToComPort>();
        ECX--;
        // We are storing as 16 bits, but for now I will transmit 8 bits
        // So we inc again to skip the 0
        ESI++;
        Jump(".WriteChar");
      }
    }

    public class SendPtr : Inlines {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)DsVsip.Pointer;
        Call<WriteALToComPort>();

        // pointer value
        ESI = EBP[8];
        WriteBytesToComPort(4);
      }
    }
  }
}
