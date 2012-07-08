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
using System.IO;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace Cosmos.Build.Builder {
  public partial class MainWindow : Window {
    protected int mTailLineCount = 5;
    protected int mTailCurrent = 0;
    protected List<TextBlock> mTailLines = new List<TextBlock>();

    public MainWindow() {
      InitializeComponent();
      mApp = (App)Application.Current;

      for (int i = 0; i < mTailLineCount; i++) {
        var xTextBlock = new TextBlock();
        xTextBlock.FontSize = 16;
        mTailLines.Add(xTextBlock);
        spnlTail.Children.Add(xTextBlock);
      }

      // GetInstallList();
    }

    protected void GetInstallList() {
      var xSB = new StringBuilder();

      string[] xKeys;
      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\", false)) {
        xKeys = xKey.GetSubKeyNames();
      }
      foreach (string xSubKey in xKeys) {
        using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\" + xSubKey, false)) {
          string xValue = (string)xKey.GetValue("ProductName");
          xSB.AppendLine(xValue);
        }
      }

      Clipboard.SetText(xSB.ToString());
    }

    bool mPreventAutoClose = false;
    App mApp;
    TextBlock mSection;
    TextBlock mContent;
    StringBuilder mClipboard = new StringBuilder();
    DispatcherTimer mCloseTimer;

    public bool Build() {
      string xAppPath = System.AppDomain.CurrentDomain.BaseDirectory;
      string xCosmosPath = Path.GetFullPath(xAppPath + @"..\..\..\..\..\");
      int xReleaseNo = 7;

      if (App.IsUserKit) {
        string x = Interaction.InputBox("Enter Release Number", "Cosmos Builder");
        if (string.IsNullOrEmpty(x)) {
          return false;
        }
        xReleaseNo = int.Parse(x);
      }

      var xTask = new CosmosTask(xCosmosPath, xReleaseNo);
      xTask.Log.LogLine += new Installer.Log.LogLineHandler(Log_LogLine);
      xTask.Log.LogSection += new Installer.Log.LogSectionHandler(Log_LogSection);
      xTask.Log.LogError += new Installer.Log.LogErrorHandler(Log_LogError);
      xTask.ResetHive = App.ResetHive;
      xTask.IsUserKit = App.IsUserKit;

      var xThread = new System.Threading.Thread(delegate() {
        xTask.Run();
        ThreadDone();
      });
      xThread.Start();

      return true;
    }

    void ThreadDone() {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        if (App.StayOpen == false) {
          mCloseTimer = new DispatcherTimer();
          mCloseTimer.Interval = TimeSpan.FromSeconds(5);
          mCloseTimer.Tick += delegate {
            mCloseTimer.Stop();
            if (!mPreventAutoClose) {
              Close();
            }
          };
          mCloseTimer.Start();
        }
      });
    }

    void ClearTail() {
      mTailCurrent = 0;
      foreach (var x in mTailLines) {
        x.Text = "";
      }
    }

    void Log_LogError() {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        ClearTail();

        mSection.Foreground = Brushes.Red;
        mContent.Visibility = Visibility.Visible;
        mPreventAutoClose = true;
      });
    }

    void Log_LogSection(string aLine) {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        ClearTail();

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
      });
    }

    void mSection_MouseUp(object sender, MouseButtonEventArgs e) {
      var xSection = (TextBlock)sender;
      var xContent = (TextBlock)xSection.Tag;
      xContent.Visibility = xContent.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
      mPreventAutoClose = true;
    }

    void ScrollTail() {
      for (int i = 0; i < mTailLineCount - 1; i++) {
        mTailLines[i].Text = mTailLines[i + 1].Text;
      }
    }

    void WriteTail(string aText) {
      if (mTailCurrent == mTailLineCount - 1) {
        ScrollTail();
      }
      mTailLines[mTailCurrent].Text = aText;
      if (mTailCurrent < mTailLineCount - 1) {
        mTailCurrent++;
      }
    }

    void Log_LogLine(string aLine) {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        WriteTail(aLine);

        mClipboard.AppendLine(aLine);

        mContent.Inlines.Add(aLine);
        mContent.Inlines.Add(new LineBreak());
      });      
    }

    void Window_Loaded(object sender, RoutedEventArgs e) {
      if (!Build()) {
        Close();
      }
    }

    void butnCopy_Click(object sender, RoutedEventArgs e) {
      mPreventAutoClose = true;
      Clipboard.SetText(mClipboard.ToString());
    }

  }
}
