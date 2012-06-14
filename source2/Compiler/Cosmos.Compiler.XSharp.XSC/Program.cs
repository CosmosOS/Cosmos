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

      //using (var xInput = new StringReader(tboxInput.Text)) {
      //  using (var xOut = new StringWriter()) {

      //    var xGenerator = new Generator();
      //    xGenerator.Name = Path.GetFileNameWithoutExtension(inputFilename);
      //    xGenerator.Namespace = defaultNamespace;
      //    xGenerator.Execute(input, output);
          
      //    Cosmos.Compiler.XSharp.Generator.Execute(xInput, "InputFileName", xOut, "Default.Namespace");
      //    textOutput.Text = xOut.ToString();
      //  }
      //}
    }
  }
}
