using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Installer {
  public class Log {
    public Log() {
      mEchoing = true;
    }

    protected bool mEchoing;
    public bool Echoing {
      get { return mEchoing; }
    }

    public void Echo() {
      Echo("");
    }

    public void Echo(string aText) {
      // TODO
    }

    public void EchoOn() {
      mEchoing = true;
    }

    public void EchoOff() {
      mEchoing = false;
    }

  }
}
