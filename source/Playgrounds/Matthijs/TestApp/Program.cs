//#define GenerateTests
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using System.IO;
using System.Reflection;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler.X86.X;
using Indy.IL2CPU.Tests.AssemblerTests.X86;
using Assembler=Indy.IL2CPU.Assembler.X86.Assembler;

namespace TestApp {
    class Program {
        class Renderer : Y86 {
            public void DoRender() {

            }
        }
        private static void Render(ELFAssembler aAssembler) {
            aAssembler.Instructions.Clear();
            aAssembler.DataMembers.Clear();
            aAssembler.DataMembers.Add(new DataMember("Magic", 0x1BADB002) { IsGlobal = true, Alignment = 4 });
            aAssembler.DataMembers.Add(new DataMember("Flags", 0x00000003) { IsGlobal = true, Alignment = 4 });
            aAssembler.DataMembers.Add(new DataMember("Checksum", 0 - 0x1BADB005) { IsGlobal = true, Alignment = 4 });
            aAssembler.DataMembers.Add(new DataMember("MyData", new byte[1] { 65 }) { IsGlobal = true });
            var xItem = new Move {
                DestinationValue = 0xB8000,
                DestinationIsIndirect = true,
                SourceValue = 65,
                Size = 8
            };
            //4byte: magic
            // 4byte flags
            // 4byte checksum
            aAssembler.StartLabel = new Label("_the_start") { IsGlobal = true };
            new Move {
                DestinationValue = 0xB8000,
                DestinationIsIndirect = true,
                SourceValue = 66,
                Size = 8
            };
            new Label("_before_end");
            new Halt();
            new Jump { DestinationLabel = "_before_end" };
        }

        static void Main(string[] args) {
            try {

                var xAssembler = new ELFAssembler();
                Render(xAssembler);
                using (var xOutText = new StreamWriter(@"D:\.NET\Cosmos\ELFOut\asm\output.asm")) {
                    xAssembler.FlushText(xOutText);
                }
                xAssembler = new ELFAssembler();
                Render(xAssembler);
                using (var xOutBin = new FileStream(@"D:\.NET\Cosmos\ELFOut\bin\output.bin", FileMode.Create)) {
                    xAssembler.FlushBinary(xOutBin, 0);
                }

            } catch (Exception E) {
                Console.WriteLine("Error: " + E.ToString());
            } finally {
                Console.WriteLine("Done.");
                Console.ReadLine();
            }
        }
    }
}