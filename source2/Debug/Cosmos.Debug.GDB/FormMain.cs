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

        public FormMain() {
            InitializeComponent();
        }

        // TODO
        // Set breakpoints
        // watches
        // View stack

        protected string Unescape(string aInput) {
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
                    // ^ ?
                    xLine = xLine.Remove(0, 1);
                    if ((xType == '~') || (xType == '&')) {
                        xLine = Unescape(xLine);
                    }
                    lboxDebug.Items.Add(xLine);
                    if (xType == '~') {
                        xResult.Add(xLine);
                    }
                }
                Application.DoEvents();
            }

            lboxDebug.Items.Add("-----");
            return xResult;
        }

        protected List<string> SendCmd(string aCmd) {
            lboxDebug.Items.Add(aCmd);
            mGDBProcess.StandardInput.WriteLine(aCmd);
            return GetResponse();
        }

        protected void Disassemble() {
            var xResult = SendCmd("disassemble");
            lboxDisassemble.Items.Clear();
            // In some cases GDB might return no results. This is common when no symbols are loaded.
            if (xResult.Count > 0) {
                // 1 and -2 to eliminate header and footer line
                for (int i = 1; i <= xResult.Count - 2; i++) {
                    var s = Unescape(xResult[i]);
                    //"0x0056d2b9 <_end_data+0>:\tmov    DWORD PTR ds:0x550020,ebx\n"
                    var xSplit1 = s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    
                    var xSplit2 = xSplit1[0].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string xAddr = xSplit2[0].Substring(2).ToUpper();
                    string xLabel;
                    if (xSplit2.Length > 1) {
                        xLabel = xSplit2[1];
                    }

                    xSplit2 = xSplit1[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string xOp = xSplit2[0];
                    string xData = "";
                    if (xSplit2.Length > 1) {
                        for (int j = 1; j < xSplit2.Length; j++) {
                            xData += xSplit2[j] + " ";
                        }
                        xData = xData.TrimEnd();
                    }

                    lboxDisassemble.Items.Add((xAddr + ":  " + xOp + " " + xData).TrimEnd());
                }
            }
        }

        private void butnConnect_Click(object sender, EventArgs e) {
            butnConnect.Enabled = false;
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
            //SendCmd("break *0x0056d2b9");
            SendCmd("continue");
            Update();
        }

        private void butnStepOne_Click(object sender, EventArgs e) {
            SendCmd("stepi");
            Update();
        }

        protected class CPUReg {
            public string mName;
            public UInt32 mValue;
            public string mText;

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
            a16.Text = xHex.Substring(4);
            a8H.Text = xHex.Substring(4, 2);
            a8L.Text = xHex.Substring(6);
        }

        protected void GetRegisters() {
            var xResult = SendCmd("info registers");

            int i = 0;
            CPUReg xReg;
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
                    lablEIP.Text = xReg.mValue.ToString("X8");
                    lablEIPText.Text = xReg.mText;
                } else if (xReg.mName == "EFLAGS") {
                    // http://en.wikipedia.org/wiki/FLAGS_register_%28computing%29
                    lablFlags.Text = xReg.mValue.ToString("X8");
                    lablFlagsText.Text = xReg.mText;
                } else if (xReg.mName == "ESP") {
                    lablESP.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "EBP") {
                    lablEBP.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "ESI") {
                    lablESI.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "EDI") {
                    lablEDI.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "SS") {
                    lablSS.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "CS") {
                    lablCS.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "DS") {
                    lablDS.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "ES") {
                    lablES.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "FS") {
                    lablFS.Text = xReg.mValue.ToString("X8");
                } else if (xReg.mName == "GS") {
                    lablGS.Text = xReg.mValue.ToString("X8");
                }
            }
        }

        protected void Update() {
            Disassemble();
            GetRegisters();
        }

        private void butnSendCmd_Click(object sender, EventArgs e) {
            SendCmd(textSendCmd.Text);
            textSendCmd.Text = "";
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
            if (butnConnect.Enabled) {
                butnConnect.PerformClick();
            }
        }


    }
}
