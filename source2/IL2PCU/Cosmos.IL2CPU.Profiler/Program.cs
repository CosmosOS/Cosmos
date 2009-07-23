using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  class Program {

    static void Main(string[] args) {
      DoScan();
    }

    private static void DoScan() {
      var xSW = new Stopwatch();
      xSW.Start();

      var xScanner = new ILScanner();
      xScanner.Execute(typeof(Program).GetMethod("ScannerEntryPoint", BindingFlags.NonPublic | BindingFlags.Static));

      xSW.Stop();
      Console.WriteLine("Total time : {0}", xSW.Elapsed);
      Console.WriteLine("Method count: {0}", xScanner.MethodCount);
      //Console.WriteLine("Instruction count: {0}", xScanner.InstructionCount);
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
