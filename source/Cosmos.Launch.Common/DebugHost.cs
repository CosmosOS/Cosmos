using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Launch.Common {
  public abstract class DebugHost {
    protected string[] mArgs;

    public DebugHost(string[] aArgs, int aMinArgCount) {
      if (aArgs.Length < aMinArgCount) {
        throw new Exception("Not enough arguments.");
      }
      mArgs = aArgs;
    }

    protected StringBuilder mInputLine = new StringBuilder();
    protected string CheckInputLine() {
      while (true) {
        // Don't use Console read methods directly, most (ReadKey, etc) have issues with redirected input.
        int xCharInt = Console.In.Read();
        if (xCharInt == -1) {
          break;
        }

        var xChar = (char)xCharInt;
        if (xChar == '\r') {
          string xResult = mInputLine.ToString();
          mInputLine.Clear();
          return xResult;
        } else {
          mInputLine.Append(xChar);
        }
      }
      return null;
    }

    public int Go(string aEdition) {
      try {
        Console.WriteLine("Cosmos Visual Studio Debug Host for " + aEdition);
        Console.WriteLine("Waiting for start signal.");

        // This is here to allow this process to start, but pause till the caller tells it to continue. 
        Console.ReadLine();

        return Run();
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Thread.Sleep(2000);
        return 0;
      }
    }

    protected abstract int Run();
  }
}
