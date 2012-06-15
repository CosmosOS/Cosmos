using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public abstract class VMWare : Base {
    protected string mVmxFile;

    public VMWare(string aVmxFile) {
    }

    public override void Stop() {
      using (var xHost = new VMWareVirtualHost()) {
        //xHost.ConnectToVMWareWorkstation();
        xHost.ConnectToVMWarePlayer();
        using (VMWareVirtualMachine xMachine = xHost.Open(mVmxFile)) {
          xMachine.PowerOff();
        }
        xHost.Close();
      }
    }
  }
}
