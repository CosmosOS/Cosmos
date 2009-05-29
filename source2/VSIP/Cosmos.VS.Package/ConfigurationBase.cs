using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using Cosmos.Build.Common;
using Microsoft.VisualStudio.Project;

namespace Cosmos.VS.Package
{
	public partial class ConfigurationBase : CustomPropertyPage
	{
		protected static Int32 CurrentConfigurationIndex = 0;
		protected static event EventHandler ConfigurationChanged;

		protected static void OnConfigurationChanged(Object sender, EventArgs e)
		{ ConfigurationBase.ConfigurationChanged(sender, e); }
	
		public ConfigurationBase()
		{
			InitializeComponent();
			this.comboArchitecture.Items.AddRange(EnumValue.GetEnumValues(typeof(Architecture)));

			this.comboConfiguration.SelectedIndexChanged += new EventHandler(comboConfiguration_SelectedIndexChanged);
			ConfigurationBase.ConfigurationChanged += new EventHandler(ConfigurationBase_ConfigurationChanged);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			this.comboConfiguration.SelectedIndexChanged -= new EventHandler(comboConfiguration_SelectedIndexChanged);
			ConfigurationBase.ConfigurationChanged -= new EventHandler(ConfigurationBase_ConfigurationChanged);
		}

		void ConfigurationBase_ConfigurationChanged(object sender, EventArgs e)
		{
			if (Object.ReferenceEquals(sender, this) == false)
			{
				this.projCurrentConfig = null;

				if (comboConfiguration.Items.Count > 0)
				{
					System.Diagnostics.Debug.Print(String.Format("{0}->ConfigurationBase_ConfigurationChanged", this.GetType().Name));
					comboConfiguration.SelectedIndex = ConfigurationBase.CurrentConfigurationIndex;
					
					this.IgnoreDirty = true;
					this.FillProperties();
					this.IgnoreDirty = false;
				}
			}
		}

		void comboConfiguration_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboConfiguration.SelectedIndex > -1)
			{
				if (comboConfiguration.SelectedIndex != ConfigurationBase.CurrentConfigurationIndex)
				{
					System.Diagnostics.Debug.Print(String.Format("{0}->comboConfiguration_SelectedIndexChanged", this.GetType().Name));

					Boolean hasUnsavedChanges = false;
					CustomPropertyPage[] propPages = CustomPropertyPage.Pages;
					foreach (CustomPropertyPage page in propPages)
					{
						if (page.IsDirty == true)
						{ hasUnsavedChanges = true; break; }
					}

					if (hasUnsavedChanges == true)
					{
						UnsavedConfigChangesDialog dialog = new UnsavedConfigChangesDialog();
						DialogResult result;

						dialog.Message = String.Format(dialog.Message, comboConfiguration.Items[ConfigurationBase.CurrentConfigurationIndex].ToString());

						result = dialog.ShowDialog();

						if (result == DialogResult.Yes)
						{
							foreach (CustomPropertyPage page in propPages)
							{ page.ApplyChanges(); }
							hasUnsavedChanges = false;

						} else if (result == DialogResult.No)
						{
							foreach (CustomPropertyPage page in propPages)
							{ page.IsDirty = false; }
							hasUnsavedChanges = false;

						} else
						{ comboConfiguration.SelectedIndex = ConfigurationBase.CurrentConfigurationIndex; }

						dialog.Dispose();
						dialog = null;
					}

					if (hasUnsavedChanges == false)
					{
						this.projCurrentConfig = null;

						ConfigurationBase.CurrentConfigurationIndex = comboConfiguration.SelectedIndex;
						BuildOptionsPropertyPage.CurrentBuildTarget = EnumValue.Parse(this.GetConfigProperty("BuildTarget"), TargetHost.QEMU);
						ConfigurationBase.OnConfigurationChanged(this, EventArgs.Empty);

						this.IgnoreDirty = true;
						this.FillProperties();
						this.IgnoreDirty = false;
					}
				}
			}
		}

		private ProjectConfig projCurrentConfig;
		protected ProjectConfig CurrentConfiguration
		{
			get
			{
				Int32 index = ConfigurationBase.CurrentConfigurationIndex;
				if (index > 0)
				{
					index--;
					return this.ProjectConfigs[index];
				} else {
					if( this.projCurrentConfig == null )
					{
						String activeConfig = this.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
						foreach (ProjectConfig config in this.ProjectConfigs)
						{
							if (String.Equals(config.ConfigName, activeConfig, StringComparison.InvariantCulture) == true)
							{
								this.projCurrentConfig = config;
								break;
							}
						}
					}

					return this.projCurrentConfig;
				}
			}
		}

		protected override void FillConfigurations()
		{
			base.FillConfigurations();

			this.comboConfiguration.Items.Add(String.Format("Active ({0})", base.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName));
			foreach( ProjectConfig config in base.ProjectConfigs )
			{ this.comboConfiguration.Items.Add(config.ConfigName); }

			if (this.comboConfiguration.SelectedIndex != ConfigurationBase.CurrentConfigurationIndex)
			{ this.comboConfiguration.SelectedIndex = ConfigurationBase.CurrentConfigurationIndex; }

			this.comboArchitecture.SelectedIndex = 0;

			if ((Int32)BuildOptionsPropertyPage.CurrentBuildTarget < 0)
			{ BuildOptionsPropertyPage.CurrentBuildTarget = EnumValue.Parse(this.GetConfigProperty("BuildTarget"), TargetHost.QEMU); }
		}

		public override void SetConfigProperty(String name, String value)
		{
			CCITracing.TraceCall();
			if (value == null)
			{ value = String.Empty; }

			if (this.ProjectMgr != null)
			{
				this.CurrentConfiguration.SetConfigurationProperty(name, value);
				this.ProjectMgr.SetProjectFileDirty(true);
			}
		}

		public override String GetConfigProperty(String name)
		{
			String value;

			value = this.CurrentConfiguration.GetConfigurationProperty(name, true);

			return value;
		}

	}
}
