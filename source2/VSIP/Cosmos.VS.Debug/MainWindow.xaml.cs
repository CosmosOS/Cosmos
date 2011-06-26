using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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

namespace Cosmos.VS.Debug
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      PipeThread.DataPacketReceived += new Action<byte, byte[]>(PipeThread_DataPacketReceived);
      var xServerThread = new Thread(PipeThread.ThreadStartServer);
      xServerThread.Start();
    }

    void PipeThread_DataPacketReceived(byte aCommand, byte[] aMsg)
    {
      // Assembly
      if ((int)aCommand == 3)
      {
        if (asmUC1.listBox1.Dispatcher.CheckAccess())
        {
          string xData = Encoding.ASCII.GetString(aMsg);
          asmUC1.listBox1.Items.Add(xData);
        }
        else
        {
          asmUC1.listBox1.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
          {
            string xData = Encoding.ASCII.GetString(aMsg);
            asmUC1.listBox1.Items.Add(xData);
          });
        }
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      PipeThread.KillThread = true;
      if (!PipeThread.Connected) {
        // Kick it out of the WaitForConnection
        var xPipe = new NamedPipeClientStream(".", PipeThread.PipeName, PipeDirection.Out);
        xPipe.Connect(100);
      }
    }
  }
}
