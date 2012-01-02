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

namespace Cosmos.VS.Windows.Test {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      string xPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\..");

      byte[] xData = System.IO.File.ReadAllBytes(System.IO.Path.Combine(xPath, "SourceTest.bin"));
      ucAssembly.Update(xData);
    }
  }
}
