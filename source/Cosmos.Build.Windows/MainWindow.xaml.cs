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
using System.Windows.Shapes;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        // TODO: Move this into BuildUC
        public bool PhaseBuild(Builder aBuilder, DebugModeEnum aDebugMode, byte aComPort) {
            var xBuildUC = new BuildUC();
            xBuildUC.Height = float.NaN;
            xBuildUC.Width = float.NaN;
            Content = xBuildUC;

            IEnumerable<BuildLogMessage> xMessages = new BuildLogMessage[0];
            aBuilder.PreventFreezing += xBuildUC.PreventFreezing;
            aBuilder.DebugLog += xBuildUC.DoDebugMessage;
            aBuilder.ProgressChanged += xBuildUC.DoProgressMessage;
            try {
                aBuilder.Compile(aDebugMode, aComPort);

                aBuilder.DebugLog -= xBuildUC.DoDebugMessage;
                aBuilder.ProgressChanged -= xBuildUC.DoProgressMessage;

                xMessages = (from item in xBuildUC.Messages
                             where item.Severity != LogSeverityEnum.Informational
                             select item).ToArray();

                //If there were any warnings or errors, then show dialog again
                if (xMessages.Count() > 0) {
                    xBuildUC.BuildRunning = false;
                    return false;
                }
            } catch (Exception E) {
                var xTheMessages = (from item in xBuildUC.Messages
                                    where item.Severity != LogSeverityEnum.Informational
                                    select item).ToList();
                xTheMessages.Add(new BuildLogMessage() {
                    Severity = LogSeverityEnum.Error,
                    Message = E.ToString()
                });
                xBuildUC.Messages.Clear();
                foreach (var item in xTheMessages) {
                    xBuildUC.Messages.Add(item);
                }
                return false;
            }
            Close();
            return true;
        }
    }
}
