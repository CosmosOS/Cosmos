using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualBasic;

using VSPropertyPages;

using Cosmos.Build.Common;

using DebugMode = Cosmos.Build.Common.DebugMode;

namespace Cosmos.VS.ProjectSystem.VS.PropertyPages
{
    internal partial class OldCosmosPropertyPageControl : WinFormsPropertyPageUI
    {
        protected class ProfileItem
        {
            public string Prefix;

            public string Name;

            public bool IsPreset;

            public override string ToString() => Name;
        }

        private OldCosmosPropertyPageViewModel mViewModel;

        protected ProfilePresets mPresets = new ProfilePresets();

        protected int mVMwareAndBochsDebugPipe;
        protected int mHyperVDebugPipe;

        protected bool mShowTabBochs;
        protected bool mShowTabQemu;
        protected bool mShowTabDebug;
        protected bool mShowTabDeployment;
        protected bool mShowTabLaunch;
        protected bool mShowTabVMware;
        protected bool mShowTabHyperV;
        protected bool mShowTabPXE;
        protected bool mShowTabUSB;
        protected bool mShowTabISO;
        protected bool mShowTabSlave;

        protected bool FreezeEvents;

        public OldCosmosPropertyPageControl(OldCosmosPropertyPageViewModel aViewModel)
        {
            InitializeComponent();

            mViewModel = aViewModel;

            #region Profile

            butnProfileClone.Click += new EventHandler(butnProfileClone_Click);
            butnProfileDelete.Click += new EventHandler(butnProfileDelete_Click);
            butnProfileRename.Click += new EventHandler(butnProfileRename_Click);

            lboxProfile.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xProfile = (ProfileItem)lboxProfile.SelectedItem;
                if (xProfile.Prefix != mViewModel.BuildProperties.Profile)
                {
                    // Save existing profile
                    mViewModel.BuildProperties.SaveProfile(mViewModel.BuildProperties.Profile);
                    // Load newly selected profile
                    mViewModel.BuildProperties.LoadProfile(xProfile.Prefix);
                    mViewModel.BuildProperties.Profile = xProfile.Prefix;

                    UpdateUI();
                }
            };

            #endregion

            # region Deploy

            lboxDeployment.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xValue = (DeploymentType)((EnumValue)lboxDeployment.SelectedItem).Value;
                if (xValue != mViewModel.BuildProperties.Deployment)
                {
                    mViewModel.BuildProperties.Deployment = xValue;
                }
            };

            #endregion

            # region Launch

            lboxLaunch.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xValue = (LaunchType)((EnumValue)lboxLaunch.SelectedItem).Value;
                if (xValue != mViewModel.BuildProperties.Launch)
                {
                    mViewModel.BuildProperties.Launch = xValue;
                    // Bochs and Qemu requires an ISO. Force Deployment property.
                    if (xValue == LaunchType.Bochs)
                    {
                        if (DeploymentType.ISO != mViewModel.BuildProperties.Deployment)
                        {
                            foreach (EnumValue scannedValue in lboxDeployment.Items)
                            {
                                if (DeploymentType.ISO == (DeploymentType)scannedValue.Value)
                                {
                                    lboxDeployment.SelectedItem = scannedValue;
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            #endregion

            #region Compile

            comboFramework.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var value = (Framework)((EnumValue)comboFramework.SelectedItem).Value;
                if (value != mViewModel.BuildProperties.Framework)
                {
                    mViewModel.BuildProperties.Framework = value;
                }
            };
            comboBinFormat.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var value = (BinFormat)((EnumValue)comboBinFormat.SelectedItem).Value;
                if (value != mViewModel.BuildProperties.BinFormat)
                {
                    mViewModel.BuildProperties.BinFormat = value;
                }
            };

            #endregion

            #region Assembler

            checkUseInternalAssembler.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool value = checkUseInternalAssembler.Checked;
                if (value != mViewModel.BuildProperties.UseInternalAssembler)
                {
                    mViewModel.BuildProperties.UseInternalAssembler = value;
                }
            };

            #endregion

            #region VMware

            cmboVMwareEdition.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (VMwareEdition)((EnumValue)cmboVMwareEdition.SelectedItem).Value;
                if (x != mViewModel.BuildProperties.VMwareEdition)
                {
                    mViewModel.BuildProperties.VMwareEdition = x;
                }
            };

            #endregion

            #region PXE

            comboPxeInterface.TextChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = comboPxeInterface.Text.Trim();
                if (x != mViewModel.BuildProperties.PxeInterface)
                {
                    mViewModel.BuildProperties.PxeInterface = x;
                }
            };

