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
using Cosmos.Compiler.Debug;

namespace Cosmos.Compiler.Builder {
    public partial class DebugWindow : Window {
        protected class TraceItem {
            public UInt32 EIP { get; set; }
            public string SourceFile { get; set; }
            public MsgType Type { get; set; }
        }
        
        protected DebugMode mDebugMode;
        protected SourceInfos mSourceMapping;
        protected List<Run> mLines = new List<Run>();
        protected FontFamily mFont = new FontFamily("Courier New");
        protected bool mTracing = true;
        protected RoutedCommand mStepCommand;
        protected DebugConnector mDebugConnector;
        protected ObservableCollection<TraceItem> mTraceLog = new ObservableCollection<TraceItem>();

        // This and other status items are here and not in DebugConnector
        // so we will always be in sync with the UI. Since notificaitons
        // are invoked, the DebugConnector could have a state different than what
        // the UI is showing if it is still catching up
        protected bool mAtBreakPoint = false;
        protected bool mIsConnected = true;
        
        protected void UpdateCaptions() {
            butnTrace.Content = "Trace " + (mTracing ? "Off" : "On");
            butnBreak.Content = mAtBreakPoint ? "Continue" : "Break";
        }

        public DebugWindow() {
            InitializeComponent();

            mStepCommand = new RoutedCommand();
            UpdateCaptions();
            
            // Managing state takes some work. We have to introduce new responses from the DebugStub
            // or add a byte to all trace returns
            butnTrace.Visibility = Visibility.Hidden;

            listLog.ItemsSource = mTraceLog;
            listLog.SelectionChanged += new SelectionChangedEventHandler(listLog_SelectionChanged);
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
                // When using Callstack, VS uses GreenYellow
                // Yellow for stepping
                // Red for breakpoints
                xRunSelected.Background = Brushes.GreenYellow;
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
            mDebugMode = DebugMode.Source;
            mSourceMapping = aSourceMapping;
            
            mDebugConnector = aDebugConnector;
            mDebugConnector.ConnectionLost += ConnectionLost;
            mDebugConnector.CmdTrace += CmdTrace;
            mDebugConnector.CmdText += CmdText;
        }

        protected void CmdText(string aText) {
            var xAction = (Action)delegate() {
                Log(MsgType.Message, aText);
            };
            Dispatcher.BeginInvoke(xAction);
        }
        
        protected void CmdTrace(MsgType aMsgType, UInt32 aEIP) {
            var xAction = (Action)delegate() {
                var xSourceInfo = mSourceMapping.GetMapping(aEIP);
                var xTraceItem = new TraceItem() {
                    EIP = aEIP
                    , Type = aMsgType
                };
                // Should not be null, but is possible with some plugs
                if (xSourceInfo != null) {
                    // Dont show path or extension, reducing widhth is important
                    var xFileInfo = new FileInfo(xSourceInfo.SourceFile);
                    xTraceItem.SourceFile = xFileInfo.Name.Substring(0
                        , xFileInfo.Name.Length - xFileInfo.Extension.Length);
                }
                
                mTraceLog.Add(xTraceItem);
                mAtBreakPoint = aMsgType == MsgType.BreakPoint;
                if (mAtBreakPoint | (aMsgType == MsgType.Error)) {
                    listLog.SelectedIndex = listLog.Items.Count - 1;
                    listLog.ScrollIntoView(listLog.SelectedItem);
                }
                UpdateCaptions();
            };
            Dispatcher.BeginInvoke(xAction);
        }
        
        protected void Log(MsgType aType, string aMsg) {
            var xTraceItem = new TraceItem() {
                Type = aType
                , SourceFile = aMsg
            };
            mTraceLog.Add(xTraceItem);
        }

        protected void ConnectionLost(Exception ex) {
            var xAction = (Action)delegate() {
                Log(MsgType.Error, "Connection to Cosmos lost.");
                mIsConnected = false;
            };
            Dispatcher.BeginInvoke(xAction);
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

        private void listLog_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var xItem = listLog.SelectedItem as TraceItem;
            if (xItem != null) {
                if (xItem.EIP > 0) {
                    if (mDebugMode == DebugMode.Source) {
                        SelectCode(xItem.EIP);
                    } else {
                        throw new Exception("Current debug mode is not supported.");
                    }
                }
            }
        }

        private void ExecuteStepCommand(object sender, ExecutedRoutedEventArgs e) {
             mDebugConnector.SendCommand((int)Command.Step);
        }

        private void ExecuteTestCommand(object sender, ExecutedRoutedEventArgs e) {

        }

        private void ExecuteTraceCommand(object sender, ExecutedRoutedEventArgs e) {
            mDebugConnector.SendCommand((byte)(mTracing ? 1 : 2));
            mTracing = !mTracing;
            UpdateCaptions();
        }

        private void ExecuteBreakCommand(object sender, ExecutedRoutedEventArgs e) {
            mDebugConnector.SendCommand((int)Command.Break);
            // Only reset, dont set. Set is done by the break notification
            if (mAtBreakPoint) {
                mAtBreakPoint = false;
                UpdateCaptions();
            }
        }

        private void ExecuteClearCommand(object sender, ExecutedRoutedEventArgs e) {
            mTraceLog.Clear();
        }

        private void StepCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mIsConnected;
        }

        private void TraceCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mIsConnected;
        }

        private void BreakCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mIsConnected;
        }

        private void ClearCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (listLog == null) {
                e.CanExecute = false;
            } else {
                e.CanExecute = listLog.Items.Count > 0;
            }
        }
        
    }

}