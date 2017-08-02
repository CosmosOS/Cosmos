using System;
using System.Collections.Generic;

namespace Cosmos.IL2CPU {
    public class Program {
        public const string CosmosRoot = "";
        private const string KernelFile = CosmosRoot + "";
        private const string OutputFile = CosmosRoot + "";
        private static Dictionary<string, string> CmdOptions = new Dictionary<string, string>();
        private static List<string> References = new List<string>();
        private static List<string> AdditionalReferences = new List<string>();
        private static List<string> AssemblySearchDirs = new List<string>();

        public static int Run(string[] args, Action<string> logMessage, Action<string> logError) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (logMessage == null) {
                throw new ArgumentNullException("logMessage");
            }
            if (logError == null) {
                throw new ArgumentNullException("logError");
            }

            try {
                var tmp = "";
                foreach (var s in args) {
                    tmp += s;
                    string[] s1 = s.Split(':');
                    string argID = s1[0].ToLower();
                    if (argID == "References".ToLower()) {
                        References.Add(s.Replace(s1[0] + ":", ""));
                    }
                    else if (argID == "AdditionalReferences".ToLower()) {
                        AdditionalReferences.Add(s.Replace(s1[0] + ":", ""));
                    }
                    else if (argID == "AssemblySearchDirs".ToLower()) {
                        AssemblySearchDirs.Add(s.Replace(s1[0] + ":", ""));
                    }
                    else {
                        CmdOptions.Add(argID, s.Replace(s1[0] + ":", ""));
                    }
                }

                var xEngine = new CompilerEngine();

                CompilerEngine.KernelPkg = "";
                if (CmdOptions.ContainsKey("KernelPkg".ToLower())) {
                    CompilerEngine.KernelPkg = Convert.ToString(CmdOptions["KernelPkg".ToLower()]);
                }

                xEngine.DebugEnabled = Convert.ToBoolean(CmdOptions["DebugEnabled".ToLower()]);
                logMessage("Loaded : DebugEnabled");
                xEngine.StackCorruptionDetectionEnabled =
                    Convert.ToBoolean(CmdOptions["StackCorruptionDetectionEnabled".ToLower()]);
                logMessage("Loaded : StackCorruptionDetectionEnabled");
                xEngine.DebugMode = CmdOptions["DebugMode".ToLower()];
                logMessage("Loaded : DebugMode");
                xEngine.StackCorruptionDetectionLevel = CmdOptions["StackCorruptionDetectionLevel".ToLower()];
                logMessage("Loaded : StackCorruptionDetectionLevel");
                xEngine.TraceAssemblies = CmdOptions["TraceAssemblies".ToLower()];
                logMessage("Loaded : TraceAssemblies");
                xEngine.DebugCom = Convert.ToByte(CmdOptions["DebugCom".ToLower()]);
                logMessage("Loaded : DebugCom");
                xEngine.UseNAsm = Convert.ToBoolean(CmdOptions["UseNAsm".ToLower()]);
                logMessage("Loaded : UseNAsm");
                xEngine.OutputFilename = CmdOptions["OutputFilename".ToLower()];
                logMessage("Loaded : OutputFilename");
                xEngine.EnableLogging = Convert.ToBoolean(CmdOptions["EnableLogging".ToLower()]);
                logMessage("Loaded : EnableLogging");
                xEngine.EmitDebugSymbols = Convert.ToBoolean(CmdOptions["EmitDebugSymbols".ToLower()]);
                logMessage("Loaded : EmitDebugSymbols");
                xEngine.IgnoreDebugStubAttribute = Convert.ToBoolean(CmdOptions["IgnoreDebugStubAttribute".ToLower()]);
                logMessage("Loaded : IgnoreDebugStubAttribute");
                xEngine.References = References.ToArray();
                logMessage("Loaded : References");
                xEngine.AssemblySearchDirs = AssemblySearchDirs.ToArray();
                logMessage("Loaded : AssemblySearchDirs");

                xEngine.OnLogError = logError;
                xEngine.OnLogWarning = m => logMessage(String.Format("Warning: {0}", m));
                xEngine.OnLogMessage = logMessage;
                xEngine.OnLogException = (m) => logError(String.Format("Exception: {0}", m.ToString()));
                xEngine.AssemblerLog = "Cosmos.Assembler.log";
                if (xEngine.Execute()) {
                    logMessage("Executed OK");
                    //          File.WriteAllText(@"e:\compiler.log", "OK");
                    return 0;
                }
                else {
                    logMessage("Errorred");
                    //          File.WriteAllText(@"e:\compiler.log", "Errored");
                    return 2;
                }
            }
            catch (Exception E) {
                logError(String.Format("Error occurred: " + E.ToString()));
                return 1;
            }
        }
    }
}
