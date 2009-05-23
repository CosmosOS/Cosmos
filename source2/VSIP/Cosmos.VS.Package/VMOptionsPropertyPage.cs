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
	[Guid(Guids.VMOptionsPropertyPage)]
	public partial class VMOptionsPropertyPage : ConfigurationBase
	{
		public VMOptionsPropertyPage()
		{
			InitializeComponent();

			//TODO: Remove test call.
			this.SetConfiguration(new VMOptionsQemu());
		}

		protected void SetConfiguration(Control configuration)
		{
			this.panelVMConfig.SuspendLayout();

			this.panelVMConfig.Controls.Clear();

			if (configuration != null)
			{
				this.panelVMConfig.Controls.Add(configuration);
				
				configuration.Location = new Point(0, 0);
				configuration.Anchor = AnchorStyles.Top;

				//page should also be the right width.
				//if( configuration.Size.Width <= this.ClientSize.Width )
				//{
				configuration.Size = new Size(this.ClientSize.Width, configuration.Size.Height);
				configuration.Anchor = configuration.Anchor | AnchorStyles.Left | AnchorStyles.Right;
				//}

				if (configuration.Size.Height <= this.ClientSize.Height)
				{
					configuration.Size = new Size(configuration.Size.Width, this.ClientSize.Height);
					configuration.Anchor = configuration.Anchor | AnchorStyles.Bottom;
				}

				this.panelVMConfig.Visible = true;
			}
			else
			{ this.panelVMConfig.Visible = false; }

			this.panelVMConfig.ResumeLayout();
		}
	}
}
