using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FF = System.Windows.Media.FontFamily;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DebugListener
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            mListener = new UdpClient(643);
            StartListen();

        }

        private void StartListen()
        {
            var xThread = new Thread(DoListen);
            xThread.IsBackground = true;
            xThread.Start();
        }

        private void DoListen()
        {
            IPEndPoint xEndpoint = null;
            var xOut = mListener.Receive(ref xEndpoint);
            var xString = Encoding.ASCII.GetString(xOut);
            Dispatcher.Invoke(new Action(() => ShowText(xString)));
        }
            private void ShowText(string aNewInfo){
                tbxOutput.Text += aNewInfo;
            }
        private UdpClient mListener;
    }
}
