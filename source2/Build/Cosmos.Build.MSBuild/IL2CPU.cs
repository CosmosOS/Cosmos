using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Reflection;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using System.IO;
using Cosmos.Build.Common;
using Microsoft.Win32;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Cosmos.Build.MSBuild {
  public class IL2CPU : AppDomainIsolatedTask {
    protected IL2CPUTask mTask = new IL2CPUTask();

    [Required]
    public string DebugMode {
      get;
      set;
    }

    public bool DebugEnabled {
      get;
      set;
    }

    public bool StackCorruptionDetectionEnabled
    {
      get; 
      set;
    }

    public string TraceAssemblies {
      get;
      set;
    }

    public bool IgnoreDebugStubAttribute {
      get;
      set;
    }

    public byte DebugCom {
      get;
      set;
    }

    [Required]
    public bool UseNAsm {
      get;
      set;
    }

    [Required]
    public ITaskItem[] References {
      get;
      set;
    }

    [Required]
    public string OutputFilename {
      get;
      set;
    }

    public bool EnableLogging {
      get;
      set;
    }

    public bool EmitDebugSymbols {
      get;
      set;
    }

    protected void LogMessage(string aMsg) {
      Log.LogMessage(aMsg);
    }

    protected void LogInformation(string aMsg) {
      Log.LogMessage(MessageImportance.High, aMsg);
    }

    protected void LogWarning(string aMsg) {
      Log.LogWarning(aMsg);
    }

    protected void LogError(string aMsg) {
      Log.LogError(aMsg);
    }

    protected void LogException(Exception e) {
      Log.LogErrorFromException(e, true);
    }

    public override bool Execute() {
      var xSW = Stopwatch.StartNew();

      try {
        mTask.OnLogMessage = LogMessage;
        mTask.OnLogError = LogError;
        mTask.OnLogWarning = LogWarning;
        mTask.OnLogException = LogException;

        mTask.DebugEnabled = DebugEnabled;
        mTask.StackCorruptionDetectionEnabled = StackCorruptionDetectionEnabled;
        mTask.DebugMode = DebugMode;
        mTask.TraceAssemblies = TraceAssemblies;
        mTask.DebugCom = DebugCom;
        mTask.UseNAsm = UseNAsm;
        mTask.References = References;
        mTask.OutputFilename = OutputFilename;
        mTask.EnableLogging = EnableLogging;
        mTask.EmitDebugSymbols = EmitDebugSymbols;
        mTask.IgnoreDebugStubAttribute = IgnoreDebugStubAttribute;
        Log.LogMessage(MessageImportance.High,
          string.Format("IL2CPU invoked with DebugMode='{0}', DebugEnabled='{1}', TraceAssemblies='{2}', IgnoreDebugStub='{3}'",
            DebugMode, DebugEnabled, TraceAssemblies ?? "{NULL}", IgnoreDebugStubAttribute
          ));
        return mTask.Execute();
      } finally {
        xSW.Stop();
        Log.LogMessage(MessageImportance.High, "IL2CPU task took {0}", xSW.Elapsed);
      }
    }
  }
}