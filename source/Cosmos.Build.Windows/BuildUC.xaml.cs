using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Indy.IL2CPU;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Cosmos.Build.Windows {

    public class BuildLogMessage {
		public LogSeverityEnum Severity {
			get;
			set;
		}
		public string Message {
			get;
			set;
		}
	}
	
    public class BuildLogMessages : ObservableCollection<BuildLogMessage> {
		public BuildLogMessages() { }
	}

    public partial class BuildUC : UserControl {
        public BuildUC() {
            InitializeComponent();
            Messages = (BuildLogMessages)((ObjectDataProvider)FindResource("BuildMessages")).Data;
            if (Messages == null) {
                throw new Exception("Message collection not found!");
            }

            mTimer = new DispatcherTimer(DispatcherPriority.Input);
            mTimer.Tick += mTimer_Tick;
            mTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);// 100msec
            mTimer.Start();
            //var xSyncContext = new DispatcherSynchronizationContext();
            //SynchronizationContext.SetSynchronizationContext(xSyncContext);
            //mWorker = new BackgroundWorker();
            //mWorker.WorkerReportsProgress = true;
            //mWorker.WorkerSupportsCancellation = false;
            //mWorker.ProgressChanged += delegate(object aSender,
            //                                    ProgressChangedEventArgs e)
            //{
            //                               do {
            //                                   string xRemainingTime = String.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
            //                                                                         "{0:T}",
            //                                                                         new DateTime(CalculateRemainingTime(mCurrent,
            //                                                                                                             mMax).Ticks));

            //                                   progressText.Content = String.Format("Processing method {0:d} of {1:d}{2}({3} remaining){4}{5} {6} - {7}",
            //                                                                        mCurrent,
            //                                                                        mMax,
            //                                                                        Environment.NewLine,
            //                                                                        xRemainingTime,
            //                                                                        Environment.NewLine,
            //                                                                        mMessage, mCurrent, mMax);
            //                                   ProgressMax = mMax;
            //                                   ProgressCurrent = mCurrent;
            //                                   Thread.Sleep(100);
            //                                   //var xFrame = new DispatcherFrame();
            //                                   //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam)
            //                                   //{
            //                                   //    xFrame.Continue = false;
            //                                   //    return null;
            //                                   //}), null);
            //                                   //Dispatcher.PushFrame(xFrame);
            //                               } while ((mCurrent < mMax) && mMax >0);
            //};
            //mWorker.DoWork += delegate
            //{
            //    do
            //    {
            //        Thread.Sleep(10);
            //        mWorker.ReportProgress(1);
            //    } while (mCurrent < mMax);
            //};
            //mWorker.RunWorkerAsync();
        }

        private BackgroundWorker mWorker;

        void mTimer_Tick(object sender, EventArgs e)
        {
            string xRemainingTime = String.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                                                  "{0:T}",
                                                  new DateTime(CalculateRemainingTime(mCurrent,
                                                                                      mMax).Ticks));

            progressText.Content = String.Format("Processing method {0:d} of {1:d}{2}({3} remaining){4}{5} {6} - {7}",
                                                 mCurrent,
                                                 mMax,
                                                 Environment.NewLine,
                                                 xRemainingTime,
                                                 Environment.NewLine,
                                                 mMessage, mCurrent, mMax);
            ProgressMax = mMax;
            ProgressCurrent = mCurrent;

        }

        private DispatcherTimer mTimer;

		public readonly BuildLogMessages Messages;
		public int ProgressMax {
			get {
				return (int)progressBuild.Maximum;
			}
			set {
				progressBuild.Maximum = value;
			}
		}

		public int ProgressCurrent {
			get {
				return (int)progressBuild.Value;
			}
			set {
				progressBuild.Value = value;
			}
		}

		public void DoDebugMessage(LogSeverityEnum aSeverity, string aMessage) {
			if (aSeverity == LogSeverityEnum.Informational) {
				return;
			}
			var xMessage = new BuildLogMessage() {
				Severity = aSeverity,
				Message = aMessage
			};
		    Dispatcher.BeginInvoke(DispatcherPriority.Input,
		                           new DispatcherOperationCallback(delegate(object aTheMessage) {
		                                                               Messages.Add((BuildLogMessage)aTheMessage);
		                                                               lboxLog.ScrollIntoView((BuildLogMessage)aTheMessage);
		                                                               return null;
		                                                           }),
		                           xMessage);
			var xFrame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam) {
				xFrame.Continue = false;
				return null;
			}), null);
			Dispatcher.PushFrame(xFrame);
		}

        private int mMax;
        private int mCurrent;
        private string mMessage;

        public void PreventFreezing() {
            var xFrame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam)
            {
                xFrame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(xFrame);
        }

        public void DoProgressMessage(int aMax, int aCurrent, string aMessage)
        {
            mCurrent = aCurrent;
            mMax = aMax;
            mMessage = aMessage;
            PreventFreezing();
//            var xFrame = new DispatcherFrame();
//            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
//                                                     new DispatcherOperationCallback(delegate(object aData)
//            {
//                string xRemainingTime = String.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
//"{0:T}", new DateTime(CalculateRemainingTime(mCurrent, mMax).Ticks));

//                progressText.Content = String.Format("Processing method {0:d} of {1:d}{2}({3} remaining){4}{5}",
//                                                                  mCurrent,
//                                                                  mMax,
//                                                                  Environment.NewLine,
//                                                                  xRemainingTime,
//                                                                  Environment.NewLine,
//                                                                  mMessage);
//                ProgressMax = mMax;
//                ProgressCurrent = mCurrent;
//                //xFrame.Continue = false;
//                return null;
//            }), null);
//            Dispatcher.PushFrame(xFrame);
        }

        public bool BuildRunning = true;

        public static System.Diagnostics.Stopwatch xBuildTimer;
        public TimeSpan CalculateRemainingTime(int completedCount, int totalCount)
        {
            if(!BuildRunning) {
                xBuildTimer.Stop();
            }
            if (xBuildTimer == null)
            {
                xBuildTimer = new System.Diagnostics.Stopwatch();
                xBuildTimer.Start();
            }

            if (totalCount <= 0) //Avoid Divide by Zero
                totalCount = 1;
            long percentComplete = ((completedCount * 100 / totalCount));
            //Avoid Divide by Zero
            if (percentComplete <= 0) {
                percentComplete = 1;
            }

            long remaining = ((xBuildTimer.ElapsedMilliseconds) / percentComplete) * (100 - percentComplete);

            return new TimeSpan(remaining * 1000 * 10);
        }
    }
}
