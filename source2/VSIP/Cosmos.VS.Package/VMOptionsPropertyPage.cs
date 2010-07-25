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
	[Guid(Guids.VMOptionsPropertyPage)]
	public partial class VMOptionsPropertyPage : ConfigurationBase
	{
		private SubPropertyPageBase pageSubPage;

		public VMOptionsPropertyPage()
		{
			InitializeComponent();

			BuildPage.BuildTargetChanged += new EventHandler(BuildOptionsPropertyPage_BuildTargetChanged);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
		{ this.FillProperties(); }

		private void ClearSubPage()
		{
			foreach (Control control in this.panelSubPage.Controls)
			{
				this.panelSubPage.Controls.Remove(control);
				control.Dispose();
			}
		}

		private void SetSubPropertyPage(TargetHost target)
		{
			Boolean subpageChanged = false;

			switch (target)
			{
				case TargetHost.QEMU:
					if ((this.pageSubPage is VMOptionsQemu) == false)
					{
						subpageChanged = true;
						this.pageSubPage = new VMOptionsQemu();
					}
					break;
				default:
					subpageChanged = true;
					this.pageSubPage = null;
					break;
			}

			if (subpageChanged == true)
			{
				this.panelSubPage.SuspendLayout();

				this.ClearSubPage();
				if (this.pageSubPage != null)
				{
					this.pageSubPage.SetOwner(this);
					this.panelSubPage.Controls.Add(pageSubPage);

					this.pageSubPage.Location = new Point(0, 0);
					this.pageSubPage.Anchor = AnchorStyles.Top;

					this.pageSubPage.Size = new Size(this.ClientSize.Width, this.pageSubPage.Size.Height);
					this.pageSubPage.Anchor = this.pageSubPage.Anchor | AnchorStyles.Left | AnchorStyles.Right;

					if (this.pageSubPage.Size.Height <= this.ClientSize.Height)
					{
						this.pageSubPage.Size = new Size(this.pageSubPage.Size.Width, this.ClientSize.Height);
						this.pageSubPage.Anchor = this.pageSubPage.Anchor | AnchorStyles.Bottom;
					}

					this.panelSubPage.Visible = true;
				} else
				{
					this.panelSubPage.Visible = false;
				}

				this.panelSubPage.ResumeLayout();
			}

		}

		protected override void FillProperties()
		{
			base.FillProperties();

			this.SetSubPropertyPage(BuildPage.CurrentBuildTarget);

			if (this.pageSubPage != null)
			{ this.pageSubPage.FillProperties(); }
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
