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

namespace Cosmos.Build.Builder {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
    }

    public void Build() {
      var xTask = new CosmosTask();
      xTask.Run(@"D:\source\Cosmos\");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      Build();
    }

  }
}
