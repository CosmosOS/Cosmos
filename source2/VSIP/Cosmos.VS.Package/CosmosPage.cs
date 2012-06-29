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
    protected bool mShowTabDeployment;
    protected bool mShowTabLaunch;
    protected bool mShowTabDebug;
    protected bool mShowTabVMware;
    protected bool mShowTabPXE;
    protected bool mShowTabUSB;
    protected bool mShowTabISO;
    protected bool mShowTabSlave;

    protected List<string> mPropNames = new List<string>();

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

      if (mShowTabDebug) {
        TabControl1.TabPages.Add(tabDebug);
      }
      if (mShowTabDeployment) {
        TabControl1.TabPages.Add(tabDeployment);
      }
      if (mShowTabLaunch) {
        TabControl1.TabPages.Add(tabLaunch);
      }
      //
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
      if (mShowTabSlave) {
        TabControl1.TabPages.Add(tabSlave);
      }

      if (TabControl1.TabPages.Contains(xTab)) {
        TabControl1.SelectedTab = xTab;
      }
    }

    protected void UpdateUI() {
      mShowTabDebug = true;
      mShowTabDeployment = false;
      mShowTabLaunch = false;
      mShowTabVMware = false;
      mShowTabPXE = false;
      mShowTabUSB = false;
      mShowTabISO = false;
      mShowTabSlave = false;
      //
      lablCurrentProfile.Text = lboxProfile.SelectedItem.ToString();
      chckEnableDebugStub.Checked = false;
      lablBuildOnly.Visible = false;
      lablNonFunctional.Visible = false;
      cmboCosmosPort.Enabled = true;
      cmboVisusalStudioPort.Enabled = true;

      // Set visibilty and for preset set/reset values.
      if (mProps.Profile == Profile.ISO) {
        lablDeployText.Text = "Creates a bootable ISO image which can be burned to a DVD."
         + " After running the selected project, an explorer window will open containing the ISO file."
         + " The ISO file can then be burned to a CD or DVD and used to boot a physical or virtual system.";
        lablBuildOnly.Visible = true;
        mShowTabISO = true;
        lboxDeployment.SelectedItem = Deployment.ISO;
        lboxLaunch.SelectedItem = Launch.None;

      } else if (mProps.Profile == Profile.USB) {
        lablDeployText.Text = "Makes a USB device such as a flash drive or external hard disk bootable.";
        lablBuildOnly.Visible = true;
        mShowTabUSB = true;
        lboxDeployment.SelectedItem = Deployment.PXE;
        lboxLaunch.SelectedItem = Launch.PXE;

      } else if (mProps.Profile == Profile.VMware) {
        lablDeployText.Text = "Use VMware to deploy and debug.";
        mShowTabVMware = true;
        chckEnableDebugStub.Checked = true;
        cmboCosmosPort.Enabled = false;
        cmboVisusalStudioPort.Enabled = false;
        lboxDeployment.SelectedItem = Deployment.ISO;
        lboxLaunch.SelectedItem = Launch.VMware;

      } else if (mProps.Profile == Profile.PXE) {
        lablDeployText.Text = "Creates a PXE setup and hosts a DCHP and TFTP server to deploy directly to physical hardware. Allows debugging with a serial cable.";
        mShowTabPXE = true;
        lboxDeployment.SelectedItem = Deployment.PXE;
        lboxLaunch.SelectedItem = Launch.None;

      } else {
        lablDeployText.Text = "Oops. What the frak did you click?";
      }

      UpdateTabs();
    }

    public CosmosPage() {
      InitializeComponent();

      foreach (var xField in typeof(BuildProperties).GetFields()) {
        // IsLiteral determines if its value is written at 
        //   compile time and not changeable
        // IsInitOnly determine if the field can be set 
        //   in the body of the constructor
        // for C# a field which is readonly keyword would have both true 
        //   but a const field would have only IsLiteral equal to true
        if (xField.IsLiteral && !xField.IsInitOnly && xField.FieldType == typeof(string)) {
          mPropNames.Add((string)xField.GetValue(mProps));
        }
      }

      # region Profile
      lboxProfile.Items.AddRange(EnumValue.GetEnumValues(typeof(Profile), true));
      lboxProfile.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var xValue = (Profile)((EnumValue)lboxProfile.SelectedItem).Value;
        if (xValue != mProps.Profile) {
          SaveProfile(mProps.Profile.ToString());
          mProps.Profile = xValue;
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

    protected BuildProperties mProps = new BuildProperties();
    public override PropertiesBase Properties {
      get { return mProps; }
    }

    protected void SaveProfile(string aName) {
      foreach (var xName in mPropNames) {
        if (xName != BuildProperties.ProfileString) {
          mProps.SetProperty(aName + "_" + xName, GetConfigProperty(xName));
        }
      }
    }

    protected override void FillProperties() {
      base.FillProperties();
      mProps.Reset();
      // Initialize defaults?
      foreach (var xName in mPropNames) {
        mProps.SetProperty(xName, GetConfigProperty(xName));
      }

      // Profile
      lboxProfile.SelectedItem = EnumValue.Find(lboxProfile.Items, mProps.Profile);

      // Deployment   
      lboxDeployment.SelectedItem = EnumValue.Find(lboxDeployment.Items, mProps.Profile);

      // Launch
      lboxLaunch.SelectedItem = EnumValue.Find(lboxLaunch.Items, mProps.Profile);

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
