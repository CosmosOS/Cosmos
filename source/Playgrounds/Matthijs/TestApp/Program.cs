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
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EAX, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EAX, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EAX, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EAX, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBX, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBX, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBX, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBX, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ECX, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ECX, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ECX, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ECX, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDX, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDX, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDX, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDX, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESP, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESP, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESP, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESP, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBP, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBP, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBP, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EBP, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESI, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESI, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESI, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.ESI, DestinationReg = Registers.CR4 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDI, DestinationReg = Registers.CR0 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDI, DestinationReg = Registers.CR2 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDI, DestinationReg = Registers.CR3 };
                //new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.EDI, DestinationReg = Registers.CR4 };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.EAX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.EBX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.ECX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.EDX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.ESP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.EBP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.ESI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR0, DestinationReg = Registers.EDI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.EAX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.EBX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.ECX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.EDX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.ESP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.EBP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.ESI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR2, DestinationReg = Registers.EDI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.EAX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.EBX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.ECX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.EDX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.ESP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.EBP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.ESI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR3, DestinationReg = Registers.EDI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.EAX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.EBX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.ECX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.EDX };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.ESP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.EBP };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.ESI };
                new global::Indy.IL2CPU.Assembler.X86.MoveCR { SourceReg = Registers.CR4, DestinationReg = Registers.EDI };
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