using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine {

  public class DebugWindows {
  
    public static void Test(string aData) {
      using (var xPipe = new NamedPipeClientStream(".", "CosmosDebug", PipeDirection.Out)) {
        // The connect function will indefinately wait for the pipe to become available
        // If that is not acceptable specify a maximum waiting time (in ms)
        xPipe.Connect();
        using (var xWriter = new StreamWriter(xPipe)) {
          xWriter.WriteLine(aData);
        }
      }
    }

  }

}
