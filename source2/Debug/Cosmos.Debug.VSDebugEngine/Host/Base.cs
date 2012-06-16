using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public abstract class Base {
    public abstract string Start(string aIsoFile, bool aGDB);
    public abstract void Stop();
  }
}
