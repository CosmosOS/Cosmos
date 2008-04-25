using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using Indy.IL2CPU;
using System.IO;

namespace Cosmos.Build.Windows {
	public partial class DebugWindow: Window {
		protected TcpClient mTCPClient;
        protected NetworkStream mTCPStream;
		protected byte[] mTCPData = new byte[4];
		protected int mCurrentPos = 0;
		private DebugModeEnum mDebugMode;
		private SourceInfos mSourceMappings;
        protected List<Run> mLines = new List<Run>();
        protected FontFamily mFont = new FontFamily("Courier New");

		public DebugWindow() {
			InitializeComponent();
            lboxLog.SelectionChanged += new SelectionChangedEventHandler(lboxLog_SelectionChanged);
            butnTraceOff.Click += new RoutedEventHandler(butnTraceOff_Click);
            butnTraceOn.Click += new RoutedEventHandler(butnTraceOn_Click);
		}

        void butnTraceOn_Click(object sender, RoutedEventArgs e) {
            SendDebugCmd(1);
        }

        protected void SendDebugCmd(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mTCPStream.Write(xData, 0, xData.Length);
        }

        void butnTraceOff_Click(object sender, RoutedEventArgs e) {
            SendDebugCmd(2);
        }

        public void LoadSourceFile(string aPathname) {
            var xSourceCode = System.IO.File.ReadAllLines(aPathname);
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

        void xRun_MouseDown(object sender, MouseButtonEventArgs e) {
            var xRun = (Run)sender;
            if (xRun.Background == Brushes.Red) {
                xRun.Background = Brushes.LightGray;
            } else {
                xRun.Background = Brushes.Red;
            }
        }

        protected Run Select(int aLine, int aColBegin, int aLength) {
            Run xRunSelected = null;
            if (aLength != 0) {
                var xPara = (Paragraph)fdsvSource.Document.Blocks.FirstBlock;
                var xSelectedLine = mLines[aLine];
                string xText = xSelectedLine.Text;
                if (aLength == -1) {
                    aLength = xText.Length - aColBegin;
                }

                if (aColBegin > 0) {
                    var xRunLeft = new Run(xText.Substring(0, aColBegin - 1));
                    xRunLeft.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunLeft);
                }

                xRunSelected = new Run(xText.Substring(aColBegin, aLength));
                xRunSelected.FontFamily = mFont;
                xRunSelected.Background = Brushes.Red;
                xPara.Inlines.InsertBefore(xSelectedLine, xRunSelected);

                if (aColBegin + aLength < xText.Length) {
                    var xRunRight = new Run(xText.Substring(aColBegin + aLength));
                    xRunRight.FontFamily = mFont;
                    xPara.Inlines.InsertBefore(xSelectedLine, xRunRight);
                }

                xPara.Inlines.Remove(xSelectedLine);
            }
            return xRunSelected;
        }

        public void SelectText(int aLineBegin, int aColBegin, int aLineEnd, int aColEnd) {
            aLineBegin--;
            aColBegin--;
            aLineEnd--;
            aColEnd--;
            //Currently can only be called once - need to fix it to reset so it can be called multiple times
            Run xRunSelected;
            if (aLineBegin == aLineEnd) {
                xRunSelected = Select(aLineBegin, aColBegin, aColEnd - aColBegin);
            } else {
                xRunSelected = Select(aLineBegin, aColBegin, -1);
                for (int i = aLineBegin + 1; i <= aLineEnd - 1; i++) {
                    Select(i, 0, -1);
                }
                Select(aLineEnd, 0, aColEnd + 1);
            }
            xRunSelected.BringIntoView();
        }
        
