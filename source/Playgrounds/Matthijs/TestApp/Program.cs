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
                new Move { DestinationReg = Registers.EAX, SourceValue = 1 };
                new Move { DestinationReg = Registers.EBX, SourceValue = 2 };
                new Move { DestinationReg = Registers.ECX, SourceValue = 3 };
                new Move { DestinationReg = Registers.EDX, SourceValue = 4 };
                new Move { DestinationReg = Registers.ESI, SourceValue = 5 };
                new Move { DestinationReg = Registers.EDI, SourceValue = 6 };
            }
        }
        static void Main(string[] args) {
            try {
                var xAsm = new Assembler();
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
                                                                            "TheOutput.bin"), FileMode.Create)) {
                    xAsm.FlushBinary(xOutput, 0x200000);
                } 

                //using (var xOutput = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                //                                                                         "Output"),
                //                                                            "TheOutput.asm"))) {
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