using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;

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
  public class AssemblyTW : ToolWindowPane2 {
    public AssemblyTW() {
      //ToolBar = new CommandID(GuidList.guidAsmToolbarCmdSet, (int)PkgCmdIDList.AsmToolbar);
      Caption = "Cosmos Assembly";

      // Set the image that will appear on the tab of the window frame
      // when docked with an other window.
      // The resource ID correspond to the one defined in the resx file
      // while the Index is the offset in the bitmap strip. Each image in
      // the strip being 16x16.
      BitmapResourceID = 301;
      BitmapIndex = 1;

      // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
      // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
      // the object returned by the Content property.
      mUserControl = new AssemblyUC();
      Content = mUserControl;
    }
  }

  public partial class AssemblyUC : DebuggerUC {
    protected List<AsmLine> mLines = new List<AsmLine>();
    // Text of code as rendered. Used for clipboard etc.
    protected StringBuilder mCode = new StringBuilder();
    protected bool mFilter = true;

    public AssemblyUC() {
      InitializeComponent();

      mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      butnFilter.Click += new RoutedEventHandler(butnFilter_Click);
      butnCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      butnStep.Click += new RoutedEventHandler(butnStep_Click);

      Update(null, mData);
    }

    void butnStep_Click(object sender, RoutedEventArgs e) {
      //Global.PipeUp.SendCommand(Cosmos.Debug.Consts.DwCmd.AsmStep, null);
    }

    void butnFilter_Click(object sender, RoutedEventArgs e) {
      mFilter = !mFilter;
      Display(mFilter);
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
      string xLabelPrefix = null;

      foreach (var xLine in mLines) {
        string xDisplayLine = xLine.ToString();
        if (aFilter) {
          if (xLine is AsmLabel) {
            var xAsmLabel = (AsmLabel)xLine;
            xDisplayLine = xAsmLabel.Label + ":";

            // Skip ASM labels
            if (xAsmLabel.Comment.ToUpper() == "ASM") {
              continue;
            }

            // Insert a blank line before labels, but not if its the top line
            if (tblkSource.Inlines.Count > 0) {
              tblkSource.Inlines.Add(new LineBreak());
              mCode.AppendLine();
            }

            if (xLabelPrefix == null) {
              var xLabelParts = xAsmLabel.Label.Split('.');
              xLabelPrefix = xLabelParts[0] + ".";
            }
          } else {
            xDisplayLine = "\t" + xLine.ToString();
          }

          // Replace all and not just labels so we get jumps, calls etc
          if (xLabelPrefix != null) {
            xDisplayLine = xDisplayLine.Replace(xLabelPrefix, "");
          }
        }

        // Even though our code is often the source of the tab, it makes
        // more sense to do it this was because the number of space stays
        // in one place and also lets us differentiate from natural spaces.
        xDisplayLine = xDisplayLine.Replace("\t", "    ");

        var xRun = new Run(xDisplayLine);
        xRun.FontFamily = xFont;

        // Set colour of line
        if (xLine is AsmLabel) {
          xRun.Foreground = Brushes.Black;
        } else if (xLine is AsmComment) {
          xRun.Foreground = Brushes.Green;
        } else {
          xRun.Foreground = Brushes.Blue;
        }
        
        tblkSource.Inlines.Add(xRun);
        tblkSource.Inlines.Add(new LineBreak());

        mCode.AppendLine(xDisplayLine);
      }
    }

    protected override void DoUpdate(string aTag) {
      string xCode = Encoding.UTF8.GetString(mData);

      // Should always be \r\n, but just in case we split by \n and ignore \r
      string[] xLines = xCode.Replace("\r", "").Split('\n');
      mLines.Clear();
      foreach (string xLineText in xLines) {
        string xLine = xLineText.Trim();
        string xTestLine = xLine.ToUpper();
        var xParts = xLine.Split(' ');

        // Skip certain items we never care about.
        // ie remove noise
        if (xTestLine == "INT3") {
          continue;
        } else if (xLine.Length == 0) {
          // Remove all empty lines because we completely reformat output.
          // Parse below also expects some data, not empty string.
          continue;
        }

        if (xParts[0].EndsWith(":")) {
          string xLabel = xParts[0].Substring(0, xParts[0].Length - 1);
          string xComment = "";
          if (xParts.Length > 1) {
            xComment = xParts[1].Substring(1).Trim();
          }
          mLines.Add(new AsmLabel(xLabel, xComment));
        } else if (xTestLine.StartsWith(";")) {
          string xComment = xLine.Trim().Substring(1).Trim();
          mLines.Add(new AsmComment(xComment));
        } else {
          mLines.Add(new AsmCode(xLine));
        }
      }

      Display(mFilter);
    }

  }
}