using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.VS.Windows {
  static public class Global {
    static public Cosmos.Debug.Common.PipeClient PipeUp;
    static public EnvDTE.OutputWindowPane OutputPane;

    static Global() {
      PipeUp = new Cosmos.Debug.Common.PipeClient(Cosmos.Debug.Consts.Pipes.UpName);
    }
  }
}
