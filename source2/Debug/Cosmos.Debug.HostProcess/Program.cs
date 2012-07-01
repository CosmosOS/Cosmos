using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;

namespace Cosmos.Launch.VMware {
  public class Program {
    static int Main(string[] aArgs) {
      var xHost = new DebugHost(aArgs);
      return xHost.Go("VMware");
    }
  }
}
