using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using XSharp.Nasm;

namespace Cosmos.Compiler.XSharp {
  public class AsmGenerator {
    protected TokenPatterns mPatterns = new TokenPatterns();

    public bool EmitUserComments = false;
    protected int mLineNo = 0;
    protected string mPathname = "";

    public Assembler Generate(string aSrcPathname) {
      mPatterns.EmitUserComments = EmitUserComments;
      mLineNo = 0;
      var xResult = new Assembler();
      using (var xInput = new StreamReader(aSrcPathname)) {
        while (true) {
          mLineNo++;
          string xLine = xInput.ReadLine();
          if (xLine == null) {
            break;
          }

          var xAsm = ProcessLine(xLine);
          xResult.Data.AddRange(xAsm.Data);
          xResult.Code.AddRange(xAsm.Code);
        }
      }
      return xResult;
    }

    public void GenerateToFiles(string aSrcPathname) {
      mPathname = Path.GetFileName(aSrcPathname);
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutputCode = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asm"))) {
          using (var xOutputData = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asmdata"))) {
            Generate(xInput, xOutputData, xOutputCode);
          }
        }
      }
    }

    public void Generate(TextReader aInput, TextWriter aOutputData, TextWriter aOutputCode) {
      mPatterns.EmitUserComments = EmitUserComments;
      mLineNo = 0;
      while (true) {
        mLineNo++;
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        var xAsm = ProcessLine(xLine);
        foreach (var x in xAsm.Data) {
          aOutputData.WriteLine(x);
        }
        foreach (var x in xAsm.Code) {
          aOutputCode.WriteLine(x);
        }
      }
    }

    protected Assembler ProcessLine(string aLine) {
      Assembler xAsm;

      aLine = aLine.Trim();
      if (String.IsNullOrEmpty(aLine)) {
        xAsm = new Assembler();
        xAsm += "";
        return xAsm;
      }

      // Curretly we use a new assembler for every line.
      // If we dont it could create a really large in memory object.
      xAsm = mPatterns.GetCode(aLine);
      if (xAsm == null) {
        var xMsg = new StringBuilder();
        if (mPathname != "") {
          xMsg.Append("File " + mPathname + ", ");
        }
        xMsg.Append("Line " + mLineNo + ", ");
        xMsg.Append("Parsing error: " + aLine);
        throw new Exception(xMsg.ToString());
      }
      return xAsm;
    }
  }
}
