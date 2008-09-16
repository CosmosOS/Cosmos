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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Indy.IL2CPU;
using System.Windows.Threading;

namespace Cosmos.Build.Windows {
    public partial class OptionsWindow : Window {
        [DllImport("user32.dll")]
        public static extern int ShowWindow(int Handle, int showState);

        [DllImport("kernel32.dll")]
        public static extern int GetConsoleWindow();

        protected Block mOptionsBlockPrefix;
        protected Builder mBuilder = new Builder();
        protected string mLastSelectedUSBDrive;

        public void Display() {
            // Hide the console window
            int xConsoleWindow = GetConsoleWindow();
            ShowWindow(xConsoleWindow, 0);

            bool xDoBuild = true;
            var xShowOptions = chbxShowOptions.IsChecked.Value;
            // If the user doenst have the option to auto show, then look
            // for control key pressed
            if (!xShowOptions) {
                // We should use the WPF Keyboard.IsKeyDown, but it does not work here.
                // It appears that it gets initialized at some point later
                // or after a WPF window is shown, but it does not work here for sure
                // so instead we have to us an extern.
                xShowOptions = KeyState.IsKeyDown(System.Windows.Forms.Keys.RControlKey)
                    || KeyState.IsKeyDown(System.Windows.Forms.Keys.LControlKey);
            }
            if (xShowOptions) {
                xDoBuild = ShowDialog().Value;
            }
            if (xDoBuild) {
                SaveSettingsToRegistry();

                mComPort = (byte)cmboDebugPort.SelectedIndex;
                if (mComPort > 3) {
                    throw new Exception("Debug port not supported yet!");
                }
                mComPort++;
                if (rdioDebugModeNone.IsChecked.Value) {
                    mDebugMode = DebugModeEnum.None;
                } else if (rdioDebugModeIL.IsChecked.Value) {
                    mDebugMode = DebugModeEnum.IL;
                    throw new NotSupportedException("Debug mode IL isn't supported yet, use Source instead.");
                } else if (rdioDebugModeSource.IsChecked.Value) {
                    mDebugMode = DebugModeEnum.Source;
                    mComPort = 1;
                } else {
                    throw new Exception("Unknown debug mode.");
                }

                // Call IL2CPU
                if (chbxCompileIL.IsChecked.Value) {
                    //TODO: Eventually eliminate the console window completely
                    if (chbxShowConsoleWindow.IsChecked.Value) {
                        ShowWindow(xConsoleWindow, 1);
                    }
                    var xMainWindow = new MainWindow();
                    xMainWindow.Show();
                    if (xMainWindow.PhaseBuild(mBuilder, mDebugMode, mComPort) == false) {
                        return;
                    }
                }
                
                DebugWindow xDebugWindow = null;
                // Debug Window is only displayed if Qemu + Debug checked
                // or if other VM + Debugport selected
                if (!rdioDebugModeNone.IsChecked.Value) {
                    xDebugWindow = new DebugWindow();
                    if (mDebugMode == DebugModeEnum.Source) {
                        var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(
                            mBuilder.BuildPath + "output.bin"
                            , mBuilder.ToolsPath + @"cygwin\objdump.exe");
                        var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping
                            , mBuilder.BuildPath + "Tools/asm/debug.cxdb");
                              
                        DebugConnector xDebugConnector;
                        if (rdioQEMU.IsChecked.Value) {
                            xDebugConnector = new DebugConnectorQEMU();
                        } else if (rdioVMWare.IsChecked.Value) {
                            xDebugConnector = new DebugConnectorVMWare();
                        } else {
                            throw new Exception("TODO: Make a connector for raw serial");
                        }
                        xDebugWindow.SetSourceInfoMap(xSourceMappings, xDebugConnector);
                    } else {
                        throw new Exception("Debug mode not supported: " + mDebugMode);
                    }
                }

                // Launch emulators or other final actions
                if (rdioQEMU.IsChecked.Value) {
                    // Uncomment if problems with QEMU to see output
                    // TODO: Capture and send to debug window
                    //ShowWindow(xConsoleWindow, 1);
                    mBuilder.MakeQEMU(chbxQEMUUseHD.IsChecked.Value,
                                      chbxQEMUUseGDB.IsChecked.Value,
                                      mDebugMode != DebugModeEnum.None,
                                      chckQEMUUseNetworkTAP.IsChecked.Value,
                                      cmboNetworkCards.SelectedValue,
                                      cmboAudioCards.SelectedValue);
                } else if (rdioVMWare.IsChecked.Value) {
                    mBuilder.MakeVMWare(rdVMWareServer.IsChecked.Value);
                } else if (rdioVPC.IsChecked.Value) {
                    mBuilder.MakeVPC();
                } else if (rdioISO.IsChecked.Value) {
                    mBuilder.MakeISO();
                } else if (rdioPXE.IsChecked.Value) {
                    mBuilder.MakePXE();
                } else if (rdioUSB.IsChecked.Value) {
                    mBuilder.MakeUSB(cmboUSBDevice.Text[0]);
                }

                if (xDebugWindow != null) {
                    xDebugWindow.ShowDialog();
                }
            }
        }

