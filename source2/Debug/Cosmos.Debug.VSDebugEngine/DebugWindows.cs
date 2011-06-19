using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine {

  public class DebugWindows {

    public static void Test(string aData) {
      using (var xPipe = new NamedPipeClientStream(".", "CosmosDebugWindows", PipeDirection.Out)) {
        xPipe.Connect();
        using (var xWriter = new StreamWriter(xPipe)) {
          xWriter.WriteLine(aData);
        }
        xPipe.Flush();
      }
    }

  }

}
