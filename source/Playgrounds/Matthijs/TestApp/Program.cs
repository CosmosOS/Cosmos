using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Indy.IL2CPU.Assembler;
using System.IO;
using System.Reflection;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86.X;
using Indy.IL2CPU.Tests.AssemblerTests.X86;
using Assembler=Indy.IL2CPU.Assembler.X86.Assembler;
using Indy.IL2CPU.Compiler;
using Indy.IL2CPU.IL.X86;
using Cosmos.Compiler.Builder;
using Indy.IL2CPU;
using System.Collections.ObjectModel;

namespace TestApp {
    class Program {
        static void Main(string[] args)
        {
            try
            {
                var xCompilerHelper = new CompilerHelper();
                xCompilerHelper.DebugLog +=
                    ((aSeverity, aMessage) => Console.WriteLine("{0}: {1}", aSeverity, aMessage));
                xCompilerHelper.GetAssembler += ((arg1, arg2) => new Assembler());
                xCompilerHelper.GetOpCodeMap += (() => new X86OpCodeMap());
                xCompilerHelper.SaveAssembler +=new Action<Assembly,Indy.IL2CPU.Assembler.Assembler>(
                    delegate(Assembly arg1, Indy.IL2CPU.Assembler.Assembler arg2)
                        {
                            using (var xOut = new StreamWriter(Path.Combine(@"e:\temp\", arg1.GetName().Name + ".asm")))
                            {
                                arg2.FlushText(xOut);
                            }
                        });
                xCompilerHelper.CompileExe(typeof(MatthijsTest.Program).Assembly);
            }
            catch (Exception E)
            {
                Console.WriteLine("Error: " + E.ToString());
                Console.ReadLine();
                if (E.Message == "Temporary abort")
                {
                    Terminate = true;
                }
            }
            finally
            {
                Console.WriteLine("Done.");
                GC.Collect();
                if (!Terminate)
                {
                    //Console.ReadLine();
                }
            }
        }

        private static bool Terminate = false;
    }
}