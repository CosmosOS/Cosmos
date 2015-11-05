using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XSharp.Compiler {
  class Program {
    static void Main(string[] aArgs) {
      try {
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
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
      }
    }
  }
}
