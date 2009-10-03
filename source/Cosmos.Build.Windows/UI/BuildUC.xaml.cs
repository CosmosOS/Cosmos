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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Cosmos.IL2CPU;

namespace Cosmos.Compiler.Builder
{

    public partial class BuildUC : UserControl, IBuildProgressUC
    {
        readonly private BuildProgress buildProgress ;

        public BuildUC()
        {
            InitializeComponent();
            buildProgress = new BuildProgress();

        }

        protected Builder mBuilder;


        //void aBuilder_BuildInformationMessage(string obj)
        //{
        //    var xAction = (Action)delegate()
        //    {
        //        listInformation.Items.Add(obj);
        //    };
        //    // Do not use BeginInvoke - if BeginInvoke is used these stack up 
        //    // and continue to come in and tie up the main thread after the engine completes
        //    // and the window is closed
        //    Dispatcher.Invoke(xAction);
        //}

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

        private void Engine_CompilingStaticFields(int aValue, int aMax)
        {

            progStaticFieldsProcessed.Maximum = aMax;
            progStaticFieldsProcessed.Value = aValue;

        }

        private void Engine_CompilingMethods(int aValue, int aMax)
        {
            if (progMethodsScanned.Text != aMax.ToString())
                progMethodsScanned.Text = aMax.ToString(); 

            if (progMethodsProcessed.Maximum != aMax)
                progMethodsProcessed.Maximum = aMax;
            
            if (progMethodsProcessed.Value != aValue)
            progMethodsProcessed.Value = aValue;

        }

        public event Action CompileCompleted;

        protected void aBuilder_CompileCompleted()
        {
            Dispatcher.BeginInvoke(
             (Action)delegate()
            {
                if (listErrors.Items.Count == 0)
                    CompileCompleted.Invoke();

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





        public void NewError(string error)
        {
            Dispatcher.BeginInvoke(
            (Action)delegate()
            {

                listErrors.Items.Add(error);
            });

        }


        /// <summary>
        /// Tells us if there has been a noteworthy change.
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        bool HasChanged(BuildProgress progress)
        {
            if (progress.Step != buildProgress.Step)
                return true;

            if (buildProgress.MaxMethods == 0 && progress.MaxMethods != 0  
                || buildProgress.MaxFields == 0 && progress.MaxFields !=0)
                return true;

            if (progress.MethodProgressPercent != buildProgress.MethodProgressPercent)
                return true;

            if (progress.FieldProgressPercent != buildProgress.FieldProgressPercent)
                return true;


            return false;



        }

        public void BuildProgressChanged(BuildProgress progress)
        {
            if (HasChanged(progress) == false)
                return;

            if (progress.MaxMethods == 0)
            {
                System.Diagnostics.Debug.WriteLine("Test");
                return;
            }
            buildProgress.MaxFields = progress.MaxFields;
            buildProgress.MethodsProcessed = progress.MethodsProcessed;
            buildProgress.MaxMethods = progress.MaxMethods;
            buildProgress.FieldsProcessed = progress.FieldsProcessed;

            //invoke on UI 
            Dispatcher.BeginInvoke(
            (Action)delegate()
            {
                UpdateProgress();


                // CompileCompleted.Invoke();

            });

        }

        private void UpdateProgress()
        {
            if (buildProgress.Step != null)
                textStep.Text = buildProgress.Step;


            if (buildProgress.MaxFields != null && buildProgress.FieldsProcessed != null)
                Engine_CompilingStaticFields(buildProgress.FieldsProcessed.Value, buildProgress.MaxFields.Value);

            if (buildProgress.MaxMethods != null && buildProgress.MethodsProcessed != null && buildProgress.MaxMethods != 0)
                Engine_CompilingMethods(buildProgress.MethodsProcessed.Value, buildProgress.MaxMethods.Value);


        }


    }
}
