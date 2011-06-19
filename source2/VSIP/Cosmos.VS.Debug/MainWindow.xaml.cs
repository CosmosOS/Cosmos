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

namespace Cosmos.VS.Debug {
  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();

      var xServerThread = new Thread(ThreadStartServer);
      xServerThread.Start();
    }

    public void ThreadStartServer() {
      using (var xPipe = new NamedPipeServerStream("CosmosDebugWindows", PipeDirection.In)) {
        xPipe.WaitForConnection();
        using (var xReader = new StreamReader(xPipe)) {
          string xLine = xReader.ReadLine();

          Dispatcher.Invoke(DispatcherPriority.Normal, 
            (Action)delegate() {
              listBox1.Items.Add(xLine);
            }
          );
        }
      }
    }

  }
}
