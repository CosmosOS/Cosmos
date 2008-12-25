//#define GenerateTests
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using System.IO;
using System.Reflection;
using Indy.IL2CPU.Assembler.X86.X;
using Indy.IL2CPU.Tests.AssemblerTests.X86;

namespace TestApp {
    class Program {
        class Renderer : Y86 {
            public void DoRender() {
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AH, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.AL, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BH, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.BL, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CH, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.CL, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DH, DestinationReg = Registers.DL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.AH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.AL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.BH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.BL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.CH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.CL, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.DH, Size = 8 };
                new global::Indy.IL2CPU.Assembler.X86.Xor { SourceReg = Registers.DL, DestinationReg = Registers.DL, Size = 8 };
            }
        }
        static void Main(string[] args) {
            try {
#if !GenerateTests
                var xAsm = new Assembler();
                xAsm.Initialize();
                //xAsm.DataMembers.Add(new Indy.IL2CPU.Assembler.DataMember("TestData", new byte[] { 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 }));
                xAsm.Instructions.Clear();
                xAsm.DataMembers.Clear();
                var xRenderer = new Renderer();
                xRenderer.DoRender();
                if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"))) {
                    Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "Output"));
                }
                using (var xOutput = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"),
                                                                            "TheOutput.asm"))) {
                    xAsm.FlushText(xOutput);
                }
                using (Stream xOutput = new FileStream(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output"),
                                                                            "TheOutput.bin"), FileMode.Create)) {
                    xAsm.FlushBinary(xOutput, 0x200000);
                }
#else
                TestCodeGenerator.Execute();
#endif

                //InvalidOpcodeTester.Initialize();
                //InvalidOpcodeTester.ExecuteSingle(typeof(Move), 0);
                //InvalidOpcodeTester.GenerateHtml(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                //                                                                         "Output.html"));
                //InvalidOpcodeTester.GenerateXml(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                //                                                                         "Output.xml"));
            }catch(InvalidOpcodeTester.AbortException){
                InvalidOpcodeTester.GenerateHtml(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output.html"));
                InvalidOpcodeTester.GenerateXml(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                         "Output.xml"));
            } catch (Exception E) { Console.WriteLine(E.ToString()); } 
            finally {
                Console.WriteLine("Finished");
                Console.ReadLine();
                                    Console.ReadLine();
            }
        }
    }
}