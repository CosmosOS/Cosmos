using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

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

        protected System.Diagnostics.Process mGDBProcess;
        protected Action<Response> mOnResponse;
        protected List<string> mBuffer = new List<string>();

        // DO  NOT change to sync reads.. with process output there are SERIOUS bugs in the StreamReader..
        // Unforunately the .NET implementation when no more data exists
        // sticks forever and there seems to be no way to abort it including closing the stream.
        // StreamReader in general has other issues on non seekable streams as well and accounts for why even our
        // implementation looks poor in places.
        void mGDBProcess_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            string xData = e.Data.Trim();
            if (xData == "(gdb)") {
                ProcessResponse();
            } else {
                mBuffer.Add(xData);
            }
        }

        protected void ProcessResponse() {
            var xResponse = new Response();
            
            foreach (string xLine in mBuffer) {
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

                string sData = Unescape(xLine.Substring(1));
                if (xType == '&') {
                    xResponse.Command = sData;
                } else if (xType == '^') {
                    xResponse.Error = sData != "done";
                } else if (xType == '~') {
                    xResponse.Text.Add(Unescape(sData));
                }
            }

            mBuffer.Clear();
            Windows.mMainForm.Invoke(mOnResponse, new object[] { xResponse });
        }

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

        public void SendCmd(string aCmd) {
            mGDBProcess.StandardInput.WriteLine(aCmd);
        }

        protected bool mConnected = false;
        public bool Connected {
            get { return mConnected; }
        }

        //TODO: Make path dynamic
        protected string mCosmosPath = @"m:\source\Cosmos\";
        //static protected string mCosmosPath = @"c:\Data\sources\Cosmos\il2cpu\";

        public GDB(int aRetry, Action<Response> aOnResponse) {
            mOnResponse = aOnResponse;

            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = mCosmosPath+ @"Build\Tools\gdb.exe";
            xStartInfo.Arguments = @"--interpreter=mi2";
            xStartInfo.WorkingDirectory = mCosmosPath + @"source2\Users\Kudzu\Breakpoints\bin\debug";
            xStartInfo.CreateNoWindow = true;
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardInput = true;
            xStartInfo.RedirectStandardOutput = true;
            mGDBProcess = System.Diagnostics.Process.Start(xStartInfo);
            mGDBProcess.StandardInput.AutoFlush = true;

            mGDBProcess.OutputDataReceived += new DataReceivedEventHandler(mGDBProcess_OutputDataReceived);
            mGDBProcess.BeginOutputReadLine();

            SendCmd("symbol-file Breakpoints.obj");
            SendCmd("target remote :8832");

            //while (!mConnected) {
            //    var x = SendCmd("target remote :8832");
            //    mConnected = !x.Error;
            //    aRetry--;
            //    if (aRetry == 0) {
            //        return;
            //    }

            //    System.Threading.Thread.Sleep(1000);
            //    System.Windows.Forms.Application.DoEvents();
            //}

            SendCmd("set architecture i386");
            SendCmd("set language asm");
            SendCmd("set disassembly-flavor intel");
            SendCmd("break Kernel_Start");
            SendCmd("continue");
            SendCmd("delete 1");
        }

    }
}
