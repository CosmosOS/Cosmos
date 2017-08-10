using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using Cosmos.Debug.Common;
using System.Windows.Threading;
using System.Windows.Shapes;
using Cosmos.Debug.DebugConnectors;

namespace Cosmos.VS.Windows
{
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.

    [Guid("f019fb29-c2c2-4d27-9abf-739533c939be")]
    public class AssemblyTW : ToolWindowPane2
    {
        public AssemblyTW()
        {
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

    public partial class AssemblyUC : DebuggerUC
    {
        protected List<AsmLine> mLines = new List<AsmLine>();
        protected Dictionary<Run, AsmLine> mRunsToLines = new Dictionary<Run, AsmLine>();
        protected Dictionary<Rectangle, Run> mGutterRectsToRun = new Dictionary<Rectangle, Run>();
        protected Dictionary<Rectangle, AsmCode> mGutterRectsToCode = new Dictionary<Rectangle, AsmCode>();
        // Text of code as rendered. Used for clipboard etc.
        protected StringBuilder mCode = new StringBuilder();
        protected bool mFilter = true;
        protected string mCurrentLabel;
        protected string[] mParams;
        protected string[] mFilterLabels;
        protected List<string> mASMBPs = new List<string>();

        public AssemblyUC()
        {
            InitializeComponent();

            mitmCopy.Click += new RoutedEventHandler(mitmCopy_Click);
            butnFilter.Click += new RoutedEventHandler(butnFilter_Click);
            butnCopy.Click += new RoutedEventHandler(mitmCopy_Click);
            butnStepOver.Click += new RoutedEventHandler(butnStepOver_Click);
            butnStepInto.Click += new RoutedEventHandler(butnStepInto_Click);
            butnStepMode.Click += new RoutedEventHandler(butnStepMode_Click);

            Update(null, mData);
        }

        void butnStepMode_Click(object sender, RoutedEventArgs e)
        {
            if(butnStepMode.BorderBrush == Brushes.Black)
            {
                butnStepMode.BorderBrush = Brushes.LightBlue;
                Global.PipeUp.SendCommand(Windows2Debugger.SetStepModeSource);
                butnStepMode.Content = "Step mode: Source";
            }
            else
            {
                butnStepMode.BorderBrush = Brushes.Black;
                Global.PipeUp.SendCommand(Windows2Debugger.SetStepModeAssembler);
                butnStepMode.Content = "Step mode: Assembler";
            }
        }

        void butnStepInto_Click(object sender, RoutedEventArgs e)
        {
            // Disable until step is done to prevent user concurrently clicking.
            //butnStepOver.IsEnabled = false;
            //butnStepInto.IsEnabled = false;
            Global.PipeUp.SendCommand(Windows2Debugger.AsmStepInto);
            Display(mFilter);
        }
        void butnStepOver_Click(object sender, RoutedEventArgs e)
        {
            // Disable until step is done to prevent user concurrently clicking.
            //butnStepOver.IsEnabled = false;
            //butnStepInto.IsEnabled = false;
            Global.PipeUp.SendCommand(Windows2Debugger.Continue);
        }

        protected bool canStepOver = false;

        void butnFilter_Click(object sender, RoutedEventArgs e)
        {
            mFilter = !mFilter;
            Display(mFilter);
        }

        void mitmCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mCode.ToString());
        }

