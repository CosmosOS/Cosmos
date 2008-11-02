using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using System.IO;
using System.Reflection;
using Indy.IL2CPU.Assembler.X86.X;

namespace TestApp {
    class Program {
        class Renderer : Y86 {
            public void DoRender() {
                Label = "Kernel_Start";
                EAX = 0xB8000;
                IfDefined("DefinedSymbol");
                Memory[0xB8002, 8] = 66;
                Memory[0xB8003, 8] = 15;
                IfDefined("UnDefinedSymbol");
                Memory[0xB8004, 8] = 67;
                Memory[0xB8005, 8] = 15;
                EndIfDefined();
                EndIfDefined();
                new Halt();
            }
        }
        static void Main(string[] args) {
            try {
                var xAsm = new Assembler(delegate(string aGroup) {
                    return Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                     "Output"),
                                        aGroup + ".out");
                });
                xAsm.Initialize();
                xAsm.DataMembers.Add(new Indy.IL2CPU.Assembler.DataMember("TestData", new byte[]{65, 66, 67, 68,69,70, 71, 72, 73, 74}));
                xAsm.Instructions.Clear();
                xAsm.DataMembers.Clear();
                var xRenderer = new Renderer();
                xRenderer.DoRender();
                if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"))) {
                    Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "Output"));
                }
                using (Stream xOutput = new FileStream(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"),
                                                                            "TheOutput.out"), FileMode.Create)) {
                    xAsm.FlushBinary(xOutput, 0x500000);
                }

                //using (StreamWriter xOutput = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                //                                                                         "Output"),
                //                                                            "TheOutput.out"))) {
                //    xAsm.FlushText(xOutput);
                //}

                // now the file should have been written
            } catch (Exception E) { Console.WriteLine(E.ToString()); } 
            finally {
                Console.WriteLine("Finished");
                Console.ReadLine();
            }
        }
    }
}