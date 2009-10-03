using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Threading;

namespace Cosmos.Compiler.Builder
{
    //Todo rename startup
    public class BuildUI
    {
       
        protected MainWindow mMainWindow;
    

        private ConsoleWindow consoleWindow; 


        protected void Execute()
        {
            // Hide the console window
            consoleWindow = new ConsoleWindow();
             consoleWindow.HideWindow(); 

            var xApp = new System.Windows.Application();
            xApp.Startup += new StartupEventHandler(xApp_Startup);
            // If an exception occurs here, something bad happened in final stages of build.
            // Or you forgot to close QEMU last time and this happens when debugging and you try
            // to run QEMU again.
            xApp.Run();
        }

        void xApp_Startup(object sender, StartupEventArgs e)
        {
            // Create here, after we hide console Window so it gets hidden quickly
            mMainWindow = new MainWindow();
            mMainWindow.ShowActivated = true; 
            mMainWindow.Show();
        }


         

        void UIEvents_StopButtonPressedEvent()
        {
            mMainWindow.Close();
        }


        public static void Run()
        {
            var xBuildUI = new BuildUI();
            xBuildUI.Execute();
        }
    }
}