            cmboSlavePort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboSlavePort.SelectedItem;
                if (x != mViewModel.BuildProperties.SlavePort)
                {
                    mViewModel.BuildProperties.SlavePort = x;
                }
            };

            butnPxeRefresh.Click += new EventHandler(butnPxeRefresh_Click);

            #endregion

            #region Debug

            comboDebugMode.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (DebugMode)((EnumValue)comboDebugMode.SelectedItem).Value;
                if (x != mViewModel.BuildProperties.DebugMode)
                {
                    mViewModel.BuildProperties.DebugMode = x;
                }
            };

            chckEnableDebugStub.CheckedChanged += delegate (object aSender, EventArgs e)
            {
                if (FreezeEvents) return;
                panlDebugSettings.Enabled = chckEnableDebugStub.Checked;
                mViewModel.BuildProperties.DebugEnabled = chckEnableDebugStub.Checked;
            };

            comboTraceMode.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (TraceAssemblies)((EnumValue)comboTraceMode.SelectedItem).Value;
                if (x != mViewModel.BuildProperties.TraceAssemblies)
                {
                    mViewModel.BuildProperties.TraceAssemblies = x;
                }
            };

            checkIgnoreDebugStubAttribute.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkIgnoreDebugStubAttribute.Checked;
                if (x != mViewModel.BuildProperties.IgnoreDebugStubAttribute)
                {
                    mViewModel.BuildProperties.IgnoreDebugStubAttribute = x;
                }
            };

            cmboCosmosDebugPort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboCosmosDebugPort.SelectedItem;
                if (x != mViewModel.BuildProperties.CosmosDebugPort)
                {
                    mViewModel.BuildProperties.CosmosDebugPort = x;
                }
            };

            cmboVisualStudioDebugPort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboVisualStudioDebugPort.SelectedItem;
                if (x != mViewModel.BuildProperties.VisualStudioDebugPort)
                {
                    mViewModel.BuildProperties.VisualStudioDebugPort = x;
                }
            };

            checkEnableGDB.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkEnableGDB.Checked;
                if (x != mViewModel.BuildProperties.EnableGDB)
                {
                    mViewModel.BuildProperties.EnableGDB = x;
                }
                checkStartCosmosGDB.Enabled = x;
                checkStartCosmosGDB.Checked = x;
            };

            checkStartCosmosGDB.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkStartCosmosGDB.Checked;
                if (x != mViewModel.BuildProperties.StartCosmosGDB)
                {
                    mViewModel.BuildProperties.StartCosmosGDB = x;
                }
            };

            checkEnableBochsDebug.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkEnableBochsDebug.Checked;
                if (x != mViewModel.BuildProperties.EnableBochsDebug)
                {
                    mViewModel.BuildProperties.EnableBochsDebug = x;
                }
                checkStartBochsDebugGui.Enabled = x;

                if (x == false)
                {
                    checkStartBochsDebugGui.Checked = x;
                }
            };

            checkStartBochsDebugGui.CheckedChanged += delegate (object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkStartBochsDebugGui.Checked;
                if (x != mViewModel.BuildProperties.StartBochsDebugGui)
                {
                    mViewModel.BuildProperties.StartBochsDebugGui = x;
                }
            };

            #endregion

            FillProperties();
        }

        protected void RemoveTab(TabPage aTab)
        {
            if (TabControl1.TabPages.Contains(aTab))
            {
                TabControl1.TabPages.Remove(aTab);
            }
        }

        protected void UpdateTabs()
        {
            var xTab = TabControl1.SelectedTab;

            RemoveTab(tabDebug);
            RemoveTab(tabDeployment);
            RemoveTab(tabLaunch);
            RemoveTab(tabVMware);
            RemoveTab(tabHyperV);
            RemoveTab(tabPXE);
            RemoveTab(tabUSB);
            RemoveTab(tabISO);
            RemoveTab(tabSlave);
            RemoveTab(tabBochs);
            RemoveTab(tabQemu);

            if (mShowTabDebug)
            {
                TabControl1.TabPages.Add(tabDebug);
            }

            if (mShowTabDeployment)
            {
                TabControl1.TabPages.Add(tabDeployment);
            }
            if (mShowTabPXE)
            {
                TabControl1.TabPages.Add(tabPXE);
            }
            if (mShowTabUSB)
            {
                TabControl1.TabPages.Add(tabUSB);
            }
            if (mShowTabISO)
            {
                TabControl1.TabPages.Add(tabISO);
            }

            if (mShowTabLaunch)
            {
                TabControl1.TabPages.Add(tabLaunch);
            }
            if (mShowTabVMware)
            {
                TabControl1.TabPages.Add(tabVMware);
            }
            if (mShowTabHyperV)
            {
                TabControl1.TabPages.Add(tabHyperV);
            }
            if (mShowTabSlave)
            {
                TabControl1.TabPages.Add(tabSlave);
            }
            if (mShowTabBochs)
            {
                TabControl1.TabPages.Add(tabBochs);
            }
            if (mShowTabQemu)
            {
                TabControl1.TabPages.Add(tabQemu);
            }

            if (TabControl1.TabPages.Contains(xTab))
            {
                TabControl1.SelectedTab = xTab;
            }
            else
            {
                TabControl1.SelectedTab = tabProfile;
            }
        }

        protected void UpdatePresetsUI()
        {
            FreezeEvents = true;

            mShowTabDebug = true;
            cmboCosmosDebugPort.Enabled = true;
            cmboVisualStudioDebugPort.Enabled = true;

            if (mViewModel.BuildProperties.Profile == "ISO")
            {
                mShowTabDebug = false;
                chckEnableDebugStub.Checked = false;

            }
            else if (mViewModel.BuildProperties.Profile == "USB")
            {
                mShowTabDebug = false;
                mShowTabUSB = true;

            }
            else if (mViewModel.BuildProperties.Profile == "VMware")
            {
                mShowTabVMware = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;

            }
            else if (mViewModel.BuildProperties.Profile == "HyperV")
            {
                mShowTabHyperV = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mHyperVDebugPipe;
            }
            else if (mViewModel.BuildProperties.Profile == "PXE")
            {
                chckEnableDebugStub.Checked = false;
            }
            else if (mViewModel.BuildProperties.Profile == "Bochs")
            {
                mShowTabBochs = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;
            }
            else if (mViewModel.BuildProperties.Profile == "Qemu")
            {
                mShowTabQemu = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;
            }
            else if (mViewModel.BuildProperties.Profile == "IntelEdison")
            {
                mShowTabBochs = false;
                mShowTabVMware = false;
                cmboVisualStudioDebugPort.Enabled = false;
            }
            FreezeEvents = false;
        }

        protected void UpdateUI()
        {
            UpdatePresetsUI();
            var xProfile = (ProfileItem)lboxProfile.SelectedItem;
            if (xProfile == null)
            {
                return;
            }

            if (mShowTabDebug == false)
            {
                chckEnableDebugStub.Checked = false;
            }

            lablCurrentProfile.Text = xProfile.Name;
            lablDeployText.Text = mViewModel.BuildProperties.Description;
            lboxDeployment.SelectedItem = mViewModel.BuildProperties.Deployment;

            // Launch
            lboxLaunch.SelectedItem = mViewModel.BuildProperties.Launch;

            lablBuildOnly.Visible = mViewModel.BuildProperties.Launch == LaunchType.None;

            lboxDeployment.SelectedItem = EnumValue.Find(lboxDeployment.Items, mViewModel.BuildProperties.Deployment);
            lboxLaunch.SelectedItem = EnumValue.Find(lboxLaunch.Items, mViewModel.BuildProperties.Launch);
            cmboVMwareEdition.SelectedItem = EnumValue.Find(cmboVMwareEdition.Items, mViewModel.BuildProperties.VMwareEdition);
            chckEnableDebugStub.Checked = mViewModel.BuildProperties.DebugEnabled;
            chkEnableStackCorruptionDetection.Checked = mViewModel.BuildProperties.StackCorruptionDetectionEnabled;
            comboStackCorruptionDetectionLevel.SelectedItem = EnumValue.Find(comboStackCorruptionDetectionLevel.Items, mViewModel.BuildProperties.StackCorruptionDetectionLevel);

            panlDebugSettings.Enabled = mViewModel.BuildProperties.DebugEnabled;
            cmboCosmosDebugPort.SelectedIndex = cmboCosmosDebugPort.Items.IndexOf(mViewModel.BuildProperties.CosmosDebugPort);
            if (!String.IsNullOrWhiteSpace(mViewModel.BuildProperties.VisualStudioDebugPort))
            {
                cmboVisualStudioDebugPort.SelectedIndex =
                    cmboVisualStudioDebugPort.Items.IndexOf(mViewModel.BuildProperties.VisualStudioDebugPort);
            }
            comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mViewModel.BuildProperties.Framework);
            comboBinFormat.SelectedItem = EnumValue.Find(comboBinFormat.Items, mViewModel.BuildProperties.BinFormat);
            checkUseInternalAssembler.Checked = mViewModel.BuildProperties.UseInternalAssembler;
            checkEnableGDB.Checked = mViewModel.BuildProperties.EnableGDB;
            checkStartCosmosGDB.Checked = mViewModel.BuildProperties.StartCosmosGDB;
            checkEnableBochsDebug.Checked = mViewModel.BuildProperties.EnableBochsDebug;
            checkStartBochsDebugGui.Checked = mViewModel.BuildProperties.StartBochsDebugGui;
            // Locked to COM1 for now.
            //cmboCosmosDebugPort.SelectedIndex = 0;

            #region Slave

            cmboSlavePort.SelectedIndex = cmboSlavePort.Items.IndexOf(mViewModel.BuildProperties.SlavePort);

            #endregion

            checkIgnoreDebugStubAttribute.Checked = mViewModel.BuildProperties.IgnoreDebugStubAttribute;
            comboDebugMode.SelectedItem = EnumValue.Find(comboDebugMode.Items, mViewModel.BuildProperties.DebugMode);
            comboTraceMode.SelectedItem = EnumValue.Find(comboTraceMode.Items, mViewModel.BuildProperties.TraceAssemblies);

            lablPreset.Visible = xProfile.IsPreset;

            mShowTabDeployment = !xProfile.IsPreset;
            mShowTabLaunch = !xProfile.IsPreset;
            //
            mShowTabISO = mViewModel.BuildProperties.Deployment == DeploymentType.ISO;
            mShowTabUSB = mViewModel.BuildProperties.Deployment == DeploymentType.USB;
            mShowTabPXE = mViewModel.BuildProperties.Deployment == DeploymentType.PXE;
            //
            mShowTabVMware = mViewModel.BuildProperties.Launch == LaunchType.VMware;
            mShowTabHyperV = mViewModel.BuildProperties.Launch == LaunchType.HyperV;
            mShowTabSlave = mViewModel.BuildProperties.Launch == LaunchType.Slave;
            mShowTabBochs = mViewModel.BuildProperties.Launch == LaunchType.Bochs;
            mShowTabQemu = mViewModel.BuildProperties.Launch == LaunchType.Qemu;
            //
            UpdateTabs();
        }

        protected int FillProfile(string aPrefix, string aName)
        {
            var xProfile = new ProfileItem { Prefix = aPrefix, Name = aName, IsPreset = true };
            return lboxProfile.Items.Add(xProfile);
        }

        protected int FillProfile(int aID)
        {
            var xProfile = new ProfileItem { Prefix = "User" + aID.ToString("000"), IsPreset = false };
            xProfile.Name = mViewModel.BuildProperties.GetProperty(xProfile.Prefix + "_Name");
            return lboxProfile.Items.Add(xProfile);
        }

        protected void FillProfiles()
        {
            lboxProfile.Items.Clear();

            foreach (var xPreset in mPresets)
            {
                FillProfile(xPreset.Key, xPreset.Value);
            }

            for (int i = 1; i < 100; i++)
            {
                if (!String.IsNullOrEmpty(mViewModel.BuildProperties.GetProperty("User" + i.ToString("000") + "_Name")))
                {
                    FillProfile(i);
                }
            }
        }

        void butnProfileRename_Click(object sender, EventArgs e)
        {
            var xItem = (ProfileItem)lboxProfile.SelectedItem;

            if (xItem == null)
            {
                // This should be impossible, but we check for it anwyays.

            }
            else if (xItem.IsPreset)
            {
                MessageBox.Show("Preset profiles cannot be renamed.");

            }
            else
            {
                string xName = Interaction.InputBox("Profile Name", "Rename Profile", mViewModel.BuildProperties.Name);
                if (xName != "")
                {
                    mViewModel.BuildProperties.Name = xName;
                    xItem.Name = xName;
                    typeof(ListBox).InvokeMember(
                        "RefreshItems",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                        null,
                        lboxProfile,
                        new object[] { });
                }
            }
        }

        void butnProfileDelete_Click(object sender, EventArgs e)
        {
            var xItem = (ProfileItem)lboxProfile.SelectedItem;

            if (xItem == null)
            {
                // This should be impossible, but we check for it anwyays.

            }
            else if (xItem.IsPreset)
            {
                MessageBox.Show("Preset profiles cannot be deleted.");

            }
            else if (MessageBox.Show("Delete profile '" + xItem.Name + "'?", "", MessageBoxButtons.YesNo)
                     == DialogResult.Yes)
            {
                // Select a new profile first, so the selectchange logic wont barf
                lboxProfile.SelectedIndex = 0;
                lboxProfile.Items.Remove(xItem);
                mViewModel.BuildProperties.DeleteProfile(xItem.Prefix);
            }
        }

        void butnProfileClone_Click(object sender, EventArgs e)
        {
            var xItem = (ProfileItem)lboxProfile.SelectedItem;
            if (xItem == null)
            {
                // This should be impossible, but we check for it anwyays.
                return;
            }

            int xID;
            string xPrefix = null;
            for (xID = 1; xID < 100; xID++)
            {
                xPrefix = "User" + xID.ToString("000");
                if (mViewModel.BuildProperties.GetProperty(xPrefix + "_Name") == "")
                {
                    break;
                }
            }
            if (xID == 100)
            {
                MessageBox.Show("No more profile space is available.");
                return;
            }

            mViewModel.BuildProperties.Name = xItem.Prefix + " User " + xID.ToString("000");
            mViewModel.BuildProperties.Description = "";
            mViewModel.BuildProperties.SaveProfile(xPrefix);
            lboxProfile.SelectedIndex = FillProfile(xID);
        }

        void butnPxeRefresh_Click(object sender, EventArgs e)
        {
            FillNetworkInterfaces();
        }

        /// <summary>
        /// Load properties for the given profile.
        /// </summary>
        /// <param name="aPrefix">Name of the profile for which load properties.</param>
        protected void LoadProfileProps(string aPrefix)
        {
            string xPrefix = aPrefix + (aPrefix == "" ? "" : "_");
            foreach (var xName in BuildProperties.PropNames)
            {
                if (!mViewModel.BuildProperties.ProjectIndependentProperties.Contains(xName))
                {
                    string xValue = mViewModel.GetProjectProperty(xPrefix + xName);
                    // This is important that we dont copy empty values, so instead the defaults will be used.
                    if (!String.IsNullOrWhiteSpace(xValue))
                    {
                        mViewModel.BuildProperties.SetProperty(xPrefix + xName, xValue);
                    }
                }
            }
        }

        protected void LoadProps()
        {
            // Load mViewModel.BuildProperties from project config file.
            // The reason for loading into mViewModel.BuildProperties seems to be so we can track changes, and cancel if necessary.
            mViewModel.BuildProperties.Reset();

            // Reset cache only on first one
            // Get selected profile
            mViewModel.LoadProfile();

            mViewModel.LoadProjectProperties();

            // Load selected profile props
            LoadProfileProps("");
            foreach (var xPreset in mPresets)
            {
                LoadProfileProps(xPreset.Key);
            }
            for (int id = 1; id < 100; id++)
            {
                var xPrefix = "User" + id.ToString("000");
                LoadProfileProps(xPrefix);
            }
        }

        private void FillProperties()
        {
            LoadProps();

            FillProfiles();
            foreach (ProfileItem xItem in lboxProfile.Items)
            {
                if (xItem.Prefix == mViewModel.BuildProperties.Profile)
                {
                    lboxProfile.SelectedItem = xItem;
                    break;
                }
            }

            lboxDeployment.Items.AddRange(EnumValue.GetEnumValues(typeof(DeploymentType), true));
            comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework), true));
            comboBinFormat.Items.AddRange(EnumValue.GetEnumValues(typeof(BinFormat), true));
            lboxLaunch.Items.AddRange(EnumValue.GetEnumValues(typeof(LaunchType), true));

            #region VMware

            cmboVMwareEdition.Items.AddRange(EnumValue.GetEnumValues(typeof(VMwareEdition), true));

            #endregion

            #region Debug

            cmboCosmosDebugPort.Items.Clear();
            FillComPorts(cmboCosmosDebugPort.Items);

            cmboVisualStudioDebugPort.Items.Clear();
            FillComPorts(cmboVisualStudioDebugPort.Items);
            mVMwareAndBochsDebugPipe = cmboVisualStudioDebugPort.Items.Add(@"Pipe: Cosmos\Serial");
            mHyperVDebugPipe = cmboVisualStudioDebugPort.Items.Add(@"Pipe: CosmosSerial");

            comboDebugMode.Items.AddRange(EnumValue.GetEnumValues(typeof(DebugMode), false));
            comboTraceMode.Items.AddRange(EnumValue.GetEnumValues(typeof(TraceAssemblies), false));
            comboStackCorruptionDetectionLevel.Items.AddRange(EnumValue.GetEnumValues(typeof(StackCorruptionDetectionLevel), true));

            #endregion

            #region PXE

            FillNetworkInterfaces();

            cmboSlavePort.Items.Clear();
            cmboSlavePort.Items.Add("None");
            FillComPorts(cmboSlavePort.Items);

            #endregion

            UpdateUI();
        }

        protected void FillComPorts(System.Collections.IList aList)
        {
            //TODO http://stackoverflow.com/questions/2937585/how-to-open-a-serial-port-by-friendly-name
            foreach (string xPort in SerialPort.GetPortNames())
            {
                aList.Add("Serial: " + xPort);
            }
        }

        protected void FillNetworkInterfaces()
        {
            comboPxeInterface.Items.Clear();

            comboPxeInterface.Items.AddRange(GetNetworkInterfaces().ToArray());

            if (mViewModel.BuildProperties.PxeInterface == String.Empty)
            {
                if (comboPxeInterface.Items.Count > 0)
                {
                    comboPxeInterface.Text = comboPxeInterface.Items[0].ToString();
                }
                else
                {
                    comboPxeInterface.Text = "192.168.42.1";
                }
            }
            else
            {
                comboPxeInterface.Text = mViewModel.BuildProperties.PxeInterface;
            }
        }

        protected List<string> GetNetworkInterfaces()
        {
            NetworkInterface[] nInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var interfaces_list = new List<string>();

            foreach (NetworkInterface nInterface in nInterfaces)
            {
                if (nInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = nInterface.GetIPProperties();

                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            interfaces_list.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return interfaces_list;
        }

        private void chkEnableStackCorruptionDetection_CheckedChanged(object sender, EventArgs e)
        {
            if (!FreezeEvents)
            {
                mViewModel.BuildProperties.StackCorruptionDetectionEnabled = chkEnableStackCorruptionDetection.Checked;
                comboStackCorruptionDetectionLevel.Enabled = mViewModel.BuildProperties.StackCorruptionDetectionEnabled;
            }
        }

        private void stackCorruptionDetectionLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!FreezeEvents)
            {
                var x = (StackCorruptionDetectionLevel)((EnumValue) comboStackCorruptionDetectionLevel.SelectedItem).Value;
                if (x != mViewModel.BuildProperties.StackCorruptionDetectionLevel)
                {
                    mViewModel.BuildProperties.StackCorruptionDetectionLevel = x;
                }
            }
        }
    }
}
