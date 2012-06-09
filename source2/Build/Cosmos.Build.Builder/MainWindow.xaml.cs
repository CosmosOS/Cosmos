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
using System.Security.Permissions;
using System.Windows.Threading;

namespace Cosmos.Build.Builder {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      mApp = (App)Application.Current;
    }

    protected bool mErrorOccurred = false;
    protected App mApp;
    protected TextBlock mSection;
    protected TextBlock mContent;
    protected StringBuilder mClipboard = new StringBuilder();

    public void Build() {
      var xTask = new CosmosTask(@"D:\source\Cosmos");
      xTask.Log.LogLine += new Installer.Log.LogLineHandler(Log_LogLine);
      xTask.Log.LogSection += new Installer.Log.LogSectionHandler(Log_LogSection);
      xTask.Log.LogError += new Installer.Log.LogErrorHandler(Log_LogError);
      xTask.ResetHive = mApp.Args.Contains("-RESETHIVE");
      xTask.Run();
    }

    void Log_LogError() {
      mSection.Foreground = Brushes.Red;
      mContent.Visibility = Visibility.Visible;
      mErrorOccurred = true;
    }

    void Log_LogSection(string aLine) {
      mClipboard.AppendLine();
      mClipboard.AppendLine(new string('=', aLine.Length));
      mClipboard.AppendLine(aLine);
      mClipboard.AppendLine(new string('=', aLine.Length));
      mClipboard.AppendLine();

      mSection = new TextBlock();
      mSection.Text = aLine;
      mSection.Background = Brushes.LightGray;
      mSection.Foreground = Brushes.Green;
      mSection.FontSize = 18;
      mSection.FontWeight = FontWeights.Bold;
      mSection.MouseUp += new MouseButtonEventHandler(mSection_MouseUp);
      spnlLog.Children.Add(mSection);

      mContent = new TextBlock();
      mContent.Visibility = Visibility.Collapsed;
      spnlLog.Children.Add(mContent);
      mSection.Tag = mContent;
    }

    void mSection_MouseUp(object sender, MouseButtonEventArgs e) {
      var xSection = (TextBlock)sender;
      var xContent = (TextBlock)xSection.Tag;
      xContent.Visibility = xContent.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    void Log_LogLine(string aLine) {
      mClipboard.AppendLine(aLine);

      mContent.Inlines.Add(aLine);
      mContent.Inlines.Add(new LineBreak());
    }

    void Window_Loaded(object sender, RoutedEventArgs e) {
    }

    void butnCopy_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(mClipboard.ToString());
    }

    private void butnTest_Click(object sender, RoutedEventArgs e) {
      Build();
      if (mErrorOccurred == false && mApp.Args.Contains("-STAYOPEN") == false) {
        Close();
      }
    }
  
  }
}
