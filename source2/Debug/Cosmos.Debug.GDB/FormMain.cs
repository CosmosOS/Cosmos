using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormMain : Form {
        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // Set breakpoints
        // watches
        // View stack

        private void mitmExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void mitmStepInto_Click(object sender, EventArgs e) {
            GDB.SendCmd("stepi");
            Update();
        }

        private void mitmStepOver_Click(object sender, EventArgs e) {
            GDB.SendCmd("nexti");
            Update();
        }

        protected void Connect(int aRetry) {
            mitmConnect.Enabled = false;

            Windows.CreateForms();
            GDB.Connect(aRetry);
            // Must be after Connect for now as it depends on Widnows being created
            // Also sets saved breakpoints, so GDB needs to be connected
            if (Settings.Filename != "") {
                Settings.Load();
            }

            Windows.UpdateAllWindows();
        }

        private void mitmConnect_Click(object sender, EventArgs e) {
            Connect(1);
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Update();
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e) {
            GDB.SendCmd("continue");
            Update();
        }

        private void mitmMainViewCallStack_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mCallStackForm);
        }

        private void mitmMainViewWatches_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mWatchesForm);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.Save();
        }

        protected FormWindowState mLastWindowState = FormWindowState.Normal;
        private void FormMain_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                // Window is being minimized
                Windows.Hide();
            } else if ((mLastWindowState == FormWindowState.Minimized) && (WindowState != FormWindowState.Minimized)) {
                // Window is being restored
                Windows.Reshow();
            }
            mLastWindowState = WindowState;
        }

        private void mitmViewLog_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mLogForm);
        }

        private void FormMain_Load(object sender, EventArgs e) {
            Windows.mMainForm = this;
        }

        private void mitmViewBreakpoints_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mBreakpointsForm);
        }

        private void mitmViewDisassembly_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mDisassemblyForm);
        }

        private void mitmRegisters_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mRegistersForm);
        }

        private void FormMain_Shown(object sender, EventArgs e) {
            // Dont put this in load. Load happens in main call from Main.cs and on exceptions just
            // goes out, no message. 
            // Also we want to show other forms after main form, not before.
            // We also only want to run this once, not on each possible show.
            if (mitmConnect.Enabled) {
                if (Settings.AutoConnect) {
                    Connect(30);
                }
            }

        }

    }
}
