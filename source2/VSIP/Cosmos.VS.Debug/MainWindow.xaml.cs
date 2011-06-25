using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cosmos.VS.Debug {
  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();

      PipeThread.DataPacketReceived += new Action<byte, byte[]>(PipeThread_DataPacketReceived);
      var xServerThread = new Thread(PipeThread.ThreadStartServer);
      xServerThread.Start();
    }

    void PipeThread_DataPacketReceived(byte aMsgType, byte[] aData) {
        MessageBox.Show("Message");
      listBox1.Items.Add(aData);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        MessageBox.Show("Window Closing");
        PipeThread.KillThread = true;
    }

  }
}
