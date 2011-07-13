using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cosmos.Cosmos_VS_Windows {
  public partial class StackUC : UserControl {

    public static byte[] mFrameData;
    public static byte[] mStackData;

    public StackUC() {
      InitializeComponent();
      UpdateFrame(mFrameData);
      UpdateStack(mStackData);
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