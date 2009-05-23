using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Cosmos.VS.Debugger.PortSupplier {
  public class Registration {
    static private string GuidStr(Type aType) {
      return "{" + aType.GUID.ToString().ToUpper() + "}";
    }

    // IDebugEngine2::SetMetric
    static public void Register() {
			// Wow6432Node if on x64...
      // work on localmachine - current user too?
      // localmachine has no 9.0exp
      var xVSKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0\", false);
      var xCLSID = xVSKey.OpenSubKey(@"CLSID", true);
      var xMetricsKey = xVSKey.OpenSubKey(@"AD7Metrics", false);
      var xPortSupplierKey = xMetricsKey.OpenSubKey("PortSupplier", true);
      RegistryKey xKey;

      string xGUID = GuidStr(typeof(PortSupplier));
      xPortSupplierKey.DeleteSubKey(xGUID, false);
      xKey = xPortSupplierKey.CreateSubKey(xGUID);
      xKey.SetValue("Name", "Cosmos Port Supplier");
      xKey.SetValue("CLSID", xGUID);

      // Getting permission error here
      xKey = xCLSID.CreateSubKey(xGUID);
      var xAsm = typeof(PortSupplier).Assembly;
      //TODO: Extract from class directly
      xKey.SetValue ("Assembly", "Cosmos.VS.Debugger.PortSupplier");
      xKey.SetValue ("Class", typeof(PortSupplier).FullName);
      xKey.SetValue ("Codebase", xAsm.Location);
      xKey.SetValue ("InProcServer32", typeof(Object).Assembly.Location);
    }
  }
}
