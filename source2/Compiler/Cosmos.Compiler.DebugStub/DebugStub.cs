using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Debug;
using Cosmos.Compiler.XSharp;

namespace Cosmos.Compiler.DebugStub {
  public class DebugStub : CodeGroup {
    protected const uint VidBase = 0xB8000;

    static public int mComNo = 0;
    protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
    static public UInt16 mComAddr;

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

    public class WaitForDbgHandshake : CodeBlock {
      public override void Assemble() {
        // "Clear" the UART out
        AL = 0;
        Call<DebugStub.WriteALToComPort>();

        // QEMU (and possibly others) send some garbage across the serial line first.
        // Actually they send the garbage inbound, but garbage could be inbound as well so we 
        // keep this.
        // To work around this we send a signature. DC then discards everything before the signature.
        // QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
        // feature so we kept it.
        Push(Consts.SerialSignature);
        ESI = ESP;
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
        EAX.Pop();

        // We could use the signature as the start signal, but I prefer
        // to keep the logic separate, especially in DC.
        AL = (int)DsMsgType.Started; // Send the actual started signal
        Call<DebugStub.WriteALToComPort>();

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
        EDI = DebugStub.VidBase + (10 * 80 + 20) * 2;

        // Read and copy string till 0 terminator
        Label = "DebugStub_Init_ReadChar";
        AL = Memory[ESI, 8];
        AL.Compare(0);
        JumpIf(Flags.Equal, "DebugStub_Init_AfterMsg");
        ESI++;
        Memory[EDI, 8] = AL;
        EDI++;
        EDI++;
        Jump("DebugStub_Init_ReadChar");
        //TODO: Local labels in X#
        Label = "DebugStub_Init_AfterMsg";
      }
    }

    public class WriteALToComPort : CodeBlock {
      // Input: AL
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call("WriteByteToComPort");
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class WriteAXToComPort : CodeBlock {
      // Input: AX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        // Is a local var, cant use Return(4). X# issues the return.
        // This also allow the function to preserve EAX.
        EAX.Pop();
      }
    }

    public class WriteEAXToComPort : CodeBlock {
      // Input: EAX
      // Output: None
      // Modifies: EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
        Call("WriteByteToComPort");
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
        Call("ReadALFromComPort");
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
        ESI = DebugStub.VidBase;
        // TODO: X# upgrade this
        Label = "DebugStub_Cls_More";
        //TODO: Fix to direct memory write after we fix the X# bug with Memory[ESI, 8] = 0x0A;
        AL = 0x00;
        Memory[ESI, 8] = AL; // Text
        ESI++;

        AL = 0x0A;
        Memory[ESI, 8] = AL; // Colour
        ESI++;

        ESI.Compare(DebugStub.VidBase + 25 * 80 * 2);
        JumpIf(Flags.LessThan, "DebugStub_Cls_More");
      }
    }

    public class SendRegisters : CodeBlock {
      public override void Assemble() {
        AL = (int)DsMsgType.Registers; // Send the actual started signal
        Call<DebugStub.WriteALToComPort>();
        
        ESI = Memory["DebugPushAllPtr", 32];
        for (int i = 1; i <= 32; i++) {
          Call("WriteByteToComPort");
        }
        ESI = AddressOf("DebugESP");
        for (int i = 1; i <= 4; i++) {
          Call("WriteByteToComPort");
        }
        ESI = AddressOf("DebugEIP");
        for (int i = 1; i <= 4; i++) {
          Call("WriteByteToComPort");
        }
      }
    }

    public class SendFrame : CodeBlock {
      public override void Assemble() {
        AL = (int)DsMsgType.Frame;
        Call<DebugStub.WriteALToComPort>();

        int xCount = 8 * 4;
        EAX = (uint)xCount;
        Call<DebugStub.WriteAXToComPort>();

        ESI = Memory["DebugEBP", 32];
        for (int i = 1; i <= xCount; i++) {
          Call("WriteByteToComPort");
        }
      }
    }

    public class SendStack : CodeBlock {
      public override void Assemble() {
        AL = (int)DsMsgType.Stack;
        Call<DebugStub.WriteALToComPort>();

        // Send size of bytes
        ESI = Memory["DebugESP", 32];
        EAX = Memory["DebugEBP", 32];
        EAX.Sub(ESI);
        Call<DebugStub.WriteAXToComPort>();

        // Send actual bytes
        //
        // Need to reload ESI, WriteAXToCompPort modifies it
        ESI = Memory["DebugESP", 32];
        Label = "DebugStub_SendStack_SendByte";
        ESI.Compare(Memory["DebugEBP", 32]);
        JumpIf(Flags.Equal, "DebugStub_SendStack_Exit");
        Call("WriteByteToComPort");
        Jump("DebugStub_SendStack_SendByte");

        Label = "DebugStub_SendStack_Exit";
      }
    }

