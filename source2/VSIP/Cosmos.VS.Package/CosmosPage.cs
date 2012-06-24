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
  // We put all items on ONE form because VS is such a nightmware to managed mulitple forms
  // and add new ones.

  // Magic width and height.
  // 492, 288

  [Guid(Guids.CosmosPage)]
  public partial class CosmosPage : ConfigurationBase {
    public static BuildTarget CurrentBuildTarget = BuildTarget.VMWare;
    public static event EventHandler BuildTargetChanged;

    protected static void OnBuildTargetChanged(Object sender, EventArgs e) {
      if (CosmosPage.BuildTargetChanged != null) {
        CosmosPage.BuildTargetChanged(sender, e);
      }
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


      lboxDeploy.Items.AddRange(EnumValue.GetEnumValues(typeof(BuildTarget), true));
      lboxDeploy.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var value = (BuildTarget)((EnumValue)lboxDeploy.SelectedItem).Value;
        if (value != mProps.BuildTarget) {
          mProps.BuildTarget = value;
          IsDirty = true;

          //comboFlavor.Visible = value == TargetHost.VMWare;

          CurrentBuildTarget = value;
          OnBuildTargetChanged(this, EventArgs.Empty);
        }
      };

      comboFlavor.Items.AddRange(EnumValue.GetEnumValues(typeof(VMwareFlavor), true));
      comboFlavor.SelectedIndexChanged += delegate(Object sender, EventArgs e) {
        var x = (VMwareFlavor)((EnumValue)comboFlavor.SelectedItem).Value;
        if (x != mProps.VMWareFlavor) {
          mProps.VMWareFlavor = x;
          IsDirty = true;
        }
      };

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
      lboxDeploy.SelectedItem = EnumValue.Find(lboxDeploy.Items, mProps.BuildTarget);
      // We need to manually trigger it once, because the indexchanged event compares
      // it against the source, and they will of course be the same.
      CurrentBuildTarget = (BuildTarget)((EnumValue)lboxDeploy.SelectedItem).Value;
      OnBuildTargetChanged(this, EventArgs.Empty);

      mProps.SetProperty("Framework", GetConfigProperty("Framework"));
      comboFramework.SelectedItem = EnumValue.Find(comboFramework.Items, mProps.Framework);

      mProps.SetProperty("UseInternalAssembler", GetConfigProperty("UseInternalAssembler"));
      checkUseInternalAssembler.Checked = mProps.UseInternalAssembler;

      mProps.SetProperty("VMWareFlavor", GetConfigProperty("VMWareFlavor"));
      comboFlavor.SelectedItem = EnumValue.Find(comboFlavor.Items, mProps.VMWareFlavor);

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
      var xEnumValue = (EnumValue)lboxDeploy.SelectedItem;
      var xValue = (BuildTarget)xEnumValue.Value;
    }
  }
}