        protected Run mSelectedCodeRun = null;
        protected void Display(bool aFilter)
        {
            Log("Display({0})", aFilter);
            Log("Current Label '{0}'", mCurrentLabel);
            mCode.Clear();
            tblkSource.Inlines.Clear();
            mRunsToLines.Clear();
            Log("Display - Done clearing");
            if (mData.Length == 0)
            {
                return;
            }

            int nextCodeDistFromCurrent = 0;
            bool foundCurrentLine = false;

            var xFont = new FontFamily("Consolas");
            //We need multiple prefix filters because method header has different prefix to IL labels.
            //We use:
            // - First "Method_" label prefix
            // - First label without "GUID_" or "METHOD_" on it as that will be the name of the current method
            List<string> xLabelPrefixes = new List<string>();
            bool foundMETHOD_Prefix = false;
            bool foundMethodName = false;
            int mCurrentLineNumber = 0;
            Log("Display - Processing lines");
            foreach (var xLine in mLines)
            {
                string xDisplayLine = xLine.ToString();

                if (aFilter)
                {
                    if (xLine is AsmLabel)
                    {
                        var xAsmLabel = (AsmLabel)xLine;
                        xDisplayLine = xAsmLabel.Label + ":";

                        // Skip ASM labels
                        if (xAsmLabel.Comment.ToUpper() == "ASM")
                        {
                            continue;
                        }

                        if (!foundMETHOD_Prefix && xAsmLabel.Label.StartsWith("METHOD_"))
                        {
                            var xLabelParts = xAsmLabel.Label.Split('.');
                            xLabelPrefixes.Add(xLabelParts[0] + ".");
                            foundMETHOD_Prefix = true;
                        }
                        else if(!foundMethodName && !xAsmLabel.Label.StartsWith("METHOD_")
                                                 && !xAsmLabel.Label.StartsWith("GUID_"))
                        {
                            var xLabelParts = xAsmLabel.Label.Split(':');
                            xLabelPrefixes.Add(xLabelParts[0] + ".");
                            foundMethodName = true;
                        }
                    }
                    else
                    {
                        xDisplayLine = xLine.ToString();
                    }

                    // Replace all and not just labels so we get jumps, calls etc
                    foreach(string xLabelPrefix in xLabelPrefixes)
                    {
                        xDisplayLine = xDisplayLine.Replace(xLabelPrefix, "");
                    }
                }

                if (xLine is AsmLabel)
                {
                    // Insert a blank line before labels, but not if its the top line
                    if (tblkSource.Inlines.Count > 0)
                    {
                        tblkSource.Inlines.Add(new LineBreak());
                        if (!foundCurrentLine)
                        {
                            mCurrentLineNumber++;
                        }

                        mCode.AppendLine();
                    }
                }
                else
                {
                    xDisplayLine = "\t" + xDisplayLine;
                }

                // Even though our code is often the source of the tab, it makes
                // more sense to do it this was because the number of space stays
                // in one place and also lets us differentiate from natural spaces.
                xDisplayLine = xDisplayLine.Replace("\t", "  ");

                var xRun = new Run(xDisplayLine);
                xRun.FontFamily = xFont;
                mRunsToLines.Add(xRun, xLine);

                var gutterRect = new Rectangle()
                {
                    Width = 11,
                    Height = 11,
                    Fill = Brushes.WhiteSmoke
                };
                tblkSource.Inlines.Add(gutterRect);

                // Set colour of line
                if (xLine is AsmLabel)
                {
                    xRun.Foreground = Brushes.Black;
                }
                else if (xLine is AsmComment)
                {
                    xRun.Foreground = Brushes.Green;
                }
                else if (xLine is AsmCode)
                {
                    var xAsmCode = (AsmCode)xLine;

                    gutterRect.MouseUp += gutterRect_MouseUp;
                    gutterRect.Fill = Brushes.LightGray;
                    mGutterRectsToCode.Add(gutterRect, xAsmCode);
                    mGutterRectsToRun.Add(gutterRect, xRun);
                    Log("Current AsmCodeLabel: '{0}'", xAsmCode.AsmLabel);
                    if (xAsmCode.LabelMatches(mCurrentLabel))
                    {
                        xRun.Foreground = Brushes.WhiteSmoke;
                        xRun.Background = Brushes.DarkRed;

                        Package.StateStorer.CurrLineId = GetLineId(xAsmCode);
                        Package.StoreAllStates();

                        foundCurrentLine = true;
                        nextCodeDistFromCurrent = 0;
                    }
                    else
                    {
                        if (foundCurrentLine)
                        {
                            nextCodeDistFromCurrent++;
                        }

                        if(mASMBPs.Contains(GetLineId(xAsmCode)))
                        {
                            xRun.Background = Brushes.MediumVioletRed;
                        }
                        else if(Package.StateStorer.ContainsStatesForLine(GetLineId(xAsmCode)))
                        {
                            xRun.Background = Brushes.LightYellow;
                        }

                        xRun.Foreground = Brushes.Blue;
                    }

                    xRun.MouseUp += OnASMCodeTextMouseUp;

                }
                else
                { // Unknown type
                    xRun.Foreground = Brushes.HotPink;
                }

                if (!foundCurrentLine)
                {
                    mCurrentLineNumber++;
                }
                tblkSource.Inlines.Add(xRun);
                tblkSource.Inlines.Add(new LineBreak());

                mCode.AppendLine(xDisplayLine);
            }
            Log("Display - Done processing lines");
            //EdMan196: This line of code was worked out by trial and error.
            double offset = mCurrentLineNumber * 13.1;
            Log("Display - Scroll to offset");
            ASMScrollViewer.ScrollToVerticalOffset(offset);
        }

