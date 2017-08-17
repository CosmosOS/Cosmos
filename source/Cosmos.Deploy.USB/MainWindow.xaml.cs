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
using System.IO;

namespace Cosmos.Deploy.USB {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      Loaded += new RoutedEventHandler(MainWindow_Loaded);
    }

    void MainWindow_Loaded(object sender, RoutedEventArgs e) {
      Title = App.Title;

      if (!File.Exists(App.ObjFile)) {
        MessageBox.Show("Specified .obj file does not exist.", Title);
        App.Current.Shutdown(-1);
        return;
      }

      tboxObjFile.Text = App.ObjFile;

      RefreshDrives();
    }

    protected void RefreshDrives() {
      var xDrives = DriveInfo.GetDrives().Where(q => q.DriveType == DriveType.Removable).ToArray();

      lboxTarget.Items.Clear();
      foreach (var x in xDrives)
      {
        if (!x.IsReady)
            continue;
        decimal xSize = x.TotalSize / (1000 * 1000 * 1000);
        lboxTarget.Items.Add(x.Name + "   " + xSize.ToString("0.0") + " GiB   " + x.DriveFormat + "   [" + x.VolumeLabel + "]");
      }

      if (lboxTarget.Items.Count == 1) {
        lboxTarget.SelectedIndex = 0;
      }
    }

    private void butnRefresh_Click(object sender, RoutedEventArgs e) {
      RefreshDrives();
    }

    private void butnCancel_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    private void butnCreate_Click(object sender, RoutedEventArgs e) {
      if (lboxTarget.SelectedItem == null) {
        MessageBox.Show("Please select a target drive.", Title);
        return;
      }

      string xDrive = (string)lboxTarget.SelectedItem;
      xDrive = xDrive[0].ToString();

      try {
        Cosmos.Build.Common.UsbMaker.Generate(xDrive, tboxObjFile.Text);
      } catch (Exception ex) {
        MessageBox.Show(ex.Message);
        return;
      }

      MessageBox.Show("Drive " + xDrive + ": has been made a bootable Cosmos device.");
      Close();
    }
  }
}
