using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Cosmos.Debug.GDB {
    public class GDB {
        static protected System.Diagnostics.Process mGDBProcess;
        static protected System.IO.StreamReader mGDB;

        static public string Unescape(string aInput) {
            // Remove surrounding ", /n, then unescape and trim
            return Regex.Unescape(aInput.Substring(1, aInput.Length - 2).Replace('\n', ' ').Trim());
        }

        static public  List<string> GetResponse() {
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
                    // ^ done
                    xLine = xLine.Remove(0, 1);
                    if ((xType == '~') || (xType == '&')) {
                        xLine = Unescape(xLine);
                    }
                    Windows.mLogForm.Log(xType + xLine);
                    if (xType == '~') {
                        xResult.Add(xLine);
                    }
                }
            }

            Windows.mLogForm.Log("-----");
            return xResult;
        }

        static public List<string> SendCmd(string aCmd) {
            mGDBProcess.StandardInput.WriteLine(aCmd);
            return GetResponse();
        }

        //TODO: Make path dynamic
        static protected string mCosmosPath = @"m:\source\Cosmos\";

        static public void Connect(bool aRetry) {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = mCosmosPath+ @"Build\Tools\gdb.exe";
            xStartInfo.Arguments = @"--interpreter=mi2";
            //TODO: Make path dynamic
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

            SendCmd("target remote :8832");
            //&target remote :8832
            //&:8832: No connection could be made because the target machine actively refused it.
            //^error,msg=":8832: No connection could be made because the target machine actively refused it."
            
            //&target remote :8832
            //~Remote debugging using :8832
            //~[New Thread 1]
            //~0x000ffff0 in ?? ()
            //^done

            SendCmd("set architecture i386");
            SendCmd("set language asm");
            SendCmd("set disassembly-flavor intel");
            SendCmd("break Kernel_Start");
            SendCmd("continue");
            SendCmd("delete 1");
        }

    }
}
