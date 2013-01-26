using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TaskScheduler;
using System.Reflection;

namespace Cosmos.Build.Builder {
  public partial class MainWindow : Window {
    protected int mTailLineCount = 5;
    protected int mTailCurrent = 0;
    protected List<TextBlock> mTailLines = new List<TextBlock>();
    protected string mCosmosDir;
    protected string mSetupPath;
    protected int mReleaseNo = 7;

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

      using (var xKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products\", false)) {
        var xSubKeyNames = xKey.GetSubKeyNames();
        foreach (string xSubKeyName in xSubKeyNames) {
          using (var xSubKey = xKey.OpenSubKey(xSubKeyName, false)) {
            string xValue = (string)xSubKey.GetValue("ProductName");
            xSB.AppendLine(xValue);
          }
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

    void InstallScheduledTask() {
      ITaskService xService = new TaskScheduler.TaskScheduler();
      xService.Connect();

      ITaskDefinition xTaskDef = xService.NewTask(0);
      xTaskDef.RegistrationInfo.Description = "Cosmos DevKit UAC Bypass";
      xTaskDef.RegistrationInfo.Author = "Cosmos Group";
      xTaskDef.Settings.Compatibility = _TASK_COMPATIBILITY.TASK_COMPATIBILITY_V2_1;
      xTaskDef.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

      IActionCollection xActions = xTaskDef.Actions;
      IAction xAction = xActions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
      IExecAction xExecAction = xAction as IExecAction;
      xExecAction.Path = mSetupPath;
      xExecAction.Arguments = @"/SILENT";

      ITaskFolder xFolder = xService.GetFolder(@"\");
      // 6 as argument means this task can be created or updated ["CreateOrUpdate" flag]
      // if Name id empty or null, System will create a task with name as GUID
      xFolder.RegisterTaskDefinition("CosmosSetup", xTaskDef, 6, null, null, _TASK_LOGON_TYPE.TASK_LOGON_NONE, null);
    }

    // http://yoursandmyideas.wordpress.com/2012/01/07/task-scheduler-in-c-net/
    bool ScheduledTaskIsInstalled() {
      ITaskService xService = new TaskScheduler.TaskScheduler();
      xService.Connect();

      var xTasks = new List<IRegisteredTask>();
      ITaskFolder xFolder = xService.GetFolder(@"\");
      foreach (IRegisteredTask xTask in xFolder.GetTasks(0)) {
        if (string.Equals(xTask.Name, "CosmosSetup")) {
          if(xTask.Definition.Actions.Count > 0)
          {
              IAction xAction = xTask.Definition.Actions[1];
              if (xAction is IExecAction)
              {
                  IExecAction xAction2 = xAction as IExecAction;
                  if (xAction2.Path == null || xAction2.Arguments != "/SILENT" || xAction2.Path != mSetupPath)
                  { 
                      return false;
                  }
                  return true;
              }
              else
              {
                  throw new Exception("Task is of unkonwn type.");
              }
          }
          return false;
        }
      }
      return false;
    }

    void InstallTaskAsAdmin() {
      // Restart with UAC and just install scheduled task
      using (var xProcess = new Process()) {
        var xPSI = xProcess.StartInfo;
        xPSI.UseShellExecute = true;
        xPSI.FileName = Assembly.GetEntryAssembly().GetName().CodeBase;
        xPSI.Arguments = "-InstallTask";
        xPSI.Verb = "runas";
        xProcess.Start();
        xProcess.WaitForExit();
      }
    }

    public bool Build() {
      if (App.IsUserKit) {
        string x = Interaction.InputBox("Enter Release Number", "Cosmos Builder");
        if (string.IsNullOrEmpty(x)) {
          return false;
        }
        mReleaseNo = int.Parse(x);
      } else {
        if (App.UseTask) {
          if (!ScheduledTaskIsInstalled()) {
            InstallTaskAsAdmin();
          }
        }
      }

      var xTask = new CosmosTask(mCosmosDir, mReleaseNo);
      xTask.Log.LogLine += new Installer.Log.LogLineHandler(Log_LogLine);
      xTask.Log.LogSection += new Installer.Log.LogSectionHandler(Log_LogSection);
      xTask.Log.LogError += new Installer.Log.LogErrorHandler(Log_LogError);

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
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        ClearTail();

        mSection.Foreground = Brushes.Red;
        mContent.Visibility = Visibility.Visible;
        mPreventAutoClose = true;
      });
    }

    void Log_LogSection(string aLine) {
      Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() {
        Title = aLine;

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

    protected bool mLoaded = false;
    void Window_Loaded(object sender, RoutedEventArgs e) {
      if (!App.HasParams) {
        MessageBox.Show("Builder not meant to be called directly. Use install.bat instead.");
        Close();
        return;
      }

      LoadPosition();
      mLoaded = true;

      string xAppPath = System.AppDomain.CurrentDomain.BaseDirectory;
      mCosmosDir = Path.GetFullPath(xAppPath + @"..\..\..\..\..\");
      mSetupPath = Path.Combine(mCosmosDir, @"Setup2\Output\CosmosUserKit-" + mReleaseNo + ".exe");
      if (App.InstallTask) {
        InstallScheduledTask();
        Close();
      } else if (!Build()) {
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
