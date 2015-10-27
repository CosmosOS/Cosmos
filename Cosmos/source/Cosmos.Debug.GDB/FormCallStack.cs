using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class FormCallStack : Form {
        protected class CallStack {
            public readonly UInt32 Address;
            public readonly string Label;

            public CallStack(string aInput) {
				var xSplit = aInput.Split(Global.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                Address = Global.FromHexWithLeadingZeroX(xSplit[1]);
                Label = xSplit[3];
            }

            public override string ToString() {
                if (Label.Length > 0) {
                    return Label;
                }
                return Address.ToString("X8");
            }
        }

        public FormCallStack() {
            InitializeComponent();
        }

        public void Redo() {
            Global.GDB.SendCmd("where");
        }

        public void OnWhere(GDB.Response aResponse) {
            lboxCallStack.BeginUpdate();
            try {
                lboxCallStack.Items.Clear();
                foreach (var x in aResponse.Text) {
                    //#0  0x0056d5df in DebugStub_Start ()
                    //#1  0x0057572b in System_Void__Cosmos_User_Kernel_Program_Init____DOT__00000001 ()
                    //#2  0x00550018 in Before_Kernel_Stack ()
                    //#3  0x005a5427 in __ENGINE_ENTRYPOINT__ ()
                    //~Backtrace stopped: frame did not save the PC
                    if (x.StartsWith("#")) {
                        lboxCallStack.Items.Add(new CallStack(x));
                    }
                }
            } finally {
                lboxCallStack.EndUpdate();
            }
        }

        private void menuCallStackGoto_Click(object sender, EventArgs e) {
            var x = (CallStack)lboxCallStack.SelectedItem;
            if (x != null) {
                Windows.mRegistersForm.ResetRegisters();
                // Address doesn't work for some reason
                Windows.mMainForm.Disassemble(x.Label);
            }
        }

        private void lboxCallStack_DoubleClick(object sender, EventArgs e) {
            menuCallStackGoto.PerformClick();
        }

        private void FormCallStack_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            Hide();
        }
    }
}
