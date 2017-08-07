using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.VS.Package
{
	public partial class UnsavedConfigChangesDialog : Form
	{
		public UnsavedConfigChangesDialog()
		{
			InitializeComponent();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void buttonNo_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.No;
			this.Close();
		}

		private void buttonYes_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Yes;
			this.Close();
		}

		public String Message
		{ get { return this.labelMessage.Text; } set { this.labelMessage.Text = value; } }
	}
}
