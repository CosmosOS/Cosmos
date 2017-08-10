using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormLog : Form {
        public FormLog() {
            InitializeComponent();
        }

        public void Log(GDB.Response aResponse) {
            lboxCmd.SelectedIndex = lboxCmd.Items.Add(aResponse);
            Application.DoEvents();
        }

        private void mitmCopyToClipboard_Click(object sender, EventArgs e) {
            var xSB = new StringBuilder();
            foreach (string x in lboxDebug.Items) {
                xSB.AppendLine(x);
            }
            Clipboard.SetText(xSB.ToString());
        }

        private void mitmClear_Click(object sender, EventArgs e) {
            lboxDebug.Items.Clear();
        }

        private void FormLog_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void butnSendCmd_Click(object sender, EventArgs e) {
            Global.GDB.SendCmd(textSendCmd.Text);
            textSendCmd.Clear();
        }

        private void textSendCmd_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                butnSendCmd.PerformClick();
            }
        }

        private void lboxCmd_SelectedIndexChanged(object sender, EventArgs e) {
            var xResponse = (GDB.Response)lboxCmd.SelectedItem;
            lboxDebug.Items.Clear();

            lboxDebug.Items.Add(xResponse.Command);
            if (xResponse.Error) {
                lboxDebug.Items.Add("ERROR: " + xResponse.ErrorMsg);
            }
            lboxDebug.Items.Add(string.Empty);

            foreach (var x in xResponse.Text) {
                lboxDebug.Items.Add(x);
            }

			if (xResponse.Reply != xResponse.Command)
			{
				lboxDebug.Items.Add(string.Empty);
				lboxDebug.Items.Add(xResponse.Reply);
			}
        }

    }
}
