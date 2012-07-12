using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.Compiler.XSharp;

namespace XSharp.XSC {
  class Program {
    static void Main(string[] aArgs) {
      try {
        if (aArgs[0].ToUpper() == "-CSHARP") {
          string xSrc = aArgs[1];
          string xNamespace = aArgs[2];

          var xGenerator = new CSharpGenerator();
          xGenerator.Execute(xNamespace, xSrc);
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
      }
    }
  }
}
