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
using System.Collections;

namespace Cosmos.Compiler.Builder {
    public partial class OptionsUC : UserControl {
        protected string mLastSelectedUSBDrive;
        protected bool mSaveSettings = true;
        protected Options mOptions;
        
        protected DebugMode mDebugMode;
        public DebugMode DebugMode {
            get { return mDebugMode; }
        }
        
        public byte ComPort {
            get { return (byte)cmboDebugPort.SelectedIndex; }
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
            if (rdioDebugModeNone.IsChecked.Value) {
                mDebugMode = DebugMode.None;
            } else if (rdioDebugModeIL.IsChecked.Value) {
                mDebugMode = DebugMode.IL;
                throw new NotSupportedException("Debug mode IL isn't supported yet, use Source instead.");
            } else if (rdioDebugModeSource.IsChecked.Value) {
                mDebugMode = DebugMode.Source;
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

            rdioMSNET.Checked += new RoutedEventHandler(FrameworkChanged);
            rdioProjectMono.Checked += new RoutedEventHandler(FrameworkChanged);

            tblkBuildPath.Text = aBuildPath;
            tblkISOPath.Text = aBuildPath + "Cosmos.iso";

            cmboDebugPort.Items.Add("Disabled");
            cmboDebugPort.Items.Add("COM1");
            cmboDebugPort.Items.Add("COM2");
            cmboDebugPort.Items.Add("COM3");
            cmboDebugPort.Items.Add("COM4");
            cmboDebugPort.Items.Add("COM5");
            cmboDebugPort.Items.Add("COM6"); 
            cmboDebugPort.Items.Add("COM7");
            cmboDebugPort.Items.Add("COM8"); 
            cmboDebugPort.Items.Add("COM9");
            cmboDebugPort.Items.Add("COM10"); 
            cmboDebugPort.Items.Add("COM11");
            cmboDebugPort.Items.Add("COM12");

            foreach (DictionaryEntry xNIC in Options.QEmuNetworkCard) {
                cmboNetworkCards.Items.Add(xNIC.Key);
            }
            foreach (DictionaryEntry xSoundCard in Options.QEmuAudioCard)
            {
                cmboAudioCards.Items.Add(xSoundCard.Key);
            }
            foreach (DictionaryEntry xDebugComMode in Options.QEmuDebugComType)
            {
                cmbDebugComMode.Items.Add(xDebugComMode.Key);
            }
            LoadOptions();
            // Call here for when this dialog is bypassed, others read these values
            UpdateProperties();
        }

        void FrameworkChanged(object sender, RoutedEventArgs e) {
            var xIsUsingMSNet = rdioMSNET.IsChecked.Value;
            chbxUseInternalAssembler.IsEnabled = xIsUsingMSNet;
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
                Options.TraceAssemblies = TraceAssemblies.All;
            } else if (rdioDebugAssembliesCosmos.IsChecked.Value) {
                Options.TraceAssemblies = TraceAssemblies.Cosmos;
            } else if (rdioDebugAssembliesUser.IsChecked.Value) {
                Options.TraceAssemblies = TraceAssemblies.User;
            }
            Options.Target = "QEMU";
            if (rdioVMWare.IsChecked.Value) {
                Options.Target = "VMWare";
            } else if (rdioVPC.IsChecked.Value) {
                Options.Target = "VPC";
            } else if (rdioISO.IsChecked.Value) {
                Options.Target = "ISO";
            } else if (rdioPXE.IsChecked.Value) {
                Options.Target = "PXE";
            } else if (rdioUSB.IsChecked.Value) {
                Options.Target = "USB";
            }
            Options.DebugPort = cmboDebugPort.Text;

            if (rdioDebugModeNone.IsChecked.Value) {
                Options.DebugMode = DebugMode.None;
            } else if (rdioDebugModeIL.IsChecked.Value) {
                Options.DebugMode = DebugMode.IL;
            } else if (rdioDebugModeSource.IsChecked.Value) {
                Options.DebugMode = DebugMode.Source;
            }
            Options.UseGDB = chbxQEMUUseGDB.IsChecked.Value;
            Options.CreateHDImage = chbxQEMUUseHD.IsChecked.Value;
            Options.UseNetworkTAP = chckQEMUUseNetworkTAP.IsChecked.Value;
            Options.NetworkCard = cmboNetworkCards.Text;
            Options.AudioCard = cmboAudioCards.Text;

            Options.VMWareEdition = rdVMWareWorkstation.IsChecked.Value ? "Workstation" : "Server";
            if (cmboUSBDevice.SelectedItem != null) {
                Options.USBDevice = cmboUSBDevice.Text;
            }
            if (rdioMSNET.IsChecked.Value) {
                Options.dotNETFrameworkImplementation = dotNETFrameworkImplementationEnum.Microsoft;
            }
            if(rdioProjectMono.IsChecked.Value){
                Options.dotNETFrameworkImplementation = dotNETFrameworkImplementationEnum.ProjectMono;
            }
            Options.ShowOptions = chbxShowOptions.IsChecked.Value;
            Options.CompileIL = chbxCompileIL.IsChecked.Value;
            Options.DebugComMode = cmbDebugComMode.Text;
            Options.Save();
        }

