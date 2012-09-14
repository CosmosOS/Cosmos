using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public abstract class Base {
    protected NameValueCollection mParams;
    protected bool mUseGDB;

    public EventHandler OnShutDown;

    public Base(NameValueCollection aParams, bool aUseGDB) {
      mParams = aParams;
      mUseGDB = aUseGDB;
    }

    public abstract void Start();
    public abstract void Stop();
  }
}
