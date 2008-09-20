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

    public partial class BuildUC : UserControl {
        public BuildUC() {
            InitializeComponent();
        }

        protected Builder mBuilder;
        
        public void BeginBuild(Builder aBuilder, DebugModeEnum aDebugMode, byte aComPort) {
            mBuilder = aBuilder;
            aBuilder.Engine.ProgressChanged += DoProgressMessage;
            aBuilder.CompileCompleted += new Action(aBuilder_CompileCompleted);
            aBuilder.BeginCompile(aDebugMode, aComPort);
        }

        public event Action CompileCompleted;

        protected void aBuilder_CompileCompleted() {
            Dispatcher.BeginInvoke(
             (Action)delegate() {
                mBuilder.Engine.ProgressChanged -= DoProgressMessage;
                CompileCompleted.Invoke();
             }
            );
        }

        protected void ProgressMessageReceived(string aMsg) {
            listProgress.SelectedIndex = listProgress.Items.Add(aMsg);
            listProgress.ScrollIntoView(listProgress.Items[listProgress.SelectedIndex]);
        }
        
        public void DoProgressMessage(string aMessage) {
            var xAction = (Action)delegate() { 
                ProgressMessageReceived(aMessage); 
            };
            // Do not use BeginInvoke - if BeginInvoke is used these stack up 
            // and continue to come in and tie up the main thread after the engine completes
            // and the window is closed
            Dispatcher.Invoke(xAction);
        }

    }
}
