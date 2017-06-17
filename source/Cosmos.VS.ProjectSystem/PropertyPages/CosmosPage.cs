using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Cosmos.Build.Common;
using Cosmos.VS.Package;
using Microsoft.VisualBasic;
using DebugMode = Cosmos.Build.Common.DebugMode;

namespace Cosmos.VS.ProjectSystem.PropertyPages
{
    // We put all items on ONE form because VS is such a C++ developers wet dream to manage mulitple pages
    // and add new ones.

    // We use our own profiles instead of Project Configrations because:
    // -Couldnt find a proper way to tell VS there are no configuration independent pages. The MPF way (as well as
    //  0 sized array) both don't work.
    // -When we assign our own project configuration types and the solution configurations refer to now non existent
    //  project configurations, there are errors.
    //
    // BOTH of the above probably could be solved with some digging. BUT in the end we learned that project configs
    // really wont do what we want. For the user to change the active config for a *single* project they would need
    // to change it manually in the solution config, or maintain on solution config for every project config type.
    // Maintaining so many solution configs is not only impractical but causes a whole bunch of other issues.
    //
    // So instead we keep our own list of profiles, and when the user selects them we write out a copy of its values
    // to the active configuration (all since we are not configuration dependent) for MSBuild and other code to use.

    [Guid(Guids.guidCosmosPropertyPageString)]
    public partial class CosmosPage : CustomPropertyPage
    {
        protected class ProfileItem
        {
            public string Prefix;

            public string Name;

            public bool IsPreset;

            public override string ToString()
            {
                return Name;
            }
        }

        protected ProfilePresets mPresets = new ProfilePresets();

        protected int mVMwareAndBochsDebugPipe;
        protected int mHyperVDebugPipe;

        protected bool mShowTabBochs;
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

        public CosmosPage()
        {
            InitializeComponent();

            # region Profile

            butnProfileClone.Click += new EventHandler(butnProfileClone_Click);
            butnProfileDelete.Click += new EventHandler(butnProfileDelete_Click);
            butnProfileRename.Click += new EventHandler(butnProfileRename_Click);

            lboxProfile.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xProfile = (ProfileItem)lboxProfile.SelectedItem;
                if (xProfile.Prefix != mProps.Profile)
                {
                    // Save existing profile
                    mProps.SaveProfile(mProps.Profile);
                    // Load newly selected profile
                    mProps.LoadProfile(xProfile.Prefix);
                    mProps.Profile = xProfile.Prefix;

                    IsDirty = true;
                    UpdateUI();
                }
            };

            #endregion

            # region Deploy

            lboxDeployment.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xValue = (DeploymentType)((EnumValue)lboxDeployment.SelectedItem).Value;
                if (xValue != mProps.Deployment)
                {
                    mProps.Deployment = xValue;
                    IsDirty = true;
                }
            };

            #endregion

            # region Launch

            lboxLaunch.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                var xValue = (LaunchType)((EnumValue)lboxLaunch.SelectedItem).Value;
                if (xValue != mProps.Launch)
                {
                    mProps.Launch = xValue;
                    IsDirty = true;
                    // Bochs requires an ISO. Force Deployment property.
                    if (LaunchType.Bochs == xValue)
                    {
                        if (DeploymentType.ISO != mProps.Deployment)
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
                if (value != mProps.Framework)
                {
                    mProps.Framework = value;
                    IsDirty = true;
                }
            };
            comboBinFormat.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var value = (BinFormat)((EnumValue)comboBinFormat.SelectedItem).Value;
                if (value != mProps.BinFormat)
                {
                    mProps.BinFormat = value;
                    IsDirty = true;
                }
            };

            textOutputPath.TextChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                string value = textOutputPath.Text;
                if (!string.Equals(value, mProps.OutputPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    mProps.OutputPath = textOutputPath.Text;
                    IsDirty = true;
                }
            };

            #endregion

            #region Assembler

