using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using Cosmos.Build.Builder.Models;
using Cosmos.Build.Installer;

namespace Cosmos.Build.Builder.Views
{
    public partial class MainWindow : Window
    {
        private static readonly string[] NewLineStringArray = new string[] { Environment.NewLine };

        string mCosmosDir;
        string mSetupPath;
        // Needs updating with each new release.
        int mReleaseNo = 106027;

        public MainWindow()
        {
            InitializeComponent();
        }

        bool mPreventAutoClose = false;
        StringBuilder mClipboard = new StringBuilder();
        DispatcherTimer mCloseTimer;

        public bool Build()
        {
            Log.LogLine += new Log.LogLineHandler(Log_LogLine);
            Log.LogSection += new Log.LogSectionHandler(Log_LogSection);
            Log.LogError += new Log.LogErrorHandler(Log_LogError);

            if (App.BuilderConfiguration.UserKit)
            {
                mReleaseNo = Int32.Parse(DateTime.Now.ToString("yyyyMMdd"));
            }
            if (mPreventAutoClose)
            {
                return true;
            }

            var xTask = new CosmosTask(mCosmosDir, mReleaseNo);

            var xThread = new System.Threading.Thread(delegate ()
            {
                xTask.Run();
                ThreadDone();
            });
            xThread.Start();

            return true;
        }

        void ThreadDone()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                if (App.BuilderConfiguration.StayOpen == false)
                {
                    mCloseTimer = new DispatcherTimer();
                    mCloseTimer.Interval = TimeSpan.FromSeconds(5);
                    mCloseTimer.Tick += delegate
                    {
                        mCloseTimer.Stop();
                        if (mPreventAutoClose)
                        {
                            if (WindowState == WindowState.Minimized)
                            {
                                WindowState = WindowState.Normal;
                            }
                        }
                        else
                        {
                            Close();
                        }
                    };
                    mCloseTimer.Start();
                }
            });
        }

        void ClearTail() => DoOnViewModel(vm => vm.TailItems.Clear());

        void Log_LogError()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                ClearTail();

                DoOnViewModel(vm => vm.CurrentSection?.SetError());
                
                mPreventAutoClose = true;
            });
        }

        void Log_LogLine(string aLine)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                WriteTail(aLine);

                DoOnViewModel(vm => vm.CurrentSection?.LogMessage(aLine));

                mClipboard.AppendLine(aLine);
            });
        }

        void Log_LogSection(string name)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                DoOnViewModel(vm => vm.Sections.Add(new Section(name)));

                ClearTail();

                mClipboard.AppendLine();
                mClipboard.AppendLine(new string('=', name.Length));
                mClipboard.AppendLine(name);
                mClipboard.AppendLine(new string('=', name.Length));
                mClipboard.AppendLine();
            });
        }

        void WriteTail(string aText) => DoOnViewModel(
            vm =>
            {
                foreach (var line in aText.Split(NewLineStringArray, StringSplitOptions.None))
                {
                    vm.TailItems.Push(line);
                }
            });

        protected bool mLoaded = false;
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mLoaded = true;

            DoOnViewModel(vm => vm.LogBuilder = mClipboard);

            string xAppPath = AppContext.BaseDirectory;
            mCosmosDir = Path.GetFullPath(xAppPath + @"..\..\..\..\..\");
            mSetupPath = Path.Combine(mCosmosDir, @"Setup\Output\" + CosmosTask.GetSetupName(mReleaseNo) + ".exe");
            if (!Build())
            {
                Close();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // User had non minimized window, or maximized it, or otherwise manually intervened.
            // Even if starting minimized, this event gets called with Normal before load.
            // This is why we have mLoaded.
            if (mLoaded && WindowState != WindowState.Minimized)
            {
                mPreventAutoClose = true;
            }
        }

        private void DoOnViewModel(Action<ViewModels.MainWindowViewModel> action)
        {
            if (DataContext is ViewModels.MainWindowViewModel viewModel)
            {
                action(viewModel);
            }
        }
    }
}
