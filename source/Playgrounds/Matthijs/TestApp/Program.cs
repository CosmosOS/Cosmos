using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using Cosmos.Compiler;
using Cosmos.Compiler.IL;


namespace TestApp {
    class Program {
        static void Main(string[] args)
        {
            try
            {
                //var xSW = new Stopwatch();
                //xSW.Start();
                var xTest = X86Util.GetInstructionCreatorArray();
                //xSW.Stop();
                //Console.WriteLine("Time to create InstructionArray (1): {0}", xSW.Elapsed);
                //xSW.Reset();
                //xSW.Start();
                xTest = X86Util.GetInstructionCreatorArray();
                //xSW.Stop();
                //Console.WriteLine("Time to create InstructionArray (2): {0}", xSW.Elapsed);
                //xSW.Reset();
                var xScanner = new Scanner();
                xScanner.Ops = xTest;
                //xSW.Start();
                xScanner.Execute(typeof (Program).GetMethod("Entrypoint", BindingFlags.NonPublic | BindingFlags.Static));
                //xSW.Stop();
                //Console.WriteLine("Scan time (1): {0}", xSW.Elapsed);
                //xSW.Reset();
                //xScanner = new Scanner();
                //xScanner.Ops = xTest;
                //xSW.Start();
                //xScanner.Execute(typeof(Program).GetMethod("Entrypoint", BindingFlags.NonPublic | BindingFlags.Static));
                //xSW.Stop();
                //Console.WriteLine("Scan time (2): {0}", xSW.Elapsed);
                Console.WriteLine("Method count: {0}", xScanner.MethodCount);
                //Console.WriteLine("Done");
                //Gen();
            }
            catch(Exception E)
            {
                Console.WriteLine(E.ToString());
                Console.ReadLine();
            }
        }

        private static void Gen()
        {
            throw new Exception("Watch out, will overwrite existing code!");
            var xOps = new List<OpCodeEnum>();
            #region fill xOps
            foreach(OpCodeEnum xOp in Enum.GetValues(typeof(OpCodeEnum)))
            {
                var xShort = ILReader.GetNonShortcutOpCode(xOp);
                if(!xOps.Contains(xShort))
                {
                    xOps.Add(xShort);
                }
            }
            #endregion
            foreach(var xOp in xOps)
            {
                using (var xWriter = new StreamWriter(@"E:\Cosmos\source\Compiler\Compiler\IL\X86\" + xOp + ".cs"))
                {               
                    xWriter.WriteLine("using System;");
                    xWriter.WriteLine();
                    xWriter.WriteLine("namespace Cosmos.Compiler.IL.X86");
                    xWriter.WriteLine("{");
                    {
                        xWriter.WriteLine("\t[OpCode(OpCodeEnum.{0})]", xOp);
                        xWriter.WriteLine("\tpublic class {0}: Op", xOp);
                        xWriter.WriteLine("\t{");
                        {
                            xWriter.WriteLine();
                            xWriter.WriteLine();
                            xWriter.WriteLine();
                            if (File.Exists(@"E:\Cosmos\source\Compiler\Indy.IL2CPU.X86\IL\x86\" + xOp + ".cs"))
                            {
                                xWriter.WriteLine("\t\t#region Old code");
                                {
                                    foreach (
                                        var xLine in
                                            File.ReadAllLines(@"E:\Cosmos\source\Compiler\Indy.IL2CPU.X86\IL\x86\" + xOp +
                                                              ".cs"))
                                    {
                                        xWriter.WriteLine("\t\t// " + xLine);
                                    }
                                }
                                xWriter.WriteLine("\t\t#endregion");
                            }
                        }
                        xWriter.WriteLine("\t}");
                    }
                    xWriter.WriteLine("}");
                }
            }
        }


        private static void Entrypoint()
        {
            Console.WriteLine("Hello, World!");
        }

        private static bool Terminate = false;
    }
}