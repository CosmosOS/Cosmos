using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.Assembler;

namespace XSharp.Compiler {
  class Program {
    static void Main(string[] aArgs) {
      try {
        string xSrc = aArgs[0];

        var xGenerator = new AsmGenerator();

        string[] xFiles;
        if (Directory.Exists(xSrc))
        {
          xFiles = Directory.GetFiles(xSrc, "*.xs");
        }
        else
        {
          xFiles = new string[] { xSrc };
        }
        foreach (var xFile in xFiles)
        {
          xGenerator.GenerateToFiles(xFile);
        }

        //    var xAsm = new Assembler();
        //    var xStreamReader = new StringReader(@"namespace Test
        //    var CallerEBP
        //        while ESI != .CallerEBP {
        //    ! nop
        //}
        //    ");
        //    var xResult = xGenerator.Generate(xStreamReader);
        Console.WriteLine("done");
      } catch (Exception ex) {
        Console.WriteLine(ex.ToString());
        Environment.Exit(1);
      }
    }
  }
}
