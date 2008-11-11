#define BINARY
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
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.AL, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.AH, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.BL, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.BH, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.CL, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.CH, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.DL, SourceReg = Registers.DH };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.AL };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.AH };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.BL };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.BH };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.CL };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.CH };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.DL };
                new Move { DestinationReg = Registers.DH, SourceReg = Registers.DH };

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
#if BINARY
                using (Stream xOutput = new FileStream(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"),
                                                                            "TheOutput.bin"), FileMode.Create)) {
                    xAsm.FlushBinary(xOutput, 0x200000);
                } 
#else
                using (var xOutput = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"),
                                                                            "TheOutput.asm"))) {
                    xAsm.FlushText(xOutput);
                }
#endif

                // now the file should have been written
            } catch (Exception E) { Console.WriteLine(E.ToString()); } 
            finally {
                Console.WriteLine("Finished");
                Console.ReadLine();
            }
        }
    }
}