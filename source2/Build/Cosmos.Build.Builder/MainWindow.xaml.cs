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

    protected StringBuilder mClipboard = new StringBuilder();

    public void Build() {
      var xTask = new CosmosTask(@"D:\source\Cosmos");
      xTask.Log.LogLine += new Installer.Log.LogLineHandler(Log_LogLine);
      xTask.Log.LogSection += new Installer.Log.LogSectionHandler(Log_LogSection);
      xTask.Run();
    }

    void Log_LogSection(string aLine) {
      mClipboard.AppendLine(aLine);

      var xRun = new Run(aLine);
      xRun.Background = Brushes.Red;
      tblkLog.Inlines.Add(xRun);
      tblkLog.Inlines.Add(new LineBreak());
    }

    void Log_LogLine(string aLine) {
      mClipboard.AppendLine(aLine);

      tblkLog.Inlines.Add(aLine);
      tblkLog.Inlines.Add(new LineBreak());
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      Build();
    }

    private void butnCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mClipboard.ToString());
    }

  }
}
