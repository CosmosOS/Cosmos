using System;
using System.IO;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;

namespace HelloWorldAssembler {
	public class Program {
		public static void Main(string[] args) {
			using (StreamWriter xSW = new StreamWriter("out.asm")) {
				using (Assembler a = new Assembler(xSW)) {
					a.OutputType = Assembler.OutputTypeEnum.DLL;
					a.Includes.Add("win32w.inc");
					new DataMember("_class", "TCHAR", "'Win32 program template'");
					new Noop();
					a.Flush();
				}
			}
			Console.WriteLine("Done.");
			Console.ReadLine();
		}
	}
}