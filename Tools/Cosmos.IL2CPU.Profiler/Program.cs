using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cosmos.Build.Common;
using IL2CPU.Debug.Symbols;
using Cosmos.System;
using Console = System.Console;

namespace Cosmos.IL2CPU.Profiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.DoScan();
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void DoScan()
        {

            var xSW = new Stopwatch();
            xSW.Start();
            string MDFFile = AppContext.BaseDirectory + "TestKernel.mdf";
            if (File.Exists(MDFFile))
                File.Delete(MDFFile);

            var outFile = AppContext.BaseDirectory + "TestKernel.out";
            if (File.Exists(outFile))
                File.Delete(outFile);

            var logFile = AppContext.BaseDirectory + "TestKernel.log";
            if (File.Exists(logFile))
                File.Delete(logFile);

            var xAsmblr = new AppAssembler(1, "Cosmos.Assembler.Log");
            using (var xScanner = new ILScanner(xAsmblr))
            {
                xScanner.LogException = (Exception e) =>
                {
                    Console.WriteLine("ILScanner exception : " + e.Message);
                };
                using (var xDebugInfo = new DebugInfo(MDFFile, true, true))
                {
                    xAsmblr.DebugInfo = xDebugInfo;
                    xAsmblr.DebugEnabled = true;
                    xAsmblr.DebugMode = DebugMode.Source;
                    xAsmblr.TraceAssemblies = TraceAssemblies.All;
                    xAsmblr.IgnoreDebugStubAttribute = false;

                    xAsmblr.Assembler.Initialize();
                    //TODO: Add plugs into the scanning equation to profile scanning them too
                    Type xFoundType = typeof(FakeKernel);
                    var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
                    var xEntryPoint = typeof(Kernel).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
                    xScanner.EnableLogging(logFile);
                    xScanner.QueueMethod(xEntryPoint);
                    xScanner.Execute(xCtor);
                    using (var xOut = new StreamWriter(File.OpenWrite(outFile)))
                    {
                        xAsmblr.Assembler.FlushText(xOut);
                        xAsmblr.FinalizeDebugInfo();
                    }
                    xSW.Stop();
                    Console.WriteLine("Total time : {0}", xSW.Elapsed);
                    Console.WriteLine("Method count: {0}", xScanner.MethodCount);
                    //Console.WriteLine("Instruction count: {0}", xScanner.InstructionCount);
                }
            }
        }

        static void xScanner_TempDebug(string obj)
        {
            Console.WriteLine(obj);
        }

        // This is a dummy entry point for the scanner to start at.
        // Its not even a Cosmos app, just a standard Windows console app,
        // but that fine for the scanner profiling as it does
        // not actually compile it.
        private static void ScannerEntryPoint()
        {
            Console.WriteLine("Hello, World!");
            var xInt = 0;
            object xObj = xInt;
            xObj.ToString();
        }
    }
}
