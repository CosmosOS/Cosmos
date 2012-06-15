using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class VMWareWorkstation : VMWare {
    public VMWareWorkstation(string aVmxFile)
      : base(aVmxFile) {
    }
  }
}
