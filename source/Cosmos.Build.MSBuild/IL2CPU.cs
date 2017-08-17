using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace Cosmos.Build.MSBuild
{
    public class IL2CPU : BaseToolTask
    {
        public string KernelPkg { get; set; }

        [Required]
        public string CosmosBuildDir { get; set; }

        [Required]
        public string WorkingDir { get; set; }

        [Required]
        public string DebugMode { get; set; }

        public bool DebugEnabled { get; set; }

        public bool StackCorruptionDetectionEnabled { get; set; }

        public string StackCorruptionDetectionLevel { get; set; }

        public string TraceAssemblies { get; set; }

        public bool IgnoreDebugStubAttribute { get; set; }

        public byte DebugCom { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public string OutputFilename { get; set; }

        public bool EnableLogging { get; set; }

        public bool EmitDebugSymbols { get; set; }

        public string AssemblySearchDirs { get; set; }

        protected void LogMessage(string aMsg)
        {
            Log.LogMessage(aMsg);
        }

        protected void LogInformation(string aMsg)
        {
            Log.LogMessage(MessageImportance.High, aMsg);
        }

        protected void LogWarning(string aMsg)
        {
            Log.LogWarning(aMsg);
        }

        protected void LogError(string aMsg)
        {
            Log.LogError(aMsg);
        }

        protected void LogException(Exception e)
        {
            Log.LogErrorFromException(e, true);
        }

        public override bool Execute()
        {
            var xSW = Stopwatch.StartNew();

            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    {"KernelPkg", Convert.ToString(KernelPkg)},
                    {"DebugEnabled", Convert.ToString(DebugEnabled)},
                    {"StackCorruptionDetectionEnabled", Convert.ToString(StackCorruptionDetectionEnabled)},
                    {"StackCorruptionDetectionLevel", Convert.ToString(StackCorruptionDetectionLevel)},
                    {"DebugMode", Convert.ToString(DebugMode)},
                    {"TraceAssemblies", Convert.ToString(TraceAssemblies)},
                    {"DebugCom", Convert.ToString(DebugCom)},
                    {"OutputFilename", Convert.ToString(OutputFilename)},
                    {"EnableLogging", Convert.ToString(EnableLogging)},
                    {"EmitDebugSymbols", Convert.ToString(EmitDebugSymbols)},
                    {"IgnoreDebugStubAttribute", Convert.ToString(IgnoreDebugStubAttribute)}
                };

                List<string> refs =
                    (from reference in References
                     where reference.MetadataNames.OfType<string>().Contains("FullPath")
                     select reference.GetMetadata("FullPath")
                        into xFile
                     select Convert.ToString(xFile)).ToList();

                string Arguments = args.Aggregate("", (current, arg) => current + "\"" + arg.Key + ":" + arg.Value + "\" ");
                Arguments = refs.Aggregate(Arguments, (current, Ref) => current + "\"References:" + Ref + "\" ");
                Arguments = AssemblySearchDirs.Split(';').Aggregate(Arguments, (current, Dir) => current + "\"AssemblySearchDirs:" + Dir + "\" ");

                Log.LogMessage(MessageImportance.High, $"Invoking il2cpu.exe {Arguments}");
                return ExecuteTool(WorkingDir, Path.Combine(CosmosBuildDir, @"IL2CPU\IL2CPU.exe"), Arguments, "IL2CPU");
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, $"IL2CPU invoked with DebugMode='{DebugMode}', DebugEnabled='{DebugEnabled}',StackCorruptionDetectionLevel='{StackCorruptionDetectionLevel ?? "{NULL}"}', TraceAssemblies='{TraceAssemblies ?? "{NULL}"}', IgnoreDebugStub='{IgnoreDebugStubAttribute}'");
                Log.LogMessage(MessageImportance.High, "IL2CPU task took {0}", xSW.Elapsed);
            }
        }

        public override bool ExtendLineError(bool hasErrored, string errorMessage, out LogInfo log)
        {
            log = new LogInfo
            {
                logType = WriteType.Error,
                message = errorMessage
            };
            return true;
        }
    }
}
