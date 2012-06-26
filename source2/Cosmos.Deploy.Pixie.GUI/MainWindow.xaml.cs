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
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace Cosmos.Deploy.Pixie.GUI {
  public partial class MainWindow : Window {
    protected byte[] mNicIP = new byte[4];

    public MainWindow() {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      Title = App.Title;

      if (!Directory.Exists(App.PxePath)) {
        MessageBox.Show("Specified path does not exist.", Title);
        App.Current.Shutdown(-1);
        return;
      }

      lablNIC.Content = App.IpAddress;
      lablPath.Content = App.PxePath;

      var xBytes = App.IpAddress.Split(".".ToCharArray());
      if (xBytes.Length != 4) {
        MessageBox.Show("Invalid IP address specified.", Title);
        App.Current.Shutdown(-1);
        return;
      }
      for (int i = 0; i < mNicIP.Length; i++) {
        mNicIP[i] = byte.Parse(xBytes[i]);
      }

      ClearFile();
      Start();
    }

    void ClearFile() {
      lablCurrentFile.Content = "";
      lablCurrentSize.Content = "";
      progFile.Value = 0;
    }

    protected void Log(string aSender, string aText) {
      lboxLog.SelectedItem = lboxLog.Items.Add("[" + aSender + "] " + aText);
    }

    protected Thread mDhcpThread;
    protected DHCP mDHCP;
    protected Thread mTftpThread;
    protected TrivialFTP mTFTP;
    protected void Start() {
      mDhcpThread = new Thread(delegate() {
        // Need full path to boot file because it needs to get the size
        mDHCP = new DHCP(mNicIP, Path.Combine(App.PxePath, "pxelinux.0"));

        mDHCP.OnLog += delegate(DHCP aSender, string aText) {
          Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            Log("DHCP", aText);
          });
        };

        mDHCP.Execute();
      });
      mDhcpThread.Start();

      mTftpThread = new Thread(delegate() {
        mTFTP = new TrivialFTP(mNicIP, App.PxePath);

        mTFTP.OnFileStart += delegate(TrivialFTP aSender, string aFilename, long aSize) {
          Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            Log("TFTP", "Starting file " + aFilename);
            lablCurrentFile.Content = aFilename;
            double xMB = (double)aSize / (1024 * 1024);
            lablCurrentSize.Content = xMB.ToString("0.00") + " MB";
            progFile.Value = 0;
            progFile.Maximum = aSize;
          });
        };

        mTFTP.OnFileTransfer += delegate(TrivialFTP aSender, string aFilename, long aPosition) {
          Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            progFile.Value = aPosition;
          });
        };

        mTFTP.OnFileCompleted += delegate(TrivialFTP aSender, string aFilename) {
          Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            ClearFile();
            Log("TFTP", "Completed " + aFilename);
          });
        };

        mTFTP.Execute();
      });
      mTftpThread.Start();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      mDhcpThread.Abort();
      mDHCP.Stop();
      mTftpThread.Abort();
      mTFTP.Stop();
    }

  }
}
