using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Cosmos.Compiler.XSharp {
  // This is a separate class from AsmGenerator and no common base because it is
  // temporary and will go away soon.
  public class CSharpGenerator {
    protected TextReader mInput;
    protected TextWriter mOutput;
    protected TokenPatterns mPatterns = new TokenPatterns();

    public bool EmitUserComments = false;
    public bool EmitXSharpCodeComments = false;
    protected int mLineNo = 0;
    protected string mPathname = "";

    public void Execute(string aNamespace, string aSrcPathname) {
      mPathname = Path.GetFileName(aSrcPathname);
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutput = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".cs"))) {
          Execute(aNamespace, Path.GetFileNameWithoutExtension(aSrcPathname), xInput, xOutput);
        }
      }
    }

    public void Execute(string aNamespace, string aClassname, TextReader aInput, TextWriter aOutput) {
      mInput = aInput;
      mOutput = aOutput;
      mPatterns.EmitUserComments = EmitUserComments;

      mLineNo = 0;
      EmitHeader(aNamespace, aClassname);
      while (true) {
        mLineNo++;
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        ProcessLine(xLine);
      }

      EmitFooter();
    }

    protected void EmitHeader(string aNamespace, string aClassname) {
      mOutput.WriteLine("using System;");
      mOutput.WriteLine("using System.Linq;");
      mOutput.WriteLine("using Cosmos.Assembler;");
      mOutput.WriteLine("using Cosmos.Assembler.x86;");
      mOutput.WriteLine();
      mOutput.WriteLine("namespace " + aNamespace + " {");
      mOutput.WriteLine("\tpublic class " + aClassname + " : Cosmos.Assembler.Code {");
      mOutput.WriteLine();
      mOutput.WriteLine("\t\tpublic " + aClassname + "(Assembler.Assembler aAssembler) : base(aAssembler) {}");
      mOutput.WriteLine();
      mOutput.WriteLine("\t\tpublic override void Assemble() {");
    }

    protected void EmitFooter() {
      mOutput.WriteLine("\t\t}");
      mOutput.WriteLine("\t}");
      mOutput.WriteLine("}");
    }

    protected void ProcessLine(string aLine) {
      aLine = aLine.Trim();
      if (String.IsNullOrEmpty(aLine)) {
        mOutput.WriteLine();
        return;
      }
      if (EmitXSharpCodeComments && !aLine.StartsWith("//")) {
        string xLine = aLine.Replace("\"", "\\\"");
        mOutput.WriteLine("\t\t\tnew Comment(\"X#: " + xLine + "\");");
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
        mOutput.WriteLine("\t\t\t" + xLine);
      }
    }

  }
}