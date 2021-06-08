using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

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

    static public List<uint> Split(byte[] aData) {
      var xResult = new List<uint>(aData.Length / 4);
      for (int i = 0; i < aData.Length; i = i + 4) {
                uint xValue = (uint)
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

    public void Add(string aLabel, uint aData) {
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
