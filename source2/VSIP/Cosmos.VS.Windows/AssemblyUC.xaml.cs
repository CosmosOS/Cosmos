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
  /// This class implements the tool window exposed by this package and hosts a user control.
  ///
  /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
  /// usually implemented by the package implementer.
  ///
  /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
  /// implementation of the IVsUIElementPane interface.
  //
  [Guid("f019fb29-c2c2-4d27-9abf-739533c939be")]
  public class AssemblyTW : ToolWindowPane {
    public static AssemblyUC mUC;

    public AssemblyTW() : base(null) {
      //this.ToolBar = new CommandID(GuidList.guidAsmToolbarCmdSet, (int)PkgCmdIDList.AsmToolbar);
      Caption = "Cosmos Assembly";

      // Set the image that will appear on the tab of the window frame
      // when docked with an other window
      // The resource ID correspond to the one defined in the resx file
      // while the Index is the offset in the bitmap strip. Each image in
      // the strip being 16x16.
      BitmapResourceID = 301;
      BitmapIndex = 1;

      // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
      // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
      // the object returned by the Content property.
      mUC = new AssemblyUC();
      Content = mUC;
    }
  }

  public partial class AssemblyUC : DebuggerUC {
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
      string xLabelPrefix = null;

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
            if (xLabelPrefix == null) {
              var xLabelParts = xDisplayLine.Split('.');
              xLabelPrefix = xLabelParts[0] + ".";
            }
          } else {
            xDisplayLine = "\t" + xDisplayLine;
          }

          // Replace all and not just labels so we get jumps, calls etc
          if (xLabelPrefix != null) {
            xDisplayLine = xDisplayLine.Replace(xLabelPrefix, "");
          }
        }

        if (xTestParts[0].EndsWith(":")) {
          xBrush = Brushes.Black;
        } else if (xTestLine.StartsWith(";")) {
          xBrush = Brushes.Green;
        }

        // Even though our code is often the source of the tab, it makes
        // more sense to do it this was because the number of space stays
        // in one place and also lets us differentiate from natural spaces.
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