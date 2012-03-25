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
  [Guid("A64D0FCC-8DCC-439A-9B16-3C43128AAD51")]
  public class StackTW : ToolWindowPane2 {
    public StackTW() {
      Caption = "Cosmos Stack";
      BitmapResourceID = 301;
      BitmapIndex = 1;

      mUserControl = new StackUC();
      Content = mUserControl;
    }
  }
  
  public partial class StackUC : DebuggerUC {
    public StackUC() {
      InitializeComponent();
    }

    protected override void DoUpdate(string aTag) {
      if (aTag == "STACK") {
        UpdateStack(mData);
      } else if (aTag == "FRAME") {
        UpdateFrame(mData);
      }
    }

    public void UpdateFrame(byte[] aData) {
      var xValues = MemoryViewUC.Split(aData);
      int xCount = xValues.Count;
      memvEBP.Clear();
      for (int i = 0; i < xCount; i++) {
        // We start at EBP + 8, because lower is not transmitted
        // [EBP] is old EBP - not needed
        // [EBP + 4] is saved EIP - not needed
        memvEBP.Add("[EBP + " + (i * 4 + 8) + "]", xValues[i]);
      }
    }

    public void UpdateStack(byte[] aData) {
      var xValues = MemoryViewUC.Split(aData);
      int xCount = xValues.Count;
      memvESP.Clear();
      for (int i = 0; i < xCount; i++) {
        memvESP.Add("[EBP - " + ((xCount - i) * 4) + "] [ESP + " + (i * 4) + "]", xValues[i]);
      }
    }
  }
}