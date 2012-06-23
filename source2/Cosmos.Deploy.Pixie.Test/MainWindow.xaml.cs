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
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie.Test {
  public partial class MainWindow : Window {
    DHCP mBOOTP;

    public MainWindow() {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      try {
        mBOOTP = new DHCP();
      } catch (SocketException ex) {
        MessageBox.Show(ex.SocketErrorCode.ToString());
      } catch (Exception ex) {
        MessageBox.Show(ex.Message);
      }
    }

  }
}
