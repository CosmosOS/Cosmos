using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Launch.Slave {
  public class DebugHost : Cosmos.Launch.Common.DebugHost {
    public DebugHost(string[] aArgs)
      : base(aArgs) {
    }

    protected override int Run() {
      return 0;
    }
  }
}
