using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            var xSocket = new Socket(AddressFamily.InterNetwork
                    , SocketType.Dgram, ProtocolType.Udp);
            xSocket.EnableBroadcast = true;
            var xIP = IPAddress.Broadcast;
            var xEndPoint = new IPEndPoint(xIP, 2222);

            byte[] xBytes = new byte[1];
            xBytes[0] = 22;
            xSocket.SendTo(xBytes, xEndPoint);
        }

        UdpState mUdpState = new UdpState();
        protected class UdpState {
            public IPEndPoint EndPoint;
            public UdpClient Client;
        }
        private void button2_Click(object sender, EventArgs e) {
            mUdpState.EndPoint = new IPEndPoint(IPAddress.Any, 2222);
            mUdpState.Client = new UdpClient(mUdpState.EndPoint);
            mUdpState.Client.BeginReceive(new AsyncCallback(UdpReceive), mUdpState);
        }
        public void UdpReceive(IAsyncResult aResult) {
            var xState = (UdpState)aResult.AsyncState;
            var xBytes = xState.Client.EndReceive(aResult, ref xState.EndPoint);
//            Dispatcher.BeginInvoke(DispatcherPriority.Input, new XPTDataDelegate(XPTData), xBytes);
        }
    }
}
