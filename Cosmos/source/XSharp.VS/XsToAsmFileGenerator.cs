using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.IO;
using Cosmos.Assembler;
using XSharp.Common;

namespace XSharp.VS
{
  // This class generates .asm files from .xs files.
  //
  // The .asm is not used for actual compiling, but for now we still generate .asm files on save because:
  // 1) It allows user to syntax check by saving, or running custom tool.
  // 2) It allows easy viewing of the output (XSharp.Test can also be used)
  // When we get .xsproj types, we can eliminate this class.
  public class XsToAsmFileGenerator : IVsSingleFileGenerator
  {
    public int DefaultExtension(out string oDefaultExt)
    {
      oDefaultExt = ".asm";
      return VSConstants.S_OK;
    }

    public int Generate(string aInputFilePath, string aInputFileContents, string aDefaultNamespace, IntPtr[] aOutputFileContents, out uint oPcbOutput, IVsGeneratorProgress aGenerateProgress)
    {
      string xResult;
      using (var xInput = new StringReader(aInputFileContents))
      {
        using (var xOut = new StringWriter())
        {
          try
          {
            new Assembler();
            try
            {
              var xGen = new AsmGenerator();
              xGen.Generate(xInput, xOut);
              xResult = $"Generated at {DateTime.Now} {Environment.NewLine} {Environment.NewLine} {xOut} {Environment.NewLine}";
            }
            finally
            {
              Assembler.ClearCurrentInstance();
            }
          }
          catch (Exception ex)
          {
            var xSB = new StringBuilder();
            xSB.Append(xOut);
            xSB.AppendLine();

            for (Exception e = ex; e != null; e = e.InnerException)
            {
              xSB.AppendLine(e.Message);
            }
            xResult = xSB.ToString();
          }
        }
      }

      aOutputFileContents[0] = IntPtr.Zero;
      oPcbOutput = 0;
      var xBytes = Encoding.UTF8.GetBytes(xResult);
      if (xBytes.Length > 0)
      {
        aOutputFileContents[0] = Marshal.AllocCoTaskMem(xBytes.Length);
        Marshal.Copy(xBytes, 0, aOutputFileContents[0], xBytes.Length);
        oPcbOutput = (uint)xBytes.Length;
      }

      return VSConstants.S_OK;
    }

  }
}
