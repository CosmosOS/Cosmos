using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Cosmos.Debug.Consts;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cosmos.VS.Windows {
  [Guid("39A8D0A0-26A3-4234-A660-0C4C8BF40FF3")]
  public class InternalTW : ToolWindowPane2 {
    public InternalTW() {
      Caption = "Cosmos Internal";

      BitmapResourceID = 301;
      BitmapIndex = 1;

      mUserControl = new InternalUC();
      Content = mUserControl;
    }
  }
  
  public partial class InternalUC : DebuggerUC {
    public InternalUC() {
      InitializeComponent();

      butnPingVSIP.Click += new RoutedEventHandler(butnPingVSIP_Click);
      butnPingDS.Click += new RoutedEventHandler(butnPingDS_Click);
    }

    void butnPingDS_Click(object sender, RoutedEventArgs e) {
      Global.PipeUp.SendCommand(Cosmos.Debug.Consts.DwCmd.PingDebugStub, null);
    }

    void butnPingVSIP_Click(object sender, RoutedEventArgs e) {
      MessageBox.Show("This only works if there is an active debug session, see comment in code for this event for more details.");
      // Note: This will only work if the debugger is active,
      // ie Cosmos is booted. This is because the receiving
      // pipe is currently part of AD7Process which has a lifespan
      // tied to an active debug session.
      Global.PipeUp.SendCommand(Cosmos.Debug.Consts.DwCmd.PingVSIP, null);
    }
  }
}
