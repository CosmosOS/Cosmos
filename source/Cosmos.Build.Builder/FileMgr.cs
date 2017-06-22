using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Build.Builder {
  public class FileMgr : IDisposable {

    // Dummy pattern to allow scoping via using.
    // Hacky, but for what we are doing its fine and the GC
    // effects are negligible in our usage.
    protected virtual void Dispose(bool aDisposing) {
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
