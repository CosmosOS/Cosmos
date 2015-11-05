using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.IO;

namespace XSharp.VS {
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
        using (var xOutData = new StringWriter()) {
          using (var xOutCode = new StringWriter()) {
            try {
              var xGen = new XSharp.Compiler.AsmGenerator();
              xGen.Generate(xInput, xOutData, xOutCode);
              xResult =
                "; Generated at " + DateTime.Now.ToString() + "\r\n"
                 + "\r\n"
                + xOutData.ToString() + "\r\n"
                + xOutCode.ToString() + "\r\n";
            } catch (Exception ex) {
              var xSB = new StringBuilder();
              xSB.Append(xOutData);
              xSB.AppendLine();
              xSB.Append(xOutCode);
              xSB.AppendLine();

              for (Exception e = ex; e != null; e = e.InnerException) {
                xSB.AppendLine(e.Message);
              }
              xResult = xSB.ToString();
            }
          }
        }
      }

      aOutputFileContents[0] = IntPtr.Zero;
      oPcbOutput = 0;
      var xBytes = Encoding.UTF8.GetBytes(xResult);
      if (xBytes.Length > 0) {
        aOutputFileContents[0] = Marshal.AllocCoTaskMem(xBytes.Length);
        Marshal.Copy(xBytes, 0, aOutputFileContents[0], xBytes.Length);
        oPcbOutput = (uint)xBytes.Length;
      }

      return VSConstants.S_OK;
    }

  }
}