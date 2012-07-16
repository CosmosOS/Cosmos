using System;
using System.Linq;
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
    protected string mCurrentLabel;

    public AssemblyUC() {
      InitializeComponent();

      mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      butnFilter.Click += new RoutedEventHandler(butnFilter_Click);
      butnCopy.Click += new RoutedEventHandler(mitmCopy_Click);
      butnStepOver.Click += new RoutedEventHandler(butnStepOver_Click);

      Update(null, mData);
    }

    void butnStepOver_Click(object sender, RoutedEventArgs e) {
      var xCodeLinesQry = from x in mLines
                       where x is AsmCode
                       select (AsmCode)x;
      // Get the next code entry that is not Int3.
      // TODO: Need to adjust to find after mCurrentLabel right now its hard coded as a test for the second item from the
      // beginning of the method.
      var xCodeLines = xCodeLinesQry.Where(q => !q.IsDebugCode).ToArray();
      if (xCodeLines.Length > 1) {
        var xCodeLine = xCodeLines[1];
        if (xCodeLine.AsmLabel != null) {
          Global.PipeUp.SendCommand(Cosmos.Debug.Consts.Windows2Debugger.SetAsmBreak, xCodeLine.AsmLabel.Label);
        }
      }
    }

    void butnFilter_Click(object sender, RoutedEventArgs e) {
      mFilter = !mFilter;
      Display(mFilter);
    }

    void mitmCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mCode.ToString());
    }

    protected void Display(bool aFilter) {
      mCode.Clear();
      tblkSource.Inlines.Clear();
      if (mData.Length == 0) {
        return;
      }

      var xFont = new FontFamily("Consolas");
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

            if (xLabelPrefix == null) {
              var xLabelParts = xAsmLabel.Label.Split('.');
              xLabelPrefix = xLabelParts[0] + ".";
            }
          } else {
            if (xLine is AsmCode) {
              var xAsmCode = (AsmCode)xLine;
              if (xAsmCode.IsDebugCode) {
                continue;
              }
            }
            xDisplayLine = xLine.ToString();
          }

          // Replace all and not just labels so we get jumps, calls etc
          if (xLabelPrefix != null) {
            xDisplayLine = xDisplayLine.Replace(xLabelPrefix, "");
          }
        }

        if (xLine is AsmLabel) {
          // Insert a blank line before labels, but not if its the top line
          if (tblkSource.Inlines.Count > 0) {
            tblkSource.Inlines.Add(new LineBreak());
            mCode.AppendLine();
          }
        } else {
          xDisplayLine = "\t" + xDisplayLine;
        }

        // Even though our code is often the source of the tab, it makes
        // more sense to do it this was because the number of space stays
        // in one place and also lets us differentiate from natural spaces.
        xDisplayLine = xDisplayLine.Replace("\t", "  ");

        var xRun = new Run(xDisplayLine);
        xRun.FontFamily = xFont;

        // Set colour of line
        if (xLine is AsmLabel) {
          xRun.Foreground = Brushes.Black;
        } else if (xLine is AsmComment) {
          xRun.Foreground = Brushes.Green;
        } else if (xLine is AsmCode) {
          var xAsmCode = (AsmCode)xLine;
          if (xAsmCode.LabelMatches(mCurrentLabel)) {
            xRun.Foreground = Brushes.WhiteSmoke;
            xRun.Background = Brushes.DarkRed;
          } else {
            xRun.Foreground = Brushes.Blue;
          }
        } else { // Unknown type
          xRun.Foreground = Brushes.HotPink;
        }
        
        tblkSource.Inlines.Add(xRun);
        tblkSource.Inlines.Add(new LineBreak());

        mCode.AppendLine(xDisplayLine);
      }
    }

    protected void Parse() {
      string xCode = Encoding.UTF8.GetString(mData);
      // Should always be \r\n, but just in case we split by \n and ignore \r
      string[] xLines = xCode.Replace("\r", "").Split('\n');

      mCurrentLabel = xLines[0];

      bool xSetNextLabelToCurrent = false;
      AsmLabel xLastAsmAsmLabel = null;
      for (int i = 1; i < xLines.Length; i++) {
        string xLine = xLines[i].Trim();
        string xTestLine = xLine.ToUpper();
        var xParts = xLine.Split(' ');

        // Skip certain items we never care about. ie remove noise
        if (xLine.Length == 0) {
          // Remove all empty lines because we completely reformat output.
          // Parse below also expects some data, not empty string.
          continue;
        }

        if (xParts[0].EndsWith(":")) { // Labels
          string xLabel = xParts[0].Substring(0, xParts[0].Length - 1);
          var xAsmLabel = new AsmLabel(xLabel);
          // See if the label has a comment/tag
          if (xParts.Length > 1) {
            xAsmLabel.Comment = xParts[1].Substring(1).Trim();
            // If its an ASM tag, store it for future use to attach to next AsmCode
            if (xAsmLabel.Comment.ToUpper() == "ASM") {
              xLastAsmAsmLabel = xAsmLabel;
            }
          }
          mLines.Add(xAsmLabel);

        } else if (xTestLine.StartsWith(";")) { // Comments
          string xComment = xLine.Trim().Substring(1).Trim();
          mLines.Add(new AsmComment(xComment));

        } else { // Codee
          var xAsmCode = new AsmCode(xLine);
          xAsmCode.AsmLabel = xLastAsmAsmLabel;
          xLastAsmAsmLabel = null;

          if (xSetNextLabelToCurrent) {
            mCurrentLabel = xAsmCode.AsmLabel.Label;
            xSetNextLabelToCurrent = false;
          }

          // If its Int3 or so, we need to set the label to the next non debug op
          if (xAsmCode.IsDebugCode) {
            if (xAsmCode.LabelMatches(mCurrentLabel)) {
              xSetNextLabelToCurrent = true;
            }
            continue;
          }
          
          mLines.Add(xAsmCode);
        }
      }
    }

    protected override void DoUpdate(string aTag) {
      mLines.Clear();
      if (mData.Length == 0) {
        Display(false);
        return;
      }

      // Used for creating a test file for Cosmos.VS.Windows.Test
      if (false) {
        System.IO.File.WriteAllBytes(@"D:\source\Cosmos\source2\VSIP\Cosmos.VS.Windows.Test\SourceTest.bin", mData);
      }

      Parse();
      Display(mFilter);
    }

  }
}