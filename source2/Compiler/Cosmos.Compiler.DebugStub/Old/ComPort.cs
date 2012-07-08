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
    static public int mComNo = 0;
    static protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
    static public UInt16 mComAddr;
    static public UInt16 mComStatusAddr;

    public abstract class Inlines : CodeBlock {
      // INLINE
      // Modifies: Stack, EDI, AL
      // TODO: Modify X# to allow inlining better by using dynamic labels otherwise
      // repeated use of an inline will fail with conflicting labels.
      // TODO: Allow methods to emit a start label and return automatically
      // and mark inlines so this does not happen.
      //TODO: Allow inlining in X# wtih an attribute - or method like Call<>?
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
          Call("DebugStub_ReadByteFromComPort");
        }
      }

      protected void WriteBytesToComPort(int xCount) {
        for (int i = 1; i <= xCount; i++) {
          Call("DebugStub_WriteByteToComPort");
        }
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

  }
}
