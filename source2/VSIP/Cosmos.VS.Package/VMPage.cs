using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Cosmos.Build.Common;

namespace Cosmos.VS.Package
{
	[Guid(Guids.VMPage)]
	public partial class VMPage : ConfigurationBase
	{
		private SubPropertyPageBase pageSubPage;

		public VMPage() {
			InitializeComponent();
			BuildPage.BuildTargetChanged += new EventHandler(BuildOptionsPropertyPage_BuildTargetChanged);
            // Not sure if we need it, but it seems not always called and we have 
            // force it one time. Maybe it has to do with order of creation etc.
            FillProperties();
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			BuildPage.BuildTargetChanged -= new EventHandler(BuildOptionsPropertyPage_BuildTargetChanged);

			base.Dispose(disposing);
		}

		void BuildOptionsPropertyPage_BuildTargetChanged(object sender, EventArgs e)
		{ FillProperties(); }

		private void ClearSubPage()
		{
			foreach (Control control in panelSubPage.Controls)
			{
				panelSubPage.Controls.Remove(control);
				control.Dispose();
			}
		}

		private void SetSubPropertyPage(TargetHost target)
		{
			bool subpageChanged = false;

			switch (target) {
				case TargetHost.QEMU:
					if (!(pageSubPage is VMPageQemu)) {
						subpageChanged = true;
						pageSubPage = new VMPageQemu();
					}
					break;
				default:
					subpageChanged = true;
					pageSubPage = null;
					break;
			}

			if (subpageChanged == true)
			{
				panelSubPage.SuspendLayout();

				ClearSubPage();
				if (pageSubPage != null)
				{
					pageSubPage.SetOwner(this);
					panelSubPage.Controls.Add(pageSubPage);

					pageSubPage.Location = new Point(0, 0);
					pageSubPage.Anchor = AnchorStyles.Top;

					pageSubPage.Size = new Size(ClientSize.Width, pageSubPage.Size.Height);
					pageSubPage.Anchor = pageSubPage.Anchor | AnchorStyles.Left | AnchorStyles.Right;

					if (pageSubPage.Size.Height <= ClientSize.Height)
					{
						pageSubPage.Size = new Size(pageSubPage.Size.Width, ClientSize.Height);
						pageSubPage.Anchor = pageSubPage.Anchor | AnchorStyles.Bottom;
					}

					panelSubPage.Visible = true;
				} else
				{
					panelSubPage.Visible = false;
				}

				panelSubPage.ResumeLayout();
			}

		}

		protected override void FillProperties()
		{
			base.FillProperties();

			SetSubPropertyPage(BuildPage.CurrentBuildTarget);

			if (pageSubPage != null)
			{ pageSubPage.FillProperties(); }
		}

		public override PropertiesBase Properties
		{
			get
			{
				if (pageSubPage != null)
				{ return pageSubPage.Properties; }
				return null;
			}
		}
		
	}
}
