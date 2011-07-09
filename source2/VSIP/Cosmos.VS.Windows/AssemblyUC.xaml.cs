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
    protected string mCode;

    public AssemblyUC() {
      InitializeComponent();

      mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
    }

    void mitmCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mCode);
    }

    public void Update(byte[] aData) {
      mCode = Encoding.ASCII.GetString(aData);
      mCode = mCode.Replace("\t", "  ");

      string[] xLines = mCode.Split('\n');
      foreach (string xLine in xLines) {
        var xRun = new Run(xLine);
        tblkSource.Inlines.Add(xRun);
      }
    }

  }
}