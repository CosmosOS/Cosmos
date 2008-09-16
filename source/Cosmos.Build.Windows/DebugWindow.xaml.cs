using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using Indy.IL2CPU;
using Cosmos.IL2CPU.Debug;

namespace Cosmos.Build.Windows {
    public partial class DebugWindow : Window {
        protected enum TraceItemType {Trace, Message, Error}
    
        protected class TraceItem {
            public UInt32 EIP { get; set; }
            public string SourceFile { get; set; }
            public TraceItemType Type { get; set; }
        }
    
        protected DebugModeEnum mDebugMode;
        protected SourceInfos mSourceMapping;
        protected List<Run> mLines = new List<Run>();
        protected FontFamily mFont = new FontFamily("Courier New");
        protected bool mAutoDisplay = false;
        protected bool mTracing = true;
        protected bool mBreak = false;
        protected RoutedCommand mStepCommand;
        protected DebugConnector mDebugConnector;
        protected ObservableCollection<TraceItem> mTraceLog = new ObservableCollection<TraceItem>();

        protected void UpdateCaptions() {
            butnTrace.Content = "Trace " + (mTracing ? "Off" : "On");
            butnBreak.Content = mBreak ? "Continue" : "Break";
        }

        public DebugWindow() {
            InitializeComponent();

            mStepCommand = new RoutedCommand();
            UpdateCaptions();

            listLog.ItemsSource = mTraceLog;
            listLog.SelectionChanged += new SelectionChangedEventHandler(lboxLog_SelectionChanged);

            butnTrace.Click += new RoutedEventHandler(butnTrace_Click);
            butnTest.Click += new RoutedEventHandler(butnTest_Click);
            butnStep.Click += new RoutedEventHandler(butnStep_Click);
            butnBreak.Click += new RoutedEventHandler(butnBreak_Click);
            butnLogClear.Click += new RoutedEventHandler(butnLogClear_Click);
        }

        private void butnTrace_Click(object sender, RoutedEventArgs e) {
            mDebugConnector.SendCommand((byte)(mTracing ? 1 : 2));
            mTracing = !mTracing;
            UpdateCaptions();
        }

        private void butnLogClear_Click(object sender, RoutedEventArgs e) {
            listLog.Items.Clear();
        }

        private void butnBreak_Click(object sender, RoutedEventArgs e) {
            mDebugConnector.SendCommand(3);
            mBreak = !mBreak;
            UpdateCaptions();
            if (mBreak) {
                mAutoDisplay = true;
            }
        }

        private void butnStep_Click(object sender, RoutedEventArgs e) {
            mDebugConnector.SendCommand(4);
            mAutoDisplay = true;
        }

        private void butnTest_Click(object sender, RoutedEventArgs e) {
        }

        public void LoadSourceFile(string aPathname) {
            var xSourceCode = System.IO.File.ReadAllLines(aPathname);
            lablSourceFilename.Content = System.IO.Path.GetFileName(aPathname);
            var xPara = new Paragraph();
            mLines.Clear();
            fdsvSource.Document = new FlowDocument();
            fdsvSource.Document.PageWidth = 100 * 96;
            fdsvSource.Document.Blocks.Add(xPara);
            int xLineNo = 0;
            foreach (var xLine in xSourceCode) {
                Run xRun;

                xLineNo++;
                xRun = new Run(xLineNo.ToString().PadLeft(6) + " ");
                xRun.MouseDown += new MouseButtonEventHandler(xRun_MouseDown);
                xRun.FontFamily = mFont;
                xRun.Cursor = Cursors.Arrow;
                xRun.Background = Brushes.LightGray;
                xPara.Inlines.Add(xRun);

                xRun = new Run(xLine);
                xRun.FontFamily = mFont;
                mLines.Add(xRun);
                xPara.Inlines.Add(xRun);

                xPara.Inlines.Add(new LineBreak());
            }
        }

        private void xRun_MouseDown(object sender, MouseButtonEventArgs e) {
            var xRun = (Run)sender;
            xRun.Background = (xRun.Background == Brushes.Red) ? Brushes.LightGray : Brushes.Red;
        }

