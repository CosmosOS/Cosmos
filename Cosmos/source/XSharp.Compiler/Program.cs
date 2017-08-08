using System;
using System.IO;
using Cosmos.Assembler;
using XSharp.Common;

namespace XSharp.Compiler
{
  class Program
  {
    static void Main(string[] aArgs)
    {
      try
      {
        string xSrc = aArgs[0];

        var xGenerator = new AsmGenerator();

        //string[] xFiles;
        //if (Directory.Exists(xSrc))
        //{
        //  xFiles = Directory.GetFiles(xSrc, "*.xs");
        //}
        //else
        //{
        //  xFiles = new string[] { xSrc };
        //}
        //foreach (var xFile in xFiles)
        //{
        //  xGenerator.GenerateToFiles(xFile);
        //}

        var xAsm = new Assembler();
        var xStreamReader = new StringReader(@"namespace Test
            while byte ESI[0] != 0 {
              ! nop
            }
            ");
        var xResult = xGenerator.Generate(xStreamReader);
        Console.WriteLine("done");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        Environment.Exit(1);
      }
    }
  }
}
