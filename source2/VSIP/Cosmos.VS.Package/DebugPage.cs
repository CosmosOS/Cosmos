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

namespace Cosmos.VS.Package {
	[Guid(Guids.DebugPage)]
    // Yes this class is actualy used. Its a container for sub pages. Currently we only have one
    // (DebugPageSub) that is used for QEMU and VMWare. Maybe we will have more in the future, or maybe
    // we will merge this into one class like BuildPage.
	public partial class DebugPage : ConfigurationBase {
		private SubPropertyPageBase pageSubPage;

		public DebugPage() : base() {
			InitializeComponent();

			BuildPage.BuildTargetChanged += new EventHandler(BuildPage_BuildTargetChanged);
		}

		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}

            BuildPage.BuildTargetChanged -= new EventHandler(BuildPage_BuildTargetChanged);
			base.Dispose(disposing);
		}

        void BuildPage_BuildTargetChanged(object sender, EventArgs e) {
            FillProperties(); 
        }

		private void ClearSubPage() {
			foreach (Control control in panelSubPage.Controls) {
				panelSubPage.Controls.Remove(control);
				control.Dispose();
			}
		}

		private void SetSubPropertyPage(TargetHost target) {
			switch (target) {
                case TargetHost.VMWareWorkstation:
				case TargetHost.QEMU:
					if ((pageSubPage is DebugPageSub) == false) {
						pageSubPage = new DebugPageSub();
					}
					break;
				default:
					pageSubPage = null;
					break;
			}

			panelSubPage.SuspendLayout();

			ClearSubPage();
            panelSubPage.Visible = pageSubPage != null;
            if (panelSubPage.Visible) {
				pageSubPage.SetOwner(this);
				panelSubPage.Controls.Add(pageSubPage);

				pageSubPage.Location = new Point(0, 0);
				pageSubPage.Anchor = AnchorStyles.Top;

				pageSubPage.Size = new Size(ClientSize.Width, pageSubPage.Size.Height);
				pageSubPage.Anchor = pageSubPage.Anchor | AnchorStyles.Left | AnchorStyles.Right;

				if (pageSubPage.Size.Height <= ClientSize.Height) {
					pageSubPage.Size = new Size(pageSubPage.Size.Width, ClientSize.Height);
					pageSubPage.Anchor = pageSubPage.Anchor | AnchorStyles.Bottom;
				}
			}
			panelSubPage.ResumeLayout();
		}

		protected override void FillProperties() {
			base.FillProperties();
			SetSubPropertyPage(BuildPage.CurrentBuildTarget);
			if (pageSubPage != null) {
                pageSubPage.FillProperties(); 
            }
		}

		public override PropertiesBase Properties {
			get	{
				if (pageSubPage != null) {
                    return pageSubPage.Properties; 
                }
				return null;
			}
		}
	}
}
