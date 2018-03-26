using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder.Views {
  public partial class MainWindow : Window {
    private static readonly string[] NewLineStringArray = new string[] { Environment.NewLine };

    string mCosmosDir;
    string mSetupPath;
    // Needs updating with each new release.
    int mReleaseNo = 106027;
    string mTailCaption;


    public MainWindow() {
      InitializeComponent();
      mTailCaption = tblkTail.Text + " - ";
    }

    bool mPreventAutoClose = false;
    TextBlock mSection;
    TextBlock mContent;
    StringBuilder mClipboard = new StringBuilder();
    DispatcherTimer mCloseTimer;


    public bool Build() {
      Log.LogLine += new Log.LogLineHandler(Log_LogLine);
      Log.LogSection += new Log.LogSectionHandler(Log_LogSection);
      Log.LogError += new Log.LogErrorHandler(Log_LogError);

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
      if (DataContext is ViewModels.MainWindowViewModel viewModel)
      {
        viewModel.TailItems.Clear();
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

    void WriteTail(string aText) {
      if (DataContext is ViewModels.MainWindowViewModel viewModel)
      {
        foreach (var line in aText.Split(NewLineStringArray, StringSplitOptions.None))
        {
          viewModel.TailItems.Push(line);
        }
      }
    }

    protected bool mLoaded = false;
    void Window_Loaded(object sender, RoutedEventArgs e) {
      if (!App.mArgs.Any()) {
        MessageBox.Show("Builder not meant to be called directly. Use install.bat instead.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Close();
        return;
      }

      mLoaded = true;

      if (DataContext is ViewModels.MainWindowViewModel viewModel)
      {
        viewModel.LogBuilder = mClipboard;
      }

      string xAppPath = AppContext.BaseDirectory;
      mCosmosDir = Path.GetFullPath(xAppPath + @"..\..\..\..\..\");
      mSetupPath = Path.Combine(mCosmosDir, @"Setup\Output\" + CosmosTask.GetSetupName(mReleaseNo) + ".exe");
      if (!Build()) {
        Close();
      }
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
      // User had non minimized window, or maximized it, or otherwise manually intervened.
      // Even if starting minimized, this event gets called with Normal before load.
      // This is why we have mLoaded.
      if (mLoaded && WindowState != WindowState.Minimized) {
        mPreventAutoClose = true;
      }
    }
  }
}
