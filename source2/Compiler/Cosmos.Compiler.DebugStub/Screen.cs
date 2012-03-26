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
    public class Cls : CodeBlock {
      public override void Assemble() {
        ESI = VidBase;
        Label = "DebugStub_Cls_More";
        AL = 0x00;
        // Why add 8 to ESI every time? Why not just add 8 in the first place?
        ESI[0] = AL; // Text
        ESI++;

        AL = 0x0A;
        ESI[0] = AL; // Colour
        ESI++;

        ESI.Compare(VidBase + 25 * 80 * 2);
        JumpIf(Flags.LessThan, "DebugStub_Cls_More");
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
        AL = ESI[0];
        AL.Compare(0);
        JumpIf(Flags.Equal, ".AfterMsg");
        ESI++;
        EDI[0] = AL;
        EDI++;
        EDI++;
        Jump(".ReadChar");
        //TODO: Local labels in X#
        Label = ".AfterMsg";
      }
    }
  }
}
