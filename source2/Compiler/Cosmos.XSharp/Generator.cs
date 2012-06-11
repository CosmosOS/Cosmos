using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Cosmos.Compiler.XSharp {
  public class Generator {
    public static void Execute(TextReader input, string inputFilename, TextWriter output, string defaultNamespace) {
      var xGenerator = new Generator();
      xGenerator.Name = Path.GetFileNameWithoutExtension(inputFilename);
      xGenerator.Namespace = defaultNamespace;
      xGenerator.Execute(input, output);
    }

    public string Namespace {
      get;
      set;
    }

    public string Name {
      get;
      set;
    }

    private TextReader mInput;
    private TextWriter mOutput;

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

    // TODO change to classes with attribs, or something similar.
    // Dont need separate classes, can assmeble straight from tokens using data and instances.
    protected TokenType[] PatternLiteral = new TokenType[] { TokenType.Literal };
    protected TokenType[] PatternComment = new TokenType[] { TokenType.Comment };
    protected TokenType[] PatternRegAsnNum = new TokenType[] { TokenType.Register, TokenType.Assignment, TokenType.ValueNumber };

    protected void ProcessLine(string aLine) {
      aLine = aLine.Trim();
      if (String.IsNullOrEmpty(aLine)) {
        // Skip
        return;
      }
      var xParser = new Parser(aLine);
      var xTokens = xParser.Tokens;
      if (xParser.PatternMatches(PatternComment)) {
        mOutput.WriteLine("new Comment(\"{0}\");", xTokens[0].Value.Substring(1));
      } else if (xParser.PatternMatches(PatternLiteral)) {
        mOutput.WriteLine("new LiteralAssemblerCode(\"{0}\");", xTokens[0].Value.Substring(1));
      } else if (xParser.PatternMatches(PatternRegAsnNum)) {
        mOutput.WriteLine("new Move{{DestinationReg = RegistersEnum.{0}, SourceValue = {1}}};", xTokens[0].Value, xTokens[2].Value);
      }
    }

  }
}