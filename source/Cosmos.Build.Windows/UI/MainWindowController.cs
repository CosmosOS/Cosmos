using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU;

namespace Cosmos.Compiler.Builder
{
    /// <summary>
    /// MVP Model View Poo  patern, Clean model and View crap goes here.
    /// </summary>
    internal class MainWindowController 
    {
        IMainWindow mainWindow;
        IBuilder mBuilder = new Builder(); // hook to the logic
        bool buildErrors = false; 


        internal MainWindowController(IMainWindow mainWindow)
        {
            this.mainWindow = mainWindow; 
        }
        internal void Init()
        {
            BuildOptions options = BuildOptions.Load();
            options.BuildPath = mBuilder.BuildPath;

            mainWindow.OptionUC.Options = options;

            mBuilder.BuildProgress += new Action<BuildProgress>(mainWindow.BuildProgressUC.BuildProgressChanged);


            mBuilder.BuildCompleted += new Action(mBuilder_BuildCompleted);
                    
        }


       
        internal void StartBuild()
        {
            // get build option 
            BuildOptions options = mainWindow.OptionUC.Options;
            options.Save(); 

            //start build

            Proceed(options);
        }

        private void Proceed(BuildOptions options)
        {
            // Call IL2CPU
            if (options.CompileIL == true)
            {
                  //mBuilder.UseInternalAssembler = options.UmOptionsUC.chbxUseInternalAssembler.IsChecked.Value;
                mainWindow.ShowBuildProgress();

                //get Build internal to do it all and not just return on complete and do phase2 
               // xBuildUC.CompileCompleted += new Action(BuildUC_CompileCompleted);
       
                mBuilder.CompileCompleted+=new Action(mBuilder_CompileCompleted); 
                mBuilder.LogMessage+=new Action<LogSeverityEnum,string>(mBuilder_LogMessage);


                mBuilder.BeginCompile(options);
            }
        }

      

        void mBuilder_LogMessage(LogSeverityEnum severity, string msg)
        {
            //log to log
            mainWindow.AddToLog(msg);

            if (severity == LogSeverityEnum.Error)
            {
                mainWindow.BuildProgressUC.NewError(msg);
                buildErrors = true;
            }
        }

        void mBuilder_CompileCompleted()
        {

            //throw new NotImplementedException();
            new ConsoleWindow().ShowWindow(); 
        }


        void mBuilder_BuildCompleted()
        {
            mainWindow.Dispatcher.Invoke(new Action(PostBuildUI));

            if (!mBuilder.HasErrors)
            {
                mainWindow.ThreadedClose();  //HACK //TODO consider making the controller survive the window. 
            }
        }


        void PostBuildUI()
        {

            new ConsoleWindow().HideWindow();

            //HACK need to fire Debugger window in event 

            // Problems around with DebugWindow getting stuck, this seems to work
            //mMainWindow.Hide();
            if (mBuilder.DebugWindow != null)
            {
                // Beginnings of experiment to host QEMU
                //if (xQEMU != null) {
                //    IntPtr xDbgHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                //    SetParent(xQEMU.MainWindowHandle, xDbgHandle);
                //}
                mBuilder.DebugWindow.Show();
            }
            //mMainWindow.Close();
        }




        internal void CancelBuild()
        {
            //TODO should stop thread
            mainWindow.ThreadedClose(); 


        }




    }
}
