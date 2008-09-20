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
            aBuilder.Engine.CompilingMethods += new Action<int, int>(Engine_CompilingMethods);
            aBuilder.Engine.CompilingStaticFields += new Action<int, int>(Engine_CompilingStaticFields);
            aBuilder.CompileCompleted += new Action(aBuilder_CompileCompleted);
            aBuilder.BeginCompile(aDebugMode, aComPort);
        }

        protected void Engine_CompilingStaticFields(int aValue, int aMax) {
            var xAction = (Action)delegate() { 
                progStaticFieldsProcessed.Maximum = aMax;
                progStaticFieldsProcessed.Value = aValue;
            };
            // Do not use BeginInvoke - if BeginInvoke is used these stack up 
            // and continue to come in and tie up the main thread after the engine completes
            // and the window is closed
            Dispatcher.Invoke(xAction);
        }

        protected void Engine_CompilingMethods(int aValue, int aMax) {
            var xAction = (Action)delegate() { 
                if (progMethodsScanned.Value < 100) {
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

        protected void aBuilder_CompileCompleted() {
            Dispatcher.BeginInvoke(
             (Action)delegate() {
                CompileCompleted.Invoke();
             }
            );
        }

    }
}
