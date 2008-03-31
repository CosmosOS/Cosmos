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
	public class BuildLogMessages: ObservableCollection<BuildLogMessage> {
		public BuildLogMessages() {
		}
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

		public void DoDebugMessage(LogSeverityEnum aSeverity, string aMessage) {
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
    }
}
