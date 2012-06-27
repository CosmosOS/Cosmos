using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Cosmos.Build.Common;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;

namespace Cosmos.VS.Package {
  // We put all items on ONE form because VS is such a C++ developers wet dream to manage mulitple forms
  // and add new ones.

  // Magic width and height.
  // 492, 288

  [Guid(Guids.CosmosPage)]
  public partial class CosmosPage : CustomPropertyPage {
    protected bool mShowTabDebug;
    protected bool mShowTabVMware;
    protected bool mShowTabPXE;
    protected bool mShowTabUSB;
    protected bool mShowTabISO;
    protected bool mShowTabSlave;

    protected void RemoveTab(TabPage aTab) {
      if (TabControl1.TabPages.Contains(aTab)) {
        TabControl1.TabPages.Remove(aTab);
      }
    }

    protected void UpdateTabs() {
      var xTab = TabControl1.SelectedTab;
      
      RemoveTab(tabDebug);
      RemoveTab(tabVMware);
      RemoveTab(tabPXE);
      RemoveTab(tabUSB);
      RemoveTab(tabISO);
      RemoveTab(tabSlave);

      if (mShowTabDebug) {
        TabControl1.TabPages.Add(tabDebug);
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

      if (mProps.Profile == Profile.ISO) {
        lablDeployText.Text = "Creates a bootable ISO image which can be burned to a DVD."
         + " After running the selected project, an explorer window will open containing the ISO file."
         + " The ISO file can then be burned to a CD or DVD and used to boot a physical or virtual system.";
        lablBuildOnly.Visible = true;
        mShowTabISO = true;

      } else if (mProps.Profile == Profile.USB) {
        lablDeployText.Text = "Makes a USB device such as a flash drive or external hard disk bootable.";
        lablBuildOnly.Visible = true;
        mShowTabUSB = true;

      } else if (mProps.Profile == Profile.VMware) {
        lablDeployText.Text = "Use VMware to deploy and debug.";
        mShowTabVMware = true;
        chckEnableDebugStub.Checked = true;
        cmboCosmosPort.Enabled = false;
        cmboVisusalStudioPort.Enabled = false;

      } else if (mProps.Profile == Profile.PXE) {
        lablDeployText.Text = "Creates a PXE setup and hosts a DCHP and TFTP server to deploy directly to physical hardware. Allows debugging with a serial cable.";
        mShowTabPXE = true;

      } else {
        lablDeployText.Text = "Oops. What the frak did you click?";
      }

      UpdateTabs();
    }

    public CosmosPage() {
      InitializeComponent();

      textOutputPath.TextChanged += delegate(Object sender, EventArgs e) {
        string value = textOutputPath.Text;
        if (!string.Equals(value, mProps.OutputPath, StringComparison.InvariantCultureIgnoreCase)) {
          mProps.OutputPath = textOutputPath.Text;
          IsDirty = true;
        }
      };
      comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework), true));
      comboFramework.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var value = (Framework)((EnumValue)comboFramework.SelectedItem).Value;
        if (value != mProps.Framework) {
          mProps.Framework = value;
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


      lboxProfile.Items.AddRange(EnumValue.GetEnumValues(typeof(Profile), true));
      lboxProfile.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var value = (Profile)((EnumValue)lboxProfile.SelectedItem).Value;
        if (value != mProps.Profile) {
          mProps.Profile = value;
          IsDirty = true;
          UpdateUI();
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

    protected override void FillProperties() {
      base.FillProperties();
      mProps.Reset();

      //TODO: Why are we copying these one by one instead of automatic?
      mProps.SetProperty("OutputPath", GetConfigProperty("OutputPath"));
      textOutputPath.Text = mProps.OutputPath;

      mProps.SetProperty("BuildTarget", GetConfigProperty("BuildTarget"));
      lboxProfile.SelectedItem = EnumValue.Find(lboxProfile.Items, mProps.Profile);

      mProps.SetProperty("Framework", GetConfigProperty("Framework"));
      comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mProps.Framework);

      mProps.SetProperty("UseInternalAssembler", GetConfigProperty("UseInternalAssembler"));
      checkUseInternalAssembler.Checked = mProps.UseInternalAssembler;

      // VMware
      mProps.SetProperty("VMwareEdition", GetConfigProperty("VMwareEdition"));
      cmboVMwareEdition.SelectedItem = EnumValue.Find(cmboVMwareEdition.Items, mProps.VMwareEdition);

      mProps.SetProperty("EnableGDB", GetConfigProperty("EnableGDB"));
      checkEnableGDB.Checked = mProps.EnableGDB;

      mProps.SetProperty("StartCosmosGDB", GetConfigProperty("StartCosmosGDB"));
      checkStartCosmosGDB.Checked = mProps.StartCosmosGDB;

      mProps.SetProperty("IgnoreDebugStubAttribute", GetConfigProperty("IgnoreDebugStubAttribute"));
      checkIgnoreDebugStubAttribute.Checked = mProps.IgnoreDebugStubAttribute;

      mProps.SetProperty("DebugMode", GetConfigProperty("DebugMode"));
      comboDebugMode.SelectedItem = EnumValue.Find(comboDebugMode.Items, mProps.DebugMode);

      mProps.SetProperty("TraceMode", GetConfigProperty("TraceMode"));
      comboTraceMode.SelectedItem = EnumValue.Find(comboTraceMode.Items, mProps.TraceAssemblies);

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

    private void comboTarget_SelectedIndexChanged(object sender, EventArgs e) {
      var xEnumValue = (EnumValue)lboxProfile.SelectedItem;
      var xValue = (Profile)xEnumValue.Value;
    }

    private void chckEnableDebugStub_CheckedChanged(object sender, EventArgs e) {
      panlDebugSettings.Enabled = chckEnableDebugStub.Checked;
    }
  }
}
