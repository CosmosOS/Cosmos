using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using Cosmos.Build.Common;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualBasic;

namespace Cosmos.VS.Package {
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

  [Guid(Guids.CosmosPage)]
  public partial class CosmosPage : CustomPropertyPage {
    protected class ProfileItem {
      public string Prefix;
      public string Name;
      public bool IsPreset;

      public override string ToString() {
        return Name;
      }
    }

    protected ProfilePresets mPresets = new ProfilePresets();
    /// <summary>The index in the <see cref="cmboVisualStudioDebugPort"/> combo for the pipe name used by both
    /// Bochs and VMware environment to communicate with Vsiual Studio debugger.</summary>
    protected int mVMwareAndBochsDebugPipe;

    protected bool mShowTabBochs;
    protected bool mShowTabDebug;
    protected bool mShowTabDeployment;
    protected bool mShowTabLaunch;
    protected bool mShowTabVMware;
    protected bool mShowTabPXE;
    protected bool mShowTabUSB;
    protected bool mShowTabISO;
    protected bool mShowTabSlave;

    public override void ApplyChanges() {
      // Save now, because when we reload this form it will
      // reload out of the saved profile and trash any changes
      // in the active area.
      mProps.SaveProfile(mProps.Profile);
      base.ApplyChanges();
    }

    protected void RemoveTab(TabPage aTab) {
      if (TabControl1.TabPages.Contains(aTab)) {
        TabControl1.TabPages.Remove(aTab);
      }
    }

    protected void UpdateTabs() {
      var xTab = TabControl1.SelectedTab;

      RemoveTab(tabDebug);
      RemoveTab(tabDeployment);
      RemoveTab(tabLaunch);
      RemoveTab(tabVMware);
      RemoveTab(tabPXE);
      RemoveTab(tabUSB);
      RemoveTab(tabISO);
      RemoveTab(tabSlave);
      RemoveTab(tabBochs);

      if (mShowTabDebug) {
        TabControl1.TabPages.Add(tabDebug);
      }

      if (mShowTabDeployment) {
        TabControl1.TabPages.Add(tabDeployment);
      }
      if (mShowTabPXE) {
        TabControl1.TabPages.Add(tabPXE);
      }
      if (mShowTabUSB) {
        TabControl1.TabPages.Add(tabUSB);
      }
      if (mShowTabISO) {
        TabControl1.TabPages.Add(tabISO);
      }

      if (mShowTabLaunch) {
        TabControl1.TabPages.Add(tabLaunch);
      }
      if (mShowTabVMware) {
        TabControl1.TabPages.Add(tabVMware);
      }
      if (mShowTabSlave) {
        TabControl1.TabPages.Add(tabSlave);
      }
      if (mShowTabBochs) {
        TabControl1.TabPages.Add(tabBochs);
      }

      if (TabControl1.TabPages.Contains(xTab)) {
        TabControl1.SelectedTab = xTab;
      } else {
        TabControl1.SelectedTab = tabProfile;
      }
    }

