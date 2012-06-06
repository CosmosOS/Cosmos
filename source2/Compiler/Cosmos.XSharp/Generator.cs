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

        public void Execute(TextReader input, TextWriter output)
        {
            output.WriteLine("// Generating");
            output.WriteLine("//    Name = '{0}'", Name);
            output.WriteLine("//    Namespace = '{0}'", Namespace);
        }
    }
}