        protected Run Select(int aLine, int aColBegin, int aLength) {
            Run xRunSelected = null;
            if (aLength != 0) {
                var xPara = (Paragraph)fdsvSource.Document.Blocks.FirstBlock;
                var xSelectedLine = mLines[aLine];
                string xText = xSelectedLine.Text;
                // aLength = -1 means to end
                if (aLength == -1) {
                    aLength = xText.Length - aColBegin;
                }
                // If not begin at col 0, then we need to leave some deselected area
                if (aColBegin > 0) {
                    var xRunLeft = new Run(xText.Substring(0, aColBegin));
                    xRunLeft.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunLeft);
                }
                // Selected portion
                xRunSelected = new Run(xText.Substring(aColBegin, aLength));
                xRunSelected.FontFamily = mFont;
                xRunSelected.Background = Brushes.Red;
                xPara.Inlines.InsertBefore(xSelectedLine, xRunSelected);
                // Unselected on right if there is some
                if (aColBegin + aLength < xText.Length) {
                    var xRunRight = new Run(xText.Substring(aColBegin + aLength));
                    xRunRight.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunRight);
                }
                // Remove the old line
                xPara.Inlines.Remove(xSelectedLine);
            }
            return xRunSelected;
        }

        public void SelectText(int aLineBegin, int aColBegin, int aLineEnd, int aColEnd) {
            aLineBegin--;
            aColBegin--;
            aLineEnd--;
            aColEnd--;

            Run xRunSelected;
            // Its all on one line
            if (aLineBegin == aLineEnd) {
                xRunSelected = Select(aLineBegin, aColBegin, aColEnd - aColBegin);
            // Its split across multiple lines so we need to select multiple lines
            } else {
                xRunSelected = Select(aLineBegin, aColBegin, -1);
                for (int i = aLineBegin + 1; i <= aLineEnd - 1; i++) {
                    Select(i, 0, -1);
                }
                Select(aLineEnd, 0, aColEnd + 1);
            }
            // Scroll it into view
            fdsvSource.UpdateLayout();
            if (xRunSelected != null) {
                xRunSelected.PreviousInline.BringIntoView();
            }
        }

        public void SetSourceInfoMap(SourceInfos aSourceMapping, DebugConnector aDebugConnector) {
            mDebugMode = DebugModeEnum.Source;
            mSourceMapping = aSourceMapping;
            
            mDebugConnector = aDebugConnector;
            mDebugConnector.Dispatcher = Dispatcher;
            mDebugConnector.ConnectionLost += ConnectionLost;
            mDebugConnector.CmdTrace += CmdTrace;
        }

        protected void CmdTrace(UInt32 aEIP) {
            var xSourceInfo = mSourceMapping.GetMapping(aEIP);
            var xTraceItem = new TraceItem() {
                EIP = aEIP
                , Type = TraceItemType.Trace
            };
            // Dont show path or extension, reducing widhth is important
            var xFileInfo = new FileInfo(xSourceInfo.SourceFile);
            xTraceItem.SourceFile = xFileInfo.Name.Substring(0, xFileInfo.Name.Length - xFileInfo.Extension.Length);
            
            mTraceLog.Add(xTraceItem);
            if (mAutoDisplay) {
                try {
                    listLog.SelectedIndex = listLog.Items.Count - 1;
                } catch { }
                mAutoDisplay = false;
            }
        }
        
        protected void Log(TraceItemType aType, string aMsg) {
            var xTraceItem = new TraceItem() {
                Type = aType
                , SourceFile = aMsg
            };
            mTraceLog.Add(xTraceItem);
        }

        protected void ConnectionLost(Exception ex) {
            Title = "No debug connection.";
            listLog.Background = Brushes.Red;
            while (ex != null) {
                Log(TraceItemType.Error, ex.Message);
                ex = ex.InnerException;
            }
        }

        //protected void Anaylyze() {
        //    List<string> xItems = new List<string>();
        //    for (int i = lboxLog.Items.Count - 1; i >= 0; i--) {
        //        string xEIP = lboxLog.Items[i] as string;
        //        if (xItems.Contains(xEIP, StringComparer.InvariantCultureIgnoreCase)) {
        //            lboxLog.Items.RemoveAt(i);
        //            continue;
        //        }
        //        var xSourceInfo = mSourceMapping.GetMapping(UInt32.Parse(xEIP.Substring(2),
        //                                                                  System.Globalization.NumberStyles.HexNumber));
        //        if (xSourceInfo == null) {
        //            lboxLog.Items.RemoveAt(i);
        //            continue;
        //        }
        //    }
        //    //foreach (var xEIP in (from item in lboxLog.Items.Cast<string>()
        //    //                      select item).Distinct(StringComparer.InvariantCultureIgnoreCase)) {

        //    //    //var xViewSrc = new ViewSourceWindow();
        //    //    //int xCharStart;
        //    //    //int xCharCount;
        //    //    //GetLineInfo(xViewSrc.tboxSource.Text, xSourceInfo.Line, xSourceInfo.Column, xSourceInfo.LineEnd, xSourceInfo.ColumnEnd, out xCharStart, out xCharCount);
        //    //    //if(
        //    //    int xCharStart = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.Line);
        //    //    int xCharEnd = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.LineEnd);
        //    //    if ((xCharEnd - xCharStart) > 4) {
        //    //        MessageBox.Show(xEIP);
        //    //        return;
        //    //    }
        //    //}
        //    MessageBox.Show("Analysis finished!");
        //}

        protected void SelectCode(uint aEIP) {
            var x = mSourceMapping.GetMapping(aEIP);
            if (x != null) {
                LoadSourceFile(x.SourceFile);
                SelectText(x.Line, x.Column, x.LineEnd, x.ColumnEnd);
            }
        }

        private void lboxLog_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var xItem = listLog.SelectedItem as TraceItem;
            if (xItem != null) {
                if (mDebugMode == DebugModeEnum.Source) {
                    SelectCode(xItem.EIP);
                } else {
                    throw new Exception("Current debug mode is not supported.");
                }
            }
        }
    }

}