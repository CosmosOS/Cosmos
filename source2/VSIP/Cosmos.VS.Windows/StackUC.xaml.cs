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
    public StackUC() {
      InitializeComponent();

      tboxSourceFrame.Text = "";
      tboxSourceStack.Text = "";
    }

    protected List<string> SplitValues(byte[] aData) {
      string xData = BitConverter.ToString(aData);
      xData = xData.Trim();
      xData = xData.Replace("-", "");

      int xCount = xData.Length / 8;
      var xResult = new List<string>(xCount);
      for (int i = 0; i < xCount; i++) {
        xResult.Add(xData.Substring(i * 8, 8));
      }
      return xResult;
    }

    public void UpdateFrame(byte[] aData) {
      var xValues = SplitValues(aData);
      int xCount = xValues.Count;
      var xSB = new StringBuilder();
      xSB.AppendLine("Arguments");
      for (int i = 0; i < xCount; i++) {
        // We start at EBP + 8, because lower is not transmitted
        // [EBP] is saved EIP - not needed
        // [EBP + 4] is old EBP - not needed
        xSB.AppendLine("[EBP + " + (i * 4 + 4) + "] 0x" + xValues[i]);
      }
      tboxSourceFrame.Text = xSB.ToString();
    }

    public void UpdateStack(byte[] aData) {
      var xValues = SplitValues(aData);
      int xCount = xValues.Count;
      var xSB = new StringBuilder();
      xSB.AppendLine("Locals and Stack");
      for (int i = 0; i < xCount; i++) {
        xSB.AppendLine("[EBP - " + ((xCount - i) * 4) + "] 0x" + xValues[i]);
      }
      tboxSourceStack.Text = xSB.ToString();
    }
  }
}