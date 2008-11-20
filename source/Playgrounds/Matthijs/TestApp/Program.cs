//#define BINARY
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
                #region code
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
                new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
                #endregion

            }
        }
        static void Main(string[] args) {
            try {
                var xAsm = new Assembler();
                xAsm.Initialize();
                //xAsm.DataMembers.Add(new Indy.IL2CPU.Assembler.DataMember("TestData", new byte[]{65, 66, 67, 68,69,70, 71, 72, 73, 74}));
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