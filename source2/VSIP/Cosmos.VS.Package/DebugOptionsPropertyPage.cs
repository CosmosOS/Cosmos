using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cosmos.VS.Package
{
	[Guid(Guids.DebugOptionsPropertyPage)]
	public partial class DebugOptionsPropertyPage : ConfigurationBase
	{
		public DebugOptionsPropertyPage()
		{
			InitializeComponent();

			//TODO: Remove test call.
			this.SetConfiguration(new DebugOptionsQemu());
		}

		protected void SetConfiguration(Control configuration)
		{
			this.panelDebugConfig.SuspendLayout();

			this.panelDebugConfig.Controls.Clear();

			if (configuration != null)
			{
				this.panelDebugConfig.Controls.Add(configuration);
				configuration.Dock = DockStyle.Fill;
				this.panelDebugConfig.Visible = true;
			}
			else
			{ this.panelDebugConfig.Visible = false; }

			this.panelDebugConfig.ResumeLayout();
		}
	}
}
