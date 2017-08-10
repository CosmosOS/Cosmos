using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormWatches : Form {
		protected Dictionary<string, WatchUC> mWatches = new Dictionary<string, WatchUC>();

        public FormWatches() {
            InitializeComponent();
        }

        private void FormWatches_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

		private void textWatch_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '\r') {
				butAddWatch.PerformClick();
			}
		}

		private void butAddWatch_Click(object sender, EventArgs e) {
            AddWatch(textWatch.Text);
			textWatch.Clear();
		}

		private void AddWatch(string aLabel) {
			var xS = aLabel.Trim();
			var xParts = xS.Split(' ');
			if (xParts.Length > 2 || xS.Length == 0) {
				goto error;
			}

			var xAddress = xParts[0];
			UInt32 xAddressNum = 0;
			RegNames xReg;
			bool xRegisterSet = false;
			if (Enum.TryParse<RegNames>(xAddress, out xReg) && Enum.IsDefined(typeof(RegNames),xReg)) {
				xAddress = xReg.ToString();
				xRegisterSet = true;
			}
			else
			{
				bool catched = false;
				try {
					// force convert to detect error
					if (xAddress.StartsWith("0x")) {
						// to decimal system
						xAddressNum = Global.FromHexWithLeadingZeroX(xAddress);
					}
					else
						xAddressNum = uint.Parse(xAddress);
					xAddress = xAddressNum.ToString();
				}
				catch {
					catched = true;
				}
				if (catched)
					goto error;
			}

			if (mWatches.ContainsKey(xAddress))
				return;

			var xUC = new WatchUC();
			mWatches.Add(xAddress, xUC);

			xUC.Dock = DockStyle.Top;
			xUC.cboxEnabled.Checked = true;
			if (xRegisterSet)
			{
				xUC.mRegister = xReg;
				xUC.lablAddress.Text = xAddress;
			}
			else
				xUC.lablAddress.Text = "0x" + xAddressNum.ToString("X");
			if(xParts.Length == 2)
				xUC.mCountOfInt = xParts[1];
			xUC.Delete += new Action<string>(xUC_Delete);
			panl.Controls.Add(xUC);
			xUC.Send();
			return;
		error:
			MessageBox.Show("Needs to be in format: [0x]address [countOfInts]");
		}

		void xUC_Delete(string aID)
		{
			var xUC = mWatches[aID];
			mWatches.Remove(aID);

			// Delete UC
			Controls.Remove(xUC);
			xUC.Dispose();
		}

		public void Redo() {
			foreach (var item in mWatches.Values) {
				item.Send();
			}
		}

		public void OnWatchUpdate(GDB.Response aResponse) {
			var xCmdParts = aResponse.Command.Split(' ');
			var xAddressOfResponse = Global.FromHexWithLeadingZeroX(xCmdParts[1]);
			var xFoundForUpdate = new List<WatchUC>();

			// check if a register have this adress
			foreach (var item in mWatches.Values) {
				uint xKey;
				RegNames? xReg = item.mRegister;
				if (xReg.HasValue)
					xKey = Windows.mRegistersForm.GetRegisterValue(xReg.Value);
				else
					xKey = Global.FromHexWithLeadingZeroX(item.lablAddress.Text);
				if (xKey == xAddressOfResponse)
					xFoundForUpdate.Add(item);
			}

			if (xFoundForUpdate.Count == 0)
				return;

			var xB = new StringBuilder();
			if (aResponse.Error)
				xB.Append(aResponse.ErrorMsg);
			else {
				int xCurrentCountOnLine = 0;
				foreach (var item in aResponse.Text) {
					var xIndex = item.IndexOf('\t');
					var xPart = item.Substring(xIndex);
					var xSplit = xPart.Split(Global.TabSeparator, StringSplitOptions.RemoveEmptyEntries);
					foreach (var mem in xSplit) {
						if (xCurrentCountOnLine == 2) {
							xB.AppendLine();
							xCurrentCountOnLine = 0;
						}
						else if (xCurrentCountOnLine > 0)
							xB.Append("  ");
						xB.Append(mem.ToUpper());
						xCurrentCountOnLine++;
					}
				}
			}

			var value = xB.ToString();
			foreach (var item in xFoundForUpdate) {
				item.lablValue.Text = value; 
			}
		}
    }
}