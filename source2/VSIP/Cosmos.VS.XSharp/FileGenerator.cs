using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.IO;

namespace Cosmos.VS.XSharp {
  // This class generates .asm files from .xs files.
  //
  // The .asm is not used for actual compiling, but for now we still generate .asm files on save because:
  // 1) It allow user to syntax check by saving, or running custom tool.
  // 2) It allows easy viewing of the output (XSharp.Test can also be used)
  // When we get .xsproj types, we can eliminate this class.
  public class XsToAsmFileGenerator : IVsSingleFileGenerator {
    // Classname is registered in Cosmos.iss. If renamed, fix in Cosmos.iss.

    public int DefaultExtension(out string oDefaultExt) {
      oDefaultExt = ".asm";
      return VSConstants.S_OK;
    }

    public int Generate(string aInputFilePath, string aInputFileContents, string aDefaultNamespace, IntPtr[] aOutputFileContents, out uint oPcbOutput, IVsGeneratorProgress aGenerateProgress) {
      string xResult;
      using (var xInput = new StringReader(aInputFileContents)) {
        using (var xOutputData = new StringWriter()) {
          using (var xOutputCode = new StringWriter()) {
            try {
              var xGenerator = new Cosmos.Compiler.XSharp.AsmGenerator();
              xGenerator.Generate(xInput, xOutputData, xOutputCode);
              xResult = xOutputData.ToString() + "\r\n"
                + xOutputCode.ToString() + "\r\n";
            } catch (Exception ex) {
              var xSB = new StringBuilder();

              xSB.AppendLine(xOutputData.ToString());
              xSB.AppendLine(xOutputCode.ToString());
              for (Exception e = ex; e != null; e = e.InnerException) {
                  xSB.AppendLine(e.Message);
              }
              xResult = xSB.ToString();
            }
          }
        }
      }

      var xBytes = Encoding.UTF8.GetBytes(xResult);
      if (xBytes.Length > 0) {
        aOutputFileContents[0] = Marshal.AllocCoTaskMem(xBytes.Length);
        Marshal.Copy(xBytes, 0, aOutputFileContents[0], xBytes.Length);
        oPcbOutput = (uint)xBytes.Length;
      } else {
        aOutputFileContents[0] = IntPtr.Zero;
        oPcbOutput = 0;
      }
      return VSConstants.S_OK;
    }

  }
}