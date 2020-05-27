using System;
using Cosmos.VS.DebugEngine.AD7.Impl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.Engine.Impl {
  public static class EngineUtils {
    public static string BuildCommandLine(string exe, string args) {
      string startQuote = "\"";
      string afterExe = "\"";

      if (exe.Length <= 0) {
        //throw new ComponentException(Constants.E_WIN32_INVALID_NAME);
      }

      if (exe[0] == '\"') {
        startQuote = String.Empty;
        if (exe.Length == 1) {
          //throw new ComponentException(Constants.E_WIN32_INVALID_NAME);
        }

        // If there are any more quotes, it needs to be the last character
        int endQuote = exe.IndexOf('\"', 1);
        if (endQuote > 0) {
          if (exe.Length == 2 || endQuote != exe.Length - 1) {
            //throw new ComponentException(Constants.E_WIN32_INVALID_NAME);
          }
          afterExe = String.Empty;
        }
      } else {
        // If it doesn't start with a quote, it shouldn't have any
        if (exe.IndexOf('\"') >= 0) {
          //throw new ComponentException(Constants.E_WIN32_INVALID_NAME);
        }
      }

      if (args == null) {
        args = "";
      } else if (args.Length != 0) {
          afterExe = "\" ";
      } else {
          afterExe = " ";
      }

      return String.Concat(startQuote, exe, afterExe, args);
    }

    public static string GetAddressDescription(/*DebuggedModule module,*/AD7Engine engine, uint ip)
    {
      var d = new AD7StackFrame(engine, engine.mThread, engine.mProcess);
      d.SetFrameInfo(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME | enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS, out var info);
      return info.m_bstrFuncName;

      //string location = ip.ToString("x8", CultureInfo.InvariantCulture);

      //if (module != null)
      {
        //  string moduleName = System.IO.Path.GetFileName(module.Name);
        //  location = string.Concat(moduleName, "!", location);
      }

      //return location;
    }

    public static void CheckOk(int hr) {
      if (hr != 0) {
        throw new InvalidOperationException(hr.ToString());
      }
    }

    public static void RequireOk(int hr) {
      if (hr != 0) {
        throw new InvalidOperationException(hr.ToString());
      }
    }

    public static int GetProcessId(IDebugProcess2 process) {
      var pid = new AD_PROCESS_ID[1];
      RequireOk(process.GetPhysicalProcessId(pid));

      if (pid[0].ProcessIdType != (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM) {
        return 0;
      }

      return (int)pid[0].dwProcessId;
    }

    public static int GetProcessId(IDebugProgram2 program) {
      RequireOk(program.GetProcess(out var process));
      return GetProcessId(process);
    }

    public static int UnexpectedException(Exception e) {
      System.Diagnostics.Debug.Fail("Unexpected exception during Attach", e.ToString());
      return VSConstants.RPC_E_SERVERFAULT;
    }

    internal static bool IsFlagSet(uint value, int flagValue) {
      return (value & flagValue) != 0;
    }
  }
}
