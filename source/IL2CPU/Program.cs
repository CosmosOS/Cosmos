using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU;

namespace IL2CPU
{
    public class Program
    {

        public const string CosmosRoot = "";
        private const string KernelFile = CosmosRoot + "";
        private const string OutputFile = CosmosRoot + "";
        private static Dictionary<string, string> CmdOptions = new Dictionary<string, string>();
        private static List<string> References = new List<string>();

        public static int Main(string[] args)
        {
            return Run(args, Console.WriteLine, s =>
            {
                Console.Write("Error: ");
                Console.WriteLine(s);
            });
        }

        public static int Run(string[] args, Action<string> logMessage, Action<string> logError)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            if (logMessage == null)
            {
                throw new ArgumentNullException("logMessage");
            }
            if (logError == null)
            {
                throw new ArgumentNullException("logError");
            }

            try
            {
                var tmp = "";
                foreach (var s in args)
                {

                    tmp += s;
                    string[] s1 = s.Split(':');
                    string argID = s1[0].ToLower();
                    if (argID != "References".ToLower())
                    {
                        CmdOptions.Add(argID, s.Replace(s1[0] + ":", ""));
                    }
                    else
                    {

                        References.Add(s.Replace(s1[0] + ":", ""));
                    }

                }

                var xTask = new CompilerEngine();
                xTask.DebugEnabled = Convert.ToBoolean(CmdOptions["DebugEnabled".ToLower()]);
                logMessage("Loaded : DebugEnabled");
                xTask.StackCorruptionDetectionEnabled = Convert.ToBoolean(CmdOptions["StackCorruptionDetectionEnabled".ToLower()]);
                logMessage("Loaded : StackCorruptionDetectionEnabled");
                xTask.DebugMode = CmdOptions["DebugMode".ToLower()];
                logMessage("Loaded : DebugMode");
                xTask.StackCorruptionDetectionLevel = CmdOptions["StackCorruptionDetectionLevel".ToLower()];
                logMessage("Loaded : StackCorruptionDetectionLevel");
                xTask.TraceAssemblies = CmdOptions["TraceAssemblies".ToLower()];
                logMessage("Loaded : TraceAssemblies");
                xTask.DebugCom = Convert.ToByte(CmdOptions["DebugCom".ToLower()]);
                logMessage("Loaded : DebugCom");
                xTask.UseNAsm = Convert.ToBoolean(CmdOptions["UseNAsm".ToLower()]);
                logMessage("Loaded : UseNAsm");
                xTask.OutputFilename = CmdOptions["OutputFilename".ToLower()];
                logMessage("Loaded : OutputFilename");
                xTask.EnableLogging = Convert.ToBoolean(CmdOptions["EnableLogging".ToLower()]);
                logMessage("Loaded : EnableLogging");
                xTask.EmitDebugSymbols = Convert.ToBoolean(CmdOptions["EmitDebugSymbols".ToLower()]);
                logMessage("Loaded : EmitDebugSymbols");
                xTask.IgnoreDebugStubAttribute = Convert.ToBoolean(CmdOptions["IgnoreDebugStubAttribute".ToLower()]);
                logMessage("Loaded : IgnoreDebugStubAttribute");
                xTask.References = References.ToArray();
                logMessage("Loaded : References");

                xTask.OnLogError = logError;
                xTask.OnLogWarning = m => logMessage(String.Format("Warning: {0}", m));
                xTask.OnLogMessage = logMessage;
                xTask.OnLogException = (m) => logError(String.Format("Exception: {0}", m.ToString()));
                xTask.AssemblerLog = "Cosmos.Assembler.log";
                if (xTask.Execute())
                {
                    logMessage("Executed OK");
                    //          File.WriteAllText(@"e:\compiler.log", "OK");
                    return 0;
                }
                else
                {
                    logMessage("Errorred");
                    //          File.WriteAllText(@"e:\compiler.log", "Errored");
                    return 2;
                }
            }
            catch (Exception E)
            {
                logError(String.Format("Error occurred: " + E.ToString()));
                return 1;
            }
        }
    }
}
