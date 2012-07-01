using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Launch.Slave {
  class Program {
    static int Main(string[] aArgs) {
      var xHost = new DebugHost(aArgs);
      return xHost.Go("Attached Slave");
    }
  }
}
