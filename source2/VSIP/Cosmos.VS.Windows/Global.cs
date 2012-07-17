using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.VS.Windows {
  static public class Global {
    static public Cosmos.Debug.Common.PipeClient PipeUp;
    static public EnvDTE.OutputWindowPane OutputPane;

    static Global() {
      PipeUp = new Cosmos.Debug.Common.PipeClient(Pipes.UpName);
    }
  }
}
