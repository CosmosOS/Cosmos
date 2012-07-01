using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Launch.Common {
  public abstract class DebugHost {
    protected string[] mArgs;

    public DebugHost(string[] aArgs) {
      mArgs = aArgs;
    }

    public int Go() {
      try {
        Console.WriteLine("Cosmos Visual Studio Debug Host.");
        Console.WriteLine("Waiting for start signal.");

        // This is here to allow this process to start, but pause till the caller tells it to continue. 
        Console.ReadLine();

        return Run();
      } catch (Exception e) {
        Console.WriteLine(e.Message);
        return 0;
      }
    }

    protected abstract int Run();
  }
}
