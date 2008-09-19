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
		public LogSeverityEnum Severity { get; set; }
		public string Message { get; set; }
	}
	
    public class BuildLogMessages : ObservableCollection<BuildLogMessage> {
		public BuildLogMessages() { }
	}

    public partial class BuildUC : UserControl {
        public BuildUC() {
            InitializeComponent();
        }

		public void DoDebugMessage(LogSeverityEnum aSeverity, string aMessage) {
			if (aSeverity == LogSeverityEnum.Informational) {
				return;
			}
            //Dispatcher.BeginInvoke(DispatcherPriority.Input,
            //                       new DispatcherOperationCallback(delegate(object aTheMessage) {
            //                                                           Messages.Add((BuildLogMessage)aTheMessage);
            //                                                           lboxLog.ScrollIntoView((BuildLogMessage)aTheMessage);
            //                                                           return null;
            //                                                       }),
            //                       xMessage);
            //var xFrame = new DispatcherFrame();
            //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam) {
            //    xFrame.Continue = false;
            //    return null;
            //}), null);
            //Dispatcher.PushFrame(xFrame);
		}

        private int mMax;
        private int mCurrent;

        public void PreventFreezing() {
            var xFrame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object aParam)
            {
                xFrame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(xFrame);
        }
        
        protected delegate void ProgressMessageReceivedDelegate(string aMsg);
        protected void ProgressMessageReceived(string aMsg) {
            listProgress.Items.Add(aMsg);
            // Old code
            pbarMain.Maximum = mMax;
            pbarMain.Value = mCurrent;
        }
        
        public void DoProgressMessage(int aMax, int aCurrent, string aMessage) {
            mCurrent = aCurrent;
            mMax = aMax;
            Dispatcher.BeginInvoke(DispatcherPriority.Input
                , new ProgressMessageReceivedDelegate(ProgressMessageReceived), aMessage);
            PreventFreezing();
        }

    }
}
