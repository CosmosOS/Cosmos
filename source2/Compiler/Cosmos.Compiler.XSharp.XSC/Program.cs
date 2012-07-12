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
        string xSrc = aArgs[0];
        string xNamespace = aArgs[1];

        var xGenerator = new CSharpGenerator();
        xGenerator.Execute(xNamespace, xSrc);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
      }
    }
  }
}
