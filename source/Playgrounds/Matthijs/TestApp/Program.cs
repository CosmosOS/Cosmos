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
                DoScan(1);
                //DoScan(2);
            }
            catch(Exception E)
            {
                Console.WriteLine(E.ToString());
            }
      }

        private static void DoScan(int aIdx)
        {
            var xSW = new Stopwatch();
            xSW.Start();
            var xTest = X86Util.GetInstructionCreatorArray();
            Console.WriteLine("({1}) Create Array: {0}", xSW.Elapsed, aIdx);
            var xScanner = new Scanner();
            xScanner.Ops = xTest;
            xScanner.Execute(typeof (Program).GetMethod("Entrypoint", BindingFlags.NonPublic | BindingFlags.Static));
            xSW.Stop();
            Console.WriteLine("({1}) Scan time : {0}", xSW.Elapsed, aIdx);
            Console.WriteLine("({1}) Method count: {0}", xScanner.MethodCount, aIdx);
            Console.WriteLine("({1}) Instruction count: {0}", xScanner.InstructionCount, aIdx);
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
            var xInt = 0;
            object xObj = xInt;
            xObj.ToString();
        }

        private static bool Terminate = false;
    }
}