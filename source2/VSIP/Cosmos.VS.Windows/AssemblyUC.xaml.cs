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
using Cosmos.Compiler.Debug;
using System.Windows.Threading;
using Cosmos.VS.Debug;
using System.Threading;

namespace Cosmos.Cosmos_VS_Windows {
  public partial class AssemblyUC : UserControl {
    static public byte[] mData = new byte[0];
    string mCode = "";
    bool mFilter = true;

    public AssemblyUC() {
      InitializeComponent();

      mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      Update(mData);
    }

    void mitmCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mCode);
    }

    protected void Display(bool aFilter) {
      tblkSource.Inlines.Clear();
      if (mData.Length == 0) {
        return;
      }

      mCode = Encoding.ASCII.GetString(mData);
      mCode = mCode.Replace("\t", "  ");

      string[] xLines = mCode.Split('\n');
      foreach (string xLine in xLines) {
        string xTestLine = xLine.Trim();

        if (aFilter) {
          if (string.Compare(xTestLine, "INT3", true) == 0) {
            continue;
          } else if (xTestLine.StartsWith("; #")) {
            continue;
          } else if (xTestLine.EndsWith("#:")) {
            continue;
          }
        }

        var xRun = new Run(xLine);
        if (xTestLine.EndsWith(":")) {
          xRun.Foreground = Brushes.Black;
        } else if (xTestLine.StartsWith(";")) {
          xRun.Foreground = Brushes.Green;
        } else {
          xRun.Foreground = Brushes.Blue;
        }

        tblkSource.Inlines.Add(xRun);
      }
    }

    public void Update(byte[] aData) {
      mData = aData;
      Display(mFilter);
    }

  }
}