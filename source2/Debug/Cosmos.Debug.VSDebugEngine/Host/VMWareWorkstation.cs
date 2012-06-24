using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class VMwareWorkstation : VMware {
    public VMwareWorkstation(string aVmxFile)
      : base(aVmxFile) {
    }

    public static bool IsInstalled {
      get {
        return GetWorkstationPathname() != null;
      }
    }

    protected override string GetParams() {
      // -x: Auto power on VM. Must be small x, big X means something else.
      // -q: Close VMWare when VM is powered off.
      // Options must come beore the vmx, and cannot use shellexecute
      return "\"" + GetWorkstationPathname() + "\" -x -q \"" + mVmxFile + "\"";
    }

    protected static string GetWorkstationPathname() {
      return GetPathname("VMware Workstation", "vmware.exe");
    }

    protected override void ConnectToVMWare(VMWareVirtualHost aHost) {
      aHost.ConnectToVMWareWorkstation();
    }
  }
}
