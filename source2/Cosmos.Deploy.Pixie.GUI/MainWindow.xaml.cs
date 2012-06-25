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
using System.IO;

namespace Cosmos.Deploy.Pixie.GUI {
  public partial class MainWindow : Window {
    protected byte[] mNicIP;

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
      var mNicIP = new byte[4];
      for (int i = 0; i < mNicIP.Length; i++) {
        mNicIP[i] = byte.Parse(xBytes[i]);
      }

    }

    protected void Start() {
      // Need full path to boot file because it needs to get the size
      var xBOOTP = new DHCP(mNicIP, Path.Combine(App.PxePath, "pxelinux.0"));
      xBOOTP.Execute();

      var xTFTP = new TrivialFTP(mNicIP, App.PxePath);
      xTFTP.Execute();
    }

  }
}
