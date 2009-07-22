using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  class Program {

    static void Main(string[] args) {
      try {
          DoScan(1);
          //DoScan(2);
      } catch(Exception E) {
          Console.WriteLine(E.ToString());
      }
    }

    private static void DoScan(int aIdx) {
      var xSW = new Stopwatch();
      xSW.Start();
      
      //var xTest = X86Util.GetInstructionCreatorArray();
      Console.WriteLine("({1}) Create Array: {0}", xSW.Elapsed, aIdx);

      //var xScanner = new Scanner();
      //xScanner.Ops = xTest;
      //xScanner.Execute(typeof(Program).GetMethod("ScannerEntryPoint", BindingFlags.NonPublic | BindingFlags.Static));

      xSW.Stop();
      Console.WriteLine("({1}) Total time : {0}", xSW.Elapsed, aIdx);
      //Console.WriteLine("({1}) Method count: {0}", xScanner.MethodCount, aIdx);
      //Console.WriteLine("({1}) Instruction count: {0}", xScanner.InstructionCount, aIdx);
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
