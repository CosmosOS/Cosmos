using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Reflection;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;
using System.IO;
using Cosmos.Build.Common;
using Microsoft.Win32;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild
{
    public class IL2CPU : AppDomainIsolatedTask
    {
        protected IL2CPUTask mTask = new IL2CPUTask();

        [Required]
        public string DebugMode
        {
            get;
            set;
        }

        public string TraceAssemblies
        {
            get;
            set;
        }

        public byte DebugCom
        {
            get;
            set;
        }

        [Required]
        public bool UseNAsm
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] References
        {
            get;
            set;
        }

        [Required]
        public string OutputFilename
        {
            get;
            set;
        }

        public bool EnableLogging
        {
            get;
            set;
        }

        public bool EmitDebugSymbols
        {
            get;
            set;
        }

        protected void LogMessage(string aMsg) {
            Log.LogMessage(aMsg);
        }

        protected void LogError(string aMsg) {
            Log.LogMessage(aMsg);
        }

        protected void LogException(Exception e) {
            Log.LogErrorFromException(e, true);
        }

        public override bool Execute() {
            var xSW = Stopwatch.StartNew();
            try
            {
                mTask.OnLogMessage = LogMessage;
                mTask.OnLogError = LogError;
                mTask.OnLogException = LogException;

                mTask.DebugMode = DebugMode;
                mTask.TraceAssemblies = TraceAssemblies;
                mTask.DebugCom = DebugCom;
                mTask.UseNAsm = UseNAsm;
                mTask.References = References;
                mTask.OutputFilename = OutputFilename;
                mTask.EnableLogging = EnableLogging;
                mTask.EmitDebugSymbols = EmitDebugSymbols;
                return mTask.Execute();
            }
            finally
            {
                xSW.Stop();
                Log.LogWarning("IL2CPU task took {0}", xSW.Elapsed);
            }
        }

    }
}