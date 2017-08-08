using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Cosmos.Deploy.Pixie {
  public partial class App : Application {
    static public string PxePath;
    static public string IpAddress;
    static public string Title = "Cosmos PXE Server";

    private void Application_Startup(object sender, StartupEventArgs e) {
      if (e.Args.Length < 2) {
        MessageBox.Show("Not enough parameters.", Title);
        Shutdown(-1);
        return;
      }

      IpAddress = e.Args[0];
      PxePath = e.Args[1];
    }

  }
}
