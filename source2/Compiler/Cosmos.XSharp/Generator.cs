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

    public Generator() {
      mPatterns.Add(new TokenType[] { TokenType.LiteralAsm },
        "new LiteralAssemblerCode(\"{0}\");");
      mPatterns.Add(new TokenType[] { TokenType.Comment },
        "new Comment(\"{0}\");");

      mPatterns.Add("REG = 123"
        , "new Move{{DestinationReg = RegistersEnum.{0}, SourceValue = {2}}};");
      mPatterns.Add("REG = REG"
        , "new ;");
      mPatterns.Add("REG = REG[0]"
        , "new ;");

      mPatterns.Add("ABC = REG"
        , "new ;");

      mPatterns.Add("REG + 1"
        , "new ;");
      mPatterns.Add("REG - 1"
        , "new ;");

      mPatterns.Add(new TokenType[] { TokenType.OpCode },
        "new ;");
    }

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
      mOutput.WriteLine("\tpublic class {0}: Cosmos.IL2CPU.Plugs.AssemblerMethod", Name);
      mOutput.WriteLine("\t{");
      mOutput.WriteLine("\t\tpublic override void AssembleNew(object aAssembler, object aMethodInfo)");
      mOutput.WriteLine("\t\t{");
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
      var xTokens = xParser.Tokens;

      var xPattern = xTokens.Select(c => c.Type).ToArray();
      string xCode;
      if (!mPatterns.TryGetValue(new TokenPattern(xPattern), out xCode)) {
        throw new Exception("Invalid token pattern in X# file.");
      }
      mOutput.WriteLine(xCode, xTokens.Select(c => c.Value).ToArray());
    }

  }
}