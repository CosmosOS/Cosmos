using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Cosmos.Compiler.XSharp {
  public class AsmGenerator {
    protected TextReader mInput;
    protected TextWriter mOutput;
    protected TokenPatterns mPatterns = new TokenPatterns();

    public bool EmitUserComments = false;
    protected int mLineNo = 0;
    protected string mPathname = "";

    public AsmGenerator() {
      mPatterns.RawAsm = true;
    }

    public void Execute(string aSrcPathname) {
      mPathname = Path.GetFileName(aSrcPathname);
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutput = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".asm"))) {
          Execute(xInput, xOutput);
        }
      }
    }

    public void Execute(TextReader aInput, TextWriter aOutput) {
      mInput = aInput;
      mOutput = aOutput;
      mPatterns.EmitUserComments = EmitUserComments;

      mLineNo = 0;
      while (true) {
        mLineNo++;
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        ProcessLine(xLine);
      }
    }

    protected void ProcessLine(string aLine) {
      aLine = aLine.Trim();
      if (String.IsNullOrEmpty(aLine)) {
        mOutput.WriteLine();
        return;
      }

      var xCode = mPatterns.GetCode(aLine);
      if (xCode == null) {
        var xMsg = new StringBuilder();
        if (mPathname != "") {
          xMsg.Append("File " + mPathname + ", ");
        }
        xMsg.Append("Line " + mLineNo + ", ");
        xMsg.Append("Parsing error: " + aLine);
        throw new Exception(xMsg.ToString());
      }
      foreach (var xLine in xCode) {
        mOutput.WriteLine(xLine);
      }
    }
  }
}
