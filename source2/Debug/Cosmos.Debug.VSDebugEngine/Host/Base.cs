using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public abstract class Base {
    protected NameValueCollection mParams;

    public Base(NameValueCollection aParams) {
      mParams = aParams;
    }

    public abstract string Start(bool aGDB);
    public abstract void Stop();
    public abstract string GetHostProcessExe();
  }
}