        private void gutterRect_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var rect = (Rectangle)e.OriginalSource;

            //Double clicked so don't select it, set it as an ASM BP
            var line = mGutterRectsToCode[rect];
            var xRun = mGutterRectsToRun[rect];
            //Search for associated label
            Global.PipeUp.SendCommand(Windows2Debugger.ToggleAsmBreak2, Encoding.UTF8.GetBytes(((AsmCode)line).AsmLabel.Label));

            string lineId = GetLineId((AsmCode)line);
            if (mASMBPs.Contains(lineId))
            {
                if (line.LabelMatches(mCurrentLabel))
                {
                    xRun.Foreground = Brushes.WhiteSmoke;
                    xRun.Background = Brushes.DarkRed;
                }
                else if (Package.StateStorer.ContainsStatesForLine(GetLineId((AsmCode)mRunsToLines[xRun])))
                {
                    xRun.Foreground = Brushes.Blue;
                    xRun.Background = Brushes.LightYellow;
                }
                else
                {
                    xRun.Foreground = Brushes.Blue;
                    xRun.Background = Brushes.WhiteSmoke;
                }

                rect.Fill = Brushes.LightGray;
                mASMBPs.Remove(lineId);
            }
            else
            {
                if (!line.LabelMatches(mCurrentLabel))
                {
                    xRun.Background = Brushes.MediumVioletRed;
                }

                rect.Fill = Brushes.MediumVioletRed;
                mASMBPs.Add(lineId);
            }
        }

