using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Installer {
  public static class Global {
    static Global() {
      // IsX64 is if the system is x64, not the application
      IsX64 = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));

    }

    public static readonly bool IsX64;
  }
}
