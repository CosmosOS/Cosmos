using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Cosmos.Build.Builder {
  public partial class App : Application {
    protected string[] mArgs;
    public string[] Args {
      get { return mArgs; }
    }

    protected override void OnStartup(StartupEventArgs e) {
      mArgs = new string[e.Args.Length];
      for (int i = 0; i < mArgs.Length; i++) {
        mArgs[i] = e.Args[i].ToUpper();
      }
      base.OnStartup(e);
    }
  }
}
