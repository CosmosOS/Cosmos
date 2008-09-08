using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Build.Windows {
    public class DebugConnectorQEMU : DebugConnector {
        protected TcpListener mTCPListener;
        protected NetworkStream mTCPStream;
        protected byte[] mTCPData = new byte[4];
        protected int mCurrentPos = 0;

        public DebugConnectorQEMU() {
            mTCPListener = new TcpListener(IPAddress.Loopback, 4444);
            mTCPListener.Start();
            mTCPListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), mTCPListener);
        }

        public void DoAcceptTcpClientCallback(IAsyncResult aResult) {
            var xListener = (TcpListener) aResult.AsyncState;
            var xClient = xListener.EndAcceptTcpClient(aResult);
            mTCPStream = xClient.GetStream();
            mTCPStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), mTCPStream);
        }
        
        public override void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mTCPStream.Write(xData, 0, xData.Length);
        }

        protected void TCPRead(IAsyncResult aResult) {
            try {
                var xStream = (Stream)aResult.AsyncState;
                int xCount = xStream.EndRead(aResult);
                if (xCount != 4) {
                    if ((xCount + mCurrentPos) != 4) {
                        mCurrentPos += xCount;
                        xStream.BeginRead(mTCPData, mCurrentPos, 4 - mCurrentPos
                            , new AsyncCallback(TCPRead), xStream);
                        return;
                    }
                }
                mCurrentPos = 0;
                UInt32 xEIP = (UInt32)((mTCPData[0] << 24) | (mTCPData[1] << 16)
                    | (mTCPData[2] << 8) | mTCPData[3]);
                xStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(TCPRead), xStream);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, DebugPacketReceived, xEIP);
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }

    }
}
