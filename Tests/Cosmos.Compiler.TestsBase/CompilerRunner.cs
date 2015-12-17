using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Build.MSBuild;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU;

namespace Cosmos.Compiler.TestsBase
{
    public class CompilerRunner
    {
        public CompilerRunner()
        {
            References = new List<string>();
        }
        public List<string> References
        {
            get;
            private set;
        }

        public string OutputFile
        {
            get;
            set;
        }

        public string AssemblerLogFile
        {
            get;
            set;
        }

        public void Execute()
        {
            if (String.IsNullOrWhiteSpace(OutputFile))
            {
                throw new InvalidOperationException("No OutputFile specified!");
            }
            if (References.Count == 0)
            {
                throw new InvalidOperationException("No References specified!");
            }
            
            DebugInfo.SetRange(DebugInfo.AssemblerDebugSymbolsRange); 
            Console.WriteLine("Compiling to '{0}'", OutputFile);
            var xTask = new CompilerEngine();
            xTask.DebugEnabled = false;
            xTask.StackCorruptionDetectionEnabled = false;
            xTask.StackCorruptionDetectionLevel = "MethodFooters";
            xTask.DebugMode = "Source";
            xTask.TraceAssemblies = "User";
            xTask.DebugCom = 1;
            xTask.UseNAsm = true;
            xTask.OutputFilename = OutputFile;
            xTask.EnableLogging = true;
            xTask.EmitDebugSymbols = true;
            xTask.IgnoreDebugStubAttribute = false;
            xTask.References = GetReferences();
            xTask.AssemblerLog = AssemblerLogFile;
            xTask.OnLogError = m =>
                               {
                                   throw new Exception("Error during compilation: " + m);
                               };
            xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
            xTask.OnLogMessage = (m) =>
                                 {
                                     Console.WriteLine("Message: {0}", m);
                                 };
            xTask.OnLogException = (m) => Console.WriteLine("Exception: {0}", m.ToString());

            if (!xTask.Execute())
            {
                throw new Exception("Error occurred while running compiler!");
            }
        }

        private string[] GetReferences()
        {
            var xResult = new List<string>(References.Count);
            foreach (var xRefFile in References)
            {
                xResult.Add(xRefFile);
            }
            return xResult.ToArray();
        }
    }
}
