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
    public partial class FormRegisters : Form {
        public FormRegisters() {
            InitializeComponent();
        }

        private void FormRegisters_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void Redo() {
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
                Windows.mDisassemblyForm.SetEIP(xEIP.mValue);
            }
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

    }
}
