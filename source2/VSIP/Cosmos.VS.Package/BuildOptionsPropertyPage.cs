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

namespace Cosmos.VS.Package
{
	[Guid(Guids.BuildOptionsPropertyPage)]
	public partial class BuildOptionsPropertyPage : ConfigurationBase
	{
		public static TargetHost CurrentBuildTarget = (TargetHost)(-1);
		public static event EventHandler BuildTargetChanged;

		protected static void OnBuildTargetChanged(Object sender, EventArgs e)
		{
			if (BuildOptionsPropertyPage.BuildTargetChanged != null)
			{ BuildOptionsPropertyPage.BuildTargetChanged(sender, e); }
		}

		private BuildProperties projProperties;

		public BuildOptionsPropertyPage()
		{
			InitializeComponent();

			this.comboTarget.Items.AddRange(EnumValue.GetEnumValues(typeof(TargetHost)));
			this.comboFramework.Items.AddRange(EnumValue.GetEnumValues(typeof(Framework)));

			this.projProperties = new BuildProperties();

			this.CreateUIMonitorEvents();
		}

		private void CreateUIMonitorEvents()
		{
			this.textOutputPath.TextChanged += delegate(Object sender, EventArgs e)
			{
				String value = this.textOutputPath.Text;
				if (String.Equals(value, this.PageProperties.OutputPath, StringComparison.InvariantCultureIgnoreCase) == false)
				{
					this.PageProperties.OutputPath = this.textOutputPath.Text;
					this.IsDirty = true;
				}
			};

			this.comboTarget.SelectedIndexChanged += delegate(Object sender, EventArgs e)
			{
				TargetHost value = (TargetHost)((EnumValue)this.comboTarget.SelectedItem).Value;
				if( value != this.PageProperties.Target)
				{
					this.PageProperties.Target = value;
					this.IsDirty = true;

					BuildOptionsPropertyPage.CurrentBuildTarget = value;
					BuildOptionsPropertyPage.OnBuildTargetChanged(this, EventArgs.Empty);
				}
			};

			this.comboFramework.SelectedIndexChanged += delegate(Object sender, EventArgs e)
			{
				Framework value = (Framework)((EnumValue)this.comboFramework.SelectedItem).Value;
				if (value != this.PageProperties.Framework)
				{
					this.PageProperties.Framework = value;
					this.IsDirty = true;
				}
			};

			this.checkUseInternalAssembler.CheckedChanged += delegate(Object sender, EventArgs e)
			{
				Boolean value = this.checkUseInternalAssembler.Checked;
				if (value != this.PageProperties.UseInternalAssembler)
				{
					this.PageProperties.UseInternalAssembler = value;
					this.IsDirty = true;
				}
			};
		}

		public override PropertiesBase Properties
		{ get { return this.projProperties; } }

		protected BuildProperties PageProperties
		{ get { return (BuildProperties)this.Properties; } }

		protected override void FillProperties()
		{
			base.FillProperties();

			this.PageProperties.Reset();

			this.PageProperties.SetProperty("OutputPath", this.GetConfigProperty("OutputPath"));
			this.PageProperties.SetProperty("BuildTarget", this.GetConfigProperty("BuildTarget"));
			this.PageProperties.SetProperty("Framework", this.GetConfigProperty("Framework"));
			this.PageProperties.SetProperty("UseInternalAssembler", this.GetConfigProperty("UseInternalAssembler"));

			this.textOutputPath.Text = this.PageProperties.OutputPath;
			this.comboTarget.SelectedItem = EnumValue.Find(this.comboTarget.Items, this.PageProperties.Target);
			this.comboFramework.SelectedItem = EnumValue.Find(this.comboFramework.Items, this.PageProperties.Framework);
			this.checkUseInternalAssembler.Checked = this.PageProperties.UseInternalAssembler;
		}

		private void OutputBrowse_Click(object sender, EventArgs e)
		{
			String folderPath = String.Empty;
			FolderBrowserDialog dialog;
			dialog = new FolderBrowserDialog();
			dialog.ShowNewFolderButton = true;

			folderPath = textOutputPath.Text;
			if ((String.IsNullOrEmpty(folderPath) == false) && (folderPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1))
			{
				if (System.IO.Path.IsPathRooted(folderPath) == false)
				{ folderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.Project.FullName), folderPath); }

				while ((System.IO.Directory.Exists(folderPath) == false) && (String.IsNullOrEmpty(folderPath) == false))
				{
					Int32 index = -1;
					index = folderPath.IndexOfAny(new Char[] { System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar });
					if (index > -1)
					{
						folderPath = folderPath.Substring(0, index - 1);
					} else { folderPath = String.Empty; }
				}

				if (String.IsNullOrEmpty(folderPath) == true)
				{ folderPath = System.IO.Path.GetDirectoryName(this.Project.FullName); }
			} else
			{
				folderPath = System.IO.Path.GetDirectoryName(this.Project.FullName);
			}

			dialog.SelectedPath = folderPath;

			dialog.Description = "Select build output path";

			if (dialog.ShowDialog() == DialogResult.OK)
			{ textOutputPath.Text = dialog.SelectedPath; }
		}

        private void comboTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var xEnumValue = (EnumValue)comboTarget.SelectedItem;
                var xValue = (TargetHost)xEnumValue.Value;
                if (!(xValue == TargetHost.VMWareWorkstation || xValue == TargetHost.QEMU))
                {
                    MessageBox.Show("The selected Target is not supported!");
                }
            }
            catch (Exception E)
            {
                File.AppendAllText(@"e:\cosmoserrors.txt", "ERROR\r\n" + E.ToString() + "\r\n\r\n\r\n");
            }
        }

	}
}
