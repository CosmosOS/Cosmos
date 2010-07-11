using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
                        xLine = Regex.Unescape(xLine.Substring(1, xLine.Length - 2));
                    }
                    lboxDebug.Items.Add(xLine);
                    xResult.Add(xLine);
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

        private void butnConnect_Click(object sender, EventArgs e) {
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
            SendCmd("file CosmosKernel.obj");
            SendCmd("target remote :1234");
            SendCmd("set disassembly-flavor intel");
            SendCmd("break Kernel_Start");
            SendCmd("continue");
            SendCmd("disassemble");
            SendCmd("quit");
        }
    }
}
