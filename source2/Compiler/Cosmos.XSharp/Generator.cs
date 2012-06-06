using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.XSharp
{
    public class Generator
    {
        //Cosmos.XSharp.Generator.Generate(xInput, inputFileName, xOut);
        public static void Generate(TextReader input, string inputFilename, TextWriter output, string defaultNamespace)
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

        public void Execute(TextReader input, TextWriter output)
        {
            mInput = input;
            mOutput = output;

            while (input.Peek() != -1)
            {
                var xLine = input.ReadLine().Trim();
                if (String.IsNullOrEmpty(xLine))
                {
                    continue;
                }
                HandleLine(xLine);
            }
                                
            EmitFooter();
        }

        private void EmitHeader()
        {
            mOutput.WriteLine("using System;");
            mOutput.WriteLine("using System.Linq;");
            mOutput.WriteLine("using Cosmos.Assembler;");
            mOutput.WriteLine("using Cosmos.Assembler.x86;");
            mOutput.WriteLine("");
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

        private void HandleLine(string line)
        {
            if (line[0] == '#')
            {
                HandleComment(line.Substring(1));
                return;
            }
            if (line[0] == '!')
            {
                HandleLiteral(line.Substring(1));
                return;
            }
            if (line.Contains("="))
            {
                HandleAssignment(line);
                return;
            }
            // todo: error reporting?
            throw new Exception("Line not handled: '" + line + "'!");
            
        }

        private void HandleAssignment(string line)
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

        private void HandleLiteral(string line)
        {
            EnsureHeaderWritten();
            mOutput.WriteLine("new LiteralAssemblerCode(\"{0}\");", line);
        }

        private void HandleComment(string line)
        {
            EnsureHeaderWritten();
            mOutput.WriteLine("new Comment(\"{0}\");", line);
        }

        private bool IsRegister(string word)
        {
            word=word.ToLower();
            return word == "eax"
                || word == "ebx"
                || word == "ecx"
                || word == "edx"
                || word == "esp"
                || word == "ebp"
                || word == "esi"
                || word == "edi";
        }
    }
}