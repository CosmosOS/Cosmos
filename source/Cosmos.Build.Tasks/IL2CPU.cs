using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks
{
    public class IL2CPU : ToolTask
    {
        #region Properties

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

        #endregion

        protected override string ToolName => "IL2CPU.exe";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        protected override string GenerateFullPathToTool() => Path.Combine(CosmosBuildDir, @"IL2CPU\IL2CPU.exe");

        protected override string GenerateCommandLineCommands()
        {
            Dictionary<string, string> args = new Dictionary<string, string>
            {
                {"KernelPkg", KernelPkg},
                {"EnableDebug", DebugEnabled.ToString()},
                {"EnableStackCorruptionDetection", StackCorruptionDetectionEnabled.ToString()},
                {"StackCorruptionDetectionLevel", StackCorruptionDetectionLevel},
                {"DebugMode", DebugMode},
                {"TraceAssemblies", TraceAssemblies},
                {"DebugCom", DebugCom.ToString()},
                {"OutputFilename", Path.GetFullPath(OutputFilename)},
                {"EnableLogging", EnableLogging.ToString()},
                {"EmitDebugSymbols", EmitDebugSymbols.ToString()},
                {"IgnoreDebugStubAttribute", IgnoreDebugStubAttribute.ToString()}
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

            // replace \" by \\"
            Arguments = Arguments.Replace("\\\"", "\\\\\"");

            Log.LogMessage(MessageImportance.High, $"Invoking IL2CPU.exe {Arguments}");

            return Arguments;
        }
    }
}
