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
  public class IL2CPU : BaseToolTask
  {
    // protected CompilerEngine mTask = new CompilerEngine();

    [Required]
    public string CosmosBuildDir
    {
      get;
      set;
    }

    [Required]
    public string WorkingDir
    {
      get;
      set;
    }

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
        Dictionary<string,string> args = new Dictionary<string, string>();
        args.Add("DebugEnabled", Convert.ToString(DebugEnabled));
        args.Add("StackCorruptionDetectionEnabled", Convert.ToString(StackCorruptionDetectionEnabled));
        args.Add("DebugMode", Convert.ToString(DebugMode));
        args.Add("TraceAssemblies", Convert.ToString(TraceAssemblies));
        args.Add("DebugCom", Convert.ToString(DebugCom));
        args.Add("UseNAsm", Convert.ToString(UseNAsm));

        List<string> refs = new List<string>();
        foreach (var reference in References)
        {
          if (reference.MetadataNames.OfType<string>().Contains("FullPath"))
          {
            string xFile = reference.GetMetadata("FullPath");
            refs.Add(Convert.ToString(xFile));
          }
        }

        args.Add("OutputFilename", Convert.ToString(OutputFilename));
        args.Add("EnableLogging", Convert.ToString(EnableLogging));
        args.Add("EmitDebugSymbols", Convert.ToString(EmitDebugSymbols));
        args.Add("IgnoreDebugStubAttribute", Convert.ToString(IgnoreDebugStubAttribute));

        string Arguments = "";
        foreach (var arg in args)
        {
          Arguments += "\"" + arg.Key + ":" + arg.Value + "\" ";
        }
        foreach (var Ref in refs)
        {
          Arguments += "\"References:" + Ref + "\" ";
        }
        return base.ExecuteTool(WorkingDir,
                  Path.Combine(CosmosBuildDir, @"IL2CPU\IL2CPU.exe"),
                  Arguments,
                  "IL2CPU");
      } finally {
        xSW.Stop();
        Log.LogMessage(MessageImportance.High,
         string.Format("IL2CPU invoked with DebugMode='{0}', DebugEnabled='{1}', TraceAssemblies='{2}', IgnoreDebugStub='{3}'",
           DebugMode, DebugEnabled, TraceAssemblies ?? "{NULL}", IgnoreDebugStubAttribute
         ));

        Log.LogMessage(MessageImportance.High, "IL2CPU task took {0}", xSW.Elapsed);
      }
    }

		public override bool ExtendLineError(int exitCode, string errorMessage, out LogInfo log)
		{
			log = new LogInfo();
			log.logType = WriteType.Error;
			log.message = errorMessage;
			return true;
		}
  }
}
