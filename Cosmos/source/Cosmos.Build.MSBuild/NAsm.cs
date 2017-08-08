using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;

namespace Cosmos.Build.MSBuild {
  public class NAsm : BaseToolTask {
    #region Property
    [Required]
    public string InputFile {
      get;
      set;
    }

    [Required]
    public string OutputFile {
      get;
      set;
    }

    public bool IsELF {
      get;
      set;
    }

    [Required]
    public string ExePath {
      get;
      set;
    }

    #endregion

    private bool DoExecute()
    {
      var xNasmTask = new NAsmTask();
      xNasmTask.InputFile = InputFile;
      xNasmTask.OutputFile = OutputFile;
      xNasmTask.IsELF = IsELF;
      xNasmTask.ExePath = ExePath;
      xNasmTask.LogMessage = s => Log.LogMessage(s);
      xNasmTask.LogError = s => Log.LogError(s);
      return xNasmTask.Execute();
    }

    public override bool Execute()
    {
      var xSW = Stopwatch.StartNew();
      try
      {
        return DoExecute();
      }
      finally
      {
        xSW.Stop();
        Log.LogMessage(MessageImportance.High, "NAsm task took {0}", xSW.Elapsed);
      }
    }

    public override bool ExtendLineError(bool hasErrored, string errorMessage, out LogInfo log) {
      log = new LogInfo();
      try {
        if (errorMessage.StartsWith(InputFile)) {
          int IndexFile = errorMessage.LastIndexOf('\\', InputFile.Length);
          log.file = errorMessage.Substring(IndexFile + 1, InputFile.Length - IndexFile - 1);
          string[] split = errorMessage.Substring(InputFile.Length).Split(':');
          if (split.Length > 3 && split[2].Contains("warning"))
            log.logType = WriteType.Warning;
          else
            log.logType = WriteType.Error;
          log.lineNumber = int.Parse(split[1]);
          log.message = (split.Length == 4 ? split[3].TrimStart(' ') : string.Empty) + " Code: " + GetLine(InputFile, log.lineNumber).Trim();
        }
      } catch (Exception) {
      }
      return true;
    }

    private static string GetLine(string fileName, int line) {
      using (var sr = new StreamReader(File.OpenRead(fileName))) {
        for (int i = 1; i < line; i++)
          sr.ReadLine();
        return sr.ReadLine();
      }
    }
  }
}