        private void LoadOptions() {
            rdioDebugAssembliesAll.IsChecked = (Options.TraceAssemblies == TraceAssemblies.All);
            rdioDebugAssembliesCosmos.IsChecked = (Options.TraceAssemblies == TraceAssemblies.Cosmos);
            rdioDebugAssembliesUser.IsChecked = (Options.TraceAssemblies == TraceAssemblies.User);
        
                string xBuildType = Options.Target;
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
                chbxShowOptions.IsChecked = Options.ShowOptions;
                chbxCompileIL.IsChecked = Options.CompileIL;

                // Debug                
                cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(Options.DebugPort);
                if (cmboDebugPort.SelectedIndex == -1) {
                    cmboDebugPort.SelectedIndex = 0;
                }
                rdioDebugModeNone.IsChecked = Options.DebugMode == DebugMode.None;
                rdioDebugModeIL.IsChecked = Options.DebugMode == DebugMode.IL;
                rdioDebugModeSource.IsChecked = Options.DebugMode == DebugMode.Source;

                // QEMU
                chbxQEMUUseGDB.IsChecked = Options.UseGDB;
                chbxQEMUUseHD.IsChecked = Options.CreateHDImage;
                chckQEMUUseNetworkTAP.IsChecked = Options.UseNetworkTAP;
                cmboNetworkCards.SelectedIndex = cmboNetworkCards.Items.IndexOf(Options.NetworkCard);
                cmboAudioCards.SelectedIndex = cmboAudioCards.Items.IndexOf(Options.AudioCard);
                // VMWare
                string xVMWareVersion = Options.VMWareEdition;
                switch (xVMWareVersion) {
                    case "Server":
                        rdVMWareServer.IsChecked = true;
                        break;
                    case "Workstation":
                        rdVMWareWorkstation.IsChecked = true;
                        break;
                }
            switch(Options.dotNETFrameworkImplementation) {
                case dotNETFrameworkImplementationEnum.Microsoft:
                    rdioMSNET.IsChecked = true;
                    break;
                case dotNETFrameworkImplementationEnum.ProjectMono:
                    rdioProjectMono.IsChecked = true;
                    break;
                    default: // safe programming
                    throw new Exception(".NET Framework implementation '" + Options.dotNETFrameworkImplementation.ToString() + "' not supported!");

            }
            cmbDebugComMode.SelectedIndex = cmbDebugComMode.Items.IndexOf(Options.DebugComMode);
                // USB
                // Combo is lazy loaded, so we just store it for later
                mLastSelectedUSBDrive = Options.USBDevice;
            }

        
    }
}
