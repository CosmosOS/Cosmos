using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormMain : Form {
        protected System.Diagnostics.Process mGDBProcess;
        protected System.IO.StreamReader mGDB;
        protected string mFuncName;

        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // Set breakpoints
        // watches
        // View stack

        static protected string Unescape(string aInput) {
            // Remove surrounding ", /n, then unescape and trim
            return Regex.Unescape(aInput.Substring(1, aInput.Length - 2).Replace('\n', ' ').Trim());
        }

        protected List<string> GetResponse() {
            var xResult = new List<string>();

            //TODO: Cant find a better way than peek... 
            while (!mGDBProcess.HasExited) {
                var xLine = mGDB.ReadLine();
                // Null occurs after quit
                if (xLine == null) {
                    break;
                } else if (xLine.Trim() == "(gdb)") {
                    break;
                } else {
                    var xType = xLine[0];
                    // & echo of a command
                    // ~ text response
                    // ^ done
                    xLine = xLine.Remove(0, 1);
                    if ((xType == '~') || (xType == '&')) {
                        xLine = FormMain.Unescape(xLine);
                    }
                    Log(xType + xLine);
                    if (xType == '~') {
                        xResult.Add(xLine);
                    }
                }
                Application.DoEvents();
            }

            Log("-----");
            return xResult;
        }

        protected void Log(string aMsg) {
            lboxDebug.SelectedIndex = lboxDebug.Items.Add(aMsg);
        }

        protected List<string> SendCmd(string aCmd) {
            mGDBProcess.StandardInput.WriteLine(aCmd);
            return GetResponse();
        }

        protected class AsmLine {
            public readonly UInt32 mAddr;
            public readonly string mLabel;
            public readonly string mOp;
            public readonly string mData = "";

            public AsmLine(string aInput) {
                //"0x0056d2b9 <_end_data+0>:\tmov    DWORD PTR ds:0x550020,ebx\n"
                var s = FormMain.Unescape(aInput);
                var xSplit1 = s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var xSplit2 = xSplit1[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mAddr = UInt32.Parse(xSplit2[0].Substring(2), NumberStyles.HexNumber);
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

        protected void Disassemble() {
            lablCurrentFunction.Text = "";
            lablCurrentFunction.Visible = true;

            var xResult = SendCmd("disassemble");
            lboxDisassemble.BeginUpdate();
            try {
                lboxDisassemble.Items.Clear();
                // In some cases GDB might return no results. This is common when no symbols are loaded.
                if (xResult.Count > 0) {
                    var xSplit = Unescape(xResult[1]).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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

        protected void GetRegisters() {
            var xResult = SendCmd("info registers");

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
                } else if (xReg.mName == "EFLAGS") {
                    // http://en.wikipedia.org/wiki/FLAGS_register_%28computing%29
                    SetAddress(lablFlags, xReg);
                    lablFlagsText.Text = xReg.mText;
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

        protected void Update() {
            Disassemble();
            GetRegisters();
        }

        private void butnSendCmd_Click(object sender, EventArgs e) {
            SendCmd(textSendCmd.Text);
            textSendCmd.Clear();
        }

        private void butnDebugLogClear_Click(object sender, EventArgs e) {
            lboxDebug.Items.Clear();
        }

        private void butnCopyDebugLogToClipboard_Click(object sender, EventArgs e) {
            var xSB = new StringBuilder();
            foreach (string x in lboxDebug.Items) {
                xSB.AppendLine(x);
            }
            Clipboard.SetText(xSB.ToString());
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
            SendCmd("stepi");
            Update();
        }

        private void mitmStepOver_Click(object sender, EventArgs e) {
            SendCmd("nexti");
            Update();
        }

        private void mitmConnect_Click(object sender, EventArgs e) {
            mitmConnect.Enabled = false;
            var xStartInfo = new ProcessStartInfo();
            //TODO: Make path dynamic
            xStartInfo.FileName = @"D:\source\Cosmos\Build\Tools\gdb.exe";
            xStartInfo.Arguments = @"--interpreter=mi2";
            //TODO: Make path dynamic
            xStartInfo.WorkingDirectory = @"D:\source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\debug";
            xStartInfo.CreateNoWindow = true;
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardOutput = true;
            xStartInfo.RedirectStandardInput = true;
            mGDBProcess = System.Diagnostics.Process.Start(xStartInfo);
            mGDB = mGDBProcess.StandardOutput;
            mGDBProcess.StandardInput.AutoFlush = true;

            GetResponse();
            SendCmd("symbol-file CosmosKernel.obj");

            //
            SendCmd("target remote :8832"); // 

            SendCmd("set architecture i386");
            SendCmd("set language asm");
            SendCmd("set disassembly-flavor intel");
            SendCmd("break Kernel_Start");
            SendCmd("continue");
            SendCmd("delete 1");
            Update();
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Update();
            tabControl1.SelectedIndex = 0;
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

        protected void AddBreakpoint(string aLabel) {
            string s = aLabel.Trim();
            if (s.Length > 0) {
                var xResult = SendCmd("break " + s);
                var xSplit = xResult[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (xSplit[0] == "Breakpoint") {
                    lboxBreakpoints.SelectedIndex = lboxBreakpoints.Items.Add(new Breakpoint(s, int.Parse(xSplit[1])));
                } else {
                    MessageBox.Show(xResult[0]);
                }
            }
        }

        private void butnBreakpointAdd_Click(object sender, EventArgs e) {
            AddBreakpoint(textBreakpoint.Text);
            textBreakpoint.Clear();
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e) {
            SendCmd("continue");
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
                SendCmd("delete " + x.Index);
                lboxBreakpoints.Items.Remove(x);
            }
        }

        private void mitemDisassemblyAddBreakpoint_Click(object sender, EventArgs e) {
            var x = (AsmLine)lboxDisassemble.SelectedItem;
            if (x != null) {
                AddBreakpoint("*0x" + x.mAddr.ToString("X8"));
            }
        }


    }
}
