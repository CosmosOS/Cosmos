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

namespace Cosmos.Build.Windows {
	public partial class DebugWindow: Window {
		protected TcpClient mClient;
		protected byte[] mTCPData = new byte[4];
		protected int mCurrentPos = 0;

		public DebugWindow() {
            try
            {
                InitializeComponent();

                //Create a TCP connection to localhost:4444. We have already set up Qemu to listen to this port.
                mClient = new TcpClient();
                mClient.Connect(new IPEndPoint(IPAddress.Loopback, 4444));

                //Read TCP data from Qemu
                var xStream = mClient.GetStream();
                xStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), xStream);
                //            UInt32 xEIP = (UInt32)xStream.ReadByte();
            }
            catch (SocketException ex)
            {
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
				Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DebugPacketRcvdDelegate(DebugPacketRcvd), xEIP);
			} catch (System.IO.IOException ex) {
				Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ConnectionLostDelegate(ConnectionLost), ex);
			}

		}


	}
}
