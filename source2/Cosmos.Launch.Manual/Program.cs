using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Launch.Manual {
  class Program {
    static int Main(string[] aArgs) {
      var xHost = new DebugHost(aArgs);
      return xHost.Go("Manual Launching");
    }
  }
}
