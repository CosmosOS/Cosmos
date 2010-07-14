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

        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // Set breakpoints
        // watches
        // View stack

        public void Disassemble(string aLabel) {
            lablCurrentFunction.Text = "";
            lablCurrentFunction.Visible = true;

            var xResult = GDB.SendCmd(("disassemble " + aLabel).Trim());
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

        private void butnSendCmd_Click(object sender, EventArgs e) {
            GDB.SendCmd(textSendCmd.Text);
            textSendCmd.Clear();
        }

        private void FormMain_Shown(object sender, EventArgs e) {
            if (mitmConnect.Enabled) {
                mitmConnect.PerformClick();
            }
        }

        private void mitmExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void mitmStepInto_Click(object sender, EventArgs e) {
            GDB.SendCmd("stepi");
            Update();
        }

        public void SetEIP(UInt32 aAddr) {
            foreach (AsmLine x in lboxDisassemble.Items) {
                if (x.mAddr == aAddr) {
                    lboxDisassemble.SelectedItem = x;
                    break;
                }
            }
        }

        private void mitmStepOver_Click(object sender, EventArgs e) {
            GDB.SendCmd("nexti");
            Update();
        }

        private void mitmConnect_Click(object sender, EventArgs e) {
            mitmConnect.Enabled = false;

            Windows.CreateForms();
            Windows.RestorePositions();
            GDB.Connect();

            Windows.UpdateAllWindows();
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Update();
        }

        private void textSendCmd_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                butnSendCmd.PerformClick();
            }
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e) {
            GDB.SendCmd("continue");
            Update();
        }

        private void mitemDisassemblyAddBreakpoint_Click(object sender, EventArgs e) {
            var x = (AsmLine)lboxDisassemble.SelectedItem;
            if (x != null) {
                Windows.mBreakpointsForm.AddBreakpoint("*0x" + x.mAddr.ToString("X8"));
            }
        }

        private void mitmMainViewCallStack_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mCallStackForm);
        }

        private void mitmMainViewWatches_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mWatchesForm);
        }

        private void FormMain_Load(object sender, EventArgs e) {
            Windows.mMainForm = this;
            Settings.Load();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.Save();
        }

        protected FormWindowState mLastWindowState = FormWindowState.Normal;
        private void FormMain_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                // Window is begin minimized
                Windows.Hide();
            } else if ((mLastWindowState == FormWindowState.Minimized) && (WindowState != FormWindowState.Minimized)) {
                // Window is being restored
                Windows.Reshow();
            }
            mLastWindowState = WindowState;
        }

    }
}
