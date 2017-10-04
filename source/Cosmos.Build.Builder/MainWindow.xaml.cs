using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder {
  public partial class MainWindow : Window {
    int mTailLineCount = 10;
    int mTailCurrent = 0;
    List<TextBlock> mTailLines = new List<TextBlock>();
    string mCosmosDir;
    string mSetupPath;
    // Needs updating with each new release.
    int mReleaseNo = 106027;
    string mTailCaption;


    public MainWindow() {
      InitializeComponent();
      mApp = (App)Application.Current;
      mTailCaption = tblkTail.Text + " - ";

      for (int i = 0; i < mTailLineCount; i++) {
        var xTextBlock = new TextBlock();
        xTextBlock.Background = Brushes.Black;
        xTextBlock.Foreground = Brushes.Green;
        xTextBlock.FontSize = 16;
        mTailLines.Add(xTextBlock);
        spnlTail.Children.Add(xTextBlock);
      }
    }

    bool mPreventAutoClose = false;
    App mApp;
    TextBlock mSection;
    TextBlock mContent;
    StringBuilder mClipboard = new StringBuilder();
    DispatcherTimer mCloseTimer;


    public bool Build() {
      Log.LogLine += new Installer.Log.LogLineHandler(Log_LogLine);
      Log.LogSection += new Installer.Log.LogSectionHandler(Log_LogSection);
      Log.LogError += new Installer.Log.LogErrorHandler(Log_LogError);

      if (App.IsUserKit) {
        mReleaseNo = Int32.Parse(DateTime.Now.ToString("yyyyMMdd"));
      }
      if (mPreventAutoClose) {
        return true;
      }

      var xTask = new CosmosTask(mCosmosDir, mReleaseNo);

      var xThread = new System.Threading.Thread(delegate () {
        xTask.Run();
        ThreadDone();
      });
      xThread.Start();

      return true;
    }

    void ThreadDone() {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () {
        if (App.StayOpen == false) {
          mCloseTimer = new DispatcherTimer();
          mCloseTimer.Interval = TimeSpan.FromSeconds(5);
          mCloseTimer.Tick += delegate {
            mCloseTimer.Stop();
            if (mPreventAutoClose) {
              if (WindowState == WindowState.Minimized) {
                WindowState = WindowState.Normal;
              }
            } else {
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
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () {
        ClearTail();

        mSection.Foreground = Brushes.Red;
        mContent.Visibility = Visibility.Visible;
        mPreventAutoClose = true;
      });
    }

    void Log_LogLine(string aLine) {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () {
        WriteTail(aLine);

        mClipboard.AppendLine(aLine);

        mContent.Inlines.Add(aLine);
        mContent.Inlines.Add(new LineBreak());
      });
    }

    void Log_LogSection(string aLine) {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () {
        Title = aLine;

        ClearTail();
        tblkTail.Text = mTailCaption + aLine;

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

    protected bool mLoaded = false;
    void Window_Loaded(object sender, RoutedEventArgs e) {
      if (!App.mArgs.Any()) {
        MessageBox.Show("Builder not meant to be called directly. Use install.bat instead.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Close();
        return;
      }

      LoadPosition();
      mLoaded = true;

      string xAppPath = AppContext.BaseDirectory;
      mCosmosDir = Path.GetFullPath(xAppPath + @"..\..\..\..\");
      mSetupPath = Path.Combine(mCosmosDir, @"Setup\Output\" + CosmosTask.GetSetupName(mReleaseNo) + ".exe");
      if (!Build()) {
        Close();
      }
    }

    void butnCopy_Click(object sender, RoutedEventArgs e) {
      mPreventAutoClose = true;
      Clipboard.SetText(mClipboard.ToString());
    }

    void LoadPosition() {
      Left = Properties.Settings.Default.Location.X;
      Top = Properties.Settings.Default.Location.Y;
      Width = Properties.Settings.Default.Size.Width;
      Height = Properties.Settings.Default.Size.Height;
    }

    protected void SavePosition() {
      Properties.Settings.Default.Location = new System.Drawing.Point((int)Left, (int)Top);
      Properties.Settings.Default.Size = new System.Drawing.Size((int)Width, (int)Height);
      Properties.Settings.Default.Save();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
      // User had non minimized window, or maximized it, or otherwise manually intervened.
      // Even if starting minimized, this event gets called with Normal before load.
      // This is why we have mLoaded.
      if (mLoaded && WindowState != System.Windows.WindowState.Minimized) {
        mPreventAutoClose = true;
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      SavePosition();
    }
  }
}
