using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class VMWarePlayer : VMWare {
    public VMWarePlayer(string aVmxFile) : base(aVmxFile) {
    }
  }
}