            checkUseInternalAssembler.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool value = checkUseInternalAssembler.Checked;
                if (value != mProps.UseInternalAssembler)
                {
                    mProps.UseInternalAssembler = value;
                    IsDirty = true;
                }
            };

            #endregion

            #region VMware

            cmboVMwareEdition.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (VMwareEdition)((EnumValue)cmboVMwareEdition.SelectedItem).Value;
                if (x != mProps.VMwareEdition)
                {
                    mProps.VMwareEdition = x;
                    IsDirty = true;
                }
            };

            #endregion

            #region PXE

            comboPxeInterface.TextChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = comboPxeInterface.Text.Trim();
                if (x != mProps.PxeInterface)
                {
                    mProps.PxeInterface = x;
                    IsDirty = true;
                }
            };

            cmboSlavePort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboSlavePort.SelectedItem;
                if (x != mProps.SlavePort)
                {
                    mProps.SlavePort = x;
                    IsDirty = true;
                }
            };

            butnPxeRefresh.Click += new EventHandler(butnPxeRefresh_Click);

            #endregion

            #region Debug

            comboDebugMode.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (Cosmos.Build.Common.DebugMode)((EnumValue)comboDebugMode.SelectedItem).Value;
                if (x != mProps.DebugMode)
                {
                    mProps.DebugMode = x;
                    IsDirty = true;
                }
            };

            chckEnableDebugStub.CheckedChanged += delegate (object aSender, EventArgs e)
            {
                if (FreezeEvents) return;
                panlDebugSettings.Enabled = chckEnableDebugStub.Checked;
                mProps.DebugEnabled = chckEnableDebugStub.Checked;
                IsDirty = true;
            };

            comboTraceMode.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (TraceAssemblies)((EnumValue)comboTraceMode.SelectedItem).Value;
                if (x != mProps.TraceAssemblies)
                {
                    mProps.TraceAssemblies = x;
                    IsDirty = true;
                }
            };

            checkIgnoreDebugStubAttribute.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkIgnoreDebugStubAttribute.Checked;
                if (x != mProps.IgnoreDebugStubAttribute)
                {
                    mProps.IgnoreDebugStubAttribute = x;
                    IsDirty = true;
                }
            };

            cmboCosmosDebugPort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboCosmosDebugPort.SelectedItem;
                if (x != mProps.CosmosDebugPort)
                {
                    mProps.CosmosDebugPort = x;
                    IsDirty = true;
                }
            };

            cmboVisualStudioDebugPort.SelectedIndexChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                var x = (string)cmboVisualStudioDebugPort.SelectedItem;
                if (x != mProps.VisualStudioDebugPort)
                {
                    mProps.VisualStudioDebugPort = x;
                    IsDirty = true;
                }
            };

            checkEnableGDB.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkEnableGDB.Checked;
                if (x != mProps.EnableGDB)
                {
                    mProps.EnableGDB = x;
                    IsDirty = true;
                }
                checkStartCosmosGDB.Enabled = x;
                checkStartCosmosGDB.Checked = x;
            };

            checkStartCosmosGDB.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkStartCosmosGDB.Checked;
                if (x != mProps.StartCosmosGDB)
                {
                    mProps.StartCosmosGDB = x;
                    IsDirty = true;
                }
            };

            checkEnableBochsDebug.CheckedChanged += delegate (Object sender, EventArgs e)
            {
                if (FreezeEvents) return;
                bool x = checkEnableBochsDebug.Checked;
                if (x != mProps.EnableBochsDebug)
                {
                    mProps.EnableBochsDebug = x;
                    IsDirty = true;
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
                if (x != mProps.StartBochsDebugGui)
                {
                    mProps.StartBochsDebugGui = x;
                    IsDirty = true;
                }
            };
            #endregion
        }

        public override void ApplyChanges()
        {
            // Save now, because when we reload this form it will
            // reload out of the saved profile and trash any changes
            // in the active area.
            mProps.SaveProfile(mProps.Profile);
            base.ApplyChanges();
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

            if (mProps.Profile == "ISO")
            {
                mShowTabDebug = false;

            }
            else if (mProps.Profile == "USB")
            {
                mShowTabDebug = false;
                mShowTabUSB = true;

            }
            else if (mProps.Profile == "VMware")
            {
                mShowTabVMware = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;

            }
            else if (mProps.Profile == "HyperV")
            {
                mShowTabHyperV = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mHyperVDebugPipe;
            }
            else if (mProps.Profile == "PXE")
            {
                chckEnableDebugStub.Checked = false;
            }
            else if (mProps.Profile == "Bochs")
            {
                mShowTabBochs = true;
                chckEnableDebugStub.Checked = true;
                chkEnableStackCorruptionDetection.Checked = true;
                cmboCosmosDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.Enabled = false;
                cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;
            }
            else if (mProps.Profile == "IntelEdison")
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
            lablDeployText.Text = mProps.Description;
            lboxDeployment.SelectedItem = mProps.Deployment;

            // Launch
            lboxLaunch.SelectedItem = mProps.Launch;

            lablBuildOnly.Visible = mProps.Launch == LaunchType.None;

            lboxDeployment.SelectedItem = EnumValue.Find(lboxDeployment.Items, mProps.Deployment);
            lboxLaunch.SelectedItem = EnumValue.Find(lboxLaunch.Items, mProps.Launch);
            cmboVMwareEdition.SelectedItem = EnumValue.Find(cmboVMwareEdition.Items, mProps.VMwareEdition);
            chckEnableDebugStub.Checked = mProps.DebugEnabled;
            chkEnableStackCorruptionDetection.Checked = mProps.StackCorruptionDetectionEnabled;
            comboStackCorruptionDetectionLevel.SelectedItem = EnumValue.Find(comboStackCorruptionDetectionLevel.Items, mProps.StackCorruptionDetectionLevel);

            panlDebugSettings.Enabled = mProps.DebugEnabled;
            cmboCosmosDebugPort.SelectedIndex = cmboCosmosDebugPort.Items.IndexOf(mProps.CosmosDebugPort);
            if (!String.IsNullOrWhiteSpace(mProps.VisualStudioDebugPort))
            {
                cmboVisualStudioDebugPort.SelectedIndex =
                    cmboVisualStudioDebugPort.Items.IndexOf(mProps.VisualStudioDebugPort);
            }
            textOutputPath.Text = mProps.OutputPath;
            comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mProps.Framework);
            comboBinFormat.SelectedItem = EnumValue.Find(comboBinFormat.Items, mProps.BinFormat);
            checkUseInternalAssembler.Checked = mProps.UseInternalAssembler;
            checkEnableGDB.Checked = mProps.EnableGDB;
            checkStartCosmosGDB.Checked = mProps.StartCosmosGDB;
            checkEnableBochsDebug.Checked = mProps.EnableBochsDebug;
            checkStartBochsDebugGui.Checked = mProps.StartBochsDebugGui;
            // Locked to COM1 for now.
            //cmboCosmosDebugPort.SelectedIndex = 0;

            #region Slave

            cmboSlavePort.SelectedIndex = cmboSlavePort.Items.IndexOf(mProps.SlavePort);

            #endregion

            checkIgnoreDebugStubAttribute.Checked = mProps.IgnoreDebugStubAttribute;
            comboDebugMode.SelectedItem = EnumValue.Find(comboDebugMode.Items, mProps.DebugMode);
            comboTraceMode.SelectedItem = EnumValue.Find(comboTraceMode.Items, mProps.TraceAssemblies);

            lablPreset.Visible = xProfile.IsPreset;

            mShowTabDeployment = !xProfile.IsPreset;
            mShowTabLaunch = !xProfile.IsPreset;
            //
            mShowTabISO = mProps.Deployment == DeploymentType.ISO;
            mShowTabUSB = mProps.Deployment == DeploymentType.USB;
            mShowTabPXE = mProps.Deployment == DeploymentType.PXE;
            //
            mShowTabVMware = mProps.Launch == LaunchType.VMware;
            mShowTabHyperV = mProps.Launch == LaunchType.HyperV;
            mShowTabSlave = mProps.Launch == LaunchType.Slave;
            mShowTabBochs = (LaunchType.Bochs == mProps.Launch);
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
            xProfile.Name = mProps.GetProperty(xProfile.Prefix + "_Name");
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
                if (!string.IsNullOrEmpty(mProps.GetProperty("User" + i.ToString("000") + "_Name")))
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
                string xName = Interaction.InputBox("Profile Name", "Rename Profile", mProps.Name);
                if (xName != "")
                {
                    IsDirty = true;
                    mProps.Name = xName;
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
                IsDirty = true;
                // Select a new profile first, so the selectchange logic wont barf
                lboxProfile.SelectedIndex = 0;
                lboxProfile.Items.Remove(xItem);
                mProps.DeleteProfile(xItem.Prefix);
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
                if (mProps.GetProperty(xPrefix + "_Name") == "")
                {
                    break;
                }
            }
            if (xID == 100)
            {
                MessageBox.Show("No more profile space is available.");
                return;
            }

            IsDirty = true;
            mProps.Name = xItem.Prefix + " User " + xID.ToString("000");
            mProps.Description = "";
            mProps.SaveProfile(xPrefix);
            lboxProfile.SelectedIndex = FillProfile(xID);
        }

        void butnPxeRefresh_Click(object sender, EventArgs e)
        {
            FillNetworkInterfaces();
        }

        protected BuildProperties mProps = new BuildProperties();

        public override PropertiesBase Properties
        {
            get
            {
                return mProps;
            }
        }

        /// <summary>
        /// Load project independent properties.
        /// </summary>
        protected void LoadProjectProps()
        {
            foreach (var propertyName in mProps.ProjectIndependentProperties)
            {
                var propertyValue = ProjectMgr.GetProjectProperty(propertyName);
                mProps.SetProperty(propertyName, propertyValue);
            }
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
                if (!mProps.ProjectIndependentProperties.Contains(xName))
                {
                    string xValue = ProjectConfigs[0].GetConfigurationProperty(xPrefix + xName, false);
                    // This is important that we dont copy empty values, so instead the defaults will be used.
                    if (!string.IsNullOrWhiteSpace(xValue))
                    {
                        mProps.SetProperty(xPrefix + xName, xValue);
                    }
                }
            }
        }

        protected void LoadProps()
        {
            // Load mProps from project config file.
            // The reason for loading into mProps seems to be so we can track changes, and cancel if necessary.
            mProps.Reset();

            // Reset cache only on first one
            // Get selected profile
            mProps.SetProperty(
                BuildPropertyNames.ProfileString,
                ProjectConfigs[0].GetConfigurationProperty(BuildPropertyNames.ProfileString, true));

            LoadProjectProps();

            // Load selected profile props
            LoadProfileProps("");
            foreach (var xPreset in mPresets)
            {
                LoadProfileProps(xPreset.Key);
            }
            for (int i = 1; i < 100; i++)
            {
                string xPrefix = "User" + i.ToString("000");
                if (!string.IsNullOrWhiteSpace(ProjectConfigs[0].GetConfigurationProperty(xPrefix + "_Name", false)))
                {
                    LoadProfileProps(xPrefix);
                }
            }
        }

        protected override void FillProperties()
        {
            base.FillProperties();
            LoadProps();

            FillProfiles();
            foreach (ProfileItem xItem in lboxProfile.Items)
            {
                if (xItem.Prefix == mProps.Profile)
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

            if (mProps.PxeInterface == String.Empty)
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
                comboPxeInterface.Text = mProps.PxeInterface;
            }
        }

        protected List<string> GetNetworkInterfaces()
        {
            NetworkInterface[] nInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<string> interfaces_list = new List<string>();

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

        private void OutputBrowse_Click(object sender, EventArgs e)
        {
            string folderPath = String.Empty;
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;

            folderPath = textOutputPath.Text;
            if ((String.IsNullOrEmpty(folderPath) == false)
                && (folderPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1))
            {
                if (System.IO.Path.IsPathRooted(folderPath) == false)
                {
                    folderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Project.FullName), folderPath);
                }

                while ((System.IO.Directory.Exists(folderPath) == false) && (String.IsNullOrEmpty(folderPath) == false))
                {
                    int index = -1;
                    index =
                        folderPath.IndexOfAny(
                            new Char[] { System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar });
                    if (index > -1)
                    {
                        folderPath = folderPath.Substring(0, index - 1);
                    }
                    else
                    {
                        folderPath = String.Empty;
                    }
                }

                if (String.IsNullOrEmpty(folderPath) == true)
                {
                    folderPath = System.IO.Path.GetDirectoryName(Project.FullName);
                }
            }
            else
            {
                folderPath = System.IO.Path.GetDirectoryName(Project.FullName);
            }

            dialog.SelectedPath = folderPath;
            dialog.Description = "Select build output path";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textOutputPath.Text = dialog.SelectedPath;
            }
        }

        private void chkEnableStacckCorruptionDetection_CheckedChanged(object sender, EventArgs e)
        {
            if (!FreezeEvents)
            {
                IsDirty = true;
                mProps.StackCorruptionDetectionEnabled = chkEnableStackCorruptionDetection.Checked;
                comboStackCorruptionDetectionLevel.Enabled = mProps.StackCorruptionDetectionEnabled;
            }
        }

        private void stackCorruptionDetectionLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!FreezeEvents)
            {
                var x = (StackCorruptionDetectionLevel)((EnumValue) comboStackCorruptionDetectionLevel.SelectedItem).Value;
                if (x != mProps.StackCorruptionDetectionLevel)
                {
                    mProps.StackCorruptionDetectionLevel = x;
                    IsDirty = true;
                }
            }
        }
    }
}
