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

namespace Cosmos.Compiler.Builder
{

    public partial class BuildUC : UserControl
    {
        public BuildUC()
        {
            InitializeComponent();
        }

        protected Builder mBuilder;

        public void BeginBuild(Builder aBuilder, DebugMode aDebugMode, byte aComPort)
        {
            mBuilder = aBuilder;
            aBuilder.CompilingMethods += new Action<int, int>(Engine_CompilingMethods);
            aBuilder.CompilingStaticFields += new Action<int, int>(Engine_CompilingStaticFields);
            aBuilder.CompileCompleted += aBuilder_CompileCompleted;
            aBuilder.LogMessage += aBuilder_LogMessage;
            aBuilder.BeginCompile(aDebugMode, aComPort);
        }

        protected void aBuilder_LogMessage(LogSeverityEnum aSeverity, string aMessage)
        {
            var xAction = (Action)delegate()
            {
                listErrors.Items.Add(String.Format("{0} - {1}",
                                               aSeverity,
                                               aMessage));
            };
            // Do not use BeginInvoke - if BeginInvoke is used these stack up 
            // and continue to come in and tie up the main thread after the engine completes
            // and the window is closed
            Dispatcher.Invoke(xAction);

        }

        protected void Engine_CompilingStaticFields(int aValue, int aMax)
        {
            var xAction = (Action)delegate()
            {
                progStaticFieldsProcessed.Maximum = aMax;
                progStaticFieldsProcessed.Value = aValue;
            };
            // Do not use BeginInvoke - if BeginInvoke is used these stack up 
            // and continue to come in and tie up the main thread after the engine completes
            // and the window is closed
            Dispatcher.Invoke(xAction);
        }

        protected void Engine_CompilingMethods(int aValue, int aMax)
        {
            var xAction = (Action)delegate()
            {
                if (progMethodsScanned.Value < 100)
                {
                    progMethodsScanned.Value = 100;
                }
                progMethodsProcessed.Maximum = aMax;
                progMethodsProcessed.Value = aValue;
            };
            // Do not use BeginInvoke - if BeginInvoke is used these stack up 
            // and continue to come in and tie up the main thread after the engine completes
            // and the window is closed
            Dispatcher.Invoke(xAction);
        }

        public event Action CompileCompleted;

        protected void aBuilder_CompileCompleted()
        {
            Dispatcher.BeginInvoke(
             (Action)delegate()
            {
                if (listErrors.Items.Count == 0)
                {
                    CompileCompleted.Invoke();
                }
            });
        }

        private void listErrors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listErrors.SelectedItem == null)
            {
                return;
            }
            Clipboard.SetText((string)listErrors.SelectedItem);
        }

    }
}
