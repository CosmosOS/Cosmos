using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    public partial class BuildWindow : Window {
        public BuildWindow() {
            InitializeComponent();
			Messages = (BuildLogMessages)((ObjectDataProvider)FindResource("BuildMessages")).Data;
			if (Messages == null) {
				throw new Exception("Message collection not found!");
			}
        }

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
			Messages.Add(xMessage);
			lboxLog.ScrollIntoView(xMessage);
			var xFrame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam) {
				xFrame.Continue = false;
				return null;
			}), null);
			Dispatcher.PushFrame(xFrame);
		}

        public void DoProgressMessage(int aMax, int aCurrent, string aMessage)
        {
            string xRemainingTime = String.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                "{0:T}", new DateTime(CalculateRemainingTime(aCurrent, aMax).Ticks));

            progressText.Content = String.Format("Processing method {0:d} of {1:d}{2}({3} remaining){4}{5}",
                                                              aCurrent,
                                                              aMax,
                                                              Environment.NewLine,
                                                              xRemainingTime,
                                                              Environment.NewLine,
                                                              aMessage);
            ProgressMax = aMax;
            ProgressCurrent = aCurrent;
            var xFrame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input,
                                                     new DispatcherOperationCallback(delegate(object aParam)
            {
                xFrame.Continue = false;
                return null;
            }),
                                                     null);
            Dispatcher.PushFrame(xFrame);
        }


        public static System.Diagnostics.Stopwatch xBuildTimer;
        public TimeSpan CalculateRemainingTime(int completedCount, int totalCount)
        {
            if (xBuildTimer == null)
            {
                xBuildTimer = new System.Diagnostics.Stopwatch();
                xBuildTimer.Start();
            }

            if (totalCount <= 0) //Avoid Divide by Zero
                totalCount = 1;
            long percentComplete = ((completedCount * 100 / totalCount));
            if (percentComplete <= 0) //Avoid Divide by Zero
                percentComplete = 1;

            long remaining = ((xBuildTimer.ElapsedMilliseconds) / percentComplete) * (100 - percentComplete);

            return new TimeSpan(remaining * 1000 * 10);
        }
    }
}
