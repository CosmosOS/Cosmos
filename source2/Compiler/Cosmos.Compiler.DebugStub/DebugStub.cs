using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Debug;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Compiler.DebugStub {
  public class DebugStub : CodeGroup {
    protected const uint VidBase = 0xB8000;
    static public int mComNo = 0;
    static protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
    static public UInt16 mComAddr;
    static public UInt16 mComStatusAddr;

    // Caller's EBP
    static public DataMember32 CallerEBP;
    // Caller's EIP
    static public DataMember32 CallerEIP;
    // Caller's ESP
    static public DataMember32 CallerESP;

    static public class Tracing {
      public const byte Off = 0;
      public const byte On = 1;
    }

    // Current status of OS Debug Stub
    static public class Status {
      public const byte Run = 0;
      public const byte Break = 1;
    }

    static public class StepTrigger {
      public const byte None = 0;
      public const byte Into = 1;
      public const byte Over = 2;
      public const byte Out = 3;
    }

    public DebugStub(int aComNo) {
      mComNo = aComNo;
      mComAddr = mComPortAddresses[mComNo - 1];
      mComStatusAddr = (UInt16)(mComAddr + 5);

      // Old method, need to convert to fields
      mAsm.DataMembers.AddRange(new DataMember[]{
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

    public abstract class Inlines : CodeBlock {
      // INLINE
      // Modifies: Stack, EDI, AL
      // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
      // repeated use of an inline will fail with conflicting labels.
      // TODO: Allow methods to emit a start label and return automatically
      // and mark inlines so this does not happen.
      protected void ReadComPortX32toStack(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          // Make room on the stack for the address
          Push(0);
          // ReadByteFromComPort writes to EDI, then increments
          EDI = ESP;

          // Read address to stack via EDI
          ReadBytesFromComPort(4);
        }
      }

      protected void ReadBytesFromComPort(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          Call<ReadByteFromComPort>();
        }
      }

      protected void WriteBytesToComPort(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          Call<WriteByteToComPort>();
        }
      }
    }

    public class BreakOnAddress : Inlines {
      // Sets a breakpoint
      // Serial Params:
      //   1: x32 - EIP to break on, or 0 to disable breakpoint.
      public override void Assemble() {
        PushAll();

        // BP Address
        ReadComPortX32toStack(1);
        ECX.Pop();

        // BP ID Number
        // BP ID Number is sent after BP Address, becuase
        // reading BP address uses AL (EAX).
        EAX = 0;
        Call<ReadALFromComPort>();

        // Calculate location in table
        // Mov [EBX + EAX * 4], ECX would be better, but our asm doesn't handle this yet
        EBX = AddressOf("DebugBPs");
        EAX = EAX << 2;
        EBX.Add(EAX);

        Memory[EBX, 32] = ECX;

        PopAll();
      }
    }

    public class SendMethodContext : Inlines {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      public override void Assemble() {
        PushAll();

        AL = (int)DsMsgType.MethodContext;
        Call<DebugStub.WriteALToComPort>();

        // offset relative to ebp
        // size of data to send
        ReadComPortX32toStack(2);
        ECX.Pop();
        EAX.Pop();

        // now ECX contains size of data (count)
        // EAX contains relative to EBP
        ESI = Memory[DebugStub.CallerEBP.Name, 32];
        ESI.Add(EAX);

        Label = ".SendByte";
        ECX.Compare(0);
        JumpIf(Flags.Equal, ".AfterSendByte");
        Call<WriteByteToComPort>();
        ECX--;
        Jump(".SendByte");
        Label = ".AfterSendByte";

        PopAll();
      }
    }

    public class SendMemory : Inlines {
      // sends a stack value
      // Serial Params:
      //  1: x32 - offset relative to EBP
      //  2: x32 - size of data to send
      public override void Assemble() {
        PushAll();

        ReadComPortX32toStack(1);
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
        new Dec { DestinationReg = Registers.ECX };
        Jump("DebugStub_SendMemory_SendByte");

        Label = "DebugStub_SendMemory_After_SendByte";

        PopAll();
      }
    }

    public class SendTrace : Inlines {
      // Modifies: EAX, ESI
      public override void Assemble() {
        Memory["DebugStatus", 32].Compare(DebugStub.Status.Run);
        JumpIf(Flags.Equal, ".Normal");
        AL = (int)DsMsgType.BreakPoint;
        Jump(".Type");

        Label = ".Normal";
        AL = (int)DsMsgType.TracePoint;

        Label = ".Type";
        Call<DebugStub.WriteALToComPort>();

        // Send Calling EIP.
        ESI = AddressOf(DebugStub.CallerEIP);
        WriteBytesToComPort(4);
      }
    }

    public class SendText : Inlines {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)DsMsgType.Message;
        Call<DebugStub.WriteALToComPort>();

        // Write Length
        ESI = EBP;
        new Add { DestinationReg = Registers.ESI, SourceValue = 12 };
        ECX = Memory[ESI];
        WriteBytesToComPort(2);

        // Address of string
        ESI = Memory[EBP + 8];
        Label = "DebugStub_SendTextWriteChar";
        ECX.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_SendTextExit");
        Call<WriteByteToComPort>();
        new Dec { DestinationReg = Registers.ECX };
        // We are storing as 16 bits, but for now I will transmit 8 bits
        // So we inc again to skip the 0
        new Inc { DestinationReg = Registers.ESI };
        Jump("DebugStub_SendTextWriteChar");

        Label = "DebugStub_SendTextExit";
      }
    }

    public class SendPtr : Inlines {
      // Input: Stack
      // Output: None
      // Modifies: EAX, ECX, EDX, ESI
      public override void Assemble() {
        // Write the type
        AL = (int)DsMsgType.Pointer;
        Call<DebugStub.WriteALToComPort>();

        // pointer value
        ESI = Memory[EBP + 8];
        WriteBytesToComPort(4);
      }
    }

    public class WriteByteToComPort : Inlines {
      // Input: ESI
      // Output: None
      // Modifies: EAX, EDX
      //
      // Sends byte at [ESI] to com port and does esi + 1
      //
      // This sucks to use the stack, but x86 can only read and write ports from AL and
      // we need to read a port before we can write out the value to another port.
      // The overhead is a lot, but compared to the speed of the serial and the fact
      // that we wait on the serial port anyways, its a wash.
      //
      // This could be changed to use interrupts, but that then complicates
      // the code and causes interaction with other code. DebugStub should be
      // as isolated as possible from any other code.
      public override void Assemble() {
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
      }
    }

    public class ReadALFromComPort : Inlines {
      // Modifies: AL, DX
      public override void Assemble() {
        DX = mComStatusAddr;

        // Wait for port to be ready
        Label = ".Wait";
        AL = Port[DX];
        AL.Test(0x01);
        JumpIf(Flags.Zero, ".Wait");

        // Set address of port
        DX = mComAddr;
        // Read byte
        AL = Port[DX];
      }
    }

    // Called before Kernel runs. Inits debug stub, etc
    public class Init : CodeBlock {
      public override void Assemble() {
        Call<Cls>();
        Call<DisplayWaitMsg>();
        Call<InitSerial>();
        Call<WaitForDbgHandshake>();
        Call<Cls>();
      }
    }

    public class WaitForDbgHandshake : Inlines {
      public override void Assemble() {
        // "Clear" the UART out
        AL = 0;
        Call<WriteALToComPort>();

        // QEMU (and possibly others) send some garbage across the serial line first.
        // Actually they send the garbage inbound, but garbage could be inbound as well so we 
        // keep this.
        // To work around this we send a signature. DC then discards everything before the signature.
        // QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
        // feature so we kept it.
        Push(Consts.SerialSignature);
        ESI = ESP;
        WriteBytesToComPort(4);
        // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
        EAX.Pop();

        // We could use the signature as the start signal, but I prefer
        // to keep the logic separate, especially in DC.
        AL = (int)DsMsgType.Started; // Send the actual started signal
        Call<WriteALToComPort>();

        Call<WaitForSignature>();
        Call<ProcessCommandBatch>();
      }
    }

    public class InitSerial : CodeBlock {
      // SERIAL DOCS
      //
      // All information relating to our serial usage should be documented in 
      // this comment.
      //
      // We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)
      // MUST be:
      //  - As simple as possible
      //  - Interact as minimal as possible wtih normal Cosmos code because
      //    the debugstub must *always* work even if the normal code is fubarred
      //
      // The serial port that is used for DS should be "hidden" from Cosmos main
      // so that Cosmos main pretends it does not exist.
      //
      // IRQs would create a clash/mix of code.
      // This does make the serial code in DebugStub inefficient, but its well worth
      // the benefits received by following these rules.
      //
      // Baud rate is set to 115200. Likely our code could not exceed this rate
      // anyways the way it is written and there are compatibility issues on some
      // hardware above this rate.
      //
      // We assume a minimum level of a 16550A, which should be no problem on any 
      // common hardware today. VMWare emulates the 16550A
      //
      // We do not handle flow control for outbound data (DS --> DC).
      // The DebugConnector (DC, the code in the Visual Studio side) however is threaded
      // and easily should be able to receive data faster than we can send it.
      // Most things are transactional with data being sent only when asked for, but 
      // with tracing we do send a data directly.
      //
      // Currently there is no inbound flow control either (DC --> DS)
      // For now we assume all commands in bound are 16 bytes or less to ensure
      // that they fit in the FIFO. Commands in DS must wait for a command ID ACK
      // before sending another command.
      // See notes in ProcessCommand.
      public override void Assemble() {
        // http://www.nondot.org/sabre/os/files/Communication/ser_port.txt

        // Disable interrupts
        DX = (UInt16)(mComAddr + 1);
        AL = 0;
        Port[DX] = AL;

        // Enable DLAB (set baud rate divisor)
        DX = (UInt16)(mComAddr + 3);
        AL = 0x80;
        Port[DX] = AL;

        // 0x01 - 0x00 - 115200
        // 0x02 - 0x00 - 57600
        // 0x03 - 0x00 - 38400
        //
        // Set divisor (lo byte)
        DX = mComAddr;
        AL = 0x01;
        Port[DX] = AL;
        // hi byte
        DX = (UInt16)(mComAddr + 1);
        AL = 0x00;
        Port[DX] = AL;

        // 8N1
        DX = (UInt16)(mComAddr + 3);
        AL = 0x03;
        Port[DX] = AL;

        // Enable FIFO, clear them
        // Set 14-byte threshold for IRQ.
        // We dont use IRQ, but you cant set it to 0
        // either. IRQ is enabled/diabled separately
        DX = (UInt16)(mComAddr + 2);
        AL = 0xC7;
        Port[DX] = AL;

        // 0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it
        // 0x02 RTS
        // 0x01 DTR
        // Send 0x03 if no AFE
        DX = (UInt16)(mComAddr + 4);
        AL = 0x03;
        Port[DX] = AL;
      }
    }

    public class DisplayWaitMsg : CodeBlock {
      // http://wiki.osdev.org/Text_UI
      // Later can cycle for x changes of second register:
      // http://wiki.osdev.org/Time_And_Date
      public override void Assemble() {
        ESI = AddressOf("DebugWaitMsg");
        // 10 lines down, 20 cols in
        EDI = VidBase + (10 * 80 + 20) * 2;

        // Read and copy string till 0 terminator
        Label = ".ReadChar";
        AL = Memory[ESI, 8];
        AL.Compare(0);
        JumpIf(Flags.Equal, ".AfterMsg");
        ESI++;
        Memory[EDI, 8] = AL;
        EDI++;
        EDI++;
        Jump(".ReadChar");
        //TODO: Local labels in X#
        Label = ".AfterMsg";
      }
    }

    public class WriteALToComPort : CodeBlock {
      // Input: AL
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call<WriteByteToComPort>();
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class WriteAXToComPort : Inlines {
      // Input: AX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        WriteBytesToComPort(2);
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class WriteEAXToComPort : Inlines {
      // Input: EAX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        WriteBytesToComPort(4);
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class ProcessCommandBatch : CodeBlock {
      public override void Assemble() {
        Call<ProcessCommand>();

        // See if batch is complete
        AL.Compare(DsCommand.BatchEnd);
        JumpIf(Flags.Equal, "DebugStub_ProcessCommandBatch_Exit");

        // Loop and wait
        Jump("DebugStub_ProcessCommandBatch");

        Label = "DebugStub_ProcessCommandBatch_Exit";
      }
    }

    public class WaitForSignature : CodeBlock {
      public override void Assemble() {
        EBX = 0;

        Label = "DebugStub_WaitForSignature_Read";
        Call<ReadALFromComPort>();
        BL = AL;
        EBX.RotateRight(8);
        EBX.Compare(Consts.SerialSignature);
        JumpIf(Flags.NotEqual, "DebugStub_WaitForSignature_Read");

        //TODO: Always emit and exit label and then make a Exit method which can
        // automatically use it. I think a label might already exist.
        Label = "DebugStub_WaitForSignature_Exit";
      }
    }

    public class Cls : CodeBlock {
      public override void Assemble() {
        ESI = VidBase;
        // TODO: X# upgrade this
        Label = "DebugStub_Cls_More";
        //TODO: Fix to direct memory write after we fix the X# bug with Memory[ESI, 8] = 0x0A;
        AL = 0x00;
        Memory[ESI, 8] = AL; // Text
        ESI++;

        AL = 0x0A;
        Memory[ESI, 8] = AL; // Colour
        ESI++;

        ESI.Compare(VidBase + 25 * 80 * 2);
        JumpIf(Flags.LessThan, "DebugStub_Cls_More");
      }
    }

    public class SendRegisters : Inlines {
      public override void Assemble() {
        AL = (int)DsMsgType.Registers; // Send the actual started signal
        Call<WriteALToComPort>();
        
        ESI = Memory["DebugPushAllPtr", 32];
        WriteBytesToComPort(32);
        ESI = AddressOf(CallerESP);
        WriteBytesToComPort(4);
        ESI = AddressOf(CallerEIP);
        WriteBytesToComPort(4);
      }
    }

    public class SendFrame : Inlines {
      public override void Assemble() {
        AL = (int)DsMsgType.Frame;
        Call<WriteALToComPort>();

        int xCount = 8 * 4;
        EAX = (uint)xCount;
        Call<WriteAXToComPort>();

        ESI = Memory[CallerEBP.Name, 32];
        ESI.Add(8); // Dont transmit EIP or old EBP
        WriteBytesToComPort(xCount);
      }
    }

    public class SendStack : CodeBlock {
      public override void Assemble() {
        AL = (int)DsMsgType.Stack;
        Call<WriteALToComPort>();

        // Send size of bytes
        ESI = Memory[CallerESP.Name, 32];
        EAX = Memory[CallerEBP.Name, 32];
        EAX.Sub(ESI);
        Call<WriteAXToComPort>();

        // Send actual bytes
        //
        // Need to reload ESI, WriteAXToCompPort modifies it
        ESI = Memory[CallerESP.Name, 32];
        Label = ".SendByte";
        ESI.Compare(Memory[CallerEBP.Name, 32]);
        JumpIf(Flags.Equal, ".Exit");
        Call<WriteByteToComPort>();
        Jump(".SendByte");

        Label = ".Exit";
      }
    }

    public class ProcessCommand : CodeBlock {
      // Modifies: AL, DX (ReadALFromComPort)
      // Returns: AL
      public override void Assemble() {
        Call<ReadALFromComPort>();
        // Some callers expect AL to be returned, so we preserve it
        // in case any commands modify AL.
        //TODO: But in ASM wont let us push AL, so we push EAX for now
        EAX.Push();

        // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
        AL.Compare(DsCommand.Noop);
        JumpIf(Flags.Equal, "DebugStub_ProcessCmd_Exit");

        // Read Command ID
        Call<ReadALFromComPort>();
        Memory["DebugStub_CommandID", 32] = EAX;

        // Get AL back so we can compare it, but also put it back for later
        EAX.Pop();
        EAX.Push();

        #region handle commands
        AL.Compare(DsCommand.TraceOff);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOff_After");
        Memory["DebugTraceMode", 32] = Tracing.Off;
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_TraceOff_After";

        AL.Compare(DsCommand.TraceOn);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOn_After");
        Memory["DebugTraceMode", 32] = Tracing.On;
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_TraceOn_After";

        AL.Compare(DsCommand.Break);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_Break_After");
        Call<Break>();
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_Break_After";

        AL.Compare(DsCommand.BreakOnAddress);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_BreakOnAddress_After");
        Call<BreakOnAddress>();
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_BreakOnAddress_After";

        AL.Compare(DsCommand.SendMethodContext);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMethodContext_After");
        Call<SendMethodContext>();
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_SendMethodContext_After";

        AL.Compare(DsCommand.SendMemory);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMemory_After");
        Call<SendMemory>();
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_SendMemory_After";

        AL.Compare(DsCommand.SendRegisters);
                JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendRegisters_After");
                Call<SendRegisters>();
                Jump("DebugStub_ProcessCmd_ACK");
                Label = "DebugStub_ProcessCmd_SendRegisters_After";

        AL.Compare(DsCommand.SendFrame);
                JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendFrame_After");
                Call<SendFrame>();
                Jump("DebugStub_ProcessCmd_ACK");
                Label = "DebugStub_ProcessCmd_SendFrame_After";

        AL.Compare(DsCommand.SendStack);
                JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendStack_After");
                Call<SendStack>();
                Jump("DebugStub_ProcessCmd_ACK");
                Label = "DebugStub_ProcessCmd_SendStack_After";

        Label = "DebugStub_ProcessCmd_ACK";
        // We acknowledge receipt of the command, not processing of it.
        // We have to do this because sometimes callers do more processing
        // We ACK even ones we dont process here, but do not ACK Noop.
        // The buffers should be ok becuase more wont be sent till after our NACK
        // is received.
        // Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7. 
        // UART buffer is 16.
        // We may need to revisit this in the future to ack not commands, but data chunks
        // and move them to a buffer.
        // The buffer problem exists only to inbound data, not outbound data (relative to DebugStub)
        AL = DsMsgType.CmdCompleted;
        Call<WriteALToComPort>();
        EAX = Memory["DebugStub_CommandID", 32];
        #endregion

        Call<WriteALToComPort>();
        Label = "DebugStub_ProcessCmd_After";

        Label = "DebugStub_ProcessCmd_Exit";
        // Restore AL for callers who check the command and do
        // further processing, or for commands not handled by this routine.
        EAX.Pop();
      }
    }

    public class Executing : CodeBlock {
      // This is the secondary stub routine. After the primary (main) has decided we should do some debug
      // activities, this one is called.
      //
      // Modifies: EAX, EDI, ECX
      public override void Assemble() {
        // Look for a possible matching BP
        EAX = Memory[CallerEIP.Name, 32];
        EDI = AddressOf("DebugBPs");
        ECX = 256;
        new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
        JumpIf(Flags.NotEqual, "DebugStub_Executing_AfterBreakOnAddress");
        Call<Break>();
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_Executing_AfterBreakOnAddress";

        // See if we are stepping
        //
        // F11
        Memory["DebugBreakOnNextTrace", 32].Compare(StepTrigger.Into);
        //Old, can delete this line: CallIf(Flags.Equal, "DebugStub_Break");
        //TODO: I think we can use a using statement to create this type of block
        // and emit asm
        // using (var xBlock = new AsmBlock()) {
        //   JumpIf(something, xBlock.End/Begin);
        //   also can do xBlock.Break();
        // }
        //TODO: If statements can probably be done with anonymous delegates...
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepIntoAfter");
        Call<Break>();
        //TODO: Allow creating labels but issuing them later, then we can
        // call them with early binding
        //TODO: End - can be exit label for each method, allowing Jump(Begin/End) etc... Also make a label type and allwo Jump overload to the label itself. Or better yet, End.Jump()
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepIntoAfter";
        //
        // F10
        Memory["DebugBreakOnNextTrace", 32].Compare(StepTrigger.Over);
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepOverAfter");
        Label = "Debug__StepOver__";
        EAX = Memory[CallerEBP.Name, 32];
        EAX.Compare(Memory["DebugBreakEBP", 32]);
        // If EBP and start EBP arent equal, dont break
        // Dont use Equal because we aslo need to stop above if the user starts
        // the step at the end of a method and next item is after a return
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepOverAfter";
        //
        // Shift-F11
        Memory["DebugBreakOnNextTrace", 32].Compare(StepTrigger.Out);
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepOutAfter");

        EAX = Memory[CallerEBP.Name, 32]; // TODO: X# Allow memory object instead of string, maybe the Datamember object itself. ie EAX = DebugEBP, and below inside Compare
        EAX.Compare(Memory["DebugBreakEBP", 32]); // TODO: X# JumpIf(EAX == Memory[...... or better yet if(EAX==Memory..., new Delegate { Jump.... Jump should be handled specially so we dont jump around jumps... TODO: Also allow Compare(EAX, 0), in fact force this new syntax
        JumpIf(Flags.Equal, "DebugStub_Executing_Normal");
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepOutAfter";

        Label = "DebugStub_Executing_Normal";
        
        // If tracing is on, send a trace message
        // Tracing isnt really used any more, was used
        // by the old stand alone debugger. Might be upgraded
        // and resused in the future.
        Memory["DebugTraceMode", 32].Compare(Tracing.On);
        CallIf(Flags.Equal, "DebugStub_SendTrace");

        // Is there a new incoming command? We dont want to wait for one
        // if there isn't one already here. This is a passing check.
        Label = "DebugStub_CheckForCmd"; //TODO: ".CheckForCmd" and make it local to our class
        DX = (ushort)(mComAddr + 5u);
        AL = Port[DX];
        AL.Test(0x01);
        // If no command waiting, break from loop
        JumpIf(Flags.Zero, "DebugStub_CheckForCmd_Break");
        Call<ProcessCommand>();
        // See if there are more commands waiting
        Jump("DebugStub_CheckForCmd");
        Label = "DebugStub_CheckForCmd_Break";
      }
    }

    public class ReadByteFromComPort : CodeBlock {
      // Input: EDI
      // Output: [EDI]
      // Modified: AL, DX, EDI (+1)
      //
      // Reads a byte into [EDI] and does EDI + 1
      // http://wiki.osdev.org/Serial_ports
      public override void Assemble() {
        Call<ReadALFromComPort>();
        Memory[EDI, 8] = AL;
        EDI++;
      }
    }

    public class Break : CodeBlock {
      // Should only be called internally by DebugStub. Has a lot of preconditions
      // Externals should use BreakOnNextTrace instead
      public override void Assemble() {
        // Reset request in case we are currently responding to one or we hit a fixed breakpoint
        // before our request could be serviced (if one existed)
        Memory["DebugBreakOnNextTrace", 32] = StepTrigger.None;
        Memory["DebugBreakEBP", 32] = 0;
        // Set break status
        Memory["DebugStatus", 32] = Status.Break;
        Call<SendTrace>();

        // Wait for a command
        Label = "DebugStub_WaitCmd";
        // Check for common commands
        Call<ProcessCommand>();

        // Now check for commands that are only valid in break state
        // or commands that require additional handling while in break
        // state.

        AL.Compare(DsCommand.Continue);
        JumpIf(Flags.Equal, "DebugStub_Break_Exit");

        AL.Compare(DsCommand.StepInto);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepInto_After");
        Memory["DebugBreakOnNextTrace", 32] = StepTrigger.Into;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepInto_After";

        AL.Compare(DsCommand.StepOver);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepOver_After");
        Memory["DebugBreakOnNextTrace", 32] = StepTrigger.Over;
        // TODO: Change this so ,32 is not necessary, can be implied by 32 bit register - ie Memory["DebugBreakEBP", 32] = EBP;
        EAX = Memory[CallerEBP.Name, 32];
        Memory["DebugBreakEBP", 32] = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOver_After";

        AL.Compare(DsCommand.StepOut);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepOut_After");
        Memory["DebugBreakOnNextTrace", 32] = StepTrigger.Out;
        EAX = Memory[CallerEBP.Name, 32];
        Memory["DebugBreakEBP", 32] = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOut_After";

        // Loop around and wait for another command
        Jump("DebugStub_WaitCmd");

        Label = "DebugStub_Break_Exit";
        Memory["DebugStatus", 32] = Status.Run;
      }
    }

    public class TracerEntry : CodeBlock {
      // 0 = Not in, 1 = already running
      public DataMember32 IsRunning;

      public override void Assemble() {
        // Main entry point for the DebugStub which is executed at the 
        // beginning of all IL ops.

        // EBP is restored by PopAll, but SendFrame uses it. Could
        // get it from the PushAll data, but this is easier.
        Memory[CallerEBP.Name, 32] = EBP;

        // Could also get ESP from PushAll but this is easier
        // Another reason to do it here is that soem day we may need to use 
        // the stack before PushAll.
        //
        // We cant modify any registers since we havent done PushAll yet
        // Maybe we could do a sub(4) on memory direct.. 
        // But for now we remove from ESP which the call to us produces,
        // store ESP, then restore ESP so we don't cause stack corruption.
        ESP.Add(12); // 12 bytes for EFLAGS, CS, EIP
        Memory[CallerESP.Name, 32] = ESP;
        ESP.Sub(12);

        // If debug stub is in break, and then an IRQ happens, the IRQ
        // can call DebugStub again. This causes two DebugStubs to 
        // run which causes havoc. So we only allow one to run.
        // We arent multi threaded yet, so this works fine.
        // IRQ's are disabled between Compare and JumpIf so an IRQ cant
        // happen in between them which could also cause double entry.
        DisableInterrupts();
        Memory["DebugSuspendLevel", 32].Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Running");
        // DebugStub is already running, so exit.
        // But we need to see if IRQs are disabled.
        // If IRQ disabled, we dont reenable them after our disable
        // in this routine.
        Memory["InterruptsEnabledFlag", 32].Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Return");
        EnableInterrupts();
        Jump("DebugStub_Return");

        Label = "DebugStub_Running";
        Memory[IsRunning.Name, 32].Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Start");
        // If we made it this far we exit because DebugStub is already running.
        // We need to see if IRQs were originally enabled or disabled and
        // re-enable them if they were enabled on entry.
        Jump("DebugStub_CheckIntAndReturn");

        // All clear, mark that we are entering the debug stub
        Label = "DebugStub_Start";
        Memory[IsRunning.Name, 32] = 1;

        // DS is now marked not to re-enter, so re-enable interrupts if
        // they were enabled on entry
        Memory["InterruptsEnabledFlag", 32].Compare(0);
        JumpIf(Flags.Equal, "DebugStub_NoSTI");
        EnableInterrupts();

        // Call secondary debug stub
        Label = "DebugStub_NoSTI";
        PushAll();
        Memory["DebugPushAllPtr", 32] = ESP;
        // We just pushed all registers to the stack so we can use them
        // So we get the stack pointer and add 32. This skips over the
        // registers we just pushed.
        EBP = ESP;
        EBP.Add(32); // We dont need to restore this becuase it was pushed as part of PushAll32

        // Get actual EIP of caller.
        EAX = Memory[EBP];
        // EIP is pointer to op after our call. We subtract 1 for the opcode size of Int3
        // Note - when we used call it was 5 (the size of our call + address)
        // so we get the EIP as IL2CPU records it. Its also useful for when we will
        // be changing ops that call this stub.
        EAX.Sub(1); //TODO: EAX-- and EAX = EAX - 1;
        // Store it for later use.
        Memory[CallerEIP.Name, 32] = EAX;

        // Call secondary stub
        Call<Executing>();

        // Restore registers
        PopAll();

        // Setting the DebugRuning flag is atomic, but in the future
        // we might have other code as we do in the entry to check.
        // So just to be safe, we disable interrupts while we do this.
        DisableInterrupts();
        // Complete, mark that DebugStub is complete
        Memory[IsRunning.Name, 32] = 0;

        Label = "DebugStub_CheckIntAndReturn";
        // Re-enable interrupts if needed. This happens on normal exit, or call from above
        // when there would have been a re-entry to DS.
        Memory["InterruptsEnabledFlag", 32].Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Return");
        EnableInterrupts();

        Label = "DebugStub_Return";
        ReturnFromInterrupt();
      }
    }

  }

  public class DebugPoint : CodeGroup {
    public class DebugSuspend : CodeBlock {
      public override void Assemble() {
        Memory["DebugSuspendLevel", 32]++;
      }
    }

    public class DebugResume : CodeBlock {
      public override void Assemble() {
        Memory["DebugSuspendLevel", 32]--;
      }
    }
  }

}
