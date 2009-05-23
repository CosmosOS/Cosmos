using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cosmos.Builder.Common;

namespace Cosmos.VS.Package
{
	public partial class ConfigurationBase : CustomPropertyPage
	{
		private Boolean configIgnoreFill;
		private Microsoft.VisualStudio.Project.ProjectConfig configCurrentConfig;
		private Int32 configOldIndex;
		private Boolean configIgnoreConfigChange;

		protected static event EventHandler<ProjectConfigurationChangedEventArgs> ProjectConfigurationChanged;

		protected static void OnProjectConfigurationChanged(Object sender, ProjectConfigurationChangedEventArgs e)
		{ ConfigurationBase.ProjectConfigurationChanged(sender, e); }

		public ConfigurationBase()
		{
			InitializeComponent();

			this.configCurrentConfig = null;
			this.configIgnoreFill = false;
			this.configIgnoreConfigChange = false;
			this.comboArchitecture.Items.AddRange(EnumValue.GetEnumValues(typeof(Architecture)));

			ConfigurationBase.ProjectConfigurationChanged += new EventHandler<ProjectConfigurationChangedEventArgs>(ConfigurationBase_ProjectConfigurationChanged);
			this.comboConfiguration.SelectedIndexChanged += new EventHandler(comboConfiguration_SelectedIndexChanged);
		}

		void comboConfiguration_SelectedIndexChanged(object sender, EventArgs e)
		{
			Boolean skipEvent = false;

			if(this.configIgnoreConfigChange == false)
			{
				Boolean configIsDirty = false;
				CustomPropertyPage[] propertyPages = CustomPropertyPage.Pages;

				foreach(CustomPropertyPage page in propertyPages)
				{ if(page.IsDirty == true){ configIsDirty = true; break; } }

				if( configIsDirty == true )
				{
					UnsavedConfigurationChanges dialog;
					DialogResult result;
					
					dialog = new UnsavedConfigurationChanges();

					dialog.Message = String.Format(dialog.Message, (string)this.comboConfiguration.Items[this.configOldIndex]);

					result = dialog.ShowDialog();
					dialog.Dispose();

					this.configIgnoreConfigChange = true;
					if(result == DialogResult.Yes)
					{
						Int32 newIndex = this.comboConfiguration.SelectedIndex;
						this.comboConfiguration.SelectedIndex = this.configOldIndex;

						foreach(CustomPropertyPage page in propertyPages)
						{ if (page.IsDirty == true){ page.ApplyChanges(); } }

						this.comboConfiguration.SelectedIndex = newIndex;

					}
					else if (result == DialogResult.Cancel)
					{

						this.comboConfiguration.SelectedIndex = this.configOldIndex;
						skipEvent = true;

					}
					else
					{
						foreach (CustomPropertyPage page in propertyPages)
						{ if (page.IsDirty == true) { page.IsDirty = false; } }
					}

					this.configIgnoreConfigChange = false;
				}
				
				if( skipEvent == false )
				{
					Boolean oldValue = this.configIgnoreFill;

					this.configIgnoreFill = true;
					this.FillProperties();
					this.configIgnoreFill = oldValue;

					if (this.configIgnoreFill == false)
					{ ConfigurationBase.OnProjectConfigurationChanged(this, new ProjectConfigurationChangedEventArgs(this.comboConfiguration.SelectedIndex)); }
				}
				this.configOldIndex = this.comboConfiguration.SelectedIndex;
			}
		}

		void ConfigurationBase_ProjectConfigurationChanged(object sender, ProjectConfigurationChangedEventArgs e)
		{
			if ((Object.ReferenceEquals(this, sender) == false) && (this.comboConfiguration.Items.Count > 0))
			{
				this.configIgnoreFill = true;
				this.configIgnoreConfigChange = true;

				this.configOldIndex = this.comboConfiguration.SelectedIndex;
				this.comboConfiguration.SelectedIndex = e.Index;

				this.configIgnoreConfigChange = false;
				this.configIgnoreFill = false;
			}
		}

		private void FindProjectConfiguration()
		{
				String selectedConfig;
				this.configCurrentConfig = null;

				//the first index of the configuration is always the "Active (n)" settings.
				if( this.comboConfiguration.SelectedIndex == 0 )
				{
					selectedConfig = this.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
				}else{
					selectedConfig = (String)this.comboConfiguration.SelectedItem;
				}

				foreach (Microsoft.VisualStudio.Project.ProjectConfig projectConfig in base.ProjectConfigs)
				{
					if (String.Equals(projectConfig.ConfigName, selectedConfig, StringComparison.InvariantCulture) == true)
					{ this.configCurrentConfig = projectConfig; }
				}

				if (this.configCurrentConfig == null) { throw new Exception("Unable to find selected project configuration."); }
		}

		protected override void FillProperties()
		{
			base.FillProperties();

			if (this.configIgnoreFill == false)
			{
				this.comboConfiguration.Items.Clear();

				this.comboConfiguration.Items.Add(String.Format("Active ({0})", this.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName));
				foreach (Microsoft.VisualStudio.Project.ProjectConfig projectConfig in base.ProjectConfigs)
				{ this.comboConfiguration.Items.Add(projectConfig.ConfigName); }

				Boolean foundConfigBase = false;
				foreach (CustomPropertyPage page in CustomPropertyPage.Pages)
				{
					if ((Object.ReferenceEquals(this, page) == false) && (page is ConfigurationBase))
					{
						if (((ConfigurationBase)page).comboConfiguration.SelectedIndex > -1)
						{
							this.configOldIndex = ((ConfigurationBase)page).configOldIndex;
							if (this.comboConfiguration.SelectedIndex < 0)
							{ this.configIgnoreConfigChange = true; }
							this.comboConfiguration.SelectedIndex = ((ConfigurationBase)page).comboConfiguration.SelectedIndex;
							this.configIgnoreConfigChange = false;
							foundConfigBase = true;
							break;
						}
					}
				}
				if ((foundConfigBase == false) || (this.comboConfiguration.SelectedIndex < 0))
				{
					this.configOldIndex = 0;
					this.comboConfiguration.SelectedIndex = 0;
				}

				this.comboArchitecture.SelectedIndex = 0;
			}

			this.FindProjectConfiguration();
		}

		protected Microsoft.VisualStudio.Project.ProjectConfig CurrentProjectConfig
		{ get { return this.configCurrentConfig; } }

	}

	public class ProjectConfigurationChangedEventArgs : EventArgs
		{
			private Int32 eventConfigIndex;

			public ProjectConfigurationChangedEventArgs( Int32 configIndex )
			{
				this.eventConfigIndex = configIndex;
			}

			public Int32 Index
			{ get{ return this.eventConfigIndex; } }
	}
}
