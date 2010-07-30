using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cosmos.Debug.GDB {
    public class GDB {
        public class Response {
            public string Command = "";
            public bool Error = false;
            public string ErrorMsg = "";
            public List<string> Text = new List<string>();

            public override string ToString() {
                return Command;
            }
        }

        static protected System.Diagnostics.Process mGDBProcess;
        static protected System.IO.StreamReader mGDB;

        static public string Unescape(string aInput) {
            // Remove surrounding ", /n, then unescape and trim
            string xResult = aInput;
            if (xResult.StartsWith("\"")) {
                xResult = xResult.Substring(1, aInput.Length - 2);
                xResult = xResult.Replace('\n', ' ');
                xResult = Regex.Unescape(xResult);
            }
            return xResult.Trim();
        }

        static public Response GetResponse() {
            var xResult = new Response();

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

                    //&target remote :8832
                    //&:8832: No connection could be made because the target machine actively refused it.
                    //^error,msg=":8832: No connection could be made because the target machine actively refused it."

                    //&target remote :8832
                    //~Remote debugging using :8832
                    //~[New Thread 1]
                    //~0x000ffff0 in ?? ()
                    //^done

                    xLine = Unescape(xLine.Substring(1));
                    if (xType == '&') {
                    } else if (xType == '^') {
                        xResult.Error = xLine != "done";
                    } else if (xType == '~') {
                        xResult.Text.Add(Unescape(xLine));
                    }
                }
            }

            return xResult;
        }

        static public Response SendCmd(string aCmd) {
            mGDBProcess.StandardInput.WriteLine(aCmd);
            var xResult = GetResponse();
            xResult.Command = aCmd;
            Windows.mLogForm.Log(xResult);
            return xResult;
        }

        static protected bool mConnected = false;
        static public bool Connected {
            get { return mConnected; }
        }

        //TODO: Make path dynamic
        static protected string mCosmosPath = @"m:\source\Cosmos\";
        //static protected string mCosmosPath = @"c:\Data\sources\Cosmos\il2cpu\";

        static public void Connect(int aRetry) {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = mCosmosPath+ @"Build\Tools\gdb.exe";
            xStartInfo.Arguments = @"--interpreter=mi2";
            xStartInfo.WorkingDirectory = mCosmosPath + @"source2\Users\Kudzu\Breakpoints\bin\debug";
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

            while (!mConnected) {
                var x = SendCmd("target remote :8832");
                mConnected = !x.Error;
                aRetry--;
                if (aRetry == 0) {
                    return;
                }

                System.Threading.Thread.Sleep(1000);
                System.Windows.Forms.Application.DoEvents();
            }

            SendCmd("set architecture i386");
            SendCmd("set language asm");
            SendCmd("set disassembly-flavor intel");
            SendCmd("break Kernel_Start");
            SendCmd("continue");
            SendCmd("delete 1");
        }

    }
}
