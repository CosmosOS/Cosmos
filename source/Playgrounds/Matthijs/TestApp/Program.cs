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
        static void Main(string[] args) {
            try
            {
                var xAssembler = new Assembler();
                xAssembler.DataMembers.Add(new DataMember("Bladibla", new int[]{1}));
                xAssembler.FlushText(Console.Out);

            }
            catch (Exception E)
            {
                Console.WriteLine("Error: " + E.ToString()); Console.ReadLine();
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