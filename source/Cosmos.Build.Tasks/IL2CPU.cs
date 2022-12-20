using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

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
        public string TargetAssembly { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public ITaskItem[] PlugsReferences { get; set; }

        [Required]
        public string OutputFilename { get; set; }

        public bool EnableLogging { get; set; }

        public bool EmitDebugSymbols { get; set; }

        public bool CompileVBEMultiboot { get; set; }

        public bool RemoveBootDebugOutput { get; set; }

        public bool AllowComments { get; set; }

        public string VBEResolution { get; set; }

        #endregion

        protected override string ToolName => IsWindows() ? "IL2CPU.exe" : "IL2CPU";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        public IL2CPU()
        {
            CompileVBEMultiboot = false;
            AllowComments = false;
            VBEResolution = "800x600x32";
        }

        protected override string GenerateFullPathToTool()
        {
            if (String.IsNullOrWhiteSpace(ToolPath))
            {
                return Path.Combine(CosmosBuildDir, IsWindows() ? @"IL2CPU\IL2CPU.exe" : "IL2CPU/IL2CPU");
            }

            return Path.Combine(Path.GetFullPath(ToolPath), ToolExe);
        }

        protected override string GenerateResponseFileCommands()
        {
            var args = new Dictionary<string, string>
            {
                ["KernelPkg"] = KernelPkg,
                ["EnableDebug"] = DebugEnabled.ToString(),
                ["EnableStackCorruptionDetection"] = StackCorruptionDetectionEnabled.ToString(),
                ["StackCorruptionDetectionLevel"] = StackCorruptionDetectionLevel,
                ["DebugMode"] = DebugMode,
                ["TraceAssemblies"] = TraceAssemblies,
                ["DebugCom"] = DebugCom.ToString(),
                ["TargetAssembly"] = Path.GetFullPath(TargetAssembly),
                ["OutputFilename"] = Path.GetFullPath(OutputFilename),
                ["EnableLogging"] = EnableLogging.ToString(),
                ["EmitDebugSymbols"] = EmitDebugSymbols.ToString(),
                ["IgnoreDebugStubAttribute"] = IgnoreDebugStubAttribute.ToString(),
                ["CompileVBEMultiboot"] = CompileVBEMultiboot.ToString(),
                ["VBEResolution"] = VBEResolution.ToString(),
                ["RemoveBootDebugOutput"] = RemoveBootDebugOutput.ToString(),
                ["AllowComments"] = AllowComments.ToString()
            }.ToList();

            foreach (var reference in References)
            {
                args.Add(new KeyValuePair<string, string>("References", reference.ItemSpec));
            }

            foreach (var plugsReference in PlugsReferences)
            {
                args.Add(new KeyValuePair<string, string>("PlugsReferences", plugsReference.ItemSpec));
            }

            return String.Join(Environment.NewLine, args.Select(a => $"{a.Key}:{a.Value}"));
        }

        protected override string GetResponseFileSwitch(string responseFilePath) => $"\"ResponseFile:{responseFilePath}\"";

        public override bool Execute()
        {
            var xSW = Stopwatch.StartNew();
            try
            {
                return base.Execute();
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "IL2CPU task took {0}", xSW.Elapsed);
            }
        }
    }
}
