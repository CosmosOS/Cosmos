using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
    public class BuildUI {
        [DllImport("user32.dll")]
        protected static extern int ShowWindow(int Handle, int showState);

        [DllImport("kernel32.dll")]
        protected static extern int GetConsoleWindow();

        protected Builder mBuilder = new Builder();
        protected int mConsoleWindow;
        protected Options mOptions = new Options();
        protected MainWindow mMainWindow;
        protected OptionsUC mOptionsUC;
                
        protected void OptionsProceed() {
            // Call IL2CPU
            if (mOptionsUC.chbxCompileIL.IsChecked.Value) {
                if (mOptionsUC.chbxShowConsoleWindow.IsChecked.Value) {
                    ShowWindow(mConsoleWindow, 1);
                }
                var xBuildUC = new BuildUC();
                mMainWindow.LoadControl(xBuildUC);
                xBuildUC.CompileCompleted += new Action(BuildUC_CompileCompleted);
                xBuildUC.BeginBuild(mBuilder, mOptions, mOptionsUC.DebugMode, mOptionsUC.ComPort);
            }
        }

        protected void BuildUC_CompileCompleted() {
            mBuilder.Assemble();
            mBuilder.Link();
                
            // Debug Window is only displayed if Qemu + Debug checked
            // or if other VM + Debugport selected
            DebugWindow xDebugWindow = null;
            if (!mOptionsUC.rdioDebugModeNone.IsChecked.Value) {
                xDebugWindow = new DebugWindow();
                
                if (mOptionsUC.DebugMode == DebugModeEnum.Source) {
                    var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(
                        mBuilder.BuildPath + "output.bin"
                        , mBuilder.ToolsPath + @"cygwin\objdump.exe");
                    var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping
                        , mBuilder.BuildPath + "Tools/asm/debug.cxdb");
                          
                    DebugConnector xDebugConnector;
                    if (mOptionsUC.rdioQEMU.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorQEMU();
                    } else if (mOptionsUC.rdioVMWare.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorVMWare();
                    } else {
                        throw new Exception("TODO: Make a connector for raw serial");
                    }
                    xDebugWindow.SetSourceInfoMap(xSourceMappings, xDebugConnector);
                } else {
                    throw new Exception("Debug mode not supported: " + mOptionsUC.DebugMode);
                }
            }

            // Launch emulators or other final actions
            if (mOptionsUC.rdioQEMU.IsChecked.Value) {
                mBuilder.MakeQEMU(mOptionsUC.chbxQEMUUseHD.IsChecked.Value,
                                  mOptionsUC.chbxQEMUUseGDB.IsChecked.Value,
                                  mOptionsUC.DebugMode != DebugModeEnum.None,
                                  mOptionsUC.chckQEMUUseNetworkTAP.IsChecked.Value,
                                  mOptionsUC.cmboNetworkCards.SelectedValue,
                                  mOptionsUC.cmboAudioCards.SelectedValue);
            } else if (mOptionsUC.rdioVMWare.IsChecked.Value) {
                mBuilder.MakeVMWare(mOptionsUC.rdVMWareServer.IsChecked.Value);
            } else if (mOptionsUC.rdioVPC.IsChecked.Value) {
                mBuilder.MakeVPC();
            } else if (mOptionsUC.rdioISO.IsChecked.Value) {
                mBuilder.MakeISO();
            } else if (mOptionsUC.rdioPXE.IsChecked.Value) {
                mBuilder.MakePXE();
            } else if (mOptionsUC.rdioUSB.IsChecked.Value) {
                mBuilder.MakeUSB(mOptionsUC.cmboUSBDevice.Text[0]);
            }
            // Problems around with DebugWindow getting stuck - seeing if this helps by hiding 
            // instead of closing
            mMainWindow.Hide();
            if (xDebugWindow != null) {
                xDebugWindow.ShowDialog();
            }
            mMainWindow.Close();
        }

        protected void OptionsStop() {
            mMainWindow.Close();
        }

        protected void Execute() {
            // Hide the console window
            mConsoleWindow = GetConsoleWindow();
            ShowWindow(mConsoleWindow, 0);
            
            var xApp = new System.Windows.Application();
            xApp.Startup += new StartupEventHandler(xApp_Startup);
            // If an exception occurs here, something bad happened in final stages of build.
            // Or you forgot to close QEMU last time and this happens when debugging and you try
            // to run QEMU again.
            xApp.Run();        
        }

        void xApp_Startup(object sender, StartupEventArgs e) {
            // Create here, after we hide console Window so it gets hidden quickly
            mMainWindow = new MainWindow();
            mMainWindow.Loaded += new RoutedEventHandler(mMainWindow_Loaded);
            mMainWindow.Show();
        }

        void mMainWindow_Loaded(object sender, RoutedEventArgs e) {
            mOptions.Load();
            mOptionsUC = new OptionsUC(mBuilder.BuildPath, mOptions);
            mOptionsUC.Proceed = OptionsProceed;
            mOptionsUC.Stop = OptionsStop;
            mMainWindow.LoadControl(mOptionsUC);
        }

        public static void Run() {
            var xBuildUI = new BuildUI();
            xBuildUI.Execute();
        }
        
    }
}
