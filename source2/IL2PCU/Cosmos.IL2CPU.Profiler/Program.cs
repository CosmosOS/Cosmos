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
      try {
          DoScan(1);
          //DoScan(2);
      } catch(Exception E) {
          Console.WriteLine(E.ToString());
      }
    }

    //TODO: Encapselate this so it can be used for all projects with ops.
    private static Func<ILOp>[] DummyOps() {
      var xResult = new Func<ILOp>[0xFE1F];
      foreach (var xType in typeof(Program).Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(typeof(ILOpProfiler))) {
          var xAttrib = xType.GetCustomAttributes(typeof(OpCodeAttribute), false).FirstOrDefault() as OpCodeAttribute;
          if (xAttrib != null) {
            var xTemp = new DynamicMethod("Create_" + xAttrib.OpCode + "_Obj", typeof(ILOp), new Type[0], true);
            var xGen = xTemp.GetILGenerator();
            var xCtor = xType.GetConstructor(new Type[0]);
            xGen.Emit(OpCodes.Newobj, xCtor);
            xGen.Emit(OpCodes.Ret);
            xResult[(ushort)xAttrib.OpCode] = (Func<ILOp>)xTemp.CreateDelegate(typeof(Func<ILOp>));
          }
        }
      }
      return xResult;
    }

    private static void DoScan(int aIdx) {
      var xSW = new Stopwatch();
      xSW.Start();

      var xOps = DummyOps();
      Console.WriteLine("({1}) Create Array: {0}", xSW.Elapsed, aIdx);

      var xScanner = new ILScanner();
      xScanner.Ops = xOps;
      xScanner.Execute(typeof(Program).GetMethod("ScannerEntryPoint", BindingFlags.NonPublic | BindingFlags.Static));

      xSW.Stop();
      Console.WriteLine("({1}) Total time : {0}", xSW.Elapsed, aIdx);
      Console.WriteLine("({1}) Method count: {0}", xScanner.MethodCount, aIdx);
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