        protected void OnASMCodeTextMouseUp(object aSender, System.Windows.Input.MouseButtonEventArgs aArgs)
        {
            try
            {
                // Reset colours for previously selected item.
                if (mSelectedCodeRun != null)
                {
                    mSelectedCodeRun.Foreground = Brushes.Blue;

                    try
                    {
                        if (mASMBPs.Contains(GetLineId((AsmCode)mRunsToLines[mSelectedCodeRun])))
                        {
                            mSelectedCodeRun.Background = Brushes.MediumVioletRed;
                        }
                        else if (Package.StateStorer.ContainsStatesForLine(GetLineId((AsmCode)mRunsToLines[mSelectedCodeRun])))
                        {
                            mSelectedCodeRun.Background = Brushes.LightYellow;
                        }
                        else
                        {
                            mSelectedCodeRun.Background = null;
                        }
                    }
                    catch
                    {
                    }

                    mSelectedCodeRun = null;
                }

                Run xRun = (Run)aArgs.OriginalSource;
                // Highlight new selection, if not the current break.
                if (xRun.Background != Brushes.DarkRed)
                {
                    mSelectedCodeRun = xRun;
                    xRun.Foreground = Brushes.WhiteSmoke;
                    xRun.Background = Brushes.Blue;
                }

                //Show state for that line
                //IL Labels should be unique for any given section
                var asmLine = mRunsToLines[xRun];
                if (Package != null)
                {
                    if(Package.StateStorer.ContainsStatesForLine(GetLineId((AsmCode)asmLine)))
                    {
                        Package.StoreAllStates();
                        Package.StateStorer.CurrLineId = GetLineId((AsmCode)asmLine);
                        Package.RestoreAllStates();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in AssemblyUC.cs:OnASMTextMouseUp. Message: \r\n" + ex.Message);
            }
        }

        private string GetLineId(AsmCode asmLine)
        {
            int index = mLines.IndexOf(asmLine);
            int distFromLabel = 0;
            string lineId = "";
            for (; index > -1; index--)
            {
                if (mLines[index] is AsmLabel)
                {
                    lineId = mLines[index].ToString();
                    break;
                }
                else
                {
                    distFromLabel++;
                }
            }

            lineId += "_" + distFromLabel.ToString();

            return lineId;
        }

        //protected void Parse()
        //{
        //    string xCode = Encoding.UTF8.GetString(mData);
        //    // Should always be \r\n, but just in case we split by \n and ignore \r
        //    string[] xLines = xCode.Replace("\r", "").Split('\n');

        //    if (xLines.Length >= 3)
        //    {
        //        // First line is not code, but the current label and inserted by caller
        //        // Second line is input parameters
        //        mCurrentLabel = xLines[0];
        //        mParams = xLines[1].Split('|');
        //        mFilterLabels = xLines[2].Split('|');
        //        bool xSetNextLabelToCurrent = false;

        //        int nextCodeDistFromCurrent = 0;
        //        bool foundCurrentLine = false;

        //        AsmLabel xLastAsmAsmLabel = null;
        //        for (int i = 3; i < xLines.Length; i++)
        //        {
        //            string xLine = xLines[i].Trim();
        //            string xTestLine = xLine.ToUpper();
        //            var xParts = xLine.Split(' ');

        //            // Skip certain items we never care about. ie remove noise
        //            if (xLine.Length == 0)
        //            {
        //                // Remove all empty lines because we completely reformat output.
        //                // Parse below also expects some data, not empty string.
        //                continue;
        //            }

        //            if (xParts[0].EndsWith(":"))
        //            { // Label
        //                string xLabel = xParts[0].Substring(0, xParts[0].Length - 1);
        //                var xAsmLabel = new AsmLabel(xLabel);
        //                // See if the label has a comment/tag
        //                if (xParts.Length > 1)
        //                {
        //                    xAsmLabel.Comment = xParts[1].Substring(1).Trim();
        //                    // If its an ASM tag, store it for future use to attach to next AsmCode
        //                    if (xAsmLabel.Comment.ToUpper() == "ASM")
        //                    {
        //                        xLastAsmAsmLabel = xAsmLabel;
        //                    }
        //                }
        //                mLines.Add(xAsmLabel);

        //            }
        //            else if (xTestLine.StartsWith(";"))
        //            { // Comment
        //                string xComment = xLine.Trim().Substring(1).Trim();
        //                mLines.Add(new AsmComment(xComment));
        //            }
        //            else
        //            { // Code
        //                var xAsmCode = new AsmCode(xLine);
        //                xAsmCode.AsmLabel = xLastAsmAsmLabel;
        //                xLastAsmAsmLabel = null;

        //                if (xSetNextLabelToCurrent)
        //                {
        //                    mCurrentLabel = xAsmCode.AsmLabel.Label;
        //                    xSetNextLabelToCurrent = false;
        //                }

        //                //If its Int3 or so, we need to set the current label to the next non debug op.
        //                //And we don't want to add debug code to our parsed stuff - it shouldn't be displayed.
        //                if (xAsmCode.IsDebugCode)
        //                {
        //                    bool skip = true;
        //                    if (xAsmCode.LabelMatches(mCurrentLabel))
        //                    {
        //                        xSetNextLabelToCurrent = true;
        //                    }
        //                    else
        //                    {
        //                        //Allow debug NOPs that have not been set to INT3 to be shown
        //                        if(xAsmCode.Text.Trim().ToUpper().StartsWith("NOP"))
        //                        {
        //                            if (!mFilterLabels.Contains(xAsmCode.AsmLabel.Label))
        //                            {
        //                                skip = false;
        //                            }
        //                        }
        //                    }
        //                    if (skip)
        //                    {
        //                        //Skip adding Debug Code (INT3s) to parsed list
        //                        continue;
        //                    }
        //                }

        //                //If we want to filter this label because it is a debug INT3/NOP but not a permanent INT3
        //                if (xAsmCode.AsmLabel != null && mFilterLabels.Contains(xAsmCode.AsmLabel.Label))
        //                {
        //                    if (xAsmCode.LabelMatches(mCurrentLabel))
        //                    {
        //                        xSetNextLabelToCurrent = true;
        //                    }
        //                    //Skip adding Debug Code (INT3s) to parsed list
        //                    continue;
        //                }

        //                if (foundCurrentLine)
        //                {
        //                    nextCodeDistFromCurrent++;
        //                }

        //                if (nextCodeDistFromCurrent == 1)
        //                {
        //                    Global.PipeUp.SendCommand(Windows2Debugger.NextASMLine1, xAsmCode.Text);
        //                    Global.PipeUp.SendCommand(Windows2Debugger.NextLabel1, xAsmCode.AsmLabel.Label);
        //                }
        //                else if (!foundCurrentLine && xAsmCode.LabelMatches(mCurrentLabel))
        //                {
        //                    //Current line of code found
        //                    Global.PipeUp.SendCommand(Windows2Debugger.CurrentASMLine, xAsmCode.Text);
        //                    Global.PipeUp.SendCommand(Windows2Debugger.NextASMLine1, new byte[0]);
        //                    foundCurrentLine = true;
        //                }

        //                mLines.Add(xAsmCode);
        //            }
        //        }
        //    }
        //}

        protected void Parse()
        {
            Log("Start parsing {0} bytes", mData.Length);
            string xCode = Encoding.UTF8.GetString(mData);
            // Should always be \r\n, but just in case we split by \n and ignore \r
            string[] xLines = xCode.Replace("\r", "").Split('\n');

            // First line of packet is not code, but the current label and inserted by caller.
            mCurrentLabel = xLines[0];
            bool xSetNextLabelToCurrent = false;

            AsmLabel xLastAsmAsmLabel = null;
            for (int i = 1; i < xLines.Length; i++)
            {
                string xLine = xLines[i].Trim();
                Log("Parsing line '{0}'", xLine);
                string xTestLine = xLine.ToUpper();
                var xParts = xLine.Split(' ');

                // Skip certain items we never care about. ie remove noise
                if (xLine.Length == 0)
                {
                    // Remove all empty lines because we completely reformat output.
                    // Parse below also expects some data, not empty string.
                    continue;
                }

                if (xParts[0].EndsWith(":"))
                { // Label
                    string xLabel = xParts[0].Substring(0, xParts[0].Length - 1);
                    var xAsmLabel = new AsmLabel(xLabel);
                    // See if the label has a comment/tag
                    if (xParts.Length > 1)
                    {
                        xAsmLabel.Comment = xParts[1].Substring(1).Trim();
                        // If its an ASM tag, store it for future use to attach to next AsmCode
                        if (xAsmLabel.Comment.ToUpper() == "ASM")
                        {
                            xLastAsmAsmLabel = xAsmLabel;
                        }
                    }
                    mLines.Add(xAsmLabel);

                }
                else if (xTestLine.StartsWith(";"))
                { // Comment
                    string xComment = xLine.Trim().Substring(1).Trim();
                    mLines.Add(new AsmComment(xComment));

                }
                else
                { // Code
                    var xAsmCode = new AsmCode(xLine);
                    xAsmCode.AsmLabel = xLastAsmAsmLabel;
                    xLastAsmAsmLabel = null;

                    if (xSetNextLabelToCurrent)
                    {
                        mCurrentLabel = xAsmCode.AsmLabel.Label;
                        xSetNextLabelToCurrent = false;
                    }

                    // If its Int3 or so, we need to set the current label to the next non debug op.
                    if (xAsmCode.IsDebugCode)
                    {
                        if (xAsmCode.LabelMatches(mCurrentLabel))
                        {
                            xSetNextLabelToCurrent = true;
                        }
                        continue;
                    }

                    mLines.Add(xAsmCode);
                }
            }
        }

        private static void Log(string message, params object[] args)
        {
            // this method doesn't do anything normally, but keep it for debugging
            //File.AppendAllText(@"c:\data\sources\AssemblyUC.log", DateTime.Now.ToString("HH:mm:ss.ffffff: ") + String.Format(message, args) + Environment.NewLine);
        }

        protected override void DoUpdate(string aTag)
        {
            Log("DoUpdate");
            mLines.Clear();

            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)delegate()
                {
                    if (mData.Length == 0)
                    {
                        Display(false);
                    }
                    else
                    {
                        // Used for creating a test file for Cosmos.VS.Windows.Test
                        /*if (false)
                        {
                            System.IO.File.WriteAllBytes(@"D:\source\Cosmos\source\Cosmos.VS.Windows.Test\SourceTest.bin", mData);
                        }/**/
                    }
                    Log("DoUpdate - Parse input");
                    Parse();
                    Log("DoUpdate - Done");
                    if (mParams != null && mParams.Length > 0 && mParams[0] == "NoDisplay")
                    {
                        //Don't call display
                    }
                    else
                    {
                        Log("DoUpdate - Displaying");
                        Display(mFilter);
                        Log("DoUpdate - Done");
                    }

                    if (mParams != null && mParams.Length > 1)
                    {
                        if (mParams[1] == "AsmStepMode")
                        {
                            butnStepMode.BorderBrush = Brushes.LightBlue;
                        }
                        else
                        {
                            butnStepMode.BorderBrush = Brushes.Black;
                        }
                    }
                }
            );
        }
    }
}
