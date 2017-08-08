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
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public partial class MainWindow : Window {
    protected byte[] mNicIP = new byte[4];
    protected DispatcherTimer mCloseTimer = new DispatcherTimer();
    protected DispatcherTimer mOnlineTimer = new DispatcherTimer();
    protected bool mWarningIssued = false;

    public MainWindow() {
      InitializeComponent();

      lablWaiting.Visibility = Visibility.Hidden;

      mCloseTimer.Interval = TimeSpan.FromSeconds(5);
      mCloseTimer.Tick += (object sender, EventArgs e) => {
        if (mWarningIssued) {
          Close();
        } else {
          Log("", "Last transfer request completed. Auto closing in " + mCloseTimer.Interval.TotalSeconds + " seconds.");
          mWarningIssued = true;
        }
      };

      mOnlineTimer.Interval = TimeSpan.FromSeconds(1);
      mOnlineTimer.Tick += new EventHandler(mOnlineTimer_Tick);
    }
    
    // This code is necessary for cross over cables. NIC will appear offline till sense appears,
    // which wont happen till we turn the machine on.
    void mOnlineTimer_Tick(object sender, EventArgs e) {
      mOnlineTimer.Stop();
      
      UdpClient xUDP = null;
      try {
        xUDP = new UdpClient(new IPEndPoint(new IPAddress(mNicIP), 67));
      } catch (SocketException ex) {
        if (ex.ErrorCode != 10049) {
          throw;
        }
        if (lablWaiting.Visibility == Visibility.Hidden) {
          lablWaiting.Visibility = Visibility.Visible;
          Log("NIC", "Interface unavailable. Waiting.");
        }
        mOnlineTimer.Start();
        return;
      }

      lablWaiting.Visibility = Visibility.Hidden;
      Log("NIC", "Interface active.");
      xUDP.Close();
      Start();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)  {
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

      Log("NIC", "Checking for interface.");
      mOnlineTimer.Start();
    }

    void ClearFile() {
      lablCurrentFile.Content = "";
      lablCurrentSize.Content = "";
      progFile.Value = 0;
    }

    protected void Log(string aSender, string aText) {
      string xPrefix = aSender == "" ? "" : "[" + aSender + "] ";
      lboxLog.SelectedItem = lboxLog.Items.Add(xPrefix + aText);
    }

    protected Thread mDhcpThread;
    protected DHCP mDHCP;
    protected Thread mTftpThread;
    protected TrivialFTP mTFTP;
    protected void Start() {
      Log("DHCP", "Starting");
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

      Log("TFTP", "Starting");
      mTftpThread = new Thread(delegate() {
        mTFTP = new TrivialFTP(mNicIP, App.PxePath);

        mTFTP.OnFileStart += delegate(TrivialFTP aSender, string aFilename, long aSize) {
          Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
            mCloseTimer.Stop();
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
            mCloseTimer.Start();
          });
        };

        mTFTP.Execute();
      });
      mTftpThread.Start();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (mDhcpThread != null) {
        mDhcpThread.Abort();
        mDHCP.Stop();
      }
      if (mTftpThread != null) {
        mTftpThread.Abort();
        mTFTP.Stop();
      }
    }

  }
}
