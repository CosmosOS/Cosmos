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

namespace WindowsFormsApplication1 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            // QEMU disconencts TAP each time. So you have to let QEMU run and 
            // let it connect TAP before you run this program else it wont
            // bind to that interface.
            mUdpState.EndPoint = new IPEndPoint(IPAddress.Any, 2222);
            mUdpState.Client = new UdpClient(mUdpState.EndPoint);
            mUdpState.Client.BeginReceive(new AsyncCallback(UdpReceive), mUdpState);
        }

        private void button1_Click(object sender, EventArgs e) {
            var xSocket = new Socket(AddressFamily.InterNetwork
                    , SocketType.Dgram, ProtocolType.Udp);
            xSocket.EnableBroadcast = true;
            var xIP = IPAddress.Broadcast;
            //var xIP = new IPAddress(new byte[] { 10, 0, 2, 15 });
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

        public delegate void PacketRecievedDelegate(byte[] aData);
        
        public void UdpReceive(IAsyncResult aResult) {
            var xState = (UdpState)aResult.AsyncState;
            var xBytes = xState.Client.EndReceive(aResult, ref xState.EndPoint);
            BeginInvoke(new PacketRecievedDelegate(PacketRecieved), xBytes);
        }

        public void PacketRecieved(byte[] aData) {
            var xSB = new StringBuilder();
            foreach (var xByte in aData) {
                xSB.AppendLine(xByte.ToString("X2"));
            }
            textResults.Lines = new string[] { xSB.ToString() };
        }

        private void butnClear_Click(object sender, EventArgs e) {
            textResults.Clear();
        }

    }
}
