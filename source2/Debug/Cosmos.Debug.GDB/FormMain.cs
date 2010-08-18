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
        protected class AsmLine {
            public readonly UInt32 mAddr;
            public readonly string mLabel;
            public readonly string mOp;
            public readonly string mData = "";

            public AsmLine(string aInput) {
                //"0x0056d2b9 <_end_data+0>:\tmov    DWORD PTR ds:0x550020,ebx\n"
                var s = GDB.Unescape(aInput);
                var xSplit1 = s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var xSplit2 = xSplit1[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mAddr = Global.FromHex(xSplit2[0]);
                string xLabel;
                if (xSplit2.Length > 1) {
                    xLabel = xSplit2[1];
                }

                xSplit2 = xSplit1[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mOp = xSplit2[0];
                if (xSplit2.Length > 1) {
                    for (int j = 1; j < xSplit2.Length; j++) {
                        mData += xSplit2[j] + " ";
                    }
                    mData = mData.TrimEnd();
                }
            }

            public override string ToString() {
                // First char reserved for breakpoint (*)
                return "  " + (mAddr.ToString("X8") + ":  " + mOp + " " + mData).TrimEnd();
            }
        }

        protected string mFuncName;

        public void Disassemble(string aLabel) {
            lablCurrentFunction.Text = "";
            lablCurrentFunction.Visible = true;

            var xResult = Global.GDB.SendCmd(("disassemble " + aLabel).Trim()).Text;
            lboxDisassemble.BeginUpdate();
            try {
                lboxDisassemble.Items.Clear();
                // In some cases GDB might return no results. This is common when no symbols are loaded.
                if (xResult.Count > 0) {
                    var xSplit = GDB.Unescape(xResult[1]).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    mFuncName = xSplit[xSplit.Length - 1];
                    lablCurrentFunction.Text = mFuncName;

                    // 1 and -2 to eliminate header and footer line
                    for (int i = 1; i <= xResult.Count - 2; i++) {
                        lboxDisassemble.Items.Add(new AsmLine(xResult[i]));
                    }
                }
            } finally {
                lboxDisassemble.EndUpdate();
            }
        }

        public void SetEIP(UInt32 aAddr) {
            foreach (AsmLine x in lboxDisassemble.Items) {
                if (x.mAddr == aAddr) {
                    lboxDisassemble.SelectedItem = x;
                    break;
                }
            }
        }

        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // watches
        // View stack
        // If close without connect, it wipes out the settings file

        private void mitmExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void mitmStepInto_Click(object sender, EventArgs e) {
            lablRunning.Text = "Running";
            Application.DoEvents();
            Global.GDB.SendCmd("stepi");
            lablRunning.Text = "Stopped";
            Windows.UpdateAllWindows();
        }

        private void mitmStepOver_Click(object sender, EventArgs e) {
            lablRunning.Text = "Running";
            Application.DoEvents();
            Global.GDB.SendCmd("nexti");
            lablRunning.Text = "Stopped";
            Windows.UpdateAllWindows();
        }

        protected void Connect(int aRetry) {
            if (!mitmConnect.Enabled) {
                return;
            }
            mitmConnect.Enabled = false;

            Windows.CreateForms();
            Global.GDB = new GDB(aRetry);
            if (Global.GDB.Connected) {
                lablConnected.Visible = true;
                lablRunning.Visible = true;
                lablRunning.Text = "Stopped";
                // Must be after Connect for now as it depends on Widnows being created
                // Also sets saved breakpoints, so GDB needs to be connected
                if (Settings.Filename != "") {
                    Settings.Load();
                }
                Windows.UpdateAllWindows();
            }
        }

        private void mitmConnect_Click(object sender, EventArgs e) {
            Connect(30);
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Windows.UpdateAllWindows();
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e) {
            lablRunning.Text = "Running";
            Application.DoEvents();
            Global.GDB.SendCmd("continue");
            lablRunning.Text = "Stopped";
            Windows.UpdateAllWindows();
        }

        private void mitmMainViewCallStack_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mCallStackForm);
        }

        private void mitmMainViewWatches_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mWatchesForm);
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

        private void BringWindowsToTop() {
            mIgnoreNextActivate = true;
            foreach (var xWindow in Windows.mForms) {
                if (xWindow == this) {
                    continue;
                }
                if (xWindow.Visible) {
                    xWindow.Activate();
                }
            }
            this.Activate();
        }

        private void mitmWindowsToForeground_Click(object sender, EventArgs e)
        {
            BringWindowsToTop();
        }

        private bool mIgnoreNextActivate = false;
        private void FormMain_Activated(object sender, EventArgs e) {
            // Necessary else we get looping becuase BringWindowsToTop reactivates this.
            if (mIgnoreNextActivate) {
                mIgnoreNextActivate = false;
            } else {
                BringWindowsToTop();
            }
        }

        private void mitmSave_Click(object sender, EventArgs e) {
            Windows.SavePositions();
            Settings.Save();
        }

        private void mitemDisassemblyAddBreakpoint_Click(object sender, EventArgs e) {
            var x = (AsmLine)lboxDisassemble.SelectedItem;
            if (x != null) {
                Windows.mBreakpointsForm.AddBreakpoint("*0x" + x.mAddr.ToString("X8"));
            }
        }

        private void mitmCopyToClipboard_Click(object sender, EventArgs e) {
            var x = (AsmLine)lboxDisassemble.SelectedItem;
            Clipboard.SetText(x.ToString());
        }

    }
}
