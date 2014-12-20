using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Cosmos.System;
using Cosmos.Debug.Common;
using Cosmos.Build.Common;
using System.IO;
using Console = System.Console;

namespace Cosmos.IL2CPU.Profiler {
  class Program {

    // This program profiles the scanning engine.
    // In the future it may profile other aspects as well.

    static void Main(string[] args) {
     Program.DoScan();
     Console.WriteLine("Press any key to continue.");
     Console.ReadKey();
    }

    private static void DoScan()
    {

        var xSW = new Stopwatch();
        xSW.Start();
        string MDFFile = AppDomain.CurrentDomain.BaseDirectory + "TestKernel.mdf";
        if (File.Exists(MDFFile))
            File.Delete(MDFFile);

        var outFile = AppDomain.CurrentDomain.BaseDirectory + "TestKernel.out";
        if (File.Exists(outFile))
            File.Delete(outFile);

        var logFile = AppDomain.CurrentDomain.BaseDirectory + "TestKernel.log";
        if (File.Exists(logFile))
            File.Delete(logFile);

        var xAsmblr = new AppAssembler(1, "Cosmos.Assembler.Log");
        using (var xScanner = new ILScanner(xAsmblr))
        {
            xScanner.LogException = (Exception e) =>
            {
                Console.WriteLine("ILScanner exception : " + e.Message);
            };
            using (var xDebugInfo = new DebugInfo(MDFFile, true))
            {
                xAsmblr.DebugInfo = xDebugInfo;
                xAsmblr.DebugEnabled = true;
                xAsmblr.DebugMode = DebugMode.Source;
                xAsmblr.TraceAssemblies = TraceAssemblies.All;
                xAsmblr.IgnoreDebugStubAttribute = false;

                xAsmblr.Assembler.Initialize();
                //TODO: Add plugs into the scanning equation to profile scanning them too
                //System.Reflection.MethodInfo[] name = typeof(SSchockeTest.Kernel).GetMethods();
                Type xFoundType = typeof(FakeKernel);
                var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
                typeof(Cosmos.System.Plugs.System.ConsoleImpl).IsSubclassOf(typeof(object));
                var xEntryPoint = typeof(Kernel).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
                //var xEntryPoint = typeof(Program).GetMethod("ScannerEntryPoint", BindingFlags.NonPublic | BindingFlags.Static);
                //EnableLogging(pathToLogFile)
                xScanner.EnableLogging(logFile);
                //xScanner.TempDebug += new Action<string>(xScanner_TempDebug);
                //xScanner.
                xScanner.QueueMethod(xEntryPoint);
                xScanner.Execute(xCtor);
                using (var xOut = new StreamWriter(outFile, false))
                {
                    //if (EmitDebugSymbols) {
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
    private static void ScannerEntryPoint() {
      Console.WriteLine("Hello, World!");
      var xInt = 0;
      object xObj = xInt;
      xObj.ToString();
    }

  }
}