        protected void TargetChanged(object aSender, RoutedEventArgs e) {
            // .IsReady takes quite some time, so instead of delaying
            // every run, instead we load it on demand
            if ((aSender == rdioUSB) && (cmboUSBDevice.Items.Count == 0)) {
                var xDrives = System.IO.Directory.GetLogicalDrives();
                foreach (string xDrive in xDrives) {
                    var xType = new System.IO.DriveInfo(xDrive);
                    if (xType.IsReady) {
                        if ((xType.DriveType == System.IO.DriveType.Removable)
                         && xType.DriveFormat.StartsWith("FAT")) {
                            cmboUSBDevice.Items.Add(xDrive);
                        }
                    }
                }
                cmboUSBDevice.SelectedIndex = cmboUSBDevice.Items.IndexOf(mLastSelectedUSBDrive);
            }

            spnlDebugger.Visibility = Visibility.Visible;
            // for now its hidden. VMWare and QEMU are hardcoded to Com1 for now
            // and the Comos Debugger doesnt have direct serial connectivity yet
            wpnlDebugPort.Visibility = Visibility.Collapsed;
            
            spnlQEMU.Visibility = aSender == rdioQEMU ? Visibility.Visible : Visibility.Collapsed;
            spnlVPC.Visibility = aSender == rdioVPC ? Visibility.Visible : Visibility.Collapsed;
            spnlISO.Visibility = aSender == rdioISO ? Visibility.Visible : Visibility.Collapsed;
            spnlPXE.Visibility = aSender == rdioPXE ? Visibility.Visible : Visibility.Collapsed;
            spnlUSB.Visibility = aSender == rdioUSB ? Visibility.Visible : Visibility.Collapsed;
            spnlVMWare.Visibility = aSender == rdioVMWare ? Visibility.Visible : Visibility.Collapsed;
        }

        public OptionsWindow() {
            InitializeComponent();

            Loaded += delegate(object sender, RoutedEventArgs e) {
                this.Activate();
            };

            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
            butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

            rdioQEMU.Checked += new RoutedEventHandler(TargetChanged);
            rdioVMWare.Checked += new RoutedEventHandler(TargetChanged);
            rdioVPC.Checked += new RoutedEventHandler(TargetChanged);
            rdioISO.Checked += new RoutedEventHandler(TargetChanged);
            rdioPXE.Checked += new RoutedEventHandler(TargetChanged);
            rdioUSB.Checked += new RoutedEventHandler(TargetChanged);

            tblkBuildPath.Text = mBuilder.BuildPath;
            tblkISOPath.Text = mBuilder.BuildPath + "Cosmos.iso";

            cmboDebugPort.Items.Add("Disabled");

            foreach (string xNIC in Enum.GetNames(typeof(Builder.QemuNetworkCard))) {
                cmboNetworkCards.Items.Add(xNIC);
            }
            foreach (string xSoundCard in Enum.GetNames(typeof(Builder.QemuAudioCard))) {
                cmboAudioCards.Items.Add(xSoundCard);
            }
            LoadSettingsFromRegistry();
        }

        private void butnBuild_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private DebugModeEnum mDebugMode;
        private byte mComPort;

        private void butnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        protected const string mRegKey = @"Software\Cosmos\User Kit";

