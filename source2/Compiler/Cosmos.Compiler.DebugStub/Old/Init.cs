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
    public DebugStub(int aComNo) {
      mComNo = aComNo;
      mComAddr = mComPortAddresses[mComNo - 1];
      mComStatusAddr = (UInt16)(mComAddr + 5);

      // Old method, need to convert to fields
      mAsm.DataMembers.AddRange(new DataMember[]{
        // Breakpoint addresses
        new DataMember("DebugBPs", new int[256]),
        //TODO: Move to DebugStub (new)
        new DataMember("DebugWaitMsg", "Waiting for debugger connection...")
      });
    }

    public class WaitForSignature : CodeBlock {
      public override void Assemble() {
        EBX = 0;

        Label = "DebugStub_WaitForSignature_Read";
        Call<ReadALFromComPort>();
        BL = AL;
        EBX.RotateRight(8);
        EBX.Compare(Cosmos.Debug.Consts.Consts.SerialSignature);
        JumpIf(Flags.NotEqual, "DebugStub_WaitForSignature_Read");

        //TODO: Always emit and exit label and then make a Exit method which can
        // automatically use it. I think a label might already exist.
        Label = "DebugStub_WaitForSignature_Exit";
      }
    }

    public class WaitForDbgHandshake : Inlines {
      public override void Assemble() {
        // "Clear" the UART out
        AL = 0;
        Call("DebugStub_WriteALToComPort");

        // QEMU (and possibly others) send some garbage across the serial line first.
        // Actually they send the garbage inbound, but garbage could be inbound as well so we 
        // keep this.
        // To work around this we send a signature. DC then discards everything before the signature.
        // QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
        // feature so we kept it.
        Push(Cosmos.Debug.Consts.Consts.SerialSignature);
        ESI = ESP;
        WriteBytesToComPort(4);
        // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
        EAX.Pop();

        // We could use the signature as the start signal, but I prefer
        // to keep the logic separate, especially in DC.
        AL = (int)DsVsip.Started; // Send the actual started signal
        Call("DebugStub_WriteALToComPort");

        Call<WaitForSignature>();
        Call<ProcessCommandBatch>();
      }
    }

  }
}
