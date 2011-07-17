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
    public override void Assemble() {
    }

    protected UInt16 mComAddr;
    protected UInt16 mComStatusAddr;

    //TODO: A bit of a hack as a static? Other ideas?
    // Change to fields of CodeGroups and use reflection
    public static void EmitDataSection() {
      Assembler.Assembler.CurrentInstance.DataMembers.AddRange(new DataMember[]{
                // Tracing: 0=Off, 1=On
                new DataMember("DebugTraceMode", 0),
                // enum Status
                new DataMember("DebugStatus", 0),
                    
                // 0 = Not in, 1 = already running
                new DataMember("DebugRunning", 0),
                // Nesting control for non steppable routines
                new DataMember("DebugSuspendLevel", 0),
                // Nesting control for non steppable routines 
                new DataMember("DebugResumeLevel", 0),
                // Last EIP value
                new DataMember("DebugEIP", 0),
                // Calling code's EBP value
                new DataMember("DebugEBP", 0),
                // Calling code's ESP value
                new DataMember("DebugESP", 0),
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

    // INLINE
    // Modifies: Stack, EDI, AL
    // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
    // repeated use of an inline will fail with conflicting labels.
    // TODO: Allow methods to emit a start label and return automatically
    // and mark inlines so this does not happen.
    protected void ReadComPortX32toStack() {
      // Make room on the stack for the address
      Push(0);
      // ReadByteFromComPort writes to EDI, then increments
      EDI = ESP;

      // Read address to stack via EDI
      Call("DebugStub_ReadByteFromComPort");
      Call("DebugStub_ReadByteFromComPort");
      Call("DebugStub_ReadByteFromComPort");
      Call("DebugStub_ReadByteFromComPort");
    }

    // Sets a breakpoint
    // Serial Params:
    //   1: x32 - EIP to break on, or 0 to disable breakpoint.
    protected void BreakOnAddress() {
      Label = "DebugStub_BreakOnAddress";
      PushAll();

      // BP Address
      ReadComPortX32toStack();
      ECX.Pop();

      // BP ID Number
      // BP ID Number is sent after BP Address, becuase
      // reading BP address uses AL (EAX).
      EAX = 0;
      Call("ReadALFromComPort");

      // Calculate location in table
      // Mov [EBX + EAX * 4], ECX would be better, but our asm doesn't handle this yet
      EBX = AddressOf("DebugBPs");
      EAX = EAX << 2;
      EBX.Add(EAX);

      Memory[EBX, 32] = ECX;

      PopAll();
      Return();
    }

    // sends a stack value
    // Serial Params:
    //  1: x32 - offset relative to EBP
    //  2: x32 - size of data to send
    protected void SendMethodContext() {
      Label = "DebugStub_SendMethodContext";
      PushAll();

      AL = (int)DsMsgType.MethodContext;
      Call<DebugStub.WriteALToComPort>();

      ReadComPortX32toStack(); // offset relative to ebp
      ReadComPortX32toStack(); // size of data to send
      ECX.Pop();
      EAX.Pop();

      // now ECX contains size of data (count)
      // EAX contains relative to EBP
      Label = "DebugStub_SendMethodContext2";
      ESI = Memory["DebugEBP", 32];
      ESI.Add(EAX);

      Label = "DebugStub_SendMethodContext_SendByte";
      new Compare { DestinationReg = Registers.ECX, SourceValue = 0 };
      JumpIf(Flags.Equal, "DebugStub_SendMethodContext_After_SendByte");
      Call("WriteByteToComPort");
      new Dec { DestinationReg = Registers.ECX };
      Jump("DebugStub_SendMethodContext_SendByte");

      Label = "DebugStub_SendMethodContext_After_SendByte";

      PopAll();
      Return();
    }

    // sends a stack value
    // Serial Params:
    //  1: x32 - offset relative to EBP
    //  2: x32 - size of data to send
    protected void SendMemory() {
      Label = "DebugStub_SendMemory";
      PushAll();

      ReadComPortX32toStack();
      //EAX.Pop();
      //ESI = EBP;
      //ESI.Add(EAX);
      //ESI.Push();
      // todo: adjust ESI to the actual offset
      Label = "DebugStub_SendMemory_1";
      AL = (int)DsMsgType.MemoryData;
      Call<DebugStub.WriteALToComPort>();

      //EAX.Pop();
      //EAX.Push();

      ReadComPortX32toStack();
      Label = "DebugStub_SendMemory_2";
      ECX.Pop();
      ESI.Pop();

      // now ECX contains size of data (count)
      // ESI contains address

      Label = "DebugStub_SendMemory_3";
      Label = "DebugStub_SendMemory_SendByte";
      new Compare { DestinationReg = Registers.ECX, SourceValue = 0 };
      JumpIf(Flags.Equal, "DebugStub_SendMemory_After_SendByte");
      Call("WriteByteToComPort");
      new Dec { DestinationReg = Registers.ECX };
      Jump("DebugStub_SendMemory_SendByte");

      Label = "DebugStub_SendMemory_After_SendByte";

      PopAll();
      Return();
    }

    protected void WriteBytesToComPort(int xCount) {
      for (int i = 1; i <= xCount; i++) {
        Call("WriteByteToComPort");
      }
    }

    // Modifies: EAX, ESI
    protected void SendTrace() {
      Label = "DebugStub_SendTrace";

      Memory["DebugStatus", 32].Compare(DebugStub.Status.Run);
      JumpIf(Flags.Equal, "DebugStub_SendTrace_Normal");
      AL = (int)DsMsgType.BreakPoint;
      Jump("DebugStub_SendTraceType");

      Label = "DebugStub_SendTrace_Normal";
      AL = (int)DsMsgType.TracePoint;

      Label = "DebugStub_SendTraceType";
      Call<DebugStub.WriteALToComPort>();

      // Send Calling EIP.
      ESI = AddressOf("DebugEIP");
      WriteBytesToComPort(4);

      Return();
    }

    // Input: Stack
    // Output: None
    // Modifies: EAX, ECX, EDX, ESI
    protected void SendText() {
      Label = "DebugStub_SendText";

      // Write the type
      AL = (int)DsMsgType.Message;
      Call<DebugStub.WriteALToComPort>();

      // Write Length
      ESI = EBP;
      new Add { DestinationReg = Registers.ESI, SourceValue = 12 };
      ECX = Memory[ESI];
      Call("WriteByteToComPort");
      Call("WriteByteToComPort");

      // Address of string
      ESI = Memory[EBP + 8];
      Label = "DebugStub_SendTextWriteChar";
      ECX.Compare(0);
      JumpIf(Flags.Equal, "DebugStub_SendTextExit");
      Call("WriteByteToComPort");
      new Dec { DestinationReg = Registers.ECX };
      // We are storing as 16 bits, but for now I will transmit 8 bits
      // So we inc again to skip the 0
      new Inc { DestinationReg = Registers.ESI };
      Jump("DebugStub_SendTextWriteChar");

      Label = "DebugStub_SendTextExit";
      Return();
    }

    // Input: Stack
    // Output: None
    // Modifies: EAX, ECX, EDX, ESI
    protected void SendPtr() {
      Label = "DebugStub_SendPtr";

      // Write the type
      AL = (int)DsMsgType.Pointer;
      Call<DebugStub.WriteALToComPort>();

      // pointer value
      ESI = Memory[EBP + 8];
      Call("WriteByteToComPort");
      Call("WriteByteToComPort");
      Call("WriteByteToComPort");
      Call("WriteByteToComPort");

      Return();
    }

    // Input: ESI
    // Output: None
    // Modifies: EAX, EDX
    //
    // Sends byte at [ESI] to com port and does esi + 1
    protected void WriteByteToComPort() {
      // This sucks to use the stack, but x86 can only read and write ports from AL and
      // we need to read a port before we can write out the value to another port.
      // The overhead is a lot, but compared to the speed of the serial and the fact
      // that we wait on the serial port anyways, its a wash.
      //
      // This could be changed to use interrupts, but that then complicates
      // the code and causes interaction with other code. DebugStub should be
      // as isolated as possible from any other code.
      Label = "WriteByteToComPort";
      // Sucks again to use DX just for this, but x86 only supports
      // 8 bit address for literals on ports
      DX = mComStatusAddr;

      // Wait for serial port to be ready
      // Bit 5 (0x20) test for Transmit Holding Register to be empty.
      Label = "WriteByteToComPort_Wait";
      AL = Port[DX];
      AL.Test(0x20);
      JumpIf(Flags.Zero, "WriteByteToComPort_Wait");

      // Set address of port
      DX = mComAddr;
      // Get byte to send
      AL = Memory[ESI];
      // Send the byte
      Port[DX] = AL;

      new Inc { DestinationReg = Registers.ESI }; // TODO: ESI++ instead
      Return();
    }

    //Modifies: AL, DX
    protected void ReadALFromComPort() {
      Label = "ReadALFromComPort";
      DX = mComStatusAddr;

      // Wait for port to be ready
      Label = "ReadALFromComPort_Wait";
      AL = Port[DX];
      AL.Test(0x01);
      JumpIf(Flags.Zero, "ReadALFromComPort_Wait");

      Label = "ReadALFromComPortNoCheck";
      // Set address of port
      DX = mComAddr;
      // Read byte
      AL = Port[DX];
      Return();
    }

    // This does not run during Cosmos execution
    // This is only used by the compiler to force emission of each of our routines.
    // Each routine must be listed here, else it wont be emitted.
    protected void Emit() {
      SendTrace();
      SendText();
      SendPtr();
      WriteByteToComPort();
      ReadALFromComPort();

      SendMethodContext();
      SendMemory();

      BreakOnAddress();
    }

    public void Main(UInt16 aComAddr) {
      mComAddr = aComAddr;
      mComStatusAddr = (UInt16)(aComAddr + 5);
      EmitDataSection();
      Emit();
   }

  }

}