    protected void UpdatePresetsUI() {
      mShowTabDebug = true;
      cmboCosmosDebugPort.Enabled = true;
      cmboVisualStudioDebugPort.Enabled = true;

      if (mProps.Profile == "ISO") {
        mShowTabDebug = false;

      } else if (mProps.Profile == "USB") {
        mShowTabDebug = false;
        mShowTabUSB = true;

      } else if (mProps.Profile == "VMware") {
        mShowTabVMware = true;
        chckEnableDebugStub.Checked = true;
        chkEnableStackCorruptionDetection.Checked = true;
        cmboCosmosDebugPort.Enabled = false;
        cmboVisualStudioDebugPort.Enabled = false;
        cmboVisualStudioDebugPort.SelectedIndex = mVMwareAndBochsDebugPipe;

      } else if (mProps.Profile == "PXE") {
        chckEnableDebugStub.Checked = false;

      } else if (mProps.Profile == "Bochs") {
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
    }

    protected void UpdateUI() {
      UpdatePresetsUI();
      var xProfile = (ProfileItem)lboxProfile.SelectedItem;
      if (xProfile == null) {
        return;
      }

      if (mShowTabDebug == false) {
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
      panlDebugSettings.Enabled = mProps.DebugEnabled;
      cmboCosmosDebugPort.SelectedIndex = cmboCosmosDebugPort.Items.IndexOf(mProps.CosmosDebugPort);
      cmboVisualStudioDebugPort.SelectedIndex = cmboVisualStudioDebugPort.Items.IndexOf(mProps.VisualStudioDebugPort);
      textOutputPath.Text = mProps.OutputPath;
      comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mProps.Framework);
      checkUseInternalAssembler.Checked = mProps.UseInternalAssembler;
      checkEnableGDB.Checked = mProps.EnableGDB;
      checkStartCosmosGDB.Checked = mProps.StartCosmosGDB;
      checkEnableBochsDebug.Checked = mProps.EnableBochsDebug;
      // Locked to COM1 for now.
      //cmboCosmosDebugPort.SelectedIndex = 0;

      #region PXE
      textPxeInterface.Text = mProps.PxeInterface;
      #endregion

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
      mShowTabSlave = mProps.Launch == LaunchType.Slave;
      mShowTabBochs = (LaunchType.Bochs == mProps.Launch);
      //
      UpdateTabs();
    }

    protected int FillProfile(string aPrefix, string aName) {
      var xProfile = new ProfileItem {
        Prefix = aPrefix,
        Name = aName,
        IsPreset = true
      };
      return lboxProfile.Items.Add(xProfile);
    }

    protected int FillProfile(int aID) {
      var xProfile = new ProfileItem {
        Prefix = "User" + aID.ToString("000"),
        IsPreset = false
      };
      xProfile.Name = mProps.GetProperty(xProfile.Prefix + "_Name");
      return lboxProfile.Items.Add(xProfile);
    }

    protected void FillProfiles() {
      lboxProfile.Items.Clear();

      foreach (var xPreset in mPresets) {
        FillProfile(xPreset.Key, xPreset.Value);
      }

      for (int i = 1; i < 100; i++) {
        if (!string.IsNullOrEmpty(mProps.GetProperty("User" + i.ToString("000") + "_Name"))) {
          FillProfile(i);
        }
      }
    }

    public CosmosPage() {
      InitializeComponent();

      # region Profile
      butnProfileClone.Click += new EventHandler(butnProfileClone_Click);
      butnProfileDelete.Click += new EventHandler(butnProfileDelete_Click);
      butnProfileRename.Click += new EventHandler(butnProfileRename_Click);

      lboxProfile.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xProfile = (ProfileItem)lboxProfile.SelectedItem;
        if (xProfile.Prefix != mProps.Profile) {
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
      lboxDeployment.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xValue = (DeploymentType)((EnumValue)lboxDeployment.SelectedItem).Value;
        if (xValue != mProps.Deployment) {
          mProps.Deployment = xValue;
          IsDirty = true;
        }
      };
      #endregion

      # region Launch
      lboxLaunch.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xValue = (LaunchType)((EnumValue)lboxLaunch.SelectedItem).Value;
        if (xValue != mProps.Launch) {
          mProps.Launch = xValue;
          IsDirty = true;
          // Bochs requires an ISO. Force Deployment property.
          if (LaunchType.Bochs == xValue) {
            if (DeploymentType.ISO != mProps.Deployment) {
              foreach (EnumValue scannedValue in lboxDeployment.Items)
              {
                if (DeploymentType.ISO == (DeploymentType)scannedValue.Value) {
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
      comboFramework.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var value = (Framework)((EnumValue)comboFramework.SelectedItem).Value;
        if (value != mProps.Framework) {
          mProps.Framework = value;
          IsDirty = true;
        }
      };

      textOutputPath.TextChanged += delegate(Object sender, EventArgs e) {
        string value = textOutputPath.Text;
        if (!string.Equals(value, mProps.OutputPath, StringComparison.InvariantCultureIgnoreCase)) {
          mProps.OutputPath = textOutputPath.Text;
          IsDirty = true;
        }
      };
      #endregion

      #region Assembler
      checkUseInternalAssembler.CheckedChanged += delegate(Object sender, EventArgs e) {
        bool value = checkUseInternalAssembler.Checked;
        if (value != mProps.UseInternalAssembler) {
          mProps.UseInternalAssembler = value;
          IsDirty = true;
        }
      };
      #endregion


      #region VMware
      cmboVMwareEdition.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (VMwareEdition)((EnumValue)cmboVMwareEdition.SelectedItem).Value;
        if (x != mProps.VMwareEdition) {
          mProps.VMwareEdition = x;
          IsDirty = true;
        }
      };
      #endregion

      #region PXE
      textPxeInterface.TextChanged += delegate(Object sender, EventArgs e) {
        var x = textPxeInterface.Text.Trim();
        if (x != mProps.PxeInterface) {
          mProps.PxeInterface = x;
          IsDirty = true;
        }
      };

      cmboSlavePort.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (string)cmboSlavePort.SelectedItem;
        if (x != mProps.SlavePort) {
          mProps.SlavePort = x;
          IsDirty = true;
        }
      };
      #endregion

      #region Debug
      comboDebugMode.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (Cosmos.Build.Common.DebugMode)((EnumValue)comboDebugMode.SelectedItem).Value;
        if (x != mProps.DebugMode) {
          mProps.DebugMode = x;
          IsDirty = true;
        }
      };

      chckEnableDebugStub.CheckedChanged += delegate(object aSender, EventArgs e) {
        panlDebugSettings.Enabled = chckEnableDebugStub.Checked;
        mProps.DebugEnabled = chckEnableDebugStub.Checked;
        IsDirty = true;
      };

      comboTraceMode.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (TraceAssemblies)((EnumValue)comboTraceMode.SelectedItem).Value;
        if (x != mProps.TraceAssemblies) {
          mProps.TraceAssemblies = x;
          IsDirty = true;
        }
      };

      checkIgnoreDebugStubAttribute.CheckedChanged += delegate(Object sender, EventArgs e) {
        bool x = checkIgnoreDebugStubAttribute.Checked;
        if (x != mProps.IgnoreDebugStubAttribute) {
          mProps.IgnoreDebugStubAttribute = x;
          IsDirty = true;
        }
      };

      cmboCosmosDebugPort.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (string)cmboCosmosDebugPort.SelectedItem;
        if (x != mProps.CosmosDebugPort) {
          mProps.CosmosDebugPort = x;
          IsDirty = true;
        }
      };

      cmboVisualStudioDebugPort.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (string)cmboVisualStudioDebugPort.SelectedItem;
        if (x != mProps.VisualStudioDebugPort) {
          mProps.VisualStudioDebugPort = x;
          IsDirty = true;
        }
      };
      #endregion