        public class ProcessCommand : CodeBlock {
      // Modifies: AL, DX (ReadALFromComPort)
      // Returns: AL
      public override void Assemble() {
        Call("ReadALFromComPort");
        // Some callers expect AL to be returned, so we preserve it
        // in case any commands modify AL.
        //TODO: But in ASM wont let us push AL, so we push EAX for now
        EAX.Push();

        // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
        AL.Compare(DsCommand.Noop);
        JumpIf(Flags.Equal, "DebugStub_ProcessCmd_Exit");

        // Read Command ID
        Call("ReadALFromComPort");
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
        Call("DebugStub_Break");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_Break_After";

        AL.Compare(DsCommand.BreakOnAddress);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_BreakOnAddress_After");
        Call("DebugStub_BreakOnAddress");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_BreakOnAddress_After";

        AL.Compare(DsCommand.SendMethodContext);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMethodContext_After");
        Call("DebugStub_SendMethodContext");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_SendMethodContext_After";

        AL.Compare(DsCommand.SendMemory);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMemory_After");
        Call("DebugStub_SendMemory");
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
        Call<DebugStub.WriteALToComPort>();
        EAX = Memory["DebugStub_CommandID", 32];
        #endregion

        Call<DebugStub.WriteALToComPort>();
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
        EAX = Memory["DebugEIP", 32];
        EDI = AddressOf("DebugBPs");
        ECX = 256;
        new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
        JumpIf(Flags.NotEqual, "DebugStub_Executing_AfterBreakOnAddress");
        Call("DebugStub_Break");
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
        Call("DebugStub_Break");
        //TODO: Allow creating lables but issuing them later, then we can
        // call them with early binding
        //TODO: End - can be exit label for each method, allowing Jump(Begin/End) etc... Also make a label type and allwo Jump overload to the label itself. Or better yet, End.Jump()
        Jump("DebugStub_Executing_Normal");
        Label = "DebugStub_ExecutingStepIntoAfter";
        //
        // F10
        Memory["DebugBreakOnNextTrace", 32].Compare(StepTrigger.Over);
        JumpIf(Flags.NotEqual, "DebugStub_ExecutingStepOverAfter");
        Label = "Debug__StepOver__";
        EAX = Memory["DebugEBP", 32];
        EAX.Compare(Memory["DebugEBP", 32]);
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

        EAX = Memory["DebugEBP", 32];
        EAX.Compare(Memory["DebugBreakEBP", 32]);
        JumpIf(Flags.Equal, "DebugStub_Executing_Normal");
        CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break");
        Jump("DebugStub_Executing_Normal");
        //JumpIf(Flags.GreaterThanOrEqualTo, "DebugStub_Executing_Normal");
        //Call("DebugStub_Break");
        Label = "DebugStub_ExecutingStepOutAfter";

        Label = "DebugStub_Executing_Normal";
        // If tracing is on, send a trace message
        Memory["DebugTraceMode", 32].Compare(Tracing.On);
        CallIf(Flags.Equal, "DebugStub_SendTrace");

        // Is there a new incoming command? We dont want to wait for one
        // if there isn't one already here. This is a passing check.
        Label = "DebugStub_CheckForCmd";
        DX = (ushort)(mComAddr + 5u);
        AL = Port[DX];
        AL.Test(0x01);
        // If no command waiting, break from loop
        JumpIf(Flags.Zero, "DebugStub_CheckForCmd_Break");
        Call("DebugStub_ProcessCommand");
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
        Call("ReadALFromComPort");
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
        Call("DebugStub_SendTrace");

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
        EAX = Memory["DebugEBP", 32];
        Memory["DebugBreakEBP", 32] = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOver_After";

        AL.Compare(DsCommand.StepOut);
        JumpIf(Flags.NotEqual, "DebugStub_Break_StepOut_After");
        Memory["DebugBreakOnNextTrace", 32] = StepTrigger.Out;
        EAX = Memory["DebugEBP", 32];
        Memory["DebugBreakEBP", 32] = EAX;
        Jump("DebugStub_Break_Exit");
        Label = "DebugStub_Break_StepOut_After";

        // Loop around and wait for another command
        Jump("DebugStub_WaitCmd");

        Label = "DebugStub_Break_Exit";
        Memory["DebugStatus", 32] = Status.Run;
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
