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
using Cosmos.Debug.Consts;
using System.Windows.Threading;
using System.Threading;

namespace Cosmos.VS.Windows {
  public partial class AssemblyUC : UserControl {
    static public byte[] mData = new byte[0];
    StringBuilder mCode = new StringBuilder();
    bool mFilter = true;

    public AssemblyUC() {
      InitializeComponent();

      mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      butnPing.Click += new RoutedEventHandler(butnPing_Click);

      Update(mData);
    }

    void butnPing_Click(object sender, RoutedEventArgs e) {
      Global.mPipeUp.SendCommand(Cosmos.Debug.Consts.DwCmd.Ping, null);
    }

    void mitmCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mCode.ToString());
    }

    protected void Display(bool aFilter) {
      // Used for creating a test file for Cosmos.VS.Windows.Test
      //System.IO.File.WriteAllBytes(@"D:\source\Cosmos\source2\VSIP\Cosmos.VS.Windows.Test\SourceTest.bin", mData);

      mCode.Clear();
      tblkSource.Inlines.Clear();
      if (mData.Length == 0) {
        return;
      }

      var xFont = new FontFamily("Consolas");
      Brush xBrush;
      string xCode = Encoding.ASCII.GetString(mData);

      // Should always be \r\n, but just in case we split by \n and ignore \r
      string[] xLines = xCode.Replace("\r", "").Split('\n');
      foreach (string xLine in xLines) {
        xBrush = Brushes.Blue;
        string xDisplayLine = xLine;
        string xTestLine = xLine.Trim().ToUpper();
        var xTestParts = xTestLine.Split(' ');

        if (aFilter) {
          xDisplayLine = xDisplayLine.Trim();
          var xParts = xDisplayLine.Split(' ');

          if (xTestLine == "INT3") {
            continue;
          } else if (xTestLine.Length == 0) {
            // Remove all empty linesIW
            continue;
          } else {
            // Skip ASM labels
            if (xTestParts.Length > 1) {
              if (xTestParts[1] == ";ASM") {
                continue;
              }
            }
          }

          if (xTestParts[0].EndsWith(":")) {
            // Insert a blank line before labels, but not if its the top line
            if (tblkSource.Inlines.Count > 0) {
              tblkSource.Inlines.Add(new LineBreak());
              mCode.AppendLine();
            }
            // Remove the comment marker after the label
            xDisplayLine = xParts[0];
          } else {
            xDisplayLine = "\t" + xDisplayLine;
          }
        }

        if (xTestParts[0].EndsWith(":")) {
          xBrush = Brushes.Black;
        } else if (xTestLine.StartsWith(";")) {
          xBrush = Brushes.Green;
        }

        // Even though our code is often the source of the tab, it makes
        // more sense to do it this was because the number of space stays
        // in one place and also lets us differntiate from natural spaces.
        xDisplayLine = xDisplayLine.Replace("\t", "    ");

        var xRun = new Run(xDisplayLine);
        xRun.FontFamily = xFont;
        xRun.Foreground = xBrush;

        tblkSource.Inlines.Add(xRun);
        tblkSource.Inlines.Add(new LineBreak());
        mCode.AppendLine(xDisplayLine);
      }
    }

    public void Update(byte[] aData) {
      mData = aData;
      Display(mFilter);
    }

    private void asmFilterButton_Click(object sender, RoutedEventArgs e) {
      mFilter = !mFilter;
      Display(mFilter);
    }

    private void asmFCopyButton_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mCode.ToString());
    }

    private void asmStepButton_Click(object sender, RoutedEventArgs e) {
    }

  }
}