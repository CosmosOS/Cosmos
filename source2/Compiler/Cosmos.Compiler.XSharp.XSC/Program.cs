using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp.XSC {
  class Program {
    static void Main(string[] aArgs) {
      string xSrc = aArgs[1];
      string xDest = Path.ChangeExtension(xSrc, ".cs");
      string xNamespace = aArgs[2];

      using (var xInput = new StreamReader(xSrc)) {
        using (var xOutput = new StreamWriter(xDest)) {
          var xGenerator = new Generator();
          xGenerator.Name = Path.GetFileNameWithoutExtension(xSrc);
          xGenerator.Namespace = xNamespace;
          xGenerator.Execute(xInput, xOutput);
        }
      }
    }
  }
}
