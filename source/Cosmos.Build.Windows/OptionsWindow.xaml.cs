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
        public static extern int ShowWindow(int Handle,
                                            int showState);

        [DllImport("kernel32.dll")]
        public static extern int GetConsoleWindow();

        protected Block mOptionsBlockPrefix;
        protected Builder mBuilder = new Builder();

        public static void Display() {
            int xConsoleWindow = GetConsoleWindow();
            ShowWindow(xConsoleWindow,
                       0);

            var xOptionsWindow = new OptionsWindow();
            if (xOptionsWindow.ShowDialog().Value) {
                ShowWindow(xConsoleWindow,
                           1);
                xOptionsWindow.DoBuild();

                //Debug Window is only displayed if Qemu + Debug checked, or if other VM + Debugport selected
                bool xIsQemu = xOptionsWindow.rdioQEMU.IsChecked.Value;
                bool xUseQemuDebug = xOptionsWindow.cmboDebugMode.SelectedIndex > 0;
                if (((xIsQemu & xUseQemuDebug) | (!xIsQemu & (xOptionsWindow.mComport > 0))) && xOptionsWindow.mDebugMode != DebugModeEnum.None) {
                    var xDebugWindow = new DebugWindow();
                    if (xOptionsWindow.mDebugMode == DebugModeEnum.Source) {
                        var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(xOptionsWindow.mBuilder.BuildPath + "output.bin",
                                                                                      xOptionsWindow.mBuilder.ToolsPath + @"cygwin\objdump.exe");
                        var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping,
                                                                       xOptionsWindow.mBuilder.BuildPath + "Tools/asm/debug.cxdb");
                        xDebugWindow.SetSourceInfoMap(xSourceMappings);
                    } else {
                        throw new Exception("Debug mode not supported: " + xOptionsWindow.mDebugMode);
                    }
                    xDebugWindow.ShowDialog();
                }
            }
        }

        protected void AddSection(params Paragraph[] aParagraphs) {
            foreach (var xPara in aParagraphs) {
                RootDoc.Blocks.InsertAfter(mOptionsBlockPrefix,
                                           xPara);
            }
        }

        protected void TargetChanged(object aSender,
                                     RoutedEventArgs e) {
            RootDoc.Blocks.Remove(paraDebugOptions);
            paraDebugOptions.Inlines.Remove(spanDebugOptionsPort);
            RootDoc.Blocks.Remove(paraQEMUOptions);
            RootDoc.Blocks.Remove(paraVMWareOptions);
            RootDoc.Blocks.Remove(paraVPCOptions);
            RootDoc.Blocks.Remove(paraISOOptions);
            RootDoc.Blocks.Remove(paraPXEOptions);
            RootDoc.Blocks.Remove(paraUSBOptions);

            if (aSender == rdioUSB) {
                AddSection(paraDebugOptions);
                AddSection(paraUSBOptions);
                paraDebugOptions.Inlines.Add(spanDebugOptionsPort);
            } else if (aSender == rdioISO) {
                AddSection(paraDebugOptions);
                AddSection(paraISOOptions);
                paraDebugOptions.Inlines.Add(spanDebugOptionsPort);
            } else if (aSender == rdioVPC) {
                AddSection(paraDebugOptions);
                AddSection(paraVPCOptions);
                paraDebugOptions.Inlines.Add(spanDebugOptionsPort);
            } else if (aSender == rdioVMWare) {
                AddSection(paraDebugOptions);
                AddSection(paraVMWareOptions);
                paraDebugOptions.Inlines.Add(spanDebugOptionsPort);
            } else if (aSender == rdioPXE) {
                AddSection(paraDebugOptions);
                AddSection(paraPXEOptions);
                paraDebugOptions.Inlines.Add(spanDebugOptionsPort);
            } else if (aSender == rdioQEMU) {
                AddSection(paraDebugOptions);
                AddSection(paraQEMUOptions);
            }
        }

        public OptionsWindow() {
            InitializeComponent();
            mOptionsBlockPrefix = paraBuildPath; // paraQEMUOptions.PreviousBlock;

            Loaded += delegate(object sender,
                               RoutedEventArgs e) {
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

            spanBuildPath.Inlines.Add(mBuilder.BuildPath);
            spanISOPath.Inlines.Add(mBuilder.BuildPath + "Cosmos.iso");

            var xDrives = System.IO.Directory.GetLogicalDrives();
            foreach (string xDrive in xDrives) {
                var xType = new System.IO.DriveInfo(xDrive);
                if (xType.IsReady) {
                    if ((xType.DriveType == System.IO.DriveType.Removable) && xType.DriveFormat.StartsWith("FAT")) {
                        cmboUSBDevice.Items.Add(xDrive);
                    }
                }
            }

            cmboDebugPort.Items.Add("Disabled");
            // MtW: for now, leave COM1 out, as COM1 is used by the Cosmos kernel to output debug messages
            // Kudzu: Need to configure that too....
            //cmboDebugPort.Items.Add("COM1");
            cmboDebugPort.SelectedIndex = cmboDebugPort.Items.Add("COM2");
            cmboDebugPort.Items.Add("COM3");
            cmboDebugPort.Items.Add("COM4");
            cmboDebugPort.Items.Add("Ethernet 1");
            cmboDebugPort.Items.Add("Ethernet 2");
            cmboDebugPort.Items.Add("Ethernet 3");
            cmboDebugPort.Items.Add("Ethernet 4");

            cmboDebugMode.SelectedIndex = cmboDebugMode.Items.Add("None");
            cmboDebugMode.Items.Add("IL");
            cmboDebugMode.Items.Add("Source");

            foreach (string nic in Enum.GetNames(typeof(Builder.QemuNetworkCard))) {
                cmboNetworkCards.Items.Add(nic);
            }
            foreach (string sc in Enum.GetNames(typeof(Builder.QemuAudioCard))) {
                cmboAudioCards.Items.Add(sc);
            }

            LoadSettingsFromRegistry();
        }

        private void butnBuild_Click(object sender,
                                     RoutedEventArgs e) {
            DialogResult = true;
        }

        private DebugModeEnum mDebugMode;
        private byte mComport;

        protected void DoBuild() {
            SaveSettingsToRegistry();

            mComport = (byte)cmboDebugPort.SelectedIndex;
            if (mComport > 3) {
                throw new Exception("Debug port not supported yet!");
            }
            mComport++;
            string xDebugMode = (string)cmboDebugMode.SelectedValue;
            mDebugMode = DebugModeEnum.None;
            if (xDebugMode == "IL") {
                mDebugMode = DebugModeEnum.IL;
                throw new NotSupportedException("Debug mode IL isn't supported yet, use Source instead.");
            } else if (xDebugMode == "Source") {
                mDebugMode = DebugModeEnum.Source;
                mComport = 1;
            } else if (xDebugMode == "None") {
                mDebugMode = DebugModeEnum.None;
            } else {
                throw new Exception("Selected debug mode not supported!");
            }

            if (chckCompileIL.IsChecked.Value) {
                Console.WriteLine("Compiling...");
                var xBuildWindow = new BuildWindow();
                IEnumerable<BuildLogMessage> xMessages = new BuildLogMessage[0];

                mBuilder.DebugLog += xBuildWindow.DoDebugMessage;
                mBuilder.ProgressChanged += xBuildWindow.DoProgressMessage;
                xBuildWindow.Show();
                try {
                    mBuilder.Compile(mDebugMode,
                                     mComport);
                    
                    mBuilder.DebugLog -= xBuildWindow.DoDebugMessage;
                    mBuilder.ProgressChanged -= xBuildWindow.DoProgressMessage;

                    xMessages = (from item in xBuildWindow.Messages
                                 where item.Severity != LogSeverityEnum.Informational
                                 select item).ToArray();

                    //If there were any warnings or errors, then show dialog again
                    if (xMessages.Count() > 0) {
                        xBuildWindow = new BuildWindow();
                        xBuildWindow.Messages.Clear();

                        foreach (var item in xMessages) {
                            xBuildWindow.Messages.Add(item);
                        }
                        xBuildWindow.ShowDialog();
                        return;
                    }
                } catch (Exception E){
                    if (xBuildWindow.Visibility == System.Windows.Visibility.Visible) {
                        xBuildWindow.Close();
                    }
                    var xTheMessages = (from item in xBuildWindow.Messages
                                 where item.Severity != LogSeverityEnum.Informational
                                 select item).ToList();
                    xTheMessages.Add(new BuildLogMessage() {
                                                               Severity=LogSeverityEnum.Error,
                                                               Message = E.ToString()
                                                           });
                    xBuildWindow = new BuildWindow();
                        xBuildWindow.Messages.Clear();
                        foreach (var item in xTheMessages)
                    {
                        xBuildWindow.Messages.Add(item);
                    }
                    xBuildWindow.ShowDialog();
                    return;
                }
            }
            if (rdioQEMU.IsChecked.Value) {
                mBuilder.MakeQEMU(chckQEMUUseHD.IsChecked.Value,
                                  chckQEMUUseGDB.IsChecked.Value,
                                  mDebugMode != DebugModeEnum.None,
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
        }

        private void butnCancel_Click(object sender,
                                      RoutedEventArgs e) {
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
                xKey.SetValue("Target",
                              xTarget);

                // General
                xKey.SetValue("Compile IL",
                              true,  //Force checkbox to be on, was chckCompileIL.IsChecked.Value,
                              RegistryValueKind.DWord);
                xKey.SetValue("Debug Port",
                              cmboDebugPort.Text);
                xKey.SetValue("Debug Mode",
                              cmboDebugMode.Text);

                // QEMU
                xKey.SetValue("Use GDB",
                              chckQEMUUseGDB.IsChecked.Value,
                              RegistryValueKind.DWord);
                xKey.SetValue("Create HD Image",
                              chckQEMUUseHD.IsChecked.Value,
                              RegistryValueKind.DWord);
                xKey.SetValue("Use network TAP",
                              chckQEMUUseNetworkTAP.IsChecked.Value,
                              RegistryValueKind.DWord);
                xKey.SetValue("Network Card",
                              cmboNetworkCards.Text,
                              RegistryValueKind.String);
                xKey.SetValue("Audio Card",
                              cmboAudioCards.Text,
                              RegistryValueKind.String);
                // VMWare
                string xVMWareVersion = string.Empty;
                if (rdVMWareServer.IsChecked.Value) {
                    xVMWareVersion = "VMWare Server";
                } else if (rdVMWareWorkstation.IsChecked.Value) {
                    xVMWareVersion = "VMWare Workstation";
                }
                xKey.SetValue("VMWare Version",
                              xVMWareVersion);

                // USB
                if (cmboUSBDevice.SelectedItem != null) {
                    xKey.SetValue("USB Device",
                                  cmboUSBDevice.Text);
                }
            }
        }

        private void LoadSettingsFromRegistry() {
            using (var xKey = Registry.CurrentUser.CreateSubKey(mRegKey)) {
                string xBuildType = (string)xKey.GetValue("Target",
                                                          "QEMU");
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

                // General
                chckCompileIL.IsChecked = ((int)xKey.GetValue("Compile IL",
                                                              1) != 0);
                cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(xKey.GetValue("Debug Port",
                                                                                        ""));
                if (cmboDebugPort.SelectedIndex == -1) {
                    cmboDebugPort.SelectedIndex = 0;
                }
                cmboDebugMode.SelectedIndex = cmboDebugMode.Items.IndexOf(xKey.GetValue("Debug Mode",
                                                                                        ""));
                if (cmboDebugMode.SelectedIndex == -1) {
                    cmboDebugMode.SelectedIndex = 0;
                }

                // QEMU
                chckQEMUUseGDB.IsChecked = ((int)xKey.GetValue("Use GDB",
                                                               0) != 0);
                chckQEMUUseHD.IsChecked = ((int)xKey.GetValue("Create HD Image",
                                                              0) != 0);
                chckQEMUUseNetworkTAP.IsChecked = ((int)xKey.GetValue("Use network TAP",
                                                                      0) != 0);
                cmboNetworkCards.SelectedIndex = cmboNetworkCards.Items.IndexOf(xKey.GetValue("Network Card",
                                                                                              Builder.QemuNetworkCard.rtl8139.ToString()));
                cmboAudioCards.SelectedIndex = cmboAudioCards.Items.IndexOf(xKey.GetValue("Audio Card",
                                                                                          Builder.QemuAudioCard.es1370.ToString()));
                // VMWare
                string xVMWareVersion = (string)xKey.GetValue("VMWare Version",
                                                              "VMWare Server");
                switch (xVMWareVersion) {
                    case "VMWare Server":
                        rdVMWareServer.IsChecked = true;
                        break;
                    case "VMWare Workstation":
                        rdVMWareWorkstation.IsChecked = true;
                        break;
                }

                // USB
                cmboUSBDevice.SelectedIndex = cmboUSBDevice.Items.IndexOf(xKey.GetValue("USB Device",
                                                                                        ""));
            }
        }
    }
}