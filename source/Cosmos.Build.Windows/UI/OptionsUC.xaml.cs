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

namespace Cosmos.Compiler.Builder
{
    public partial class OptionsUC : UserControl ,IOptionUC
    {
        protected string mLastSelectedUSBDrive;
    //    protected bool mSaveSettings = true;  //not needed i think as button not on form 
        internal BuildOptions mOptions = new BuildOptions(); //TODO remove should only be set by property but it has static data we need

        protected DebugMode mDebugMode;
     

        public OptionsUC()
        {
            InitializeComponent();
            Init(); 
        }

        //BAD
        //public byte ComPort
        //{
        //    get { return (byte)cmboDebugPort.SelectedIndex; }
        //}

        protected void TargetChanged(object aSender, RoutedEventArgs e)
        {
            // .IsReady takes quite some time, so instead of delaying
            // every run, instead we load it on demand
            if ((aSender == rdioUSB) && (cmboUSBDevice.Items.Count == 0))
            {
                var xDrives = System.IO.Directory.GetLogicalDrives();
                foreach (string xDrive in xDrives)
                {
                    var xType = new System.IO.DriveInfo(xDrive);
                    if (xType.IsReady)
                    {
                        if ((xType.DriveType == System.IO.DriveType.Removable)
                         && xType.DriveFormat.StartsWith("FAT"))
                        {
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
            spnlVHD.Visibility = aSender == rdioVHD ? Visibility.Visible : Visibility.Collapsed;
            wpnlDebugPort.Visibility = (aSender == rdioVHD || aSender == rdioUSB || aSender == rdioISO || aSender == rdioPXE || aSender == rdioVHD) ? Visibility.Visible : Visibility.Collapsed;
        }



        protected void UpdateProperties()
        {
            if (rdioDebugModeNone.IsChecked.Value)
            {
                mDebugMode = DebugMode.None;
            }
            else if (rdioDebugModeIL.IsChecked.Value)
            {
                //BUG next load will be IL and trigger this
                //mDebugMode = DebugMode.IL;
                throw new NotSupportedException("Debug mode IL isn't supported yet, use Source instead.");
            }
            else if (rdioDebugModeSource.IsChecked.Value)
            {
                mDebugMode = DebugMode.Source;
            }
            else
            {
                throw new Exception("Unknown debug mode.");
            }
        }

       
        public void Init()
        {
            //  InitializeComponent();



            Loaded += new RoutedEventHandler(OptionsUC_Loaded);

     
            rdioQEMU.Checked += new RoutedEventHandler(TargetChanged);
            rdioVMWare.Checked += new RoutedEventHandler(TargetChanged);
            rdioVPC.Checked += new RoutedEventHandler(TargetChanged);
            rdioISO.Checked += new RoutedEventHandler(TargetChanged);
            rdioPXE.Checked += new RoutedEventHandler(TargetChanged);
            rdioUSB.Checked += new RoutedEventHandler(TargetChanged);

            rdioMSNET.Checked += new RoutedEventHandler(FrameworkChanged);
            rdioProjectMono.Checked += new RoutedEventHandler(FrameworkChanged);

       
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

            foreach (DictionaryEntry xNIC in mOptions.QEmuNetworkCard)
            {
                cmboNetworkCards.Items.Add(xNIC.Key);
            }
            foreach (DictionaryEntry xSoundCard in mOptions.QEmuAudioCard)
            {
                cmboAudioCards.Items.Add(xSoundCard.Key);
            }
            foreach (DictionaryEntry xDebugComMode in mOptions.QEmuDebugComType)
            {
                cmbDebugComMode.Items.Add(xDebugComMode.Key);
            }
           // LoadOptions();
            // Call here for when this dialog is bypassed, others read these values
            UpdateProperties(); //TODO WTF
        }

        void FrameworkChanged(object sender, RoutedEventArgs e)
        {
            var xIsUsingMSNet = rdioMSNET.IsChecked.Value;
            chbxUseInternalAssembler.IsEnabled = xIsUsingMSNet;
        }

        void OptionsUC_Loaded(object sender, RoutedEventArgs e)
        {
            var xShowOptions = chbxShowOptions.IsChecked.Value;
            // If the user doenst have the option to auto show, then look
            // for control key pressed
            if (!xShowOptions)
            {
                // We should use the WPF Keyboard.IsKeyDown, but it does not work here.
                // It appears that it gets initialized at some point later
                // or after a WPF window is shown, but it does not work here for sure
                // so instead we have to us an extern.
                xShowOptions = KeyState.IsKeyDown(System.Windows.Forms.Keys.RControlKey)
                    || KeyState.IsKeyDown(System.Windows.Forms.Keys.LControlKey);

                //mSaveSettings = false;
                //// Need to do this so we dont stuff up the main flow
                //// where message processing continues. 
                //butnBuild.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        protected void UpdateOptionsFromUI()
        {


            if (rdioDebugAssembliesAll.IsChecked.Value)
            {
                mOptions.TraceAssemblies = TraceAssemblies.All;
            }
            else if (rdioDebugAssembliesCosmos.IsChecked.Value)
            {
                mOptions.TraceAssemblies = TraceAssemblies.Cosmos;
            }
            else if (rdioDebugAssembliesUser.IsChecked.Value)
            {
                mOptions.TraceAssemblies = TraceAssemblies.User;
            }
            
            mOptions.Target = "QEMU";
            if (rdioVMWare.IsChecked.Value)
            {
                mOptions.Target = "VMWare";
            }
            else if (rdioVPC.IsChecked.Value)
            {
                mOptions.Target = "VPC";
            }
            else if (rdioISO.IsChecked.Value)
            {
                mOptions.Target = "ISO";
            }
            else if (rdioPXE.IsChecked.Value)
            {
                mOptions.Target = "PXE";
            }
            else if (rdioUSB.IsChecked.Value)
            {
                mOptions.Target = "USB";
            }
            else if (rdioVHD.IsChecked.Value)
            {
                mOptions.Target = "VHD";
            }
            mOptions.DebugPort = cmboDebugPort.Text;
            mOptions.DebugPortId = (byte) cmboDebugPort.SelectedIndex;

            if (rdioDebugModeNone.IsChecked.Value)
            {
                mOptions.DebugMode = DebugMode.None;
            }
            else if (rdioDebugModeIL.IsChecked.Value)
            {
                mOptions.DebugMode = DebugMode.IL;
            }
            else if (rdioDebugModeSource.IsChecked.Value)
            {
                mOptions.DebugMode = DebugMode.Source;
            }
            mOptions.UseGDB = chbxQEMUUseGDB.IsChecked.Value;
            mOptions.CreateHDImage = chbxQEMUUseHD.IsChecked.Value;
            mOptions.UseNetworkTAP = chckQEMUUseNetworkTAP.IsChecked.Value;
            mOptions.NetworkCard = cmboNetworkCards.Text;
            mOptions.AudioCard = cmboAudioCards.Text;

            mOptions.VMWareEdition = rdVMWareWorkstation.IsChecked.Value ? "Workstation" : "Server";
            if (cmboUSBDevice.SelectedItem != null)
            {
                mOptions.USBDevice = cmboUSBDevice.Text;
            }
            if (rdioMSNET.IsChecked.Value)
            {
                mOptions.dotNETFrameworkImplementation = dotNETFrameworkImplementationEnum.Microsoft;
            }
            if (rdioProjectMono.IsChecked.Value)
            {
                mOptions.dotNETFrameworkImplementation = dotNETFrameworkImplementationEnum.ProjectMono;
            }
            mOptions.ShowOptions = chbxShowOptions.IsChecked.Value;
            mOptions.CompileIL = chbxCompileIL.IsChecked.Value;
            
            
            mOptions.DebugComMode = cmbDebugComMode.Text;
            mOptions.UseInternalAssembler = chbxUseInternalAssembler.IsChecked.Value;

        //    mOptions.Save(); //moved to controller not UI job
        }

        private void LoadOptions(BuildOptions options)
        {
            
            this.mOptions = options;


            rdioDebugAssembliesAll.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.All);
            rdioDebugAssembliesCosmos.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.Cosmos);
            rdioDebugAssembliesUser.IsChecked = (mOptions.TraceAssemblies == TraceAssemblies.User);

            string xBuildType = mOptions.Target;
            switch (xBuildType)
            {
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
                case "VHD":
                    rdioVHD.IsChecked = true;
                    break;
            }

            // Misc
            chbxShowOptions.IsChecked = mOptions.ShowOptions;
            chbxCompileIL.IsChecked = mOptions.CompileIL;

            // Debug                
            cmboDebugPort.SelectedIndex = cmboDebugPort.Items.IndexOf(mOptions.DebugPort);
            if (cmboDebugPort.SelectedIndex == -1)
            {
                cmboDebugPort.SelectedIndex = 0;
            }
            rdioDebugModeNone.IsChecked = mOptions.DebugMode == DebugMode.None;
            rdioDebugModeIL.IsChecked = mOptions.DebugMode == DebugMode.IL;
            rdioDebugModeSource.IsChecked = mOptions.DebugMode == DebugMode.Source;

            // QEMU
            chbxQEMUUseGDB.IsChecked = mOptions.UseGDB;
            chbxQEMUUseHD.IsChecked = mOptions.CreateHDImage;
            chckQEMUUseNetworkTAP.IsChecked = mOptions.UseNetworkTAP;
            cmboNetworkCards.SelectedIndex = cmboNetworkCards.Items.IndexOf(mOptions.NetworkCard);
            cmboAudioCards.SelectedIndex = cmboAudioCards.Items.IndexOf(mOptions.AudioCard);
            // VMWare
            string xVMWareVersion = mOptions.VMWareEdition;
            switch (xVMWareVersion)
            {
                case "Server":
                    rdVMWareServer.IsChecked = true;
                    break;
                case "Workstation":
                    rdVMWareWorkstation.IsChecked = true;
                    break;
            }
            switch (mOptions.dotNETFrameworkImplementation)
            {
                case dotNETFrameworkImplementationEnum.Microsoft:
                    rdioMSNET.IsChecked = true;
                    break;
                case dotNETFrameworkImplementationEnum.ProjectMono:
                    rdioProjectMono.IsChecked = true;
                    break;
                default: // safe programming
                    throw new Exception(".NET Framework implementation '" + mOptions.dotNETFrameworkImplementation.ToString() + "' not supported!");

            }
            cmbDebugComMode.SelectedIndex = cmbDebugComMode.Items.IndexOf(mOptions.DebugComMode);
            // USB
            // Combo is lazy loaded, so we just store it for later
            mLastSelectedUSBDrive = mOptions.USBDevice;

            tblkBuildPath.Text = mOptions.BuildPath;
            tblkISOPath.Text = mOptions.BuildPath + "Cosmos.iso";


            chbxUseInternalAssembler.IsChecked = mOptions.UseInternalAssembler ;



        }


        #region IOptionUC Members

        public BuildOptions Options
        {
            get
            {
                UpdateOptionsFromUI();
                return this.mOptions;
            }
            set
            {
           
                LoadOptions(value);

            }
        }

        public DebugMode DebugMode
        {
            get { return mDebugMode; }
        }

        #endregion
    }
}
