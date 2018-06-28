using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cosmos.Debug.GDB {
    public partial class FormMain : Form {
        protected class GdbAsmLine {
            public readonly UInt32 mAddr;
            public readonly string mLabel;
            public readonly string mOp;
            public readonly string mData = string.Empty;
			public readonly bool mEIPHere;

            public GdbAsmLine(string aInput) {
                //"0x0056d2b9 <_end_data+0>:\tmov    DWORD PTR ds:0x550020,ebx\n"
                var s = GDB.Unescape(aInput);
				var xSplit1 = s.Split(Global.TabSeparator, StringSplitOptions.RemoveEmptyEntries);

				var xSplit2 = xSplit1[0].Split(Global.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);

				int xIndex = 0;
				//newer gdb above 6.6 or higher versions
				if (xSplit2[0] == "=>")
				{
					mEIPHere = true;
					xIndex = 1;
				}
                mAddr = Global.FromHexWithLeadingZeroX(xSplit2[xIndex]);
                string xLabel;
                if (xSplit2.Length > xIndex + 1) {
                    xLabel = xSplit2[xIndex + 1];
                }

				xSplit2 = xSplit1[1].Split(Global.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
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
                return "  " + mAddr.ToString("X8") + ":  " + mOp + " " + mData.TrimEnd();
            }
        }

		const int MAX_RETRY = 3;
        protected string mFuncName;
		protected bool mCreated;
		protected int mConnectRetry;

        protected void OnGDBResponse(GDB.Response aResponse) {
            try {
                Windows.mLogForm.Log(aResponse);
                var xCmdLine = aResponse.Command.ToLower();
                if (xCmdLine == "info registers") {
                    Windows.mRegistersForm.UpdateRegisters(aResponse);
					Windows.UpdateAfterRegisterUpdate();
				}else if(xCmdLine.Length == 0) {
					if (aResponse.Text.Count == 2 && aResponse.Text[0] == "Breakpoint")
					{
						// program breaks on aResponse.Text[1]
					}
					else
					{
						// contains address where we are
					}
                } else {
					var xCmdParts = xCmdLine.Split(Global.SpaceSeparator);
                    var xCmd = xCmdParts[0];
					if (xCmd.EndsWith("&")) {
						xCmd = xCmd.Substring(0, xCmd.Length - 1);
					}
                    if (xCmd == "disassemble") {
                        OnDisassemble(aResponse);
                    } else if (xCmd == "symbol-file") { // nothing
                    } else if (xCmd == "add-symbol-file") { //nothing
                    } else if (xCmd == "set") { // nothing
                    } else if (xCmd == "target") {

						if (Global.GDB.Connected)
						{
							mitmRefresh.Enabled = true;
							lablConnected.Visible = true;
							lablRunning.Visible = true;

							mitmConnect.Enabled = true;
							butnConnect.Enabled = true;
							mitmConnect.Text = "&Disconnect";
							butnConnect.Text = "&Disconnect";

							Settings.InitWindows();

							lboxDisassemble.SetItems(Global.AsmSource.Lines);
						}
						else
						{
							if (mConnectRetry < MAX_RETRY + 1)
							{
								Connect();
							}
							else
							{
								mitmConnect.Enabled = true;
								butnConnect.Enabled = true;
								mitmConnect.Text = "&Connect";
								butnConnect.Text = "&Connect";
								lboxDisassemble.Items.Clear();
							}
						}
					} else if (xCmd == "detach") {
						if (false == Global.GDB.Connected)
						{
							mitmConnect.Text = "&Connect";
							butnConnect.Text = "&Connect";
							mitmRefresh.Enabled = false;
							mitmContinue.Enabled = false;
							butnContinue.Enabled = false;
							mitmStepInto.Enabled = false;
							mitmStepOver.Enabled = false;
							lboxDisassemble.Items.Clear();
							lablConnected.Visible = false;
							lablRunning.Visible = false;
							textCurrentFunction.Visible = false;
						}
                    } else if (xCmd == "delete") {
                        Windows.mBreakpointsForm.OnDelete(aResponse);
                    } else if ((xCmd == "stepi") || (xCmd == "nexti")) {
					} else if (xCmd == "continue" || xCmd== "fg") {
						//lboxDisassemble.Items.Clear();
                    } else if (xCmd == "where") {
                        Windows.mCallStackForm.OnWhere(aResponse);
                    } else if (xCmd == "break") {
                        Windows.mBreakpointsForm.OnBreak(aResponse);
					} else if (xCmd.StartsWith("x/")) {
						Windows.mWatchesForm.OnWatchUpdate(aResponse);
                    } else {
                        throw new Exception("Unrecognized command response: " + aResponse.Command);
                    }
                }
            } catch (Exception e) {
                MessageBox.Show("Exception: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public void Disassemble(string aLabel) {
            textCurrentFunction.Text = string.Empty;
            textCurrentFunction.Visible = true;
			// force space free at end
			var xDisAsmCmd = "disassemble";
			var xLabelTrimed = aLabel.TrimEnd();
			if (xLabelTrimed.Length > 0)
				xDisAsmCmd += " " + xLabelTrimed;
            Global.GDB.SendCmd(xDisAsmCmd);
        }

		protected void OnDisassemble(GDB.Response xResponse)
		{
			var xResult = xResponse.Text;
			// In some cases GDB might return no results. This is common when no symbols are loaded.
			if (xResult.Count == 0)
				return;
			// Get function name
			var xSplit = GDB.Unescape(xResult[0]).Split(Global.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
			mFuncName = xSplit[xSplit.Length - 1];
			textCurrentFunction.Text = mFuncName;

			// remove ':'
			mFuncName = mFuncName.Substring(0, mFuncName.Length - 1);

			int labelLine = Global.AsmSource.GetLineOfLabel(mFuncName);
			labelLine++;

			// 1 and -2 to eliminate header and footer line
			for (int i = 1; i <= xResult.Count - 2; i++, labelLine++)
			{
				var asmLine = Global.AsmSource.Lines[labelLine];

				while (asmLine.IsLabel ||
					(asmLine.FirstToken != null && (asmLine.FirstToken == "global" || asmLine.FirstToken.StartsWith(";"))))
				{
					labelLine++;
					asmLine = Global.AsmSource.Lines[labelLine];
				}

				var gdbLine = new GdbAsmLine(xResult[i]);
				asmLine.Address = gdbLine.mAddr;

				// check if line different, if so, we set a line for tooltip
				string strGdbLine = gdbLine.ToString();
				string gdbLineWithoutAddress = strGdbLine.Substring(strGdbLine.IndexOf(":") + 3);
				string gdbLineWithoutAddressLower = gdbLineWithoutAddress.Replace(" ", string.Empty).ToLower().Replace("dwordptr",string.Empty);

				string asmlineFromFile = asmLine.OrignalLine.TrimStart('\t', ' ').ToLower().Replace("dword", string.Empty);
				string asmlineFromFileWithoutspace = asmlineFromFile.Replace(" ", string.Empty);
				if (gdbLineWithoutAddressLower != asmlineFromFileWithoutspace)
				{
					asmLine.GDBLine = gdbLineWithoutAddress;
				}
			}
		}

        public void SetEIP(UInt32 aAddr) {
			lboxDisassemble.SelectedAddress = aAddr;
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
            Global.GDB.SendCmd("stepi&");
        }

        private void mitmStepOver_Click(object sender, EventArgs e) {
            Global.GDB.SendCmd("nexti&");
        }

        protected void Connect() {
			if (Settings.OutputPath == null)
			{
				// path of asm, obj and cgdb
				using(var xDialog = new OpenFileDialog())
				{
					xDialog.Filter = "Symbols (*.asm;*.obj;*.mdf)|*.asm;*.obj;*.mdf";
					xDialog.ShowHelp = true;
					xDialog.HelpRequest += new EventHandler(xDialog_HelpRequest);

					if (xDialog.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
						return;

					if (false == Settings.LoadOnFly(xDialog.FileName))
					{
						MessageBox.Show("Error on loading selection!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
						return;
					}
					mitmSave.Enabled = true;
				}
			}

            mitmConnect.Enabled = false;
			butnConnect.Enabled = false;

			mitmConnect.Text = "Try " + mConnectRetry;
			butnConnect.Text = "Try " + mConnectRetry++;

			if (false == mCreated)
			{
				Windows.CreateForms();
				Global.AsmSource = new AsmFile(Path.Combine(Settings.OutputPath, Settings.AsmFile));
				Global.GDB = new GDB(OnGDBResponse, OnRunStateChanged);
				mCreated = true;
			}
			Global.GDB.Connect();
        }

		void xDialog_HelpRequest(object sender, EventArgs e)
		{
			MessageBox.Show("Select a folder which contain files of type asm, obj and mdf with same name.", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
		}

		private void OnRunStateChanged(bool stopped)
		{
			if (InvokeRequired)
			{
				Invoke((MethodInvoker) delegate{OnRunStateChanged(stopped);});
				return;
			}
			if (stopped)
			{
				lablRunning.Text = "Stopped";
				mitmContinue.Enabled = true;
				butnContinue.Enabled = true;
				mitmBreak.Enabled = false;
				butnBreak.Enabled = false;
				mitmConnect.Enabled = true;
				butnConnect.Enabled = true;
				mitmStepInto.Enabled = true;
				mitmStepOver.Enabled = true;
				Windows.UpdateAllWindows();
			}
			else
			{
				lablRunning.Text = "Running";
				mitmContinue.Enabled = false;
				butnContinue.Enabled = false;
				mitmBreak.Enabled = true;
				butnBreak.Enabled = true;
				mitmConnect.Enabled = false;
				butnConnect.Enabled = false;
				mitmStepInto.Enabled = false;
				mitmStepOver.Enabled = false;
			}
		}

        private void mitmConnect_Click(object sender, EventArgs e) {
			if (!mitmConnect.Enabled)
				return;
			if (mCreated && Global.GDB.Connected)
			{
				Global.GDB.Disconnect();
			}
			else
			{
				mConnectRetry = 1;
				Connect();
			}
        }

        private void mitmRefresh_Click(object sender, EventArgs e) {
            Windows.UpdateAllWindows();
        }

        private void mitmContinue_Click(object sender, EventArgs e) {
            Global.GDB.SendCmd("continue&");
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
			mitmContinue.Enabled = false;
			butnContinue.Enabled = false;
			mitmBreak.Enabled = false;
			butnBreak.Enabled = false;
			mitmStepInto.Enabled = false;
			mitmStepOver.Enabled = false;
			mitmRefresh.Enabled = false;

			if (Settings.OutputPath == null)
			{
				mitmSave.Enabled = false;
			}


            // Dont put this in load. Load happens in main call from Main.cs and on exceptions just
            // goes out, no message.
            // Also we want to show other forms after main form, not before.
            // We also only want to run this once, not on each possible show.
            if (mitmConnect.Enabled) {
                if (Settings.AutoConnect) {
					mConnectRetry = 1;
                    Connect();
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

        private void mitmWindowsToForeground_Click(object sender, EventArgs e) {
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
            Settings.Save();
        }

        private void mitemDisassemblyAddBreakpoint_Click(object sender, EventArgs e) {
			if (lboxDisassemble.SelectedIndices.Count == 0)
				return;
            var x = Global.AsmSource.Lines[lboxDisassemble.SelectedIndices[0]];
            if (x.Address  != 0) {
                Windows.mBreakpointsForm.AddBreakpoint("*0x" + x.Address.ToString("X8"));
            }
        }

        private void mitmCopyToClipboard_Click(object sender, EventArgs e) {
            if (lboxDisassemble.SelectedIndices.Count == 0)
				return;
            var x = Global.AsmSource.Lines[lboxDisassemble.SelectedIndices[0]];
            Clipboard.SetText(x.ToString());
        }

        private void butnBreakpoints_Click(object sender, EventArgs e) {
            mitmViewBreakpoints.PerformClick();
        }

		private void textCurrentFunction_TextChanged(object sender, EventArgs e) {
			base.OnTextChanged(e);

			using (Graphics g = textCurrentFunction.CreateGraphics()) {
				SizeF size = g.MeasureString(textCurrentFunction.Text, textCurrentFunction.Font);
				textCurrentFunction.Width = (int)size.Width + textCurrentFunction.Padding.Horizontal;
			}
		}

		private void mitmBreak_Click(object sender, EventArgs e) {
			Global.GDB.SendCmd("-exec-interrupt");
		}

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mCreated && Global.GDB.Connected)
			{
				Global.GDB.Disconnect();
				Global.GDB.SendCmd("quit");
			}
		}
    }
}