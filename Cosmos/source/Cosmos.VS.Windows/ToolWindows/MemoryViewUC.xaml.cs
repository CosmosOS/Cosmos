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

namespace Cosmos.VS.Windows {
  public partial class MemoryViewUC : UserControl {
    FontFamily mFont = new FontFamily("Consolas");

    public MemoryViewUC() {
      InitializeComponent();
    }

    public string Title {
      get { return tblkLabel.Text; }
      set { tblkLabel.Text = value; }
    }

    static public List<UInt32> Split(byte[] aData) {
      var xResult = new List<UInt32>(aData.Length / 4);
      for (int i = 0; i < aData.Length; i = i + 4) {
        UInt32 xValue = (UInt32)
          (aData[i + 3] << 24 |
          aData[i + 2] << 16 |
          aData[i + 1] << 8 |
          aData[i]);
        xResult.Add(xValue);
      }
      return xResult;
    }

    public void Clear() {
      spnlLabel.Children.Clear();
      spnlAddress.Children.Clear();
      spnlData.Children.Clear();
    }

    public void Add(string aLabel, UInt32 aData) {
      var xLabel = new TextBlock();
      xLabel.FontFamily = mFont;
      xLabel.Text = aLabel;
      spnlLabel.Children.Add(xLabel);

      var xData = new DataBytesUC();
      xData.Value = aData;
      spnlData.Children.Add(xData);
    }
  }
}
