using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using System.IO;

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

    private bool DoExecute() {
      if (File.Exists(OutputFile)) {
        File.Delete(OutputFile);
      }
      if (!File.Exists(InputFile)) {
        Log.LogError("Input file \"" + InputFile + "\" does not exist!");
        return false;
      } else if (!File.Exists(ExePath)) {
        Log.LogError("Exe file not found! (File = \"" + ExePath + "\")");
        return false;
      }

      var xFormat = IsELF ? "elf" : "bin";
      var xResult = ExecuteTool(Path.GetDirectoryName(OutputFile), ExePath,
          String.Format("-g -f {0} -o \"{1}\" -D{3}_COMPILATION \"{2}\"", xFormat, Path.Combine(Environment.CurrentDirectory, OutputFile), Path.Combine(Environment.CurrentDirectory, InputFile), xFormat.ToUpper()),
          "NAsm");

      if (xResult) {
        Log.LogMessage("{0} -> {1}", InputFile, OutputFile);
      }
      return xResult;
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

    public override bool ExtendLineError(int exitCode, string errorMessage, out LogInfo log) {
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
      using (var sr = new StreamReader(fileName)) {
        for (int i = 1; i < line; i++)
          sr.ReadLine();
        return sr.ReadLine();
      }
    }
  }
}