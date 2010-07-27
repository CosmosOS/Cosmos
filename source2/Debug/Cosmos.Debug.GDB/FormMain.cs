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
            // Must be after Connect for now as it depends on Widnows being created
            if (Settings.Filename != "") {
                Settings.Load();
            }
            GDB.Connect(aRetry);

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
            if (Settings.AutoConnect) {
                Connect(30);
            }
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

    }
}
