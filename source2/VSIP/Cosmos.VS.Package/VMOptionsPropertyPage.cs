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
			this.SetSubPropertyPage(new VMOptionsQemu(this));
		}

		protected override void FillProperties()
		{
			base.FillProperties();
			if( ( this.Controls.Count > 0 ) && ( this.Controls[0] is SubPropertyPageBase ) )
			{ ((SubPropertyPageBase)this.Controls[0]).FillProperties(); }
		}

		public override void ApplyChanges()
		{
			base.ApplyChanges();
			if ((this.Controls.Count > 0) && (this.Controls[0] is SubPropertyPageBase))
			{ ((SubPropertyPageBase)this.Controls[0]).ApplyChanges(); }
		}

		protected void SetSubPropertyPage(SubPropertyPageBase subpage)
		{
			this.panelVMConfig.SuspendLayout();

			this.panelVMConfig.Controls.Clear();

			if (subpage != null)
			{
				this.panelVMConfig.Controls.Add(subpage);

				subpage.Location = new Point(0, 0);
				subpage.Anchor = AnchorStyles.Top;

				//page should also be the right width.
				//if( configuration.Size.Width <= this.ClientSize.Width )
				//{
				subpage.Size = new Size(this.ClientSize.Width, subpage.Size.Height);
				subpage.Anchor = subpage.Anchor | AnchorStyles.Left | AnchorStyles.Right;
				//}

				if (subpage.Size.Height <= this.ClientSize.Height)
				{
					subpage.Size = new Size(subpage.Size.Width, this.ClientSize.Height);
					subpage.Anchor = subpage.Anchor | AnchorStyles.Bottom;
				}

				this.panelVMConfig.Visible = true;
			}
			else
			{ this.panelVMConfig.Visible = false; }

			this.panelVMConfig.ResumeLayout();
		}
	}
}
