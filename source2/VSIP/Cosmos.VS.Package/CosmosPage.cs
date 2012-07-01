using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

    protected bool mShowTabDeployment;
    protected bool mShowTabLaunch;
    protected bool mShowTabVMware;
    protected bool mShowTabPXE;
    protected bool mShowTabUSB;
    protected bool mShowTabISO;

    protected void RemoveTab(TabPage aTab) {
      if (TabControl1.TabPages.Contains(aTab)) {
        TabControl1.TabPages.Remove(aTab);
      }
    }

    protected void UpdateTabs() {
      var xTab = TabControl1.SelectedTab;

      RemoveTab(tabDeployment);
      RemoveTab(tabLaunch);
      RemoveTab(tabVMware);
      RemoveTab(tabPXE);
      RemoveTab(tabUSB);
      RemoveTab(tabISO);

      if (mShowTabDeployment) {
        TabControl1.TabPages.Add(tabDeployment);
      }
      if (mShowTabLaunch) {
        TabControl1.TabPages.Add(tabLaunch);
      }
      if (mShowTabVMware) {
        TabControl1.TabPages.Add(tabVMware);
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

      if (TabControl1.TabPages.Contains(xTab)) {
        TabControl1.SelectedTab = xTab;
      }
    }

    protected void UpdatePresetsUI() {
      chckEnableDebugStub.Checked = false;
      cmboCosmosPort.Enabled = true;
      cmboVisusalStudioPort.Enabled = true;

      if (mProps.Profile == "ISO") {

      } else if (mProps.Profile == "USB") {
        mShowTabUSB = true;

      } else if (mProps.Profile == "VMware") {
        mShowTabVMware = true;
        chckEnableDebugStub.Checked = true;
        cmboCosmosPort.Enabled = false;
        cmboVisusalStudioPort.Enabled = false;

      } else if (mProps.Profile == "PXE") {

      } else {
        lablDeployText.Text = "Oops. What the frak did you click?";
      }
    }

    protected void UpdateUI() {
      UpdatePresetsUI();
      var xProfile = (ProfileItem)lboxProfile.SelectedItem;
      if (xProfile == null) {
        return;
      }

      lablCurrentProfile.Text = xProfile.Name;
      lablDeployText.Text = mProps.Description;
      lboxDeployment.SelectedItem = mProps.Deployment;
      lboxLaunch.SelectedItem = mProps.Launch;
      lablBuildOnly.Visible = mProps.Launch == Launch.None;

      lablPreset.Visible = xProfile.IsPreset;
      mShowTabDeployment = !lablPreset.Visible;
      mShowTabLaunch = !lablPreset.Visible;

      mShowTabISO = mProps.Deployment == Deployment.ISO;
      mShowTabUSB = mProps.Deployment == Deployment.USB;

      mShowTabVMware = mProps.Launch == Launch.VMware;
      mShowTabPXE = mProps.Launch == Launch.PXE;

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
          SaveProfile(mProps.Profile);
          // Load newly selected profile
          LoadProfile(xProfile.Prefix);
          mProps.Profile = xProfile.Prefix;

          IsDirty = true;
          UpdateUI();
        }
      };
      #endregion

      # region Deploy
      lboxDeployment.Items.AddRange(EnumValue.GetEnumValues(typeof(Deployment), true));
      lboxDeployment.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xValue = (Deployment)((EnumValue)lboxDeployment.SelectedItem).Value;
        if (xValue != mProps.Deployment) {
          mProps.Deployment = xValue;
          IsDirty = true;
        }
      };
      #endregion

      # region Launch
      lboxLaunch.Items.AddRange(EnumValue.GetEnumValues(typeof(Launch), true));
      lboxLaunch.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xValue = (Launch)((EnumValue)lboxLaunch.SelectedItem).Value;
        if (xValue != mProps.Launch) {
          mProps.Launch = xValue;
          IsDirty = true;
        }
      };
      #endregion

      comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework), true));
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

      checkUseInternalAssembler.CheckedChanged += delegate(Object sender, EventArgs e) {
        bool value = checkUseInternalAssembler.Checked;
        if (value != mProps.UseInternalAssembler) {
          mProps.UseInternalAssembler = value;
          IsDirty = true;
        }
      };


      #region VMware
      cmboVMwareEdition.Items.AddRange(EnumValue.GetEnumValues(typeof(VMwareEdition), true));
      cmboVMwareEdition.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (VMwareEdition)((EnumValue)cmboVMwareEdition.SelectedItem).Value;
        if (x != mProps.VMwareEdition) {
          mProps.VMwareEdition = x;
          IsDirty = true;
        }
      };
      #endregion

      comboDebugMode.Items.AddRange(EnumValue.GetEnumValues(typeof(Cosmos.Build.Common.DebugMode), false));
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

      comboTraceMode.Items.AddRange(EnumValue.GetEnumValues(typeof(TraceAssemblies), false));
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
      SaveProfile(xPrefix);
      lboxProfile.SelectedIndex = FillProfile(xID);
    }

    protected BuildProperties mProps = new BuildProperties();
    public override PropertiesBase Properties {
      get { return mProps; }
    }

    protected void SaveProfile(string aName) {
      foreach (var xName in BuildProperties.PropNames) {
        mProps.SetProperty(aName + "_" + xName, mProps.GetProperty(xName));
      }
    }

    protected void LoadProfile(string aName) {
      foreach (var xName in BuildProperties.PropNames) {
        mProps.SetProperty(xName, mProps.GetProperty(aName + "_" + xName));
      }

      // Reforce fixed settings for presets on each load.
      if (aName == "ISO") {
        mProps.Description = "Creates a bootable ISO image which can be burned to a DVD."
         + " After running the selected project, an explorer window will open containing the ISO file."
         + " The ISO file can then be burned to a CD or DVD and used to boot a physical or virtual system.";
        mProps.Deployment = Deployment.ISO;
        mProps.Launch = Launch.None;

      } else if (aName == "USB") {
        mProps.Description = "Makes a USB device such as a flash drive or external hard disk bootable.";
        mProps.Deployment = Deployment.USB;
        mProps.Launch = Launch.None;

      } else if (aName == "VMware") {
        mProps.Description = "Use VMware Player or Workstation to deploy and debug.";
        mProps.Deployment = Deployment.ISO;
        mProps.Launch = Launch.VMware;

      } else if (aName == "PXE") {
        mProps.Description = "Creates a PXE setup and hosts a DCHP and TFTP server to deploy directly to physical hardware. Allows debugging with a serial cable.";
        mProps.Deployment = Deployment.PXE;
        mProps.Launch = Launch.PXE;
      }
    }

    protected void LoadProfileProps(string aPrefix) {
      string xPrefix = aPrefix + (aPrefix == "" ? "" : "_");
      foreach (var xName in BuildProperties.PropNames) {
        mProps.SetProperty(xPrefix + xName, ProjectConfigs[0].GetConfigurationProperty(xPrefix + xName, false));
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
        if (ProjectConfigs[0].GetConfigurationProperty(xPrefix + "_Name", false) != "") {
          LoadProfileProps(xPrefix);
        }
      }
    }

    protected override void FillProperties() {
      base.FillProperties();
      LoadProps();
      FillProfiles();

      // Profile
      foreach (ProfileItem xItem in lboxProfile.Items) {
        if (xItem.Prefix == mProps.Profile) {
          lboxProfile.SelectedItem = xItem;
          break;
        }
      }

      // Deployment   
      lboxDeployment.SelectedItem = EnumValue.Find(lboxDeployment.Items, mProps.Deployment);

      // Launch
      lboxLaunch.SelectedItem = EnumValue.Find(lboxLaunch.Items, mProps.Launch);

      // VMware
      cmboVMwareEdition.SelectedItem = EnumValue.Find(cmboVMwareEdition.Items, mProps.VMwareEdition);

      // Debug
      chckEnableDebugStub.Checked = mProps.DebugEnabled;
      checkIgnoreDebugStubAttribute.Checked = mProps.IgnoreDebugStubAttribute;
      comboDebugMode.SelectedItem = EnumValue.Find(comboDebugMode.Items, mProps.DebugMode);
      comboTraceMode.SelectedItem = EnumValue.Find(comboTraceMode.Items, mProps.TraceAssemblies);

      textOutputPath.Text = mProps.OutputPath;
      comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mProps.Framework);
      checkUseInternalAssembler.Checked = mProps.UseInternalAssembler;
      checkEnableGDB.Checked = mProps.EnableGDB;
      checkStartCosmosGDB.Checked = mProps.StartCosmosGDB;

      UpdateUI();
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

  }
}
