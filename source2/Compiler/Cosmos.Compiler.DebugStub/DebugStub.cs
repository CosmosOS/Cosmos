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
    public enum Tracing { Off = 0, On = 1 };

    static public int mComNo = 0;
    protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
    static public UInt16 mComAddr;

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
        AL = (int)MsgType.Started; // Send the actual started signal
        Call<DebugStub.WriteALToComPort>();

        Call("DebugStub_WaitForSignature");
        Call("DebugStub_ProcessCommandBatch");
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
      // Modifies: EAX, EDX, ESI
      public override void Assemble() {
        EAX.Push();
        ESI = ESP;
        Call("WriteByteToComPort");
        EAX.Pop(); // Is a local, cant use Return(4)
      }
    }

    public class ProcessCommandBatch : CodeBlock {
      public override void Assemble() {
        Call("DebugStub_ProcessCommand");

        // See if batch is complete
        AL.Compare(Command.BatchEnd);
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
        //Return();
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
        AL.Compare(Command.Noop);
        JumpIf(Flags.Equal, "DebugStub_ProcessCmd_Exit");

        // Read Command ID
        Call("ReadALFromComPort");
        Memory["DebugStub_CommandID", 32] = EAX;

        // Get AL back so we can compare it, but also put it back for later
        EAX.Pop();
        EAX.Push();

        AL.Compare(Command.TraceOff);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOff_After");
        Memory["DebugTraceMode", 32] = (int)Tracing.Off;
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_TraceOff_After";

        AL.Compare(Command.TraceOn);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_TraceOn_After");
        Memory["DebugTraceMode", 32] = (int)Tracing.On;
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_TraceOn_After";

        AL.Compare(Command.Break);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_Break_After");
        Call("DebugStub_Break");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_Break_After";

        AL.Compare(Command.BreakOnAddress);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_BreakOnAddress_After");
        Call("DebugStub_BreakOnAddress");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_BreakOnAddress_After";

        AL.Compare(Command.SendMethodContext);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMethodContext_After");
        Call("DebugStub_SendMethodContext");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_SendMethodContext_After";

        AL.Compare(Command.SendMemory);
        JumpIf(Flags.NotEqual, "DebugStub_ProcessCmd_SendMemory_After");
        Call("DebugStub_SendMemory");
        Jump("DebugStub_ProcessCmd_ACK");
        Label = "DebugStub_ProcessCmd_SendMemory_After";

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
        AL = (int)MsgType.CmdCompleted;
        Call<DebugStub.WriteALToComPort>();
        EAX = Memory["DebugStub_CommandID", 32];
        Call<DebugStub.WriteALToComPort>();
        Label = "DebugStub_ProcessCmd_After";

        Label = "DebugStub_ProcessCmd_Exit";
        // Restore AL for callers who check the command and do
        // further processing, or for commands not handled by this routine.
        EAX.Pop();
        //Return();
      }
    }

    public class Executing : CodeBlock {
      // This is the secondary stub routine. After the primary (main) has decided we should do some debug
      // activities, this one is called.
      public override void Assemble() {
        // Look for a possible matching BP
        EAX = Memory["DebugEIP", 32];
        EDI = AddressOf("DebugBPs");
        ECX = 256;
        new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 };
        JumpIf(Flags.Equal, "DebugStub_Break");
        Label = "DebugStub_Executing_AfterBreakOnAddress";

        // See if there is a break request
        Memory["DebugBreakOnNextTrace", 32].Compare(1);
        CallIf(Flags.Equal, "DebugStub_Break");

        //TODO: Change this to support CallIf(AL == 1, "DebugStub_SendTrace");
        Memory["DebugTraceMode", 32].Compare((int)DebugStub.Tracing.On);
        CallIf(Flags.Equal, "DebugStub_SendTrace");

        Label = "DebugStub_Executing_Normal";
        // Is there a new incoming command? We dont want to wait for one
        // if there isn't one already here. This is a passing check.
        Label = "DebugStub_CheckForCmd";
        DX = mComAddr;
        AL = Port[DX];
        AL.Test(0x01);
        // If no command waiting, break from loop
        JumpIf(Flags.Zero, "DebugStub_CheckForCmd_Break");
        Call("DebugStub_ProcessCommand");
        // See if there are more commands waiting
        Jump("DebugStub_CheckForCmd");
        Label = "DebugStub_CheckForCmd_Break";

        //Return();
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
