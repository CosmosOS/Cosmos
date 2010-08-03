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
	public partial class ConfigurationBase : CustomPropertyPage {
		protected static int CurrentConfigurationIndex = 0;
		protected static event EventHandler ConfigurationChanged;

		protected static void OnConfigurationChanged(Object sender, EventArgs e) {
            ConfigurationBase.ConfigurationChanged(sender, e); 
        }
	
		public ConfigurationBase() {
			InitializeComponent();
			
            comboArchitecture.Items.AddRange(EnumValue.GetEnumValues(typeof(Architecture), false));
            comboConfiguration.SelectedIndexChanged += new EventHandler(comboConfiguration_SelectedIndexChanged);
			
            ConfigurationBase.ConfigurationChanged += new EventHandler(ConfigurationBase_ConfigurationChanged);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			comboConfiguration.SelectedIndexChanged -= new EventHandler(comboConfiguration_SelectedIndexChanged);
			ConfigurationBase.ConfigurationChanged -= new EventHandler(ConfigurationBase_ConfigurationChanged);
		}

		void ConfigurationBase_ConfigurationChanged(object sender, EventArgs e) {
			if (!Object.ReferenceEquals(sender, this)) {
				projCurrentConfig = null;

				if (comboConfiguration.Items.Count > 0) {
					System.Diagnostics.Debug.Print(String.Format("{0}->ConfigurationBase_ConfigurationChanged", GetType().Name));
					comboConfiguration.SelectedIndex = ConfigurationBase.CurrentConfigurationIndex;
					
					IgnoreDirty = true;
					FillProperties();
					IgnoreDirty = false;
				}
			}
		}

		void comboConfiguration_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboConfiguration.SelectedIndex > -1)
			{
				if (comboConfiguration.SelectedIndex != ConfigurationBase.CurrentConfigurationIndex)
				{
					System.Diagnostics.Debug.Print(String.Format("{0}->comboConfiguration_SelectedIndexChanged", GetType().Name));

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
						projCurrentConfig = null;

						ConfigurationBase.CurrentConfigurationIndex = comboConfiguration.SelectedIndex;
						BuildPage.CurrentBuildTarget = EnumValue.Parse(GetConfigProperty("BuildTarget"), TargetHost.VMWare);
						ConfigurationBase.OnConfigurationChanged(this, EventArgs.Empty);

						IgnoreDirty = true;
						FillProperties();
						IgnoreDirty = false;
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
					return ProjectConfigs[index];
				} else {
					if( projCurrentConfig == null )
					{
						String activeConfig = Project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
						foreach (ProjectConfig config in ProjectConfigs)
						{
							if (String.Equals(config.ConfigName, activeConfig, StringComparison.InvariantCulture) == true)
							{
								projCurrentConfig = config;
								break;
							}
						}
					}

					return projCurrentConfig;
				}
			}
		}

		protected override void FillConfigurations()
		{
			base.FillConfigurations();

			comboConfiguration.Items.Add(String.Format("Active ({0})", base.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName));
			foreach( ProjectConfig config in base.ProjectConfigs )
			{ comboConfiguration.Items.Add(config.ConfigName); }

			if (comboConfiguration.SelectedIndex != ConfigurationBase.CurrentConfigurationIndex)
			{ comboConfiguration.SelectedIndex = ConfigurationBase.CurrentConfigurationIndex; }

			comboArchitecture.SelectedIndex = 0;

			if ((Int32)BuildPage.CurrentBuildTarget < 0)
			{ BuildPage.CurrentBuildTarget = EnumValue.Parse(GetConfigProperty("BuildTarget"), TargetHost.VMWare); }
		}

		public override void SetConfigProperty(String name, String value) {
			CCITracing.TraceCall();
			if (value == null) { 
                value = String.Empty; 
            }

			if (ProjectMgr != null) {
				CurrentConfiguration.SetConfigurationProperty(name, value);
				ProjectMgr.SetProjectFileDirty(true);
			}
		}

		public override String GetConfigProperty(String name) {
			return CurrentConfiguration.GetConfigurationProperty(name, true);
		}

	}
}
