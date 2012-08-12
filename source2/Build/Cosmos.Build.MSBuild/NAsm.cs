using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Diagnostics;

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

    public override bool Execute() {
      if (File.Exists(OutputFile)) {
        File.Delete(OutputFile);
      }
      if (!File.Exists(InputFile)) {
        Log.LogError("Input file does not exist!");
        return false;
      } else if (!File.Exists(ExePath)) {
        Log.LogError("Exe file not found! (File = '" + ExePath + "')");
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

    public override bool ExtendLineError(int exitCode, ref string errorMessage, out WriteType typ) {
      typ = WriteType.Error;
      try {
        if (errorMessage.StartsWith(InputFile)) {
          int IndexFile = errorMessage.LastIndexOf('\\', InputFile.Length);
          string file = errorMessage.Substring(IndexFile + 1, InputFile.Length - IndexFile - 1);
          string[] split = errorMessage.Substring(InputFile.Length).Split(':');
          if (split.Length > 3 && split[2].Contains("warning"))
            typ = WriteType.Warning;
          uint lineNumber = uint.Parse(split[1]);
          errorMessage = file + " Line: " + lineNumber + " Code: " + GetLine(InputFile, lineNumber).Trim();
          this.BuildEngine.LogMessageEvent(
            new BuildMessageEventArgs(errorMessage, string.Empty, string.Empty, MessageImportance.High));
        }
      } catch (Exception) {
      }
      return true;
    }

    private static string GetLine(string fileName, uint line) {
      using (var sr = new StreamReader(fileName)) {
        for (uint i = 1; i < line; i++)
          sr.ReadLine();
        return sr.ReadLine();
      }
    }
  }
}