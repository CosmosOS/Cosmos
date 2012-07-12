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

    public void Execute(string aSrcPathname) {
      mPathname = Path.GetFileName(aSrcPathname);
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutput = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asm"))) {
          Execute(xInput, xOutput);
        }
      }
    }

    public void Execute(TextReader aInput, TextWriter aOutput) {
      mPatterns.EmitUserComments = EmitUserComments;
      // Right now we just collect in RAM, but later we should flush to separate files
      // or something and merge after.
      var xAsm = new Assembler();

      mLineNo = 0;
      while (true) {
        mLineNo++;
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        var xAsm2 = ProcessLine(xLine);
        xAsm.Data.AddRange(xAsm2.Data);
        xAsm.Code.AddRange(xAsm2.Code);
      }

      foreach (var x in xAsm.Data) {
        aOutput.WriteLine(x);
      }
      foreach (var x in xAsm.Code) {
        aOutput.WriteLine(x);
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
