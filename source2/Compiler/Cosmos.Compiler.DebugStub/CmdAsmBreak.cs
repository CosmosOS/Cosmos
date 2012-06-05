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
    
    // Location where INT3 has been injected
    // 0 if no INT3 is active
    static public DataMember32 AsmBreakEIP;

    // Old byte before INT3 was injected.
    // Only 1 byte is used.
    static public DataMember32 AsmOrigByte;
    
    public class SetAsmBreak : Inlines {
      public override void Assemble() {
        ReadComPortX32toStack(1);
        EDI.Pop();
        // Save the old byte
        EAX = EDI[0];
        AsmOrigByte.Value = EAX;
        // Inject INT3
        EDI[0] = 0xCC;
        // Save EIP of the break
        AsmBreakEIP.Value = EDI;
      }
    }

    public class ClearAsmBreak : Inlines {
      public override void Assemble() {
        EDI = AsmBreakEIP.Value;
        EDI.Compare(0);
        // If 0, we don't need to clear an older one.
        JumpIf(Flags.Equal, ".Exit");
        // Clear old break point and set back to original opcode / partial opcode
        EAX = AsmOrigByte.Value;
        EDI[0] = EAX;
        AsmOrigByte.Value = 0;
      }
    }
  }
}