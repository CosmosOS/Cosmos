using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASharp.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch xSW = new Stopwatch();
            xSW.Start();

            try
            {
                string xSrc = args[0];

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

                xSW.Stop();

                Console.WriteLine("Done in " + xSW.Elapsed.TotalSeconds.ToString() + " seconds");
            }
            catch (Exception ex)
            {
                xSW.Stop();

                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
