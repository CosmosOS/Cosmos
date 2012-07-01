using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Cosmos.Build.Common;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class Slave : Base {

    public Slave(NameValueCollection aParams)
      : base(aParams) {
    }

    public override string GetHostProcessExe() {
      return "Cosmos.Launch.Slave.exe";
    }

    public override string Start(bool aGDB) {
      var xPort = mParams[BuildProperties.SlavePortString];
      if (xPort == "None") {
        throw new Exception("No slave port is set.");
      }

      var xParts = xPort.Split(' ');
      return mParams[xParts[1]];
    }

    public override void Stop() {
      // TODO - Send off
    }
  }
}
