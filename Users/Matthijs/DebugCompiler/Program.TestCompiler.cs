using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using Cosmos.Build.Common;
using Cosmos.Build.MSBuild;
using Cosmos.IL2CPU;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Data.SQLite;
using Cosmos.Debug.Common;


namespace DebugCompiler
{
    internal class Program
    {
        public const string CosmosRoot = @"c:\data\sources\OpenSource\Cosmos";

        //public const string CosmosRoot = @"C:\Users\Emile\Source\Repos\Cosmos";
        //public const string CosmosRoot = @"c:\Development\Cosmos";
        //public const string CosmosRoot = @"C:\Users\Huge\Documents\Visual Studio 2010\Projects\IL2CPU";

        //private const string KernelFile = CosmosRoot + @"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernel.dll";
        //private const string OutputFile = CosmosRoot + @"\Users\Sentinel209\SentinelKernel\bin\Debug\SentinelKernelBoot.asm";
        //private const string KernelFile = CosmosRoot + @"\Users\Matthijs\Playground\bin\Debug\Playground.dll";
        //private const string OutputFile = CosmosRoot + @"\Users\Matthijs\Playground\bin\Debug\PlaygroundBoot.asm";
        //private const string KernelFile = CosmosRoot + @"\Demos\Guess\bin\Debug\GuessKernel.dll";
        //private const string OutputFile = CosmosRoot + @"\Demos\Guess\bin\Debug\GuessKernelBoot.asm";
        private const string KernelFile = @"c:\Data\Sources\OpenSource\Edison\CosmosEdison\Sources\Playgrounds.Matthijs\bin\Debug\Playgrounds.Matthijs.dll";
        private const string OutputFile = @"c:\Data\Sources\OpenSource\Edison\CosmosEdison\Sources\Playgrounds.Matthijs\bin\Debug\Playgrounds.MatthijsBoot.asm";
        //private const string KernelFile = CosmosRoot + @"\Users\Emile\TestBed\TestBed\bin\Debug\TestBed.dll";
        //private const string OutputFile = CosmosRoot + @"\Users\Emile\TestBed\TestBed\bin\Debug\TestBedBoot.asm";

        private static void Main(string[] args)
        {
            //Console.SetOut(new StreamWriter("out", false));

            var xSW = Stopwatch.StartNew();
            try
            {
                CosmosPaths.DebugStubSrc = Path.Combine(CosmosRoot, "source", "Cosmos.Debug.DebugStub");
                var xTask = new CompilerEngine();
                xTask.DebugEnabled = true;
                xTask.StackCorruptionDetectionEnabled = true;
                xTask.DebugMode = "Source";
                xTask.TraceAssemblies = "All";
                xTask.DebugCom = 1;
                xTask.UseNAsm = true;
                xTask.OutputFilename = OutputFile;
                xTask.EnableLogging = true;
                xTask.EmitDebugSymbols = true;
                xTask.IgnoreDebugStubAttribute = false;
                xTask.References = GetReferences();
                xTask.OnLogError = (m) => Console.WriteLine("Error: {0}", m);
                xTask.OnLogWarning = (m) => Console.WriteLine("Warning: {0}", m);
                xTask.OnLogMessage = (m) =>
                                     {
                                         Console.WriteLine("Message: {0}", m);
                                     };
                xTask.OnLogException = (m) => Console.WriteLine("Exception: {0}", m.ToString());

                if (xTask.Execute())
                {
                    Console.WriteLine("Executed OK");
                }
                else
                {
                    Console.WriteLine("Errorred");
                }
                xSW.Stop();

            }
            catch (Exception E)
            {
                Console.Out.Flush();
                Console.WriteLine(E.ToString());
                //Console.ReadLine();
                return;
            }
            Console.WriteLine("Run took {0}", xSW.Elapsed);
            Console.WriteLine("Generated {0} Guids", DebugInfo.mLastGuid);
            Console.Out.Flush();
            Console.ReadKey();
        }

        //static void Main()
        //{
        //    var di = new DebugInfo(Path.Combine(CosmosRoot, @"source2\Demos\Guess\bin\Debug\Guess.cdb"));
        //    di.LoadLookups();
        //    var addr = (uint)1;
        //    var q = new SQLinq<Label>()
        //      .Where(i => i.Address <= addr)
        //      .OrderByDescending(i => i.Address)
        //      ;
        //    Console.WriteLine(q.ToSQL().ToQuery());
        //    var xLabels = di.Connection.Query<Label>(q).Select(i => i.Name).ToArray();
        //    var g = Guid.Empty;
        //    di.GetMethod(1);
        //}

        private static void SQLiteLog_Log(object sender, LogEventArgs e)
        {
            Console.WriteLine("SQL: {0}", e.Message);
        }

        private static string[] GetReferences()
        {
            return new string[]
            {
                KernelFile,
                CosmosRoot + @"\source\Cosmos.Core.Plugs\bin\x86\Debug\Cosmos.Core.Plugs.dll",
                CosmosRoot + @"\source\Cosmos.Debug.Kernel.Plugs\bin\x86\Debug\Cosmos.Debug.Kernel.Plugs.dll",
                CosmosRoot + @"\source\Cosmos.HAL\bin\x86\Debug\Cosmos.HAL.dll",
                CosmosRoot + @"\source\Cosmos.System.Plugs\bin\x86\Debug\Cosmos.System.Plugs.dll",
                CosmosRoot + @"\Users\Sentinel209\SentinelSystemLib\bin\Debug\SentinelSystemLib.dll",
            };
        }
    }
}
