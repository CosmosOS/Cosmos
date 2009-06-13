using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using System.IO;
using System.Reflection;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86.X;
//using Indy.IL2CPU.Tests.AssemblerTests.X86;
using Assembler = Indy.IL2CPU.Assembler.X86.Assembler;
//using Indy.IL2CPU.Compiler;
using Indy.IL2CPU.IL.X86;
using Cosmos.Compiler.Builder;
using Indy.IL2CPU;
using Indy.IL2CPU.Compiler;


namespace CompilerTester
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var xBasePath = BuildOptions.Load().BuildPath;
                var xCompileHelper = new CompilerHelper(); //Todo push ICompilerOptions into CompilerHelper

                xCompileHelper.GetCacheStateFile += new Func<Assembly, string>(delegate(Assembly aAssembly)
                {
                    return Path.Combine(xBasePath, aAssembly.GetName().Name + ".cachestate");
                });
                xCompileHelper.GetChecksumFile += new Func<Assembly, string>(delegate(Assembly aAssembly)
                {
                    return Path.Combine(xBasePath, aAssembly.GetName().Name + ".checksum");
                });
                xCompileHelper.GetOpCodeMap += new Func<Indy.IL2CPU.IL.OpCodeMap>(delegate { return new X86OpCodeMap(); });
                xCompileHelper.SkipList.Add(typeof(Builder).Assembly);
                xCompileHelper.GetAssembler += new Func<Assembly, bool, Indy.IL2CPU.Assembler.Assembler>(
                    delegate(Assembly aAssembly, bool aIsMain)
                    {
                        return new Assembler();
                    });
                xCompileHelper.SaveAssembler += new Action<Assembly, Indy.IL2CPU.Assembler.Assembler>(delegate(Assembly aAssembly, Indy.IL2CPU.Assembler.Assembler aAssembler)
                {
                    using (var xOut = new StreamWriter(Path.Combine(xBasePath, aAssembly.GetName().Name + ".out"), false))
                    {
                        aAssembler.FlushText(xOut);
                    }
                });
                xCompileHelper.DebugLog += delegate(LogSeverityEnum aSeverity, string aMessage) { Console.WriteLine("{0}: {1}", aSeverity, aMessage); };


                string PlugPath = Path.Combine(Path.Combine(xBasePath, "Tools"), "Plugs"); 
                xCompileHelper.Plugs.Add(Path.Combine(PlugPath, "Cosmos.Kernel.Plugs.dll"));
                xCompileHelper.Plugs.Add(Path.Combine(PlugPath, "Cosmos.Hardware.Plugs.dll"));
                xCompileHelper.Plugs.Add(Path.Combine(PlugPath, "Cosmos.Sys.Plugs.dll"));
                xCompileHelper.CompileExe(typeof(MatthijsTest.Program).Assembly);
            }
            catch (Exception E)
            {
                Console.WriteLine("Error: " + E.ToString());
                if (E.Message == "Temporary abort")
                {
                    Terminate = true;
                }
            }
            finally
            {
                Console.WriteLine("Done.");
                if (!Terminate)
                {
                    Console.ReadLine();
                }
            }
        }
        private static bool Terminate = false;
    }
}
