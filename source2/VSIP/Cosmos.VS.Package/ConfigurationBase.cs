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
		public ConfigurationBase()
		{
			InitializeComponent();


			this.comboArchitecture.Items.AddRange(EnumValue.GetEnumValues(typeof(Architecture)));
		}

		protected override void FillProperties()
		{
			base.FillProperties();

			//base.Project
		}

		protected Microsoft.VisualStudio.Project.ProjectConfig CurrentProjectConfig
		{
			get
			{
				//the first index of the configuration is always the "Active (n)" settings.
				if( this.comboConfiguration.SelectedIndex == 0 )
				{

				}else{
					foreach( Microsoft.VisualStudio.Project.ProjectConfig projectConfig in base.ProjectConfigs )
					{
						if (String.Equals(projectConfig.ConfigName, this.comboConfiguration.SelectedText, StringComparison.InvariantCulture) == true)
						{ return projectConfig; }
					}
				}
				
				throw new Exception("Unable to find selected project configuration.");
			}
		}
	}
}
