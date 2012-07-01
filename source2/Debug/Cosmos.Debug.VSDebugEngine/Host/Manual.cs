using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class Manual : Base {

    public Manual(NameValueCollection aParams)
      : base(aParams) {
    }

    public override string GetHostProcessExe() {
      return "Cosmos.Launch.Manual.exe";
    }

    public override string Start(bool aGDB) {
      return "";
    }

    public override void Stop() {
      // TODO - Send off
    }
  }
}
