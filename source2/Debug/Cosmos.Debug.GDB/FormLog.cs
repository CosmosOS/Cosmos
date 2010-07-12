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

        public void Log(string aMsg) {
            lboxDebug.SelectedIndex = lboxDebug.Items.Add(aMsg);
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

    }
}
