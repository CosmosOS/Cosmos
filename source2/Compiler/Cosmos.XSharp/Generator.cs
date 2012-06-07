using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Cosmos.Compiler.XSharp
{
    public class Generator
    {
        public static void Execute(TextReader input, string inputFilename, TextWriter output, string defaultNamespace)
        {
            var xGenerator = new Generator();
            xGenerator.Name = Path.GetFileNameWithoutExtension(inputFilename);
            xGenerator.Namespace = defaultNamespace;
            xGenerator.Execute(input, output);
        }

        public string Namespace
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        private void EnsureHeaderWritten()
        {
            if (mHeaderWritten)
            {
                return;
            }
            mHeaderWritten = true;
            EmitHeader();
        }

        private bool mHeaderWritten = false;

        private TextReader mInput;
        private TextWriter mOutput;

        public void Execute(TextReader aInput, TextWriter aOutput) {
          mInput = aInput;
          mOutput = aOutput;

          while (true) {
            string xLine = aInput.ReadLine();
            if (xLine == null) {
              break;
            }

            ProcessLine(xLine);
          }
                                
          EmitFooter();
        }

        private void EmitHeader()
        {
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

        private void EmitFooter()
        {
            EnsureHeaderWritten();
            mOutput.WriteLine("\t\t}");
            mOutput.WriteLine("\t}");
            mOutput.WriteLine("}");
        }

        private void ProcessLine(string aLine) {
          aLine = aLine.Trim();
          if (String.IsNullOrEmpty(aLine)) {
            // Skip
          } else if (aLine[0] == '#') {
            ProcessComment(aLine.Substring(1));
          } else if (aLine[0] == '!') {
            ProcessLiteral(aLine.Substring(1));
          } else {
            var xParts = Regex.Split(aLine, "([)|()|(])|(=)|(.)", RegexOptions.IgnorePatternWhitespace);

            if (aLine.Contains("=")) {
              ProcessAssignment(aLine);
            } else {
              throw new Exception("Syntax error: '" + aLine + "'");
            }
          }
        }

        private void ProcessAssignment(string line)
        {
            var xParts = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (xParts.Length != 2)
            {
                throw new Exception(String.Format("Wrong Assignment line: '{0}'", line));
            }
            var xLeft = xParts[0].Trim();
            var xRight = xParts[1].Trim();
            uint xValue;

            if (IsRegister(xLeft))
            {
                if (UInt32.TryParse(xRight, out xValue))
                {
                    mOutput.WriteLine("new Move{{DestinationReg = RegistersEnum.{0}, SourceValue = {1}}};", xLeft, xRight);
                    

                    return;
                }
            }

            throw new Exception(String.Format("Wrong Assignment line: '{0}'", line));
        }

        private void ProcessLiteral(string line)
        {
            EnsureHeaderWritten();
            mOutput.WriteLine("new LiteralAssemblerCode(\"{0}\");", line);
        }

        private void ProcessComment(string line)
        {
            EnsureHeaderWritten();
            mOutput.WriteLine("new Comment(\"{0}\");", line);
        }

        private bool IsRegister(string aWord)
        {
          aWord = aWord.ToLower();
            return
              aWord == "eax" || aWord == "ax" || aWord == "ah" || aWord == "al" ||
              aWord == "ebx" || aWord == "bx" || aWord == "bh" || aWord == "bl" ||
              aWord == "ecx" || aWord == "cx" || aWord == "ch" || aWord == "cl" ||
              aWord == "edx" || aWord == "dx" || aWord == "dh" || aWord == "dl" || 
              aWord == "esp" ||
              aWord == "ebp" ||
              aWord == "esi" ||
              aWord == "edi";
        }
    }
}