        public void SetSourceInfoMap(SourceInfos aSourceMapping) {
			try {
				mDebugMode = DebugModeEnum.Source;
				mSourceMappings = aSourceMapping;
				//Create a TCP connection to localhost:4444. We have already set up Qemu to listen to this port.
				mTCPClient = new TcpClient();
                mTCPClient.Connect(new IPEndPoint(IPAddress.Loopback, 4444));

				//Read TCP data from Qemu
				mTCPStream = mTCPClient.GetStream();
                mTCPStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), mTCPStream);
			} catch (SocketException ex) {
				Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConnectionLostDelegate(ConnectionLost), ex);
			}
		}

		protected delegate void DebugPacketRcvdDelegate(UInt32 aEIP);
		protected void DebugPacketRcvd(UInt32 aEIP) {
			string xEIP = aEIP.ToString("X8");
            Log("0x" + xEIP);
		}

        protected void Log(string aText) {
            lboxLog.Items.Add(aText);
        }

		protected delegate void ConnectionLostDelegate(Exception ex);
		protected void ConnectionLost(Exception ex) {
		    Title = "No debug connection.";
            lboxLog.Background = Brushes.Red;
			while (ex != null) {
                Log(ex.Message);
				ex = ex.InnerException;
			}
		}

		protected void TCPRead(IAsyncResult aResult) {
			try {
				var xStream = (NetworkStream)aResult.AsyncState;
				int xCount = xStream.EndRead(aResult);
				if (xCount != 4) {
					if ((xCount + mCurrentPos) != 4) {
						mCurrentPos += xCount;
						xStream.BeginRead(mTCPData, mCurrentPos, 4 - mCurrentPos, new AsyncCallback(TCPRead), xStream);
						return;
					}
				}
				mCurrentPos = 0;
				UInt32 xEIP = (UInt32)((mTCPData[0] << 24) | (mTCPData[1] << 16) | (mTCPData[2] << 8) | mTCPData[3]);
				xStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), xStream);
				Dispatcher.BeginInvoke(DispatcherPriority.Background, new DebugPacketRcvdDelegate(DebugPacketRcvd), xEIP);
			} catch (System.IO.IOException ex) {
				Dispatcher.BeginInvoke(DispatcherPriority.Background, new ConnectionLostDelegate(ConnectionLost), ex);
			}

		}

        protected void Anaylyze() {
            List<string> xItems = new List<string>();
            for (int i = lboxLog.Items.Count - 1; i >= 0; i--) {
                string xEIP = lboxLog.Items[i] as string;
                if (xItems.Contains(xEIP, StringComparer.InvariantCultureIgnoreCase)) {
                    lboxLog.Items.RemoveAt(i);
                    continue;
                }
                var xSourceInfo = mSourceMappings.GetMapping(UInt32.Parse(xEIP.Substring(2), System.Globalization.NumberStyles.HexNumber));
                if (xSourceInfo == null) {
                    lboxLog.Items.RemoveAt(i);
                    continue;
                }
            }
            //foreach (var xEIP in (from item in lboxLog.Items.Cast<string>()
            //                      select item).Distinct(StringComparer.InvariantCultureIgnoreCase)) {

            //    //var xViewSrc = new ViewSourceWindow();
            //    //int xCharStart;
            //    //int xCharCount;
            //    //GetLineInfo(xViewSrc.tboxSource.Text, xSourceInfo.Line, xSourceInfo.Column, xSourceInfo.LineEnd, xSourceInfo.ColumnEnd, out xCharStart, out xCharCount);
            //    //if(
            //    int xCharStart = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.Line);
            //    int xCharEnd = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.LineEnd);
            //    if ((xCharEnd - xCharStart) > 4) {
            //        MessageBox.Show(xEIP);
            //        return;
            //    }
            //}
            MessageBox.Show("Analysis finished!");
        }

        protected void SelectCode(uint aEIP) {
            var xSourceInfo = mSourceMappings.GetMapping(aEIP);
            LoadSourceFile(xSourceInfo.SourceFile);
            SelectText(xSourceInfo.Line, xSourceInfo.Column, xSourceInfo.LineEnd, xSourceInfo.ColumnEnd);
        }

        void lboxLog_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string xItemStr = lboxLog.SelectedItem as String;
            if (!String.IsNullOrEmpty(xItemStr)) {
                if (mDebugMode == DebugModeEnum.Source) {
                    SelectCode(UInt32.Parse(xItemStr.Substring(2), System.Globalization.NumberStyles.HexNumber));
                } else {
                    throw new Exception("Debug mode not supported!");
                }
            }
        }

    }
}
