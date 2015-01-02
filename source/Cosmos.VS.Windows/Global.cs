using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.VS.Windows {
  static public class Global {
    /// <summary>A pipe used to send requests to the AD7Process.</summary>
    static public Cosmos.Debug.Common.PipeClient PipeUp;
    static public EnvDTE.OutputWindowPane OutputPane;

    static Global() {
      PipeUp = new Cosmos.Debug.Common.PipeClient(Pipes.UpName);
    }

    static public Cosmos.Debug.Common.PipeClient ConsoleTextChannel;
  }
}
