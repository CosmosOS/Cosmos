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
    public class ProcessCommand : CodeBlock {
      // Modifies: AL, DX (ComReadAL)
      // Returns: AL
      public override void Assemble() {
        Call("DebugStub_ComReadAL");
        // Some callers expect AL to be returned, so we preserve it
        // in case any commands modify AL.
        // We push EAX to keep stack aligned. 
        EAX.Push();

        // Noop has no data at all (see notes in client DebugConnector), so skip Command ID
        // Noop also does not send ACK.
        AL.Compare(Vs2Ds.Noop);
        JumpIf(Flags.Equal, ".End");

        // Read Command ID
        Call("DebugStub_ComReadAL");
        CommandID.Value = EAX;

        // Get AL back so we can compare it, but also put it back for later
        EAX = ESP[0];
        CheckCmd("TraceOff", Vs2Ds.TraceOff);
        CheckCmd("TraceOn", Vs2Ds.TraceOn);
        CheckCmd("Break", Vs2Ds.Break);
        CheckCmd("BreakOnAddress", Vs2Ds.BreakOnAddress);
        CheckCmd("SendMethodContext", Vs2Ds.SendMethodContext);
        CheckCmd("SendMemory", Vs2Ds.SendMemory);
        CheckCmd("SendRegisters", Vs2Ds.SendRegisters);
        CheckCmd("SendFrame", Vs2Ds.SendFrame);
        CheckCmd("SendStack", Vs2Ds.SendStack);
        CheckCmd("Ping", Vs2Ds.Ping);

        Label = ".End";
        // Restore AL for callers who check the command and do
        // further processing, or for commands not handled by this routine.
        EAX.Pop();
      }

      protected void CheckCmd(string aLabel, byte aCmd) {
        AL.Compare(aCmd);
        string xAfterLabel = NewLabel();
        JumpIf(Flags.NotEqual, xAfterLabel);
        Call("DebugStub_" + aLabel);
        Call("DebugStub_AckCommand");
        Jump(".End");
        Label = xAfterLabel;
      }
    }

  }
}
