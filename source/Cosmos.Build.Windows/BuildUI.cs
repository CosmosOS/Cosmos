using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
    public class BuildUI {
        [DllImport("user32.dll")]
        protected static extern int ShowWindow(int Handle, int showState);

        [DllImport("kernel32.dll")]
        protected static extern int GetConsoleWindow();

        public static void Run() {
            // Hide the console window
            int xConsoleWindow = GetConsoleWindow();
            ShowWindow(xConsoleWindow, 0);

            var xBuilder = new Builder();

            var xOptionsWindow = new OptionsWindow();
            if (!xOptionsWindow.Display(xBuilder.BuildPath)) {
                return;
            }
            
            // Call IL2CPU
            if (xOptionsWindow.chbxCompileIL.IsChecked.Value) {
                //TODO: Eventually eliminate the console window completely
                if (xOptionsWindow.chbxShowConsoleWindow.IsChecked.Value) {
                    ShowWindow(xConsoleWindow, 1);
                }
                var xMainWindow = new MainWindow();
                xMainWindow.Show();
                var xBuildUC = new BuildUC();
                xMainWindow.Content = xBuildUC;
                if (!xBuildUC.Display(xBuilder, xOptionsWindow.DebugMode, xOptionsWindow.ComPort)) {
                    return;
                }
                xMainWindow.Close();
            }
            
            DebugWindow xDebugWindow = null;
            // Debug Window is only displayed if Qemu + Debug checked
            // or if other VM + Debugport selected
            if (!xOptionsWindow.rdioDebugModeNone.IsChecked.Value) {
                xDebugWindow = new DebugWindow();
                if (xOptionsWindow.DebugMode == DebugModeEnum.Source) {
                    var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(
                        xBuilder.BuildPath + "output.bin"
                        , xBuilder.ToolsPath + @"cygwin\objdump.exe");
                    var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping
                        , xBuilder.BuildPath + "Tools/asm/debug.cxdb");
                          
                    DebugConnector xDebugConnector;
                    if (xOptionsWindow.rdioQEMU.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorQEMU();
                    } else if (xOptionsWindow.rdioVMWare.IsChecked.Value) {
                        xDebugConnector = new DebugConnectorVMWare();
                    } else {
                        throw new Exception("TODO: Make a connector for raw serial");
                    }
                    xDebugWindow.SetSourceInfoMap(xSourceMappings, xDebugConnector);
                } else {
                    throw new Exception("Debug mode not supported: " + xOptionsWindow.DebugMode);
                }
            }

            // Launch emulators or other final actions
            if (xOptionsWindow.rdioQEMU.IsChecked.Value) {
                // Uncomment if problems with QEMU to see output
                // TODO: Capture and send to debug window
                //ShowWindow(xConsoleWindow, 1);
                xBuilder.MakeQEMU(xOptionsWindow.chbxQEMUUseHD.IsChecked.Value,
                                  xOptionsWindow.chbxQEMUUseGDB.IsChecked.Value,
                                  xOptionsWindow.DebugMode != DebugModeEnum.None,
                                  xOptionsWindow.chckQEMUUseNetworkTAP.IsChecked.Value,
                                  xOptionsWindow.cmboNetworkCards.SelectedValue,
                                  xOptionsWindow.cmboAudioCards.SelectedValue);
            } else if (xOptionsWindow.rdioVMWare.IsChecked.Value) {
                xBuilder.MakeVMWare(xOptionsWindow.rdVMWareServer.IsChecked.Value);
            } else if (xOptionsWindow.rdioVPC.IsChecked.Value) {
                xBuilder.MakeVPC();
            } else if (xOptionsWindow.rdioISO.IsChecked.Value) {
                xBuilder.MakeISO();
            } else if (xOptionsWindow.rdioPXE.IsChecked.Value) {
                xBuilder.MakePXE();
            } else if (xOptionsWindow.rdioUSB.IsChecked.Value) {
                xBuilder.MakeUSB(xOptionsWindow.cmboUSBDevice.Text[0]);
            }

            if (xDebugWindow != null) {
                xDebugWindow.ShowDialog();
            }
        }
    }
}
