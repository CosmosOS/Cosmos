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
using Cosmos.Compiler.Debug;

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

    void CommandAsmSource(byte[] aData) {
      string xData = Encoding.ASCII.GetString(aData);
      xData = xData.Replace("\t", "  ");
      asmUC1.tboxSource.Text = xData;
      //string[] xLines = xData.Split('\n');
      //foreach (var xLine in xLines) {
      //  asmUC1.lboxSource.Items.Add(xLine);
      //}
    }

    void CommandRegisters(byte[] aData) {
      int i = 0;
    }

    void PipeThread_DataPacketReceivedInvoke(byte aCommand, byte[] aData) {
      switch (aCommand) {
        case DwMsgType.Noop:
          break;

        case DwMsgType.Registers:
          CommandRegisters(aData);
          break;

        case DwMsgType.Quit:
          Close();
          break;

        case DwMsgType.AssemblySource:
          CommandAsmSource(aData);
          break;
      }
    }

    void PipeThread_DataPacketReceived(byte aCmd, byte[] aMsg) {
      Dispatcher.Invoke(DispatcherPriority.Normal,
        (Action)delegate() {
          PipeThread_DataPacketReceivedInvoke(aCmd, aMsg);
        }
      );
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      PipeThread.Stop();
    }
  }
}
