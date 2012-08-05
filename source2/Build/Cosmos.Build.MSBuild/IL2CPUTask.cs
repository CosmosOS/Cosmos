using Cosmos.Build.Common;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU;
using Microsoft.Win32;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Cosmos.Debug.Common;

namespace Cosmos.Build.MSBuild {
  // http://blogs.msdn.com/b/visualstudio/archive/2010/07/06/debugging-msbuild-script-with-visual-studio.aspx
  public class IL2CPUTask {
    const string FULLASSEMBLYNAME_KERNEL = "Cosmos.System.Kernel";

    public Action<string> OnLogMessage;
    public Action<string> OnLogError;
    public Action<string> OnLogWarning;
    public Action<Exception> OnLogException;
    protected static Action<string> mStaticLog = null;

    public string DebugMode { get; set; }
    public string TraceAssemblies { get; set; }
    public byte DebugCom { get; set; }
    public bool UseNAsm { get; set; }
    public ITaskItem[] References { get; set; }
    public string OutputFilename { get; set; }
    public bool EnableLogging { get; set; }
    public bool EmitDebugSymbols { get; set; }
    public bool IgnoreDebugStubAttribute { get; set; }

    protected void LogMessage(string aMsg) {
      if (OnLogMessage != null) {
        OnLogMessage(aMsg);
      }
    }

    protected void LogWarning(string aMsg) {
      if (OnLogWarning != null) {
        OnLogWarning(aMsg);
      }
    }

    protected void LogError(string aMsg) {
      if (OnLogError != null) {
        OnLogError(aMsg);
      }
    }

    protected void LogException(Exception e) {
      if (OnLogException != null) {
        OnLogException(e);
      }
    }

    protected static List<string> mSearchDirs = new List<string>();
    static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
      var xShortName = args.Name;
      if (xShortName.Contains(',')) {
        xShortName = xShortName.Substring(0, xShortName.IndexOf(','));
        // TODO: remove following statement if it proves unnecessary
        if (xShortName.Contains(',')) {
          throw new Exception("Algo error");
        }
      }
      foreach (var xDir in mSearchDirs) {
        var xPath = Path.Combine(xDir, xShortName + ".dll");
        if (File.Exists(xPath)) {
          return Assembly.LoadFrom(xPath);
        }
        xPath = Path.Combine(xDir, xShortName + ".exe");
        if (File.Exists(xPath)) {
          return Assembly.LoadFrom(xPath);
        }
      }
      if (mStaticLog != null) {
        mStaticLog(string.Format("Assembly '{0}' not resolved!", args.Name));
      }
      return null;
    }
    
    protected bool Initialize() {
      // Add UserKit dirs for asms to load from.
      mSearchDirs.Add(Path.GetDirectoryName(typeof(IL2CPU).Assembly.Location));
      mSearchDirs.Add(CosmosPaths.UserKit);
      mSearchDirs.Add(CosmosPaths.Kernel);
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

      // Try to load explicit path references.
      // These are the references of our boot project. We dont actually ever load the boot
      // project asm. Instead the references will contain plugs, and the kernel. We load
      // them then find the entry point in the kernel.

      // This seems to be to try to load plugs on demand from their own dirs, but 
      // it often just causes load conflicts, and weird errors like "implementation not found" 
      // for a method, even when both the output user kit dir and local bin dir have up to date
      // and same assemblies. 
      // So its removed for now and we should find a better way to dynamically load plugs in 
      // future.
      //if (References != null) {
      //  foreach (var xRef in References) {
      //    if (xRef.MetadataNames.OfType<string>().Contains("FullPath")) {
      //      var xName = xRef.GetMetadata("FullPath");
      //      var xDir = Path.GetDirectoryName(xName);
      //      if (!mSearchPaths.Contains(xDir)) {
      //        mSearchPaths.Insert(0, xDir);
      //      }
      //    }
      //  }
      //}

      mDebugMode = (DebugMode)Enum.Parse(typeof(DebugMode), DebugMode);
      if (String.IsNullOrEmpty(TraceAssemblies)) {
        mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.User;
      } else {
        if (!Enum.GetNames(typeof(TraceAssemblies)).Contains(TraceAssemblies, StringComparer.InvariantCultureIgnoreCase)) {
          LogError("Invalid TraceAssemblies specified");
          return false;
        }
        mTraceAssemblies = (TraceAssemblies)Enum.Parse(typeof(TraceAssemblies), TraceAssemblies);
      }

      return true;
    }

