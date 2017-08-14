using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
	public enum RegNames {
		EAX,
		EBX,
		ECX,
		EDX,
		ESP,
		EBP,
		ESI,
		EDI,
		EIP,
		EFLAGS,
		CS,
		DS,
		SS,
		ES,
		FS,
		GS
	}

    public partial class FormRegisters : Form {
		private readonly Color Highlight = Color.Red;
		private Dictionary<RegNames, UInt32> mCurrentValues = new Dictionary<RegNames, UInt32>();

        public FormRegisters() {
            InitializeComponent();
			foreach (var registerName in Enum.GetValues(typeof(RegNames))) {
				mCurrentValues.Add((RegNames) registerName, 0u);
			}
        }

        private void FormRegisters_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void Redo() {
            Global.GDB.SendCmd("info registers");
        }

        public void UpdateRegisters(GDB.Response aResponse) {
            var xResult = aResponse.Text;
			{
				// reset all to black
				foreach (var item in this.Controls[0].Controls) {
					Label xL = item as Label;
					if (xL != null && xL.ForeColor != Color.Black)
						xL.ForeColor = Color.Black;
				}
			}

            int i = 0;
            CPUReg xReg;
            CPUReg xEIP = null;
            while (i < xResult.Count - 1) {
                xReg = new CPUReg(xResult, ref i);
				mCurrentValues[xReg.mName] = xReg.mValue;
                if (xReg.mName == RegNames.EAX) {
                    SetRegLabels(lablEAX, lablAX, lablAH, lablAL, xReg.mValue);
                } else if (xReg.mName == RegNames.EBX) {
                    SetRegLabels(lablEBX, lablBX, lablBH, lablBL, xReg.mValue);
                } else if (xReg.mName == RegNames.ECX) {
                    SetRegLabels(lablECX, lablCX, lablCH, lablCL, xReg.mValue);
                } else if (xReg.mName == RegNames.EDX) {
                    SetRegLabels(lablEDX, lablDX, lablDH, lablDL, xReg.mValue);
                } else if (xReg.mName == RegNames.EIP) {
                    xEIP = xReg;
                    SetAddress(lablEIP, xReg);
                    lablEIPText.Text = xReg.mText;
                    lablEIPText.Visible = true;
                } else if (xReg.mName == RegNames.EFLAGS) {
                    // http://en.wikipedia.org/wiki/FLAGS_register_%28computing%29
                    SetAddress(lablFlags, xReg);
                    lablFlagsText.Text = xReg.mText;
                    lablFlagsText.Visible = true;
                } else if (xReg.mName == RegNames.ESP) {
                    SetAddress(lablESP, xReg);
                } else if (xReg.mName == RegNames.EBP) {
                    SetAddress(lablEBP, xReg);
                } else if (xReg.mName == RegNames.ESI) {
                    SetAddress(lablESI, xReg);
                } else if (xReg.mName == RegNames.EDI) {
                    SetAddress(lablEDI, xReg);
                } else if (xReg.mName == RegNames.SS) {
                    SetAddress(lablSS, xReg);
                } else if (xReg.mName == RegNames.CS) {
                    SetAddress(lablCS, xReg);
                } else if (xReg.mName == RegNames.DS) {
                    SetAddress(lablDS, xReg);
                } else if (xReg.mName == RegNames.ES) {
                    SetAddress(lablES, xReg);
                } else if (xReg.mName == RegNames.FS) {
                    SetAddress(lablFS, xReg);
                } else if (xReg.mName == RegNames.GS) {
                    SetAddress(lablGS, xReg);
                }
            }
            if (xEIP != null) {
                Windows.mMainForm.SetEIP(xEIP.mValue);
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
            public readonly RegNames mName;
            public readonly UInt32 mValue;
            public readonly string mText;

            public CPUReg(List<string> aInput, ref int rIndex) {
                var xParts = aInput[rIndex].Split(Global.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
				if(false == Enum.TryParse<RegNames>(xParts[0].ToUpper(), out mName))
					MessageBox.Show("Could not parse Register '" + xParts[0].ToUpper() + "!");
				mValue = Global.FromHexWithLeadingZeroX(xParts[1]);

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
            var xHex = aValue.ToString("X8");

			if (a32.Text != xHex) {
				a32.Text = xHex;
				a32.ForeColor = Highlight;
			}
            a32.Visible = true;

			var xPart = xHex.Substring(4);
			if (a16.Text != xPart) {
				a16.Text = xPart;
				a16.ForeColor = Highlight;
			}
            a16.Visible = true;

			xPart = xHex.Substring(4, 2);
			if (a8H.Text != xPart) {
				a8H.Text = xPart;
				a8H.ForeColor = Highlight;
			}
            a8H.Visible = true;

			xPart = xHex.Substring(6);
			if (a8L.Text != xPart) {
				a8L.Text = xPart;
				a8L.ForeColor = Highlight;
			}
            a8L.Visible = true;
        }

        protected void SetAddress(Label aLabel, CPUReg aReg) {
            aLabel.Text = aReg.mValue.ToString("X8");
            aLabel.Visible = true;
        }

		public UInt32 GetRegisterValue(RegNames reg) {
			return mCurrentValues[reg];
		}
    }
}