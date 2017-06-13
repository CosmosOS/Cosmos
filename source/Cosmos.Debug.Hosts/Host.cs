using System;
using System.Collections.Generic;

namespace Cosmos.Debug.Hosts
{
  public abstract class Host
  {
    protected Dictionary<string, string> mParams;
    protected bool mUseGDB;

    public EventHandler OnShutDown;

    public Host(Dictionary<string, string> aParams, bool aUseGDB)
    {
      mParams = aParams;
      mUseGDB = aUseGDB;
    }

    public abstract void Start();
    public abstract void Stop();
  }
}
