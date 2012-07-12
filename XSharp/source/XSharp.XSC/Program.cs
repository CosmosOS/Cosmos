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
          if (Directory.Exists(xSrc)) {
            foreach (var xFile in Directory.GetFiles(xSrc, "*.xs")) {
              xGenerator.Execute(xNamespace, xFile);
            }
          } else {
            xGenerator.Execute(xNamespace, xSrc);
          }
        } else {
          string xSrc = aArgs[0];

          var xGenerator = new AsmGenerator();
          string[] xFiles;
          if (Directory.Exists(xSrc)) {
            xFiles = Directory.GetFiles(xSrc, "*.xs");
          } else {
            xFiles = new string[] { xSrc };
          }
          foreach (var xFile in xFiles) {
            xGenerator.GenerateToFiles(xFile);
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
      }
    }
  }
}
