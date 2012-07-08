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
          Call("DebugStub_ComRead32");
        }
      }
    }

  }
}
