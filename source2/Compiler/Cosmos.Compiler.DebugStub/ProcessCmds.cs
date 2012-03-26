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
    public class AckCommand : CodeBlock {
      public override void Assemble() {
        // We acknowledge receipt of the command AND the processing of it.
        //   -In the past the ACK only acknowledged receipt.
        // We have to do this because sometimes callers do more processing.
        // We ACK even ones we dont process here, but do not ACK Noop.
        // The buffers should be ok because more wont be sent till after our NACK
        // is received.
        // Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7. 
        // UART buffer is 16.
        // We may need to revisit this in the future to ack not commands, but data chunks
        // and move them to a buffer.
        // The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).
        AL = DsVsip.CmdCompleted;
        Call<WriteALToComPort>();
        //
        EAX = DebugStub_CommandID.Value;
        Call<WriteALToComPort>();
      }
    }

    public class ProcessCommand : CodeBlock {
      // Modifies: AL, DX (ReadALFromComPort)
      // Returns: AL
      public override void Assemble() {
        Call<ReadALFromComPort>();
        // Some callers expect AL to be returned, so we preserve it
        // in case any commands modify AL.
        // We push EAX to keep stack aligned. 
        EAX.Push();

        // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
        // Noop also does not send ACK.
        AL.Compare(VsipDs.Noop);
        JumpIf(Flags.Equal, ".End");

        // Read Command ID
        Call<ReadALFromComPort>();
        DebugStub_CommandID.Value = EAX;

        // Get AL back so we can compare it, but also put it back for later
        EAX = ESP[0];
        CheckCmd(VsipDs.TraceOff, typeof(TraceOff));
        CheckCmd(VsipDs.TraceOn, typeof(TraceOn));
        CheckCmd(VsipDs.Break, typeof(Break));
        CheckCmd(VsipDs.BreakOnAddress, typeof(BreakOnAddress));
        CheckCmd(VsipDs.SendMethodContext, typeof(SendMethodContext));
        CheckCmd(VsipDs.SendMemory, typeof(SendMemory));
        CheckCmd(VsipDs.SendRegisters, typeof(SendRegisters));
        CheckCmd(VsipDs.SendFrame, typeof(SendFrame));
        CheckCmd(VsipDs.SendStack, typeof(SendStack));
        CheckCmd(VsipDs.SetAsmBreak, typeof(SetAsmBreak));
        CheckCmd(VsipDs.Ping, typeof(Ping));

        Label = ".End";
        // Restore AL for callers who check the command and do
        // further processing, or for commands not handled by this routine.
        EAX.Pop();
      }

      protected void CheckCmd(byte aCmd, Type aFunction) {
        AL.Compare(aCmd);
        string xAfterLabel = NewLabel();
        JumpIf(Flags.NotEqual, xAfterLabel);
        Call(aFunction);
        Call<AckCommand>();
        Jump(".End");
        Label = xAfterLabel;
      }
    }

    public class ProcessCommandBatch : CodeBlock {
      public override void Assemble() {
        Call<ProcessCommand>();

        // See if batch is complete
        AL.Compare(VsipDs.BatchEnd);
        JumpIf(Flags.Equal, "DebugStub_ProcessCommandBatch_Exit");

        // Loop and wait
        Jump("DebugStub_ProcessCommandBatch");

        Label = "DebugStub_ProcessCommandBatch_Exit";
        Call<AckCommand>();
      }
    }
  }
}
