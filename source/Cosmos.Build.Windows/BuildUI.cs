using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using Indy.IL2CPU;
using System.Threading;

namespace Cosmos.Compiler.Builder {
    public class BuildUI {
        [DllImport("user32.dll")]
        protected static extern int ShowWindow(int aHandle, int aShowState);

        [DllImport("kernel32.dll")]
        protected static extern int GetConsoleWindow();

        [DllImport("user32.dll")]
        protected static extern IntPtr SetParent(IntPtr aWndChild, IntPtr aWndNewParent);

        protected Builder mBuilder = new Builder();
        protected int mConsoleWindow;
        protected Options mOptions = new Options();
        protected MainWindow mMainWindow;
        protected OptionsUC mOptionsUC;
                
        protected void OptionsProceed() {
            // Call IL2CPU
            if (mOptionsUC.chbxCompileIL.IsChecked.Value) {
                var xBuildUC = new BuildUC();
                mBuilder.UseInternalAssembler = mOptionsUC.chbxUseInternalAssembler.IsChecked.Value;
                mMainWindow.LoadControl(xBuildUC);
                xBuildUC.CompileCompleted += new Action(BuildUC_CompileCompleted);
                xBuildUC.BeginBuild(mBuilder, mOptionsUC.DebugMode, mOptionsUC.ComPort, mOptionsUC.chbxQEMUUseGDB.IsChecked.GetValueOrDefault());
            }
        }

        protected void BuildUC_CompileCompleted() {
            if (mOptionsUC.chbxCompileIL.IsChecked.Value) {
                // We always show the window now since when its shown its
                // for a short time and not in "paralell" as it was before.
                if (!mOptionsUC.chbxUseInternalAssembler.IsChecked.Value) {
                    ShowWindow(mConsoleWindow, 1);
                    mBuilder.Assemble();
                    mBuilder.Link();
                    ShowWindow(mConsoleWindow, 0);
                }
            }
                
            DebugWindow xDebugWindow = null;
            System.Diagnostics.Process xQEMU = null;
            if (mOptionsUC.rdioDebugModeNone.IsChecked.Value == false) {
                xDebugWindow = new DebugWindow();
                if (mOptionsUC.DebugMode == DebugMode.Source) {
                    /*var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(
                        mBuilder.BuildPath + "output.bin", mBuilder.ToolsPath + @"cygwin\objdump.exe");*/
                    var xLabelByAddressMapping = SourceInfo.ParseFile(mBuilder.BuildPath);
                    var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping
                       , mBuilder.BuildPath + "Tools/asm/debug.cxdb");
                          
                    DebugConnector xDebugConnector=null;
                    if (mOptionsUC.rdioQEMU.IsChecked.Value){
                        if (mOptionsUC.cmbDebugComMode.Text ==
                        "TCP: Cosmos Debugger as server on port 4444, QEmu as client")
                        {
                            xDebugConnector = new DebugConnectorTCPServer();
                            Thread.Sleep(250);
                            xQEMU = mBuilder.MakeQEMU(Options.CreateHDImage, Options.UseGDB
                                , Options.DebugMode != DebugMode.None, (String)Options.QEmuDebugComType[Options.DebugComMode], Options.UseNetworkTAP
                                , (String)Options.QEmuNetworkCard[Options.NetworkCard], (String)Options.QEmuAudioCard[Options.AudioCard]);
                        }
                        
                        else if (mOptionsUC.cmbDebugComMode.Text ==
                        "TCP: Cosmos Debugger as client, QEmu as server on port 4444")
                        {
                            xQEMU = mBuilder.MakeQEMU(Options.CreateHDImage, Options.UseGDB
                                , Options.DebugMode != DebugMode.None, (String)Options.QEmuDebugComType[Options.DebugComMode], Options.UseNetworkTAP
                                , (String)Options.QEmuNetworkCard[Options.NetworkCard], (String)Options.QEmuAudioCard[Options.AudioCard]);
                            xDebugConnector = new DebugConnectorTCPClient();
                        }
                        
                        else if (mOptionsUC.cmbDebugComMode.Text ==
                            "Named pipe: Cosmos Debugger as client, QEmu as server")
                        {
                            xQEMU = mBuilder.MakeQEMU(Options.CreateHDImage, Options.UseGDB
                                , Options.DebugMode != DebugMode.None, (String)Options.QEmuDebugComType[Options.DebugComMode], Options.UseNetworkTAP
                                , (String)Options.QEmuNetworkCard[Options.NetworkCard], (String)Options.QEmuAudioCard[Options.AudioCard]);
                            xDebugConnector = new DebugConnectorPipeClient();
                        }
                        else if (mOptionsUC.cmbDebugComMode.Text ==
                            "Named pipe: Cosmos Debugger as server, QEmu as client")
                        {
                            xDebugConnector = new DebugConnectorPipeServer();
                            xQEMU = mBuilder.MakeQEMU(Options.CreateHDImage, Options.UseGDB
                                , Options.DebugMode != DebugMode.None, (String)Options.QEmuDebugComType[Options.DebugComMode], Options.UseNetworkTAP
                                , (String)Options.QEmuNetworkCard[Options.NetworkCard], (String)Options.QEmuAudioCard[Options.AudioCard]);
                        }
                    } else if (mOptionsUC.rdioVMWare.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorPipeServer();
                        mBuilder.MakeVMWare(mOptionsUC.rdVMWareServer.IsChecked.Value);
                    } else if(mOptionsUC.rdioUSB.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorSerial(mOptionsUC.ComPort);
                    } else if(mOptionsUC.rdioPXE.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorSerial(mOptionsUC.ComPort);
                    }
                    else
                    {
                        throw new Exception("Debug mode not supported: " + mOptionsUC.DebugMode);
                    }
                    xDebugWindow.SetSourceInfoMap(xSourceMappings, xDebugConnector);
                }
                else
                {
                    throw new Exception("Debug mode not supported: " + mOptionsUC.DebugMode);
                }
            }else if (mOptionsUC.rdioQEMU.IsChecked.Value) {
                 xQEMU = mBuilder.MakeQEMU(Options.CreateHDImage,
                                          Options.UseGDB,
                                          false,
                                          "",
                                          Options.UseNetworkTAP,
                                          (String)Options.QEmuNetworkCard[Options.NetworkCard],
                                          (String)Options.QEmuAudioCard[Options.AudioCard]);
            }
            else if (mOptionsUC.rdioVMWare.IsChecked.Value)
            {
                mBuilder.MakeVMWare(mOptionsUC.rdVMWareServer.IsChecked.Value);
            }

            if (mOptionsUC.rdioVPC.IsChecked.Value)
            {
                mBuilder.MakeVPC();
            }
            else if (mOptionsUC.rdioISO.IsChecked.Value)
            {
                mBuilder.MakeISO();
            }
            else if (mOptionsUC.rdioPXE.IsChecked.Value)
            {
                mBuilder.MakePXE();
            }
            else if (mOptionsUC.rdioUSB.IsChecked.Value)
            {
                mBuilder.MakeUSB(mOptionsUC.cmboUSBDevice.Text[0]);
            }
            else if (mOptionsUC.rdioVHD.IsChecked.Value)
            {
                mBuilder.MakeVHD();
            }

            // Problems around with DebugWindow getting stuck, this seems to work
            mMainWindow.Hide();
            if (xDebugWindow != null) {
                // Beginnings of experiment to host QEMU
                //if (xQEMU != null) {
                //    IntPtr xDbgHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                //    SetParent(xQEMU.MainWindowHandle, xDbgHandle);
                //}
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
            Options.Load();
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