        protected void SaveSettingsToRegistry() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(mRegKey)) {
                string xTarget = "QEMU";
                if (rdioVMWare.IsChecked.Value) {
                    xTarget = "VMWare";
                } else if (rdioVPC.IsChecked.Value) {
                    xTarget = "VPC";
                } else if (rdioISO.IsChecked.Value) {
                    xTarget = "ISO";
                } else if (rdioPXE.IsChecked.Value) {
                    xTarget = "PXE";
                } else if (rdioUSB.IsChecked.Value) {
                    xTarget = "USB";
                }
                xKey.SetValue("Target", xTarget);

                // Debug                              
                xKey.SetValue("Debug Port", cmboDebugPort.Text);
                string xDebugMode = "QEMU";
                if (rdioDebugModeNone.IsChecked.Value) {
                    xDebugMode = "None";
                } else if (rdioDebugModeIL.IsChecked.Value) {
                    xDebugMode = "IL";
                } else if (rdioDebugModeSource.IsChecked.Value) {
                    xDebugMode = "Source";
                }
                xKey.SetValue("Debug Mode", xDebugMode);

                // QEMU
                xKey.SetValue("Use GDB", chbxQEMUUseGDB.IsChecked.Value, RegistryValueKind.DWord);
                xKey.SetValue("Create HD Image", chbxQEMUUseHD.IsChecked.Value, RegistryValueKind.DWord);
                xKey.SetValue("Use network TAP", chckQEMUUseNetworkTAP.IsChecked.Value, RegistryValueKind.DWord);
                xKey.SetValue("Network Card",
                              cmboNetworkCards.Text,
                              RegistryValueKind.String);
                xKey.SetValue("Audio Card",
                              cmboAudioCards.Text,
                              RegistryValueKind.String);
                // VMWare
                xKey.SetValue("VMWare Edition", rdVMWareWorkstation.IsChecked.Value ? "Workstation" : "Server");

                // USB
                // Only save if selected since we lazy load the USB combo
                if (cmboUSBDevice.SelectedItem != null) {
                    xKey.SetValue("USB Device", cmboUSBDevice.Text);
                }

                // Misc
                xKey.SetValue("Show Options Window", chbxShowOptions.IsChecked.Value, RegistryValueKind.DWord);
                xKey.SetValue("Show Console Window", chbxShowConsoleWindow.IsChecked.Value, RegistryValueKind.DWord);
                xKey.SetValue("Compile IL", chbxCompileIL.IsChecked.Value, RegistryValueKind.DWord);                              
            }
        }

        private void LoadSettingsFromRegistry() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(mRegKey)) {
                string xBuildType = (string)xKey.GetValue("Target", "QEMU");
                switch (xBuildType) {
                    case "QEMU":
                        rdioQEMU.IsChecked = true;
                        break;
                    case "VMWare":
                        rdioVMWare.IsChecked = true;
                        break;
                    case "VPC":
                        rdioVPC.IsChecked = true;
                        break;
                    case "ISO":
                        rdioISO.IsChecked = true;
                        break;
                    case "PXE":
                        rdioPXE.IsChecked = true;
                        break;
                    case "USB":
                        rdioUSB.IsChecked = true;
                        break;
                }

                // Misc
                chbxShowOptions.IsChecked = ((int)xKey.GetValue("Show Options Window", 1) != 0);
                chbxShowConsoleWindow.IsChecked = ((int)xKey.GetValue("Show Console Window", 0) != 0);
                chbxCompileIL.IsChecked = ((int)xKey.GetValue("Compile IL", 1) != 0);

                // Debug                
                cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(xKey.GetValue("Debug Port", ""));
                if (cmboDebugPort.SelectedIndex == -1) {
                    cmboDebugPort.SelectedIndex = 0;
                }
                string xDebugMode = (string)xKey.GetValue("Debug Mode", "None");
                switch (xDebugMode) {
                    case "None":
                        rdioDebugModeNone.IsChecked = true;
                        break;
                    case "IL":
                        rdioDebugModeIL.IsChecked = true;
                        break;
                    case "Source":
                        rdioDebugModeSource.IsChecked = true;
                        break;
                }

                // QEMU
                chbxQEMUUseGDB.IsChecked = ((int)xKey.GetValue("Use GDB", 0) != 0);
                chbxQEMUUseHD.IsChecked = ((int)xKey.GetValue("Create HD Image", 0) != 0);
                chckQEMUUseNetworkTAP.IsChecked = ((int)xKey.GetValue("Use network TAP", 0) != 0);
                cmboNetworkCards.SelectedIndex = cmboNetworkCards.Items.IndexOf(xKey.GetValue("Network Card"
                 , Builder.QemuNetworkCard.rtl8139.ToString()));
                cmboAudioCards.SelectedIndex = cmboAudioCards.Items.IndexOf(xKey.GetValue("Audio Card"
                 , Builder.QemuAudioCard.es1370.ToString()));
                // VMWare
                string xVMWareVersion = (string)xKey.GetValue("VMWare Edition", "Server");
                switch (xVMWareVersion) {
                    case "Server":
                        rdVMWareServer.IsChecked = true;
                        break;
                    case "Workstation":
                        rdVMWareWorkstation.IsChecked = true;
                        break;
                }

                // USB
                // Combo is lazy loaded, so we just store it for later
                mLastSelectedUSBDrive = (string)xKey.GetValue("USB Device", "");
            }
        }

    }
}