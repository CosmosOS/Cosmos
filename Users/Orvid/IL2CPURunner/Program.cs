using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Build.Common;
using Cosmos.IL2CPU.X86;
using Cosmos.Debug.Common;
using Cosmos.IL2CPU;
using System.IO;

namespace IL2CPURunner
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            StreamWriter strw = new StreamWriter("log.txt");
            s.Start();
            System.Reflection.Assembly.Load("Cosmos.Build.Common");
            System.Reflection.Assembly.Load("Cosmos.Compiler.Assembler");
            System.Reflection.Assembly.Load("Cosmos.Compiler.Assembler.X86");
            System.Reflection.Assembly.Load("Cosmos.Core_Plugs");
            System.Reflection.Assembly.Load("Cosmos.Debug.Common");
            System.Reflection.Assembly.Load("Cosmos.Core.DebugStub");
            System.Reflection.Assembly.Load("Cosmos.Debug.Kernel");
            System.Reflection.Assembly.Load("Cosmos.Debug.Kernel.Plugs");
            System.Reflection.Assembly.Load("Cosmos.IL2CPU");
            System.Reflection.Assembly.Load("IL2CPU.API");
            System.Reflection.Assembly.Load("Cosmos.IL2CPU.X86");
            System.Reflection.Assembly.Load("Cosmos.System");
            System.Reflection.Assembly.Load("Cosmos.System_Plugs");
            var xInitMethod = typeof(StructTest.Kernel).GetConstructor(Type.EmptyTypes);
            var xOutputFilename = System.Windows.Forms.Application.StartupPath + "\\" + "StructTest";
            var xAsm = new AppAssemblerNasm(0);
            using (var xDebugInfo = new DebugInfo())
            {
                xDebugInfo.CreateCPDB(xOutputFilename + ".cpdb");
                xAsm.DebugInfo = xDebugInfo;
                xAsm.DebugMode = DebugMode.None;
                xAsm.TraceAssemblies = TraceAssemblies.All;
                xAsm.ShouldOptimize = true;

                var xNasmAsm = (AssemblerNasm)xAsm.Assembler;
                xAsm.Assembler.Initialize();
                using (var xScanner = new ILScanner(xAsm))
                {
                    // TODO: shouldn't be here?
                    xScanner.QueueMethod(xInitMethod.DeclaringType.BaseType.GetMethod("Start"));
                    xScanner.Execute(xInitMethod);

                    using (var xOut = new StreamWriter(xOutputFilename + ".asm", false))
                    {
                        xAsm.Assembler.FlushText(xOut);
                    }
                }
            }
            s.Stop();
            strw.Write("Total Time to Run: " + s.ElapsedMilliseconds.ToString() + "ms");
            strw.Close();
        }
    }
}