    public bool DebugEnabled = false;
    protected DebugMode mDebugMode = Cosmos.Build.Common.DebugMode.Source;
    protected TraceAssemblies mTraceAssemblies = Cosmos.Build.Common.TraceAssemblies.All;

    protected void LogTime(string message) {
    }

    public bool Execute() {
      //System.Diagnostics.Debugger.Launch();
      try {
        LogMessage("Executing IL2CPU on assembly");
        if (!Initialize()) {
          return false;
        }

        LogTime("Engine execute started");
        // Find the kernel's entry point. We are looking for a public class Kernel, with public static void Boot()
        var xInitMethod = RetrieveEntryPoint();
        if (xInitMethod == null) {
          return false;
        }
        var xOutputFilename = Path.Combine(Path.GetDirectoryName(OutputFilename), Path.GetFileNameWithoutExtension(OutputFilename));
        if (!DebugEnabled) {
          // Default of 1 is in Cosmos.Targets. Need to change to use proj props.
          DebugCom = 0;
        }

        var xAsm = new AppAssemblerNasm(DebugCom);
        using (var xDebugInfo = new DebugInfo(xOutputFilename + ".mdf", true)) {
          xAsm.DebugInfo = xDebugInfo;
          xAsm.DebugEnabled = DebugEnabled;
          xAsm.DebugMode = mDebugMode;
          xAsm.TraceAssemblies = mTraceAssemblies;
          xAsm.IgnoreDebugStubAttribute = IgnoreDebugStubAttribute;
          if (DebugEnabled == false) {
            xAsm.ShouldOptimize = true;
          }
          #if OUTPUT_ELF
          xAsm.EmitELF = true;
          #endif

          var xNasmAsm = (AssemblerNasm)xAsm.Assembler;
          xAsm.Assembler.Initialize();
          using (var xScanner = new ILScanner(xAsm)) {
            xScanner.TempDebug += x => LogMessage(x);
            if (EnableLogging) {
              xScanner.EnableLogging(xOutputFilename + ".log.html");
            }
            xScanner.QueueMethod(xInitMethod.DeclaringType.BaseType.GetMethod("Start"));
            xScanner.Execute(xInitMethod);

            using (var xOut = new StreamWriter(OutputFilename, false)) {
              //if (EmitDebugSymbols) {
              xNasmAsm.FlushText(xOut);
              xAsm.FinalizeDebugInfo();
            }
          }
        }
        LogTime("Engine execute finished");
        return true;
      } catch (Exception ex) {
        LogException(ex);
        LogMessage("Loaded assemblies: ");
        foreach (var xAsm in AppDomain.CurrentDomain.GetAssemblies()) {
          // HACK: find another way to skip dynamic assemblies (which belong to dynamic methods)
          try {
            LogMessage(xAsm.Location);
          } catch {
          }
        }
        return false;
      }
    }

    protected MethodBase RetrieveEntryPoint() {
      Type xFoundType = null;
      foreach (var xRef in References) {
        if (xRef.MetadataNames.OfType<string>().Contains("FullPath")) {
          var xFile = xRef.GetMetadata("FullPath");
          if (File.Exists(xFile)) {
            var xAssembly = Assembly.LoadFile(xFile);
            foreach (var xType in xAssembly.GetExportedTypes()) {
              if (xType.IsGenericTypeDefinition || xType.IsAbstract) {
                continue;
              } else if (xType.BaseType.FullName == FULLASSEMBLYNAME_KERNEL) {
                // found kernel?
                if (xFoundType != null) {
                  // already a kernel found, which is not supported.
                  LogError(string.Format("Two kernels found! '{0}' and '{1}'", xType.AssemblyQualifiedName, xFoundType.AssemblyQualifiedName));
                  return null;
                }
                xFoundType = xType;
              }
            }
          }
        }
      }
      if (xFoundType == null) {
        LogError("No Kernel found!");
        return null;
      }
      var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
      if (xCtor == null) {
        LogError("Kernel has no public default constructor");
        return null;
      }
      return xCtor;
    }
  }
}