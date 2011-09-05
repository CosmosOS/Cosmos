using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Cosmos_VS_Windows {
  static public class Global {
    static public Cosmos.Debug.Common.PipeClient mPipeUp;

    static Global() {
      mPipeUp = new Cosmos.Debug.Common.PipeClient(Cosmos.Debug.Consts.Pipes.UpName);
    }
  }
}