      checkEnableGDB.CheckedChanged += delegate(Object sender, EventArgs e) {
        bool x = checkEnableGDB.Checked;
        if (x != mProps.EnableGDB) {
          mProps.EnableGDB = x;
          IsDirty = true;
        }
        checkStartCosmosGDB.Enabled = x;
        checkStartCosmosGDB.Checked = x;
      };

      checkStartCosmosGDB.CheckedChanged += delegate(Object sender, EventArgs e) {
        bool x = checkStartCosmosGDB.Checked;
        if (x != mProps.StartCosmosGDB) {
          mProps.StartCosmosGDB = x;
          IsDirty = true;
        }
      };

      checkEnableBochsDebug.CheckedChanged += delegate(Object sender, EventArgs e)
      {
          bool x = checkEnableBochsDebug.Checked;
          if (x != mProps.EnableBochsDebug)
          {
              mProps.EnableBochsDebug = x;
              IsDirty = true;
          }
      };
    }

    void butnProfileRename_Click(object sender, EventArgs e) {
      var xItem = (ProfileItem)lboxProfile.SelectedItem;

      if (xItem == null) {
        // This should be impossible, but we check for it anwyays.

      } else if (xItem.IsPreset) {
        MessageBox.Show("Preset profiles cannot be renamed.");

      } else {
        string xName = Interaction.InputBox("Profile Name", "Rename Profile", mProps.Name);
        if (xName != "") {
          IsDirty = true;
          mProps.Name = xName;
          xItem.Name = xName;
          typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod
            , null, lboxProfile, new object[] { });
        }
      }
    }

    void butnProfileDelete_Click(object sender, EventArgs e) {
      var xItem = (ProfileItem)lboxProfile.SelectedItem;

      if (xItem == null) {
        // This should be impossible, but we check for it anwyays.

      } else if (xItem.IsPreset) {
        MessageBox.Show("Preset profiles cannot be deleted.");

      } else if (MessageBox.Show("Delete profile '" + xItem.Name + "'?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
        IsDirty = true;
        // Select a new profile first, so the selectchange logic wont barf
        lboxProfile.SelectedIndex = 0;
        lboxProfile.Items.Remove(xItem);
        mProps.DeleteProfile(xItem.Prefix);
      }
    }

    void butnProfileClone_Click(object sender, EventArgs e) {
      var xItem = (ProfileItem)lboxProfile.SelectedItem;
      if (xItem == null) {
        // This should be impossible, but we check for it anwyays.
        return;
      }

      int xID;
      string xPrefix = null;
      for (xID = 1; xID < 100; xID++) {
        xPrefix = "User" + xID.ToString("000");
        if (mProps.GetProperty(xPrefix + "_Name") == "") {
          break;
        }
      }
      if (xID == 100) {
        MessageBox.Show("No more profile space is available.");
        return;
      }

      IsDirty = true;
      mProps.Name = xItem.Prefix + " User " + xID.ToString("000");
      mProps.Description = "";
      mProps.SaveProfile(xPrefix);
      lboxProfile.SelectedIndex = FillProfile(xID);
    }

    protected BuildProperties mProps = new BuildProperties();
    public override PropertiesBase Properties {
      get { return mProps; }
    }

    protected void LoadProfileProps(string aPrefix) {
      string xPrefix = aPrefix + (aPrefix == "" ? "" : "_");
      foreach (var xName in BuildProperties.PropNames) {
        string xValue = ProjectConfigs[0].GetConfigurationProperty(xPrefix + xName, false);
        // This is important that we dont copy empty values, so instead the defaults will be used.
        if (!string.IsNullOrWhiteSpace(xValue)) {
          mProps.SetProperty(xPrefix + xName, xValue);
        }
      }
    }

    protected void LoadProps() {
      // Load mProps from project config file.
      // The reason for loading into mProps seems to be so we can track changes, and cancel if necessary.
      mProps.Reset();

      // Reset cache only on first one
      // Get selected profile
      mProps.SetProperty(BuildProperties.ProfileString, ProjectConfigs[0].GetConfigurationProperty(BuildProperties.ProfileString, true));

      // Load selected profile props
      LoadProfileProps("");
      foreach (var xPreset in mPresets) {
        LoadProfileProps(xPreset.Key);
      }
      for (int i = 1; i < 100; i++) {
        string xPrefix = "User" + i.ToString("000");
        if (!string.IsNullOrWhiteSpace(ProjectConfigs[0].GetConfigurationProperty(xPrefix + "_Name", false))) {
          LoadProfileProps(xPrefix);
        }
      }
    }

    protected override void FillProperties() {
      base.FillProperties();
      LoadProps();

      FillProfiles();
      foreach (ProfileItem xItem in lboxProfile.Items) {
        if (xItem.Prefix == mProps.Profile) {
          lboxProfile.SelectedItem = xItem;
          break;
        }
      }

      lboxDeployment.Items.AddRange(EnumValue.GetEnumValues(typeof(DeploymentType), true));
      comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework), true));
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

      comboDebugMode.Items.AddRange(EnumValue.GetEnumValues(typeof(Cosmos.Build.Common.DebugMode), false));
      comboTraceMode.Items.AddRange(EnumValue.GetEnumValues(typeof(TraceAssemblies), false));
      #endregion

      #region PXE
      cmboSlavePort.Items.Clear();
      cmboSlavePort.Items.Add("None");
      FillComPorts(cmboSlavePort.Items);
      #endregion

      UpdateUI();
    }

    protected void FillComPorts(System.Collections.IList aList) {
      //TODO http://stackoverflow.com/questions/2937585/how-to-open-a-serial-port-by-friendly-name
      foreach (string xPort in SerialPort.GetPortNames()) {
        aList.Add("Serial: " + xPort);
      }
    }

    private void OutputBrowse_Click(object sender, EventArgs e) {
      string folderPath = String.Empty;
      var dialog = new FolderBrowserDialog();
      dialog.ShowNewFolderButton = true;

      folderPath = textOutputPath.Text;
      if ((String.IsNullOrEmpty(folderPath) == false) && (folderPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1)) {
        if (System.IO.Path.IsPathRooted(folderPath) == false) {
          folderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Project.FullName), folderPath);
        }

        while ((System.IO.Directory.Exists(folderPath) == false) && (String.IsNullOrEmpty(folderPath) == false)) {
          int index = -1;
          index = folderPath.IndexOfAny(new Char[] { System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar });
          if (index > -1) {
            folderPath = folderPath.Substring(0, index - 1);
          } else {
            folderPath = String.Empty;
          }
        }

        if (String.IsNullOrEmpty(folderPath) == true) {
          folderPath = System.IO.Path.GetDirectoryName(Project.FullName);
        }
      } else {
        folderPath = System.IO.Path.GetDirectoryName(Project.FullName);
      }

      dialog.SelectedPath = folderPath;
      dialog.Description = "Select build output path";

      if (dialog.ShowDialog() == DialogResult.OK) {
        textOutputPath.Text = dialog.SelectedPath;
      }
    }

    private void chkEnableStacckCorruptionDetection_CheckedChanged(object sender, EventArgs e)
    {
      IsDirty = true;
      mProps.StackCorruptionDetectionEnabled = chkEnableStackCorruptionDetection.Checked;
    }
  }
}
