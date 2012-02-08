using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  class Program {

    // This program profiles the scanning engine.
    // In the future it may profile other aspects as well.

    static void Main(string[] args) {
     Program.DoScan();
    }

    private static void DoScan() {
      var xSW = new Stopwatch();
      xSW.Start();
       
      var xAsmblr = new Assembler();
      using (var xScanner = new ILScanner(xAsmblr))
      {


          //TODO: Add plugs into the scanning equation to profile scanning them too
          //System.Reflection.MethodInfo[] name = typeof(SSchockeTest.Kernel).GetMethods();
          Type xFoundType = typeof(SSchockeTest.Kernel);
          var xCtor = xFoundType.GetConstructor(Type.EmptyTypes);
          typeof(Cosmos.System.Plugs.System.System.ConsoleImpl).IsSubclassOf(typeof(object));
          var xEntryPoint = typeof(SSchockeTest.Kernel).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
          //var xEntryPoint = typeof(Program).GetMethod("ScannerEntryPoint", BindingFlags.NonPublic | BindingFlags.Static);
          //EnableLogging(pathToLogFile)
          xScanner.EnableLogging(AppDomain.CurrentDomain.BaseDirectory + "log.txt");
          //xScanner.TempDebug += new Action<string>(xScanner_TempDebug);
          //xScanner.
          xScanner.Execute(xCtor);

          xSW.Stop();
          Console.WriteLine("Total time : {0}", xSW.Elapsed);
          Console.WriteLine("Method count: {0}", xScanner.MethodCount);
          //Console.WriteLine("Instruction count: {0}", xScanner.InstructionCount);
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
