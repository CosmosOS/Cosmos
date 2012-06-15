using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public abstract class VMWare : Base {
    public override void Stop() {
      using (var xHost = new VMWareVirtualHost()) {
        //xHost.ConnectToVMWareWorkstation();
        xHost.ConnectToVMWarePlayer();
        using (VMWareVirtualMachine virtualMachine = xHost.Open(@"C:\Virtual Machines\xp\xp.vmx")) {
        }
      }
    }
  }
}
