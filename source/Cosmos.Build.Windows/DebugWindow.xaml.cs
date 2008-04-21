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
		protected TcpClient mClient;
		protected byte[] mTCPData = new byte[4];
		protected int mCurrentPos = 0;
		private DebugModeEnum mDebugMode;
		private SourceInfos mSourceMappings;

		public DebugWindow() {
			InitializeComponent();
		}

		public void SetSourceInfoMap(SourceInfos aSourceMapping) {
			try {
				mDebugMode = DebugModeEnum.Source;
				mSourceMappings = aSourceMapping;
				//Create a TCP connection to localhost:4444. We have already set up Qemu to listen to this port.
				mClient = new TcpClient();
				mClient.Connect(new IPEndPoint(IPAddress.Loopback, 4444));

				//Read TCP data from Qemu
				var xStream = mClient.GetStream();
				xStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), xStream);
			} catch (SocketException ex) {
				Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConnectionLostDelegate(ConnectionLost), ex);
			}
		}

		protected delegate void DebugPacketRcvdDelegate(UInt32 aEIP);
		protected void DebugPacketRcvd(UInt32 aEIP) {
			string xEIP = aEIP.ToString("X8");
			lablEIP.Content = "0x" + xEIP;
			lboxLog.SelectedIndex = lboxLog.Items.Add("0x" + xEIP);
		}

		protected delegate void ConnectionLostDelegate(Exception ex);
		protected void ConnectionLost(Exception ex) {
			textBlock1.Text = "No TCP Connection to virtual machine!" + Environment.NewLine;
			DebugGrid.Background = System.Windows.Media.Brushes.Red;
			while (ex != null) {
				textBlock1.Text += ex.Message + Environment.NewLine;
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

		private static void GetLineInfo(string aData, int aLineStart, int aColumnStart, int aLineEnd, int aColumnEnd, out int oCharStart, out int oCharCount) {
			int xCurrentPos = 0;
			int xCurrentLine = 1;
			oCharCount = 0;
			oCharStart = 0;
			while (xCurrentPos < aData.Length) {
				int xTempPos = aData.IndexOfAny(new char[] { '\r', '\n' }, xCurrentPos);
				if (xTempPos == -1) {
					if (oCharStart > 0) {
						oCharCount = xCurrentPos - oCharStart;
					}
					return;
				}
				xCurrentLine += 1;
				xCurrentPos = xTempPos;
				if (aData[xCurrentPos] == '\r' && aData.Length > (xCurrentPos + 1) && aData[xCurrentPos + 1] == '\n') {
					xCurrentPos += 2;
				} else {
					xCurrentPos += 1;
				}
				if (xCurrentLine == aLineStart) {
					oCharStart = xCurrentPos + aColumnStart;
				}
				if (xCurrentLine == aLineEnd) {
					oCharCount = (xCurrentPos + aColumnEnd) - oCharStart;
				}
				if (oCharCount > 0 && oCharStart > 0) {
					return;
				}
			}
		}

		private void lboxLog_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			var xItem = lboxLog.SelectedItem;
			string xItemStr = xItem as String;
			if (MessageBox.Show("Do you want to do analysis? (Press no if you dont know)", "Debug", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
				if (!String.IsNullOrEmpty(xItemStr)) {
					if (mDebugMode == DebugModeEnum.Source) {
						var xSourceInfo = mSourceMappings.GetMapping(UInt32.Parse(xItemStr.Substring(2), System.Globalization.NumberStyles.HexNumber));
						var xViewSrc = new ViewSourceWindow();
						//int xCharStart;
						//int xCharCount;
						//GetLineInfo(xViewSrc.tboxSource.Text, xSourceInfo.Line, xSourceInfo.Column, xSourceInfo.LineEnd, xSourceInfo.ColumnEnd, out xCharStart, out xCharCount);
						//if(
						//int xCharStart = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.Line);
						//int xCharEnd = xViewSrc.tboxSource.GetCharacterIndexFromLineIndex(xSourceInfo.LineEnd);
						//xCharStart += xSourceInfo.Column;
						//xCharEnd += xSourceInfo.ColumnEnd;
						int xCharStart;
						int xCharLength;
						xViewSrc.LoadSourceFile(xSourceInfo.SourceFile);
						GetLineInfo(xViewSrc.tboxSource.Text, xSourceInfo.Line, xSourceInfo.Column, xSourceInfo.LineEnd, xSourceInfo.ColumnEnd, out xCharStart, out xCharLength);
						//xViewSrc.tboxSource.ScrollToLine(xSourceInfo.Line);
						//xViewSrc.tboxSource.Select(xCharStart, xCharEnd - xCharStart);
						xViewSrc.CharStart = xCharStart - 1;
						xViewSrc.CharLength = xCharLength;
						xViewSrc.ShowDialog();
					} else {
						throw new Exception("Debug mode not supported!");
					}
					//xViewSrc.tboxSource.s
				}
			} else {
				var xViewSrc = new ViewSourceWindow();
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
		}
	}
}
