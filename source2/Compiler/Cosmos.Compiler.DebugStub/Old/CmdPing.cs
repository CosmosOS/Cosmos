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
    public class Ping : CodeBlock {
      public override void Assemble() {
        AL = DsVsip.Pong;
        Call("DebugStub_ComWriteAL");
      }
    }
  }
}
