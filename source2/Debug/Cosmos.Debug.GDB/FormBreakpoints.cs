using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormBreakpoints : Form {
        protected class Breakpoint {
            public readonly string Label;
            public readonly int Index;

            public Breakpoint(string aLabel, int aIndex) {
                Label = aLabel;
                Index = aIndex;
            }

            public override string ToString() {
                return Index.ToString("00") + " " + Label;
            }
        }

        public FormBreakpoints() {
            InitializeComponent();
        }

        public void LoadSession() {
            foreach (SettingsDS.BreakpointRow xBP in Settings.DS.Breakpoint.Rows) {
                AddBreakpoint(xBP.Label);
            }
        }

        public bool AddBreakpoint(string aLabel) {
            string s = aLabel.Trim();
            if (s.Length > 0) {
                var xResult = Global.GDB.SendCmd("break " + s).Text;
                var xSplit = xResult[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (xSplit[0] == "Breakpoint") {
                    lboxBreakpoints.SelectedIndex = lboxBreakpoints.Items.Add(new Breakpoint(s, int.Parse(xSplit[1])));

                    // http://stackoverflow.com/questions/27674/dynamic-top-down-list-of-controls-in-windowsforms-and-c
                    var xUC = new BreakpointUC();
                    xUC.Dock = DockStyle.Top;
                    xUC.cboxEnabled.Checked = true;
                    xUC.lablNum.Text = xSplit[1];
                    xUC.lablName.Text = s;
                    panl.Controls.Add(xUC);
                    return true;
                }
                MessageBox.Show(xResult[0]);
            }
            return false;
        }

        private void butnBreakpointAdd_Click(object sender, EventArgs e) {
            string xLabel = textBreakpoint.Text.Trim();
            if (AddBreakpoint(xLabel)) {
                textBreakpoint.Clear();

                // We dont add address types, as most of them change between compiles.
                if (!xLabel.StartsWith("*")) {
                    // Add here and not in AddBreakpoint, because during load we call AddBreakpoint
                    var xBP = Settings.DS.Breakpoint.NewBreakpointRow();
                    xBP.Label = xLabel;
                    Settings.DS.Breakpoint.AddBreakpointRow(xBP);
                    Settings.Save();
                }
            }
        }

        private void textBreakpoint_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                butnBreakpointAdd.PerformClick();
            }
        }

        private void mitmBreakpointDelete_Click(object sender, EventArgs e) {
            var x = (Breakpoint)lboxBreakpoints.SelectedItem;
            if (x != null) {
                Global.GDB.SendCmd("delete " + x.Index);
                lboxBreakpoints.Items.Remove(x);
            }
        }

        private void FormBreakpoints_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

    }
}
