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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Indy.IL2CPU;

namespace Cosmos.Build.Windows {
    public partial class OptionsUC : UserControl {
        protected string mLastSelectedUSBDrive;
        protected bool mSaveSettings = true;
        protected Options mOptions;
        
        protected DebugModeEnum mDebugMode;
        public DebugModeEnum DebugMode {
            get { return mDebugMode; }
        }
        
        protected byte mComPort;
        public byte ComPort {
            get { return mComPort; }
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
            wpnlDebugPort.Visibility = (aSender == rdioUSB||aSender==rdioISO || aSender == rdioPXE ) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public Action Proceed;
        public Action Stop;
        
        protected void UpdateProperties() {
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
        }

        private void butnBuild_Click(object sender, RoutedEventArgs e) {
            if (mSaveSettings) {
                SaveOptions();
                UpdateProperties();
            }
            Proceed();
        }
        
        private void butnCancel_Click(object sender, RoutedEventArgs e) {
            Stop();
        }
        
        public OptionsUC(string aBuildPath, Options aOptions) {
            InitializeComponent();

            mOptions = aOptions;
            
            Loaded += new RoutedEventHandler(OptionsUC_Loaded);
            
            butnBuild.Click += new RoutedEventHandler(butnBuild_Click);
            butnCancel.Click += new RoutedEventHandler(butnCancel_Click);

            rdioQEMU.Checked += new RoutedEventHandler(TargetChanged);
            rdioVMWare.Checked += new RoutedEventHandler(TargetChanged);
            rdioVPC.Checked += new RoutedEventHandler(TargetChanged);
            rdioISO.Checked += new RoutedEventHandler(TargetChanged);
            rdioPXE.Checked += new RoutedEventHandler(TargetChanged);
            rdioUSB.Checked += new RoutedEventHandler(TargetChanged);

            tblkBuildPath.Text = aBuildPath;
            tblkISOPath.Text = aBuildPath + "Cosmos.iso";

            cmboDebugPort.Items.Add("Disabled");

            foreach (string xNIC in Enum.GetNames(typeof(Builder.QemuNetworkCard))) {
                cmboNetworkCards.Items.Add(xNIC);
            }
            foreach (string xSoundCard in Enum.GetNames(typeof(Builder.QemuAudioCard))) {
                cmboAudioCards.Items.Add(xSoundCard);
            }
            LoadOptions();
            // Call here for when this dialog is bypassed, others read these values
            UpdateProperties();
        }

        void OptionsUC_Loaded(object sender, RoutedEventArgs e) {
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
            if (!xShowOptions) {
                mSaveSettings = false;
                // Need to do this so we dont stuff up the main flow
                // where message processing continues. 
                butnBuild.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        protected void SaveOptions() {
            if (rdioDebugAssembliesAll.IsChecked.Value) {
                mOptions.TraceAssemblies = TraceAssemblies.All;
            } else if (rdioDebugAssembliesCosmos.IsChecked.Value) {
                mOptions.TraceAssemblies = TraceAssemblies.Cosmos;
            } else if (rdioDebugAssembliesUser.IsChecked.Value) {
                mOptions.TraceAssemblies = TraceAssemblies.User;
            }
            mOptions.Target = "QEMU";
            if (rdioVMWare.IsChecked.Value) {
                mOptions.Target = "VMWare";
            } else if (rdioVPC.IsChecked.Value) {
                mOptions.Target = "VPC";
            } else if (rdioISO.IsChecked.Value) {
                mOptions.Target = "ISO";
            } else if (rdioPXE.IsChecked.Value) {
                mOptions.Target = "PXE";
            } else if (rdioUSB.IsChecked.Value) {
                mOptions.Target = "USB";
            }
            mOptions.DebugPort = cmboDebugPort.Text;
            string xDebugMode = "QEMU";
            if (rdioDebugModeNone.IsChecked.Value) {
                xDebugMode = "None";
            } else if (rdioDebugModeIL.IsChecked.Value) {
                xDebugMode = "IL";
            } else if (rdioDebugModeSource.IsChecked.Value) {
                xDebugMode = "Source";
            }
            mOptions.DebugMode = xDebugMode;
            mOptions.UseGDB = chbxQEMUUseGDB.IsChecked.Value;
            mOptions.CreateHDImage = chbxQEMUUseHD.IsChecked.Value;
            mOptions.UseNetworkTAP = chckQEMUUseNetworkTAP.IsChecked.Value;
            mOptions.NetworkCard = cmboNetworkCards.Text;
            mOptions.AudioCard = cmboAudioCards.Text;

            mOptions.VMWareEdition = rdVMWareWorkstation.IsChecked.Value ? "Workstation" : "Server";
            if (cmboUSBDevice.SelectedItem != null) {
                mOptions.USBDevice = cmboUSBDevice.Text;
            }
            mOptions.ShowOptions = chbxShowOptions.IsChecked.Value;
            mOptions.CompileIL = chbxCompileIL.IsChecked.Value;
            mOptions.Save();
        }

        private void LoadOptions() {
            rdioDebugAssembliesAll.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.All);
            rdioDebugAssembliesCosmos.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.Cosmos);
            rdioDebugAssembliesUser.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.User);
        
                string xBuildType = mOptions.Target;
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
                chbxShowOptions.IsChecked = mOptions.ShowOptions;
                chbxCompileIL.IsChecked = mOptions.CompileIL;

                // Debug                
                cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(mOptions.DebugPort);
                if (cmboDebugPort.SelectedIndex == -1) {
                    cmboDebugPort.SelectedIndex = 0;
                }
                string xDebugMode = mOptions.DebugMode;
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
                chbxQEMUUseGDB.IsChecked = mOptions.UseGDB;
                chbxQEMUUseHD.IsChecked = mOptions.CreateHDImage;
                chckQEMUUseNetworkTAP.IsChecked = mOptions.UseNetworkTAP;
                cmboNetworkCards.SelectedIndex = cmboNetworkCards.Items.IndexOf(mOptions.NetworkCard);
                cmboAudioCards.SelectedIndex = cmboAudioCards.Items.IndexOf(mOptions.AudioCard);
                // VMWare
                string xVMWareVersion = mOptions.VMWareEdition;
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
                mLastSelectedUSBDrive = mOptions.USBDevice;
            }
    }
}
