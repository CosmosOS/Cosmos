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
    public string Namespace { get; set; }
    public string Name { get; set; }

    public static void Execute(TextReader input, string inputFilename, TextWriter output, string defaultNamespace) {
      var xGenerator = new Generator();
      xGenerator.Name = Path.GetFileNameWithoutExtension(inputFilename);
      xGenerator.Namespace = defaultNamespace;
      xGenerator.Execute(input, output);
    }

    public void Execute(TextReader aInput, TextWriter aOutput) {
      mInput = aInput;
      mOutput = aOutput;

      EmitHeader();
      while (true) {
        string xLine = aInput.ReadLine();
        if (xLine == null) {
          break;
        }

        ProcessLine(xLine);
      }

      EmitFooter();
    }

    private void EmitHeader() {
      mOutput.WriteLine("using System;");
      mOutput.WriteLine("using System.Linq;");
      mOutput.WriteLine("using Cosmos.Assembler;");
      mOutput.WriteLine("using Cosmos.Assembler.x86;");
      mOutput.WriteLine();
      mOutput.WriteLine("namespace {0}", Namespace);
      mOutput.WriteLine("{");
      mOutput.WriteLine("\tpublic class {0} {{", Name);
      mOutput.WriteLine("\t\tpublic void Assemble() {");
    }

    private void EmitFooter() {
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
      mOutput.WriteLine(mPatterns.GetCode(xParser.Tokens));
    }

  }
}