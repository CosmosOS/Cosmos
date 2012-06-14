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
  }
}
