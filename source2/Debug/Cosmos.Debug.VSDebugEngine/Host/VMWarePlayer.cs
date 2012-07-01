using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Vestris.VMWareLib;

namespace Cosmos.Debug.VSDebugEngine.Host {
  public class VMwarePlayer : VMware {
    public VMwarePlayer(NameValueCollection aParams, string aVmxFile)
      : base(aParams, aVmxFile) {
    }

    public static bool IsInstalled {
      get {
        return GetPlayerPathname() != null;
      }
    }

    protected override string GetParams() {
      return "\"" + GetPlayerPathname() + "\" \"" + mVmxFile + "\"";
    }

    protected static string GetPlayerPathname() {
      return GetPathname("VMware Player", "vmplayer.exe");
    }

    protected override void ConnectToVMWare(VMWareVirtualHost aHost) {
      aHost.ConnectToVMWarePlayer();
    }
  }
}
