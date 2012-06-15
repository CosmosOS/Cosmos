using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Cosmos.Compiler.XSharp {
  public class Generator {
    protected TextReader mInput;
    protected TextWriter mOutput;
    protected TokenPatterns mPatterns = new TokenPatterns();

    public void Execute(string aNamespace, string aSrcPathname) {
      using (var xInput = new StreamReader(aSrcPathname)) {
        using (var xOutput = new StreamWriter(Path.ChangeExtension(aSrcPathname, ".cs"))) {
          var xGenerator = new Generator();
          xGenerator.Execute(aNamespace, Path.GetFileNameWithoutExtension(aSrcPathname), xInput, xOutput);
        }
      }
    }

    public void Execute(string aNamespace, string aClassname, TextReader aInput, TextWriter aOutput) {
      mInput = aInput;
      mOutput = aOutput;

      EmitHeader(aNamespace, aClassname);
      while (true) {
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
      mOutput.WriteLine("namespace {0} {{", aNamespace);
      mOutput.WriteLine("\tpublic class {0} : Cosmos.Assembler.Code {{", aClassname);
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
        // Skip
        return;
      }
      var xParser = new Parser(aLine, false);
      var xCode = mPatterns.GetCode(xParser.Tokens);
      foreach(var xLine in xCode) {
        mOutput.WriteLine("\t\t\t" + xLine);
      }
    }

  }
}