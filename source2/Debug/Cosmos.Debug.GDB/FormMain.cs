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
        protected string mFuncName;

        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // Set breakpoints
        // watches
        // View stack

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

        protected class CPUReg {
            public readonly string mName;
            public readonly UInt32 mValue;
            public readonly string mText;

            public CPUReg(List<string> aInput, ref int rIndex) {
                var xParts = aInput[rIndex].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mName = xParts[0].ToUpper();
                // Substring(2) is to remove the 0x which is required according to .NET docs and experimentation
                mValue = UInt32.Parse(xParts[1].Substring(2), NumberStyles.HexNumber);

                mText = aInput[rIndex + 1].Trim();

                rIndex = rIndex + 2;

                while (rIndex < aInput.Count - 1) {
                    var s = aInput[rIndex].Replace('\n', ' ').Trim();
                    if (s.Length > 1) {
                        break;
                    }
                    rIndex++;
                }
            }
        }

        protected void SetRegLabels(Label a32, Label a16, Label a8H, Label a8L, UInt32 aValue) {
            string xHex = aValue.ToString("X8");
            
            a32.Text = xHex;
            a32.Visible = true;
            
            a16.Text = xHex.Substring(4);
            a16.Visible = true;

            a8H.Text = xHex.Substring(4, 2);
            a8H.Visible = true;
            
            a8L.Text = xHex.Substring(6);
            a8L.Visible = true;
        }

        protected void SetAddress(Label aLabel, CPUReg aReg) {
            aLabel.Text = aReg.mValue.ToString("X8");
            aLabel.Visible = true;
        }

        public void GetRegisters() {
            var xResult = GDB.SendCmd("info registers");

            int i = 0;
            CPUReg xReg;
            CPUReg xEIP = null;
            while (i < xResult.Count - 1) {
                xReg = new CPUReg(xResult, ref i);
                if (xReg.mName == "EAX") {
                    SetRegLabels(lablEAX, lablAX, lablAH, lablAL, xReg.mValue);
                } else if (xReg.mName == "EBX") {
                    SetRegLabels(lablEBX, lablBX, lablBH, lablBL, xReg.mValue);
                } else if (xReg.mName == "ECX") {
                    SetRegLabels(lablECX, lablCX, lablCH, lablCL, xReg.mValue);
                } else if (xReg.mName == "EDX") {
                    SetRegLabels(lablEDX, lablDX, lablDH, lablDL, xReg.mValue);
                } else if (xReg.mName == "EIP") {
                    xEIP = xReg;
                    SetAddress(lablEIP, xReg);
                    lablEIPText.Text = xReg.mText;
                    lablEIPText.Visible = true;
                } else if (xReg.mName == "EFLAGS") {
                    // http://en.wikipedia.org/wiki/FLAGS_register_%28computing%29
                    SetAddress(lablFlags, xReg);
                    lablFlagsText.Text = xReg.mText;
                    lablFlagsText.Visible = true;
                } else if (xReg.mName == "ESP") {
                    SetAddress(lablESP, xReg);
                } else if (xReg.mName == "EBP") {
                    SetAddress(lablEBP, xReg);
                } else if (xReg.mName == "ESI") {
                    SetAddress(lablESI, xReg);
                } else if (xReg.mName == "EDI") {
                    SetAddress(lablEDI, xReg);
                } else if (xReg.mName == "SS") {
                    SetAddress(lablSS, xReg);
                } else if (xReg.mName == "CS") {
                    SetAddress(lablCS, xReg);
                } else if (xReg.mName == "DS") {
                    SetAddress(lablDS, xReg);
                } else if (xReg.mName == "ES") {
                    SetAddress(lablES, xReg);
                } else if (xReg.mName == "FS") {
                    SetAddress(lablFS, xReg);
                } else if (xReg.mName == "GS") {
                    SetAddress(lablGS, xReg);
                }
            }
            if (xEIP != null) {
                foreach (AsmLine x in lboxDisassemble.Items) {
                    if (x.mAddr == xEIP.mValue) {
                        lboxDisassemble.SelectedItem = x;
                        break;
                    }
                }
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

        private void mitmStepOver_Click(object sender, EventArgs e) {
            GDB.SendCmd("nexti");
            Update();
        }

        private void mitmConnect_Click(object sender, EventArgs e) {
            mitmConnect.Enabled = false;

            Windows.CreateForms();
            GDB.Connect();

            LoadSettings();
            foreach (SettingsDS.BreakpointRow xBP in Settings.Breakpoint.Rows) {
                AddBreakpoint(xBP.Label);
            }

            Update();
        }

        public void ResetRegisters() {
            lablEAX.Visible = false;
            lablAX.Visible = false;
            lablAH.Visible = false;
            lablAL.Visible = false;
            lablEBX.Visible = false;
            lablBX.Visible = false;
            lablBH.Visible = false;
            lablBL.Visible = false;
            lablECX.Visible = false;
            lablCX.Visible = false;
            lablCH.Visible = false;
            lablCL.Visible = false;
            lablEDX.Visible = false;
            lablDX.Visible = false;
            lablDH.Visible = false;
            lablDL.Visible = false;
            lablEIP.Visible = false;
            lablEIPText.Visible = false;
            lablFlags.Visible = false;
            lablFlagsText.Visible = false;
            lablESP.Visible = false;
            lablEBP.Visible = false;
            lablESI.Visible = false;
            lablEDI.Visible = false;
            lablSS.Visible = false;
            lablCS.Visible = false;
            lablDS.Visible = false;
            lablES.Visible = false;
            lablFS.Visible = false;
            lablGS.Visible = false;
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Update();
        }

        private void textSendCmd_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                butnSendCmd.PerformClick();
            }
        }

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

        protected bool AddBreakpoint(string aLabel) {
            string s = aLabel.Trim();
            if (s.Length > 0) {
                var xResult = GDB.SendCmd("break " + s);
                var xSplit = xResult[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (xSplit[0] == "Breakpoint") {
                    lboxBreakpoints.SelectedIndex = lboxBreakpoints.Items.Add(new Breakpoint(s, int.Parse(xSplit[1])));
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
                    var xBP = Settings.Breakpoint.NewBreakpointRow();
                    xBP.Label = xLabel;
                    Settings.Breakpoint.AddBreakpointRow(xBP);
                    SaveSettings();
                }
            }
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e) {
            GDB.SendCmd("continue");
            Update();
        }

        private void textBreakpoint_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                butnBreakpointAdd.PerformClick();
            }
        }

        private void mitmBreakpointDelete_Click(object sender, EventArgs e) {
            var x = (Breakpoint)lboxBreakpoints.SelectedItem;
            if (x != null) {
                GDB.SendCmd("delete " + x.Index);
                lboxBreakpoints.Items.Remove(x);
            }
        }

        private void mitemDisassemblyAddBreakpoint_Click(object sender, EventArgs e) {
            var x = (AsmLine)lboxDisassemble.SelectedItem;
            if (x != null) {
                AddBreakpoint("*0x" + x.mAddr.ToString("X8"));
            }
        }

        //TODO: Not supposed to be in app dir, but unless we release this as a standalone project
        //it doesnt matter. If we do that we have to create project types anyways.
        protected string ConfigPathname = Application.ExecutablePath + ".Settings";

        protected void SaveSettings() {
            Settings.WriteXml(ConfigPathname, System.Data.XmlWriteMode.IgnoreSchema);
        }

        protected void LoadSettings() {
            if (System.IO.File.Exists(ConfigPathname)) {
                Settings.ReadXml(ConfigPathname, System.Data.XmlReadMode.IgnoreSchema);
            }
        }

        private void mitmMainViewCallStack_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mCallStackForm);
        }

        private void mitmMainViewWatches_Click(object sender, EventArgs e) {
            Windows.Show(Windows.mWatchesForm);
        }

    }
}
