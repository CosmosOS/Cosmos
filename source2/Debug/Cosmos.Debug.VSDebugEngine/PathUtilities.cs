using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine {
  public static class Paths {
    public static string Cosmos() {
      using (var xKey = Registry.LocalMachine.OpenSubKey("Software\\Cosmos")) {
        return (string)xKey.GetValue("UserKit");
      }
    }

    public static string Build() {
      return Path.Combine(Cosmos(), "Build");
    }

    public static string Tools() {
      return Path.Combine(Build(), "Tools");
    }

    public static string VSIP() {
      return Path.Combine(Build(), "VSIP");
    }
  }
}