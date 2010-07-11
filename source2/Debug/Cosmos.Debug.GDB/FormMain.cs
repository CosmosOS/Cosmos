using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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

            string xBuffStr = "";
            var xBuff = new char[4096];
            while (!mGDBProcess.HasExited) {
                int i = 0;
                //TODO: Cant find a better way than peek... 
                if (mGDB.Peek() > -1) {
                    i = mGDB.Read(xBuff, 0, xBuff.Length);
                    if (i > 0) {
                        //TODO: Prob use stringbuilder instead
                        xBuffStr = xBuffStr + new string(xBuff, 0, i);
                    }
                }
                if (xBuffStr.Length > -1) {
                    if (xBuffStr == "") {
//                    if (xBuffStr == "(gdb) ") {
                        // When we redirect std input, gdb no longer gives us a prompt. Just had to rewrite all this crap without readline.. now chekcing in and going back to Readline.....
                        break;
                    }
                    int j = xBuffStr.IndexOf("\n");
                    if (j > -1) {
                        var xLine = xBuffStr.Substring(0, j - 1); // Will be /r/n
                        xBuffStr = xBuffStr.Remove(0, j + 1);
                        lboxDebug.Items.Add(xLine);
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
            mGDBProcess.StandardInput.Write(aCmd + "\r");
            return GetResponse();
        }

        private void butnConnect_Click(object sender, EventArgs e) {
            var xStartInfo = new ProcessStartInfo();
            //TODO: Make path dynamic
            xStartInfo.FileName = @"D:\source\Cosmos\Build\Tools\gdb.exe";
            xStartInfo.Arguments = "";
            //TODO: Make path dynamic
            xStartInfo.WorkingDirectory = @"D:\source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\debug";
            xStartInfo.CreateNoWindow = true;
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardOutput = true;
            xStartInfo.RedirectStandardInput = true;
            mGDBProcess = System.Diagnostics.Process.Start(xStartInfo);
            mGDB = mGDBProcess.StandardOutput;

            GetResponse();
            SendCmd("file CosmosKernel.obj");
            SendCmd("target remote :1234");
            SendCmd("set disassembly-flavor intel");
        }
    }
}
