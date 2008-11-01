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
                Memory[new AddressDirect(0xB8000),
                       8] = 65;
                Memory[new AddressDirect(0xB8001),
                       8] = 15;
                new Halt();
            }
        }
        static void Main(string[] args) {
            if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                              "Output"))) {
                Directory.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                              "Output"),
                                 true);
            }
            var xAsm = new Assembler(delegate(string aGroup) {
                return Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                 "Output"),
                                    aGroup + ".out");
            });
            var xRenderer = new Renderer();
            xRenderer.DoRender();
            using (StreamWriter xOutput = new StreamWriter(Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                                                                     "Output"),
                                                                        "TheOutput.out"))) {
                xAsm.FlushText(xOutput);
            }

            // now the file should have been written
        }
    }
}