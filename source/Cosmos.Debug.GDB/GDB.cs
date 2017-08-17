using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using Registry = Microsoft.Win32.Registry;
using Path = System.IO.Path;
using System.Runtime.InteropServices;

namespace Cosmos.Debug.GDB {
    public class GDB {
        public class Response {
            public string Command = string.Empty;
            public string Reply = string.Empty;
            public bool Error = false;
            public string ErrorMsg = string.Empty;
            public List<string> Text = new List<string>();

            public override string ToString() {
                return Command;
            }
        }

        protected Queue<string> mLastCmd = new Queue<string>();
        public System.Diagnostics.Process mGDBProcess;
        protected Action<Response> mOnResponse;
        protected Action<bool> mOnRunStateChange;
        protected List<string> mBuffer = new List<string>();

        // DO  NOT change to sync reads.. with process output there are SERIOUS bugs in the StreamReader..
        // Unforunately the .NET implementation when no more data exists
        // sticks forever and there seems to be no way to abort it including closing the stream.
        // StreamReader in general has other issues on non seekable streams as well and accounts for why even our
        // implementation looks poor in places.
        void mGDBProcess_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            all.Add(e.Data);
            if (e.Data == null)
                return;
            ProcessResponse(e.Data);
        }

        bool firstRun = true;

        List<string> all = new List<string>();
        protected bool m_IsStopped = true;

        void OnRunStateChanged(bool stopped)
        {
            m_IsStopped = stopped;
            mOnRunStateChange(stopped);
        }

        protected void ProcessResponse(string line) {
            switch (line)
            {
                case "(gdb) ":
                    if (firstRun)
                    {
                        // remove start things, like versions etc.
                        mBuffer.Clear();
                        firstRun = false;
                        return;
                    }
                    else if (mBuffer.Count == 0)
                    {
                        // occures if CTRL + C the program breaks
                        return;
                    }
                    break;
                default:
                    // only 7.x
                    if (line[0] == '*')
                    {	// represents start or stopped
                        int xIndex = line.IndexOf(',');
                        string xState = line.Substring(1, xIndex - 1);
                        switch (xState)
                        {
                            case "stopped":
                                OnRunStateChanged(true);
                                break;
                            case "running":
                                OnRunStateChanged(false);
                                break;
                        }
                    }
                    else
                        mBuffer.Add(line);
                    return;
            }
            var xResponse = new Response();

            string xCmd = mBuffer[0];
            if (xCmd[0] == '&')
            {
                xResponse.Command = Unescape(xCmd.Substring(1));
            }

            foreach (string xLine in mBuffer) {
                var xType = xLine[0];
                // & echo of a command or reply
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
                    if (xResponse.Reply.Length == 0)
                        xResponse.Reply = sData;
                    else
                        xResponse.Reply += Environment.NewLine + sData;
                } else if (xType == '~') {
                    xResponse.Text.Add(Unescape(sData));
                }
                else if (xType == '^') {
                    xResponse.Error = sData.StartsWith("error");
                }
            }

            mBuffer.Clear();

            // detect manual input of cmds
            var xSplit = xResponse.Command.Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            if (xSplit.Length == 1 && xSplit[0] == "detach")
            {
                if (false == xResponse.Error)
                    mConnected = false;
            }
            else if (xSplit.Length > 2 && xSplit[0] == "target" && xSplit[1] == "remote")
            {
                TargetCmdReply(xResponse);
            }
            try
            {
                Windows.mMainForm.Invoke(mOnResponse, new object[] { xResponse });
            }
            catch (ObjectDisposedException)
            { // check the propertie IsDisposed didnt solve the problem, so we catch
            }
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
            all.Add("Cmd: " + aCmd);
            mGDBProcess.StandardInput.WriteLine(aCmd);
        }

        protected bool mConnected = false;
        public bool Connected {
            get { return mConnected; }
        }

        public bool Stopped
        {
            get { return m_IsStopped; }
        }

        static readonly string mGDBExePath;

        static GDB()
        {
       mGDBExePath = Path.Combine(Cosmos.Build.Common.CosmosPaths.Tools, "gdb.exe");
        }

        public GDB(Action<Response> aOnResponse, Action<bool> aOnRunStateChange)
        {
            mOnResponse = aOnResponse;
            mOnRunStateChange = aOnRunStateChange;

            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = mGDBExePath;
            xStartInfo.Arguments = @"--interpreter=mi2 -silent -nx";
            xStartInfo.WorkingDirectory = Settings.OutputPath;
            xStartInfo.CreateNoWindow = true;
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = true;
            xStartInfo.RedirectStandardInput = true;
            xStartInfo.RedirectStandardOutput = true;

            mGDBProcess = System.Diagnostics.Process.Start(xStartInfo);

            mGDBProcess.OutputDataReceived += new DataReceivedEventHandler(mGDBProcess_OutputDataReceived);
            mGDBProcess.BeginOutputReadLine();

            SendCmd("set target-async 1"); // doc http://sourceware.org/gdb/onlinedocs/gdb/Asynchronous-and-non_002dstop-modes.html
            SendCmd("add-symbol-file " + Settings.ObjFile + " 0x02000000");
            SendCmd("set architecture i386");
            SendCmd("set language asm");
            SendCmd("set disassembly-flavor intel");
        }

        public void Connect()
        {
            SendCmd("target remote :8832");
        }

        public void Disconnect()
        {
            SendCmd("detach");
        }

        protected void TargetCmdReply(Response res)
        {
            mConnected = !res.Error;
            if (false == res.Error)
            {
                // get current address
                var xAddr = Global.FromHexWithLeadingZeroX(res.Text[1].Substring(0, res.Text[1].IndexOf(' ')));
                // only use breakpoint if not already deep in OS
                const uint START_ADDRESS = 0xFFFFFFF0;
                if (xAddr == START_ADDRESS)
                {
                    SendCmd("break Kernel_Start");
                    SendCmd("continue");
                    SendCmd("delete 1");
                }
            }
        }